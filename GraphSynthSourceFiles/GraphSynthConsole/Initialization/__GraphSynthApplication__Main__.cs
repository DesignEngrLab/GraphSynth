using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public static Boolean ArgContainsFilesToOpen, ArgContainsConfig, ArgContainsPluginCommands;
        public static List<string> InputArgs;
        public static List<string> ArgFiles;
        public static string ArgConfig = "";
        #endregion

        #region The main entry point for the application.

        [STAThread]
        public static void Main(string[] args)
        {
            SearchIO.popUpDialogger = new PopUpDialogger();

            InputArgs = new List<string>(args);
            ParseArguments();

            SearchIO.output("Reading in settings file", 3);
            ReadInSettings();
            SearchIO.output("opening files...", 3);
            OpenFiles();

            //SearchIO.output("Checking for update online", 3);
            //var updateThread = new Thread(CheckForUpdate);
            //updateThread.Start();

            SearchIO.output("opening plugins...", 3);
            LoadPlugins();
            SearchIO.output("----- Load in Complete ------", 3);
            if (!ArgContainsPluginCommands || !InvokeArgPlugin())
            {
                PluginDialog();
                //Console.WriteLine("Press any key to close.");
                //Console.ReadKey();
            }
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
                                                      FileName = "http://www.graphsynth.com/files/install/" + procArch +
                                                                 "/setup.exe",
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
            ArgConfig = InputArgs.Find(s => Path.GetExtension(s).Equals(".gsconfig"));
            if (string.IsNullOrWhiteSpace(ArgConfig) || !File.Exists(ArgConfig))
            {
                ArgContainsConfig = false;
                ArgFiles = InputArgs.FindAll(s => (File.Exists(s) &&
                                               (Path.GetExtension(s).Equals(".gxml") ||
                                                 Path.GetExtension(s).Equals(".grxml") ||
                                                 Path.GetExtension(s).Equals(".rsxml"))));
                for (int i = 0; i < ArgFiles.Count; i++)
                    ArgFiles[i] = Path.GetFullPath(ArgFiles[i]);
                ArgContainsFilesToOpen = (ArgFiles.Count > 0);
            }
            else
            {
                ArgConfig = Path.GetFullPath(ArgConfig);
                ArgContainsConfig = true;
                /* if any files to open are provided, they are ignored in lieu of this new setting. */
            }
            InputArgs.RemoveAll(s => Path.GetExtension(s).Equals(".gsconfig")
                                               || Path.GetExtension(s).Equals(".gxml") ||
                                                 Path.GetExtension(s).Equals(".grxml") ||
                                                 Path.GetExtension(s).Equals(".rsxml"));
            ArgContainsPluginCommands = (InputArgs.Count > 0);
            if (ArgContainsPluginCommands)
            {
                var verbosityOption = InputArgs.FindAll(s => s.StartsWith("-v")).LastOrDefault();
                int verbosity;
                if (verbosityOption != null && int.TryParse(verbosityOption.Substring(2), out verbosity))
                    settings.DefaultVerbosity = SearchIO.defaultVerbosity = verbosity;
            }
        }


        private static Boolean InvokeArgPlugin()
        {
            foreach (var algo in SearchAlgorithms)
            {
                var algoName = algo.text.ToLowerInvariant();
                algoName = algoName.Replace(" ", "");
                algoName = algoName.Replace("_", "");
                algoName = algoName.Replace("-", "");
                algoName = algoName.Replace(".", "");
                algoName = algoName.Replace(",", "");
                algoName = algoName.Replace("\\", "");
                algoName = algoName.Replace("/", "");
                algoName = algoName.Replace("|", "");
                var algoType = algo.GetType();
                if (0 < InputArgs.RemoveAll(str => str.ToLowerInvariant().Equals(algoName))
                    || 0 < InputArgs.RemoveAll(str => str.ToLowerInvariant().Equals(algoType.Name.ToLowerInvariant())))
                {
                    foreach (var inputArg in InputArgs)
                    {
                        if (!inputArg.StartsWith("-"))
                            throw new Exception("Unknown optional argument to GraphSynth: " + inputArg);
                        var eqIndex = inputArg.IndexOf("=");
                        var propertyName = (eqIndex == -1) ? inputArg.Substring(1) : inputArg.Substring(1, eqIndex - 1);
                        string propertyValue = (eqIndex == -1) ? "true" : inputArg.Substring(eqIndex + 1);

                        PropertyInfo propertyInfo = algoType.GetProperty(propertyName);
                        if (propertyInfo == null) { /*ignore it */ }
                        else if (propertyInfo.PropertyType == typeof(int))
                            propertyInfo.SetValue(algo, int.Parse(propertyValue), null);
                        else if (propertyInfo.PropertyType == typeof(double))
                            propertyInfo.SetValue(algo, double.Parse(propertyValue), null);
                        else if (propertyInfo.PropertyType == typeof(string))
                            propertyInfo.SetValue(algo, propertyValue, null);
                        else if (propertyInfo.PropertyType == typeof(bool))
                            propertyInfo.SetValue(algo, bool.Parse(propertyValue), null);
                        else throw new Exception("Unable to set property, " + propertyName + " through input options, " +
                       "because type is not string, boolean, int, or double.");
                    }
                    algo.RunSearchProcess();
                    return true;
                }
            }
            return false;
        }


        public static void OpenFiles()
        {
            settings.filer = new ConsoleFiler(settings.InputDirAbs, settings.OutputDirAbs, settings.RulesDirAbs);
            if (ArgContainsFilesToOpen)
            {
                settings.rulesets = new ruleSet[settings.numOfRuleSets];
            }
            else
            {
                settings.LoadDefaultSeedAndRuleSets();
                for (int i = 0; i < settings.numOfRuleSets; i++)
                    if (settings.rulesets[i] != null)
                        ((ruleSet)settings.rulesets[i]).RuleSetIndex = i;

            }
        }

        private static void ReadInSettings()
        {
            /* loadDefaults can be time consuming if there are many ruleSets/rules to load. */
            if (ArgContainsConfig)
                settings = GlobalSettings.readInSettings(ArgConfig);
            else settings = GlobalSettings.readInSettings();
            SearchIO.defaultVerbosity = settings.DefaultVerbosity;
            SearchIO.output("Default Verbosity set to " + settings.DefaultVerbosity, 3);
        }



        #endregion
    }
}