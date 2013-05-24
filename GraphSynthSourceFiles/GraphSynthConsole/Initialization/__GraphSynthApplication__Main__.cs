using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using GraphSynth.Representation;

namespace GraphSynth
{
    public static partial class GSApp
    {
        #region Fields & Constructor
        /* The class globalSettings is in the IOandXML directory. These values
         * are loaded in from the App.config file. */
        public static GlobalSettings settings;

        /* this is defined to aid in finding our way our the various folders used in GraphSynth */
        public static List<string> InputArgs;
        public static Boolean ArgFilesToOpen, ArgAltConfig;
        public static string AlternateConfig = "";
        #endregion

        #region The main entry point for the application.

        [STAThread]
        public static void Main(string[] args)
        {
            InputArgs = new List<string>(args);
            ParseArguments();

            SearchIO.output("Reading in settings file", 3);
            ReadInSettings();

            SearchIO.output("opening files...", 3);
            OpenFiles();

            SearchIO.output("Checking for update online", 3);
            var updateThread = new Thread(CheckForUpdate);
            updateThread.Start();

            SearchIO.output("opening plugins...", 3);
            LoadPlugins();

            PluginDialog();

            SearchIO.output("----- Load in Complete ------", 3);
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
                    var lastPos = newChanges.FindIndex(a => currentVersion.ToString().Equals(a.Value));
                    if (lastPos != -1) newChanges.RemoveRange(lastPos, newChanges.Count - lastPos);
                    var changeScript = "";
                    var i = 0;
                    foreach (var x in newChanges)
                        if (x.Name.LocalName.Equals("Change"))
                            changeScript += "  " + (++i) + ". " + x.Value + "\n";
                        else changeScript += "since version " + x.Value + "\n";
                    var result =
                        SearchIO.output("***** New Version Available!*****" +
                            "There is a new version of GraphSynth2 online. You are currently using version " +
                            currentVersion + " and the online version is " + onlineVersion +
                            ".\nHere are the recent updates: \n" + changeScript);
                    if (result)
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
            settings.filer = new ConsoleFiler(settings.InputDirAbs, settings.OutputDirAbs, settings.RulesDirAbs);
            if (ArgFilesToOpen)
            {
                settings.rulesets = new ruleSet[settings.numOfRuleSets];
            }
            else
            {
                settings.LoadDefaultSeedAndRuleSets();
                for (int i = 0; i < settings.numOfRuleSets; i++)
                    if (settings.rulesets[i] != null)
                        settings.rulesets[i].RuleSetIndex = i;

            }
        }

        private static void ReadInSettings()
        {
            /* loadDefaults can be time consuming if there are many ruleSets/rules to load. */
            settings = ArgAltConfig ? GlobalSettings.readInSettings(AlternateConfig) : GlobalSettings.readInSettings();
            SearchIO.defaultVerbosity = settings.DefaultVerbosity;
            SearchIO.output("Default Verbosity set to " + settings.DefaultVerbosity, 3);
        }



        #endregion
    }
}