using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Linq;
using GraphSynth.Representation;
using GraphSynth.UI;
using Microsoft.VisualBasic.ApplicationServices;
using StartupEventArgs = System.Windows.StartupEventArgs;

namespace GraphSynth
{
    internal class GSApp : Application
    {
        #region The main entry point for the application.

        /* All this does is create the small class that checks whether an
         * existing instance is still running. */

        [STAThread]
        public static void Main(string[] args)
        {
            var manager = new CheckForExistingInstance();
            manager.Run(args);
        }

        #endregion

        #region Fields

        /* The class globalSettings is in the IOandXML directory. These values
         * are loaded in from the App.config file. */
        public static GlobalSettings settings;
        /* this is the reference to the mainForm - the top/largest GraphSynth Window */
        public static MainWindow main;
        /* this is the reference to SearchIO text that appears to the right of the mainform */
        public static SearchIOToTextWriter console;
        /* this is defined to aid in finding our way our the various folders used in GraphSynth */

        public static List<string> InputArgs;
        public static Boolean ArgFilesToOpen, ArgAltConfig;
        public static string AlternateConfig = "";
        #endregion

        #region Main Load-in Function - overrides OnStartup

        protected override void OnStartup(StartupEventArgs args)
        {
            base.OnStartup(args);
            ParseArguments();
            /* Printing is done though the usual Console.Writeline, to a textbox. */
            console = new SearchIOToTextWriter();
            Console.SetOut(console);
            var aGS = new AboutGraphSynth(false);
            aGS.Show();

            SearchIO.output("Reading in settings file");
            ReadInSettings();

            SearchIO.output("Reading in default shapes.");
            LoadInShapes();

            SearchIO.output("starting main form...");
            SetUpMainWindow();

            SearchIO.output("opening files...");
            OpenFiles();

            SearchIO.output("Checking for update online");
            var updateThread = new Thread(CheckForUpdate);
            updateThread.Start();

            /* Okay, done with load-in, close self-promo */
            SearchIO.output("Closing Splash...");
            /* set the console.Writeline to the outputTextBox in main since the splash screen 
            * is now closed. */
            main.outputTextBox.Text = aGS.outputTextBox.Text;
            console.outputBox = main.outputTextBox;
            aGS.Close();
            SearchIO.output("----- Load in Complete ------");
            //  main.WindowState = WindowState.Normal; //for some reason with this line intact,
            // the process would never be terminated. (also see commented minimize cmd @line69 
        }

        #endregion

        #region Helper Functions to the Main Startup

        private static void CheckForUpdate()
        {
            try
            {
                var xr = XmlReader.Create("http://www.graphsynth.com/files/install/GraphSynth2_Installer.xml");
                var data = XElement.Load(xr);
                var onlineVersion = new Version(data.Element("Version").Value);
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                var isNewer = onlineVersion.CompareTo(currentVersion);
                if (isNewer == 1)
                {
                    var procArch = Assembly.GetExecutingAssembly().GetName().ProcessorArchitecture.ToString();
                    if (procArch.Contains("64")) procArch = "X64";
                    else procArch = "X86";
                    var newChanges = new List<XElement>(data.Elements());
                    newChanges.RemoveAt(0);
                    var lastPos = newChanges.FindIndex(a => a.Name.LocalName.Equals("Version")
                        && currentVersion.CompareTo(new Version(a.Value)) >= 0);
                    if (lastPos != -1) newChanges.RemoveRange(lastPos, newChanges.Count - lastPos);
                    var changeScript = "";
                    var i = 0;
                    foreach (var x in newChanges)
                        if (x.Name.LocalName.Equals("Change"))
                            changeScript += "  " + (++i) + ". " + x.Value + "\n";
                        else changeScript += "since version " + x.Value + "\n";
                    var result =
                        MessageBox.Show(
                            "There is a new version of GraphSynth2 online. You are currently using version " +
                            currentVersion + " and the online version is " + onlineVersion +
                            ".\nHere are the recent updates: \n" + changeScript +
                            "\n\n Would you like to download the updated installer" +
                            " and quit GraphSynth2 now?", "New Version Available!", MessageBoxButton.YesNo,
                            MessageBoxImage.Question, MessageBoxResult.No);
                    if (result == MessageBoxResult.Yes)
                    {
                        var process = new Process
                                          {
                                              StartInfo =
                                                  {
                                                      FileName = "http://www.graphsynth.com/files/install/" + procArch +
                                                                 "/setup.exe",
                                                      Verb = "open",
                                                      UseShellExecute = true
                                                  }
                                          };
                        process.Start();
                        xr.Close();
                        main.Close();
                    }
                }
                else
                {
                    SearchIO.output("Current version is up to date.");
                }
                xr.Close();
            }
            catch
            {
                SearchIO.output("Unable to check for update.");
            }
        }

        public static void ParseArguments()
        {
            var result = InputArgs.Find(s => Path.GetExtension(s).Equals(".gsconfig"));
            if (string.IsNullOrWhiteSpace(result) || !File.Exists(result))
                ArgAltConfig = false;
            else
            {
                AlternateConfig = Path.GetFullPath(result);
                ArgAltConfig = true;
            }
            InputArgs.RemoveAll(s => (!File.Exists(s) ||
                                      !(Path.GetExtension(s).Equals(".gxml") ||
                                        Path.GetExtension(s).Equals(".grxml") ||
                                        Path.GetExtension(s).Equals(".rsxml"))));
            for (var i = 0; i < InputArgs.Count; i++)
                InputArgs[i] = Path.GetFullPath(InputArgs[i]);
            ArgFilesToOpen = (InputArgs.Count > 0);
        }

        public static void OpenFiles()
        {
            settings.filer = new WPFFiler(settings.InputDirAbs, settings.OutputDirAbs, settings.RulesDirAbs);
            if (ArgFilesToOpen)
            {
                settings.rulesets = new ruleSet[settings.numOfRuleSets];
                foreach (var s in InputArgs)
                    main.OpenAndShow(s);
            }
            else
            {
                settings.LoadDefaultSeedAndRuleSets();
                for (int i = 0; i < settings.numOfRuleSets; i++)
                    if (settings.rulesets[i] != null)
                        ((ruleSet)settings.rulesets[i]).RuleSetIndex = i;
            }
        }


        private void ReadInSettings()
        {
            /* loadDefaults can be time consuming if there are many ruleSets/rules to load. */
            if (ArgAltConfig)
                settings = GlobalSettings.readInSettings(AlternateConfig);
            else settings = GlobalSettings.readInSettings();
            SearchIO.defaultVerbosity = settings.DefaultVerbosity;
            SearchIO.output("Default Verbosity set to " + settings.DefaultVerbosity);
        }

        private static void SetUpMainWindow()
        {
            /* declare the main window that contains other windows and is the main place/thread that
             * all other routines are run from. */
            main = new MainWindow();
            SearchIO.main = main;
            main.Show();
        }


        private static void LoadInShapes()
        {
            /*** here the default parameters from ShapeDescriptors.xaml are loaded in.*/
            var dic = new ResourceDictionary
                          {
                              Source = new Uri("Properties/DefaultShapeDescriptors.xaml", UriKind.Relative)
                          };
            foreach (string k in dic.Keys)
                Current.Resources.Add(k, XamlWriter.Save(dic[k]));
            dic = new ResourceDictionary
                      {
                          Source = new Uri("Properties/IconShapeDescriptors.xaml", UriKind.Relative)
                      };
            Current.Resources.MergedDictionaries.Add(dic);
            dic = new ResourceDictionary
                      {
                          Source = new Uri("Properties/DefaultCanvases.xaml", UriKind.Relative)
                      };
            Current.Resources.MergedDictionaries.Add(dic);
        }

        #endregion
    }


    /// <summary>
    ///   This simple class, which inherits from a Visual Basic Application Base, is 
    ///   used to check if an existing copy of GraphSynth2 is running. It came about
    ///   from having icons for gxml, grxml and rsxml, and the desire to be able to 
    ///   click from explorer to open it.
    /// </summary>
    public class CheckForExistingInstance : WindowsFormsApplicationBase
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref = "CheckForExistingInstance" /> class.
        /// </summary>
        public CheckForExistingInstance()
        {
            IsSingleInstance = true;
        }

        /// <summary>
        ///   Raises the event, which happens only the first time 
        ///   that the application is started.
        /// </summary>
        /// <param name = "e">The <see cref = "Microsoft.VisualBasic.ApplicationServices.StartupEventArgs" /> 
        ///   instance containing the event data.</param>
        /// <returns></returns>
        protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs e)
        {
            var app = new GSApp();
            GSApp.InputArgs = new List<string>(e.CommandLine);
            app.Run();
            return false;
        }

        /// <summary>
        ///   For subsequent instances of GraphSynth, simply get to the original and pass it
        ///   the files that are to be opened - these are passed as the arguments.
        /// </summary>
        /// <param name = "e"><see cref = "T:Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs" />. Contains the command-line arguments of the subsequent application instance and indicates whether the first application instance should be brought to the foreground upon exiting the exception handler.</param>
        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs e)
        {
            base.OnStartupNextInstance(e);
            GSApp.InputArgs = new List<string>(e.CommandLine);
            SearchIO.output(StringCollectionConverter.convert(GSApp.InputArgs));
            GSApp.ParseArguments();
            if (GSApp.ArgAltConfig)
                GSApp.settings =
                    GlobalSettings.readInSettings(GSApp.AlternateConfig);
            GSApp.OpenFiles();
            GSApp.main.setUpGraphElementAddButtons();
            GSApp.main.setUpGraphLayoutMenu();
            GSApp.main.setUpSearchProcessMenu();
            GSApp.main.Activate();
        }
    }
}