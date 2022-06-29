using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Linq;
using GraphSynth.Representation;
using GraphSynth.UI;
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
            SingleInstanceWatcher();
            var app = new GSApp(args.ToList());
            app.Run();
        }
        GSApp(List<string> inputArgs)
        {
            InputArgs = inputArgs;
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
            SetUpMainWindow();
            SearchIO.output("starting main form...");
            var aGS = new AboutGraphSynth(false);
            aGS.Show();

            SearchIO.output("Reading in settings file");
            ReadInSettings();

            SearchIO.output("Reading in default shapes.");
            LoadInShapes();


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
                                                      FileName = "http://www.graphsynth.com/files/install/setup.exe",
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
            SearchIO.popUpDialogger = new GraphSynth.CustomControls.PopUpDialogger(main);
            SearchIO.graphPresenter = new GraphSynth.CustomControls.GraphPresenter(main);
            SearchIO.main = main;
            main.Show();
        }

        /// <summary>The event mutex name.</summary>
        private const string UniqueEventName = "StartingGraphSynth3e2e4000-4300-4b92-80ad-7e75d48e41a9";


        /// <summary>
        /// prevent a second instance and signal it to bring its mainwindow to foreground
        /// </summary>
        /// <seealso cref="https://stackoverflow.com/a/23730146/1644202"/>
        public static void SingleInstanceWatcher()
        {
            // check if it is already open.
            // try to open it - if another instance is running, it will exist
            if (EventWaitHandle.TryOpenExisting(UniqueEventName, out var eventWaitHandle))
            {
                // Notify other instance so it could bring itself to foreground.
                eventWaitHandle.Set();
                // if this instance gets the signal to show the main window
                new Task(() =>
                {
                    while (eventWaitHandle.WaitOne())
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            // could be set or removed anytime
                            if (!Application.Current.MainWindow.Equals(null))
                            {
                                var mw = Application.Current.MainWindow;

                                if (mw.WindowState == WindowState.Minimized || mw.Visibility != Visibility.Visible)
                                {
                                    mw.Show();
                                    mw.WindowState = WindowState.Normal;
                                }

                                // According to some sources these steps guarantee that an app will be brought to foreground.
                                mw.Activate();
                                mw.Topmost = true;
                                mw.Topmost = false;
                                mw.Focus();
                            }
                        }));
                    }
                })
                .Start();
                // Terminate this instance.
                Environment.Exit(1);
            }
            else
                // listen to a new event
                eventWaitHandle = null;// new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
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
}