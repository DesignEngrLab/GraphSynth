/*************************************************************************
 *     This GlobalSettings file & interface is part of the GraphSynth.BaseClasses 
 *     Project which is the foundation of the GraphSynth Application.
 *     GraphSynth.BaseClasses is protected and copyright under the MIT
 *     License.
 *     Copyright (c) 2011 Matthew Ira Campbell, PhD.
 *
 *     Permission is hereby granted, free of charge, to any person obtain-
 *     ing a copy of this software and associated documentation files 
 *     (the "Software"), to deal in the Software without restriction, incl-
 *     uding without limitation the rights to use, copy, modify, merge, 
 *     publish, distribute, sublicense, and/or sell copies of the Software, 
 *     and to permit persons to whom the Software is furnished to do so, 
 *     subject to the following conditions:
 *     
 *     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 *     EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
 *     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGE-
 *     MENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
 *     FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 *     CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
 *     WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *     
 *     Please find further details and contact information on GraphSynth
 *     at http://www.GraphSynth.com.
 *************************************************************************/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using GraphSynth.Representation;


namespace GraphSynth
{
    /// <summary>
    /// The Global Settings class loads the the .gsconfig file into an object which 
    /// is accessed throughout the system.
    /// </summary>
    public class GlobalSettings
    {
        private static readonly char DS = Path.DirectorySeparatorChar;
        private static readonly string DSStr = DS.ToString(CultureInfo.InvariantCulture);

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalSettings"/> class.
        /// </summary>
        public GlobalSettings()
        {
            /* we must have a working directory in place just in case the one loaded from the settings fails. */
            WorkingDirAbsolute = ExecDir;
        }
        #endregion

        #region Properties NOT Stored in Config File
        /// <summary>
        ///   The directory that GraphSynth.exe is located in
        /// </summary>
        [XmlIgnore]
        public static string ExecDir
        {
            get
            {
                return Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location) + DS;
            }
        }
        /// <summary>
        /// Gets or sets the directory where the configuration file was found.
        /// </summary>
        /// <value>The config dir.</value>
        [XmlIgnore]
        private static string ConfigDir { get; set; }

        /// <summary>
        ///   Gets or sets the location of the settings file.
        /// </summary>
        /// <value>The location.</value>
        [XmlIgnore]
        public string FilePath { get; set; }

        /* The class globalSettings is in the IOandXML directory. These values
         * are loaded in from the App.config file. */
        /// <summary>
        /// Gets or sets the filer instance that controls the opening and saving of files.
        /// </summary>
        /// <value>
        /// The filer.
        /// </value>
        [XmlIgnore]
        public BasicFiler filer { get; set; }
        /* The seed graph (the top of the search tree) is stored in seed. */
        /// <summary>
        /// Gets or sets the seed graph - although this dll doesn't know what
        /// a graph is, so you need to "cast up" to really use it.
        /// </summary>
        /// <value>
        /// The seed.
        /// </value>
        [XmlIgnore]
        public designGraph seed { get; set; }
        /* an array of length Program.settings.numOfRuleSets
         * these can be sets in App.config or through the options at the top of the Design
         * pulldown menu. */

        /// <summary>
        /// Gets or sets the rulesets - although this dll doesn't know what
        /// a graph is, so you need to "cast up" to really use it.
        /// </summary>
        /// <value>
        /// The rulesets.
        /// </value>
        [XmlIgnore]
        public ruleSet[] rulesets { get; set; }

        /// <summary>
        /// Gets or sets the number of rule sets.
        /// </summary>
        /// <value>
        /// The number of rule sets.
        /// </value>
        [XmlIgnore]
        public int numOfRuleSets { get; set; }
        #endregion

        #region Properties STORED in Config File
        #region Limits
        private int maxRulesToDisplay;
        /// <summary>
        /// Gets or sets the maximum rules to display in GUI driven User Choose.
        /// </summary>
        /// <value>
        /// The max rules to display.
        /// </value>
        public int MaxRulesToDisplay
        {
            get { return maxRulesToDisplay; }
            set { maxRulesToDisplay = value; }
        }

        int maxRulesToApply;
        /// <summary>
        /// Gets or sets the max rules to apply.
        /// </summary>
        /// <value>
        /// The max rules to apply.
        /// </value>
        public int MaxRulesToApply
        {
            get { return maxRulesToApply; }
            set { maxRulesToApply = value; }
        }
        #endregion
        #region Directories
        private string wDir;
        /// <summary>
        /// Gets or sets the absolute working directory.
        /// </summary>
        /// <value>
        /// The absolute working directory.
        /// </value>
        [XmlAttribute]
        public string WorkingDirAbsolute
        {
            get
            {
                return wDir;
            }
            set
            {
                if (Directory.Exists(value))
                {
                    wDir = Path.GetFullPath(value);
                    if (!wDir.EndsWith(DSStr)) wDir += DS;
                }
            }
        }
        /// <summary>
        /// Gets or sets the relative working directory.
        /// </summary>
        /// <value>
        /// The relative working directory.
        /// </value>
        [XmlAttribute]
        public string WorkingDirRelative
        {
            get
            {
                return MyIOPath.GetRelativePath(WorkingDirAbsolute, ConfigDir);
            }
            set
            {
                if (Path.IsPathRooted(value)) return;
                var temp = ConfigDir + value;
                if (!Directory.Exists(temp)) return;
                WorkingDirAbsolute = temp;
            }
        }

        private string iDir;
        /// <summary>
        /// Gets or sets the input directory.
        /// </summary>
        /// <value>
        /// The input dir.
        /// </value>
        public string InputDir
        {
            get { return getDirectory(iDir); }
            set { iDir = setDirectory(value); }
        }

        /// <summary>
        /// Gets the absolute (rooted) input directory.
        /// </summary>
        public string InputDirAbs { get { return Path.GetFullPath(WorkingDirAbsolute + InputDir); } }

        private string oDir;
        /// <summary>
        /// Gets or sets the output directory.
        /// </summary>
        /// <value>
        /// The output dir.
        /// </value>
        public string OutputDir
        {
            get { return getDirectory(oDir); }
            set { oDir = setDirectory(value); }
        }
        /// <summary>
        /// Gets the absolute (rooted) output directory.
        /// </summary>
        public string OutputDirAbs { get { return Path.GetFullPath(WorkingDirAbsolute + OutputDir); } }

        private string rDir;
        /// <summary>
        /// Gets or sets the rules directory.
        /// </summary>
        /// <value>
        /// The rules directory.
        /// </value>
        public string RulesDir
        {
            get { return getDirectory(rDir); }
            set { rDir = setDirectory(value); }
        }
        /// <summary>
        /// Gets the absolute (rooted) rules directory.
        /// </summary>
        public string RulesDirAbs { get { return Path.GetFullPath(WorkingDirAbsolute + RulesDir); } }

        private string hDir;
        /// <summary>
        /// Gets or sets the local help dir.
        /// </summary>
        /// <value>
        /// The local help dir.
        /// </value>
        public string LocalHelpDir
        {
            get
            {
                if (string.IsNullOrWhiteSpace(hDir)) hDir = WorkingDirAbsolute;
                return getDirectory(hDir);
            }
            set { hDir = setDirectory(value); }
        }

        /// <summary>
        /// Gets or sets the online help URL.
        /// </summary>
        /// <value>
        /// The online help URL.
        /// </value>
        public string OnlineHelpURL { get; set; }

        private string glDir;
        /// <summary>
        /// Gets or sets the graph layout directory.
        /// </summary>
        /// <value>
        /// The graph layout directory.
        /// </value>
        public string GraphLayoutDir
        {
            get
            {
                if (string.IsNullOrWhiteSpace(glDir)) glDir = WorkingDirAbsolute;
                return getDirectory(glDir);
            }
            set { glDir = setDirectory(value); }
        }
        /// <summary>
        /// Gets the absolute (rooted) graph layout directory.
        /// </summary>
        public string GraphLayoutDirAbs { get { return Path.GetFullPath(WorkingDirAbsolute + GraphLayoutDir); } }

        private string sDir;
        /// <summary>
        /// Gets or sets the search directory.
        /// </summary>
        /// <value>
        /// The search directory.
        /// </value>
        public string SearchDir
        {
            get
            {
                if (string.IsNullOrWhiteSpace(sDir)) sDir = WorkingDirAbsolute;
                return getDirectory(sDir);
            }
            set
            {
                sDir = setDirectory(value);
            }
        }
        /// <summary>
        /// Gets the absolute (rooted) search directory.
        /// </summary>
        public string SearchDirAbs { get { return Path.GetFullPath(WorkingDirAbsolute + SearchDir); } }

        string getDirectory(string value)
        {
            return Path.IsPathRooted(value) ? MyIOPath.GetRelativePath(value, WorkingDirAbsolute) : value;
        }
        string setDirectory(string value)
        {
            var dir = Path.IsPathRooted(value) ? Path.GetFullPath(value) : Path.GetFullPath(WorkingDirAbsolute + value);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            if (!dir.EndsWith(DSStr)) dir += DS;
            return dir;
        }
        #endregion

        #region Files

        string erFile;
        /// <summary>
        /// Gets or sets the error log file.
        /// </summary>
        /// <value>
        /// The error log file.
        /// </value>
        public string ErrorLogFile
        {
            get { return MyIOPath.GetRelativePath(erFile, ConfigDir); }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                if (Path.IsPathRooted(value)) erFile = value;
                /* otherwise, put it in the executable directory */
                else erFile = ConfigDir + value;
                ErrorLogger.ErrorLogFile = erFile;
            }
        }

        string defaultSeedFileName;
        /// <summary>
        /// Gets or sets the default name of the seed file.
        /// </summary>
        /// <value>
        /// The default name of the seed file.
        /// </value>
        public string DefaultSeedFileName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(defaultSeedFileName)) return "";
                return MyIOPath.GetRelativePath(defaultSeedFileName, InputDirAbs);
            }
            set
            {
                defaultSeedFileName =
                Path.IsPathRooted(value) ? Path.GetFullPath(value) : Path.GetFullPath(InputDirAbs + value);
            }
        }

        /// <summary>
        /// the default ruleset filenames relative to the rules directory
        /// </summary>
        [XmlIgnore]
        public List<string> defaultRSFileNames = new List<string>();

        /// <summary>
        /// Gets or sets the default rule sets filename as a single string.
        /// </summary>
        /// <value>
        /// The default rule sets.
        /// </value>
        public string DefaultRuleSets
        {
            get
            {
                var relNames = defaultRSFileNames.Select(s => MyIOPath.GetRelativePath(s, RulesDirAbs)).ToList();
                return StringCollectionConverter.Convert(relNames);
            }
            set
            {
                var nameArray = value.Split(',');
                defaultRSFileNames.Clear();
                foreach (var s in nameArray.Where(s => !string.IsNullOrWhiteSpace(s)))
                {
                    defaultRSFileNames.Add(Path.IsPathRooted(s.Trim())
                                               ? Path.GetFullPath(s.Trim())
                                               : Path.GetFullPath(RulesDirAbs + s.Trim()));
                }
                numOfRuleSets = defaultRSFileNames.Count;
            }
        }

        string customShapesFile;
        /// <summary>
        /// Gets or sets the custom shapes file.
        /// </summary>
        /// <value>
        /// The custom shapes file.
        /// </value>
        public string CustomShapesFile
        {
            get
            {
                return MyIOPath.GetRelativePath(customShapesFile, WorkingDirAbsolute);
            }
            set { customShapesFile = Path.IsPathRooted(value) ? Path.GetFullPath(value) : Path.GetFullPath(WorkingDirAbsolute + value); }
        }

        private string compiledRuleFunctions;
        /// <summary>
        /// Gets or sets the compiled param rules.
        /// </summary>
        /// <value>
        /// The compiledparam rules.
        /// </value>
        public string CompiledRuleFunctions
        {
            get
            {
                return MyIOPath.GetRelativePath(compiledRuleFunctions, RulesDirAbs);
            }
            set
            {
                compiledRuleFunctions =
                    Path.IsPathRooted(value) ? Path.GetFullPath(value) : Path.GetFullPath(RulesDirAbs + value);
            }
        }


        #endregion

        #region Misc. Defaults

        /// <summary>
        /// Gets or sets a value indicating whether search controllers should automatically start playing
        /// when they are initiated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [search controller is set to auto-play]; otherwise, <c>false</c>.
        /// </value>
        public Boolean SearchControllerPlayOnStart { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [get help from online].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [get help from online]; otherwise, <c>false</c>.
        /// </value>
        public Boolean GetHelpFromOnline { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [recompile rule conditions].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [recompile rule conditions]; otherwise, <c>false</c>.
        /// </value>
        public Boolean RecompileRuleConditions { get; set; }

        /// <summary>
        /// Gets or sets the default verbosity.
        /// </summary>
        /// <value>
        /// The default verbosity.
        /// </value>
        public int DefaultVerbosity { get; set; }

        #endregion

        #endregion

        #region Read & Write Methods
        /// <summary>
        /// Reads in the settings.
        /// </summary>
        /// <param name="configPath">The file path.</param>
        /// <returns></returns>
        public static GlobalSettings readInSettings(string configPath = "")
        {
            GlobalSettings settings = null;
            var filePaths = GetPotentialFiles(configPath.TrimEnd(DS));
            var i = 0;
            while ((i < filePaths.Count) && !readInSettings(filePaths[i], out settings))
            {
                i++;
            }
            if (i == filePaths.Count)
            {
                SearchIO.MessageBoxShow(
                    "Welcome to GraphSynth. This very well might be your first time with this program, " +
                    "since no valid configuration file was found.\n\nPlease review and edit the default "
                    + "Settings in the Edit Drop-Down Menu.",
                    "Welcome to GraphSynth");
                if (Environment.OSVersion.Platform.ToString().StartsWith("Win"))
                    ConfigDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + DS +
                           "GraphSynth" + DS;
                else ConfigDir = ExecDir;
                settings = new GlobalSettings { FilePath = configPath };
                settings.LoadDefaults();
                settings.saveNewSettings();
            }
            else
            {
                SearchIO.output("Loaded settings at" + filePaths[i] + ".");
                if (i > 0)
                {
                    SearchIO.output("Found after trying:");
                    for (var j = 0; j < i; j++)
                        SearchIO.output(j + ". " + filePaths[j]);
                }
                settings.FilePath = filePaths[i];
            }
            return settings;
        }

        private static List<string> GetPotentialFiles(string configPath)
        {
            var filePaths = new List<string>();
            if (!string.IsNullOrWhiteSpace(configPath))
                if (configPath.Equals(Path.GetDirectoryName(configPath)))
                    filePaths.AddRange(Directory.GetFiles(configPath, "*.gsconfig", SearchOption.TopDirectoryOnly));
                else filePaths.Add(configPath);
            var tempExeDir = ExecDir.TrimEnd(DS);
            filePaths.AddRange(Directory.GetFiles(tempExeDir, "*.gsconfig", SearchOption.TopDirectoryOnly));
            /* if windows machine, add this folder */
            if (Environment.OSVersion.Platform.ToString().StartsWith("Win"))
            {
                if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + DS + "GraphSynth"))
                    filePaths.AddRange(Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                        + DS + "GraphSynth", "*.gsconfig", SearchOption.TopDirectoryOnly));
            }
            do
            {
                tempExeDir = Directory.GetParent(tempExeDir).FullName;
                filePaths.AddRange(Directory.GetFiles(tempExeDir, "*.gsconfig", SearchOption.TopDirectoryOnly));
            } while (tempExeDir.Length > 3);
            return filePaths;
        }

        private static Boolean readInSettings(string filePath, out GlobalSettings settings)
        {
            StreamReader streamReader = null;
            try
            {
                streamReader = new StreamReader(filePath);
                var SettingsDeSerializer = new XmlSerializer(typeof(GlobalSettings));
                ConfigDir = Path.GetDirectoryName(filePath) + DS;
                settings = (GlobalSettings)SettingsDeSerializer.Deserialize(streamReader);

                if (settings == null) throw new Exception("Was not able to find GraphSynthSettings section.");
                SearchIO.output("GraphSynthSettings.gsconfig loaded successfully.");
                return true;
            }
            catch (Exception e)
            {
                SearchIO.output("The configuration file produced an error:\n" + e);
                settings = null;
                return false;
            }
            finally { if (streamReader != null) streamReader.Close(); }
        }

        /// <summary>
        /// Saves the new settings.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void saveNewSettings(string filename = "")
        {
            if (string.IsNullOrWhiteSpace(filename)) filename = FilePath;
            if (string.IsNullOrWhiteSpace(filename)) filename = "GraphSynthSettings.gsconfig";
            if (!Path.IsPathRooted(filename))
            {
                /* if windows machine,the default errorlog location is in the User\AppData\Roaming\GraphSynth */
                if (Environment.OSVersion.Platform.ToString().StartsWith("Win"))
                {
                    var appDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + DS + "GraphSynth";
                    if (!Directory.Exists(appDir)) Directory.CreateDirectory(appDir);
                    filename = appDir + DS + filename;
                }
                /* otherwise, put it in the executable directory */
                else filename = ExecDir + filename;
            }
            FilePath = filename;
            var tempConfigDir = ConfigDir;
            ConfigDir = Path.GetDirectoryName(filename);
            StreamWriter streamWriter = null;
            try
            {
                streamWriter = new StreamWriter(filename);
                var SettingsSerializer = new XmlSerializer(typeof(GlobalSettings));
                SettingsSerializer.Serialize(streamWriter, this);
                SearchIO.MessageBoxShow("Saved at " + filename, "Settings file saved successfuly.");
            }
            catch (Exception e)
            {
                SearchIO.MessageBoxShow("The configuration file did not save because of error: "
                                        + e,
                                        "Error Saving config file.", "Error");
            }
            finally
            {
                ConfigDir = tempConfigDir;
                if (streamWriter != null) streamWriter.Close();
            }
        }
        /// <summary>
        /// Loads the default seed and rule sets.
        /// </summary>
        public void LoadDefaultSeedAndRuleSets()
        {
            try
            {
                rulesets = new ruleSet[numOfRuleSets];
                SearchIO.output("There are " + numOfRuleSets + " rulesets.");
                var filename = getOpenFilename(InputDirAbs, DefaultSeedFileName);
                if (filename != "")
                {
                    var graphAndCanvas = filer.Open(filename, true);
                    if (graphAndCanvas == null)
                    {
                        seed = null;
                        DefaultSeedFileName = "";
                        SearchIO.MessageBoxShow("The seed graph was not found. Please change to valid file"
                                         + " in settings.", "Seed Graph Not Found.", "Error");
                    }
                    else
                    {
                        seed = (designGraph)graphAndCanvas[0];
                        DefaultSeedFileName = filename;
                        SearchIO.addAndShowGraphWindow(graphAndCanvas,
                            "SEED: " + Path.GetFileNameWithoutExtension(filename));
                        SearchIO.output("Seed graph, " + Path.GetFileNameWithoutExtension(filename) + " ,successfully opened.");
                    }
                }
                for (var i = 0; i < numOfRuleSets; i++)
                {
                    var RSFileName = "";
                    if (i < defaultRSFileNames.Count)
                        RSFileName = defaultRSFileNames[i];
                    filename = getOpenFilename(RulesDirAbs, RSFileName);
                    if (filename == "") continue;
                    object objRS = filer.Open(filename, true);
                    if (objRS == null)
                    {
                        rulesets[i] = null;
                        defaultRSFileNames[i] = "";
                        SearchIO.output("No Rule Set found at #" + i + ".");
                    }
                    else
                    {
                        rulesets[i] = (ruleSet)((object[])objRS)[0];
                        SearchIO.addAndShowRuleSetWindow(new object[] { rulesets[i], filename },
                                                         "RS" + i + ": " + Path.GetFileNameWithoutExtension(filename));
                        // SearchIO.output("Rule Set #" + i + " sucessfully opened.");
                    }
                }
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private static string getOpenFilename(string dir, string filename)
        {
            if (string.IsNullOrWhiteSpace(filename)) return "";
            //if (!filename.EndsWith(".xml")) filename += ".xml";
            if ((Path.IsPathRooted(filename)) && File.Exists(filename)) return filename;
            if (File.Exists(dir + filename)) return dir + filename;
            if (filename.Length > 0)
                SearchIO.MessageBoxShow("File named " + filename + " not found.",
                                        " File not found.", "Error");
            return "";
        }

        /// <summary>
        /// Copies the exisiting and returns it.
        /// </summary>
        /// <returns></returns>
        public GlobalSettings Duplicate()
        {

            var copyOfSettings = new GlobalSettings
                                     {
                                         WorkingDirAbsolute = WorkingDirAbsolute,
                                         FilePath = FilePath,
                                         InputDir = InputDir,
                                         OutputDir = OutputDir,
                                         RulesDir = RulesDir,
                                         SearchDir = SearchDir,
                                         GraphLayoutDir = GraphLayoutDir,
                                         LocalHelpDir = LocalHelpDir,
                                         CompiledRuleFunctions = CompiledRuleFunctions,
                                         CustomShapesFile = CustomShapesFile,
                                         DefaultRuleSets = DefaultRuleSets,
                                         DefaultSeedFileName = DefaultSeedFileName,
                                         DefaultVerbosity = DefaultVerbosity,
                                         ErrorLogFile = ErrorLogFile,
                                         GetHelpFromOnline = GetHelpFromOnline,
                                         MaxRulesToApply = MaxRulesToApply,
                                         MaxRulesToDisplay = MaxRulesToDisplay,
                                         OnlineHelpURL = OnlineHelpURL,
                                         RecompileRuleConditions = RecompileRuleConditions,
                                         SearchControllerPlayOnStart = SearchControllerPlayOnStart
                                     };

            return copyOfSettings;
        }

        /// <summary>
        /// Loads the defaults.
        /// </summary>
        public void LoadDefaults()
        {
            try
            {
                var wd = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + DS + "GraphSynth";
                if (!Directory.Exists(wd)) Directory.CreateDirectory(wd);
                WorkingDirAbsolute = wd;
                InputDir = "input/";
                OutputDir = "output/";
                RulesDir = "rules/";
                SearchDir = "plugins/";
                OnlineHelpURL = "http://graphsynth.com/help/";
                ErrorLogFile = "ErrorLog.txt";
                CustomShapesFile = "plugins/customShapeDescriptors.xaml";
                CompiledRuleFunctions = "CompiledRuleFunctions.dll";
                DefaultVerbosity = 3;
                maxRulesToDisplay = 1000;
                maxRulesToApply = 500;
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        /// <summary>
        /// Attempts to migrate all sub-directories when the working directory is changed.
        /// Currently this only occurs in the GUI when a working directory has been changed.
        /// </summary>
        public void AttemptSubDirMigration(string newWDirAbs)
        {
            try
            {
                if (!newWDirAbs.EndsWith(DSStr)) newWDirAbs += DS;
                string tempPath;
                if (!InputDir.StartsWith(".." + DS))
                {
                    tempPath = newWDirAbs + InputDir;
                    if (Directory.Exists(tempPath))
                    {
                        iDir = tempPath;
                        if (!DefaultSeedFileName.StartsWith(".." + DS))
                        {
                            tempPath = iDir + DefaultSeedFileName;
                            if (File.Exists(tempPath)) defaultSeedFileName = tempPath;
                        }
                    }
                }
                if (!OutputDir.StartsWith(".." + DS))
                {
                    tempPath = newWDirAbs + OutputDir;
                    if (Directory.Exists(tempPath)) oDir = tempPath;
                }
                if (!RulesDir.StartsWith(".." + DS))
                {
                    tempPath = newWDirAbs + RulesDir;
                    if (Directory.Exists(tempPath))
                    {
                        rDir = tempPath;
                        if (!CompiledRuleFunctions.StartsWith(".." + DS))
                        {
                            tempPath = rDir + CustomShapesFile;
                            if (File.Exists(tempPath)) compiledRuleFunctions = tempPath;
                        }
                        var relRuleSets = DefaultRuleSets.Split(',');
                        for (var i = 0; i < relRuleSets.GetLength(0); i++)
                            if (!string.IsNullOrWhiteSpace(relRuleSets[i]) && !relRuleSets[i].StartsWith(".." + DS))
                            {
                                tempPath = rDir + relRuleSets[i];
                                if (File.Exists(tempPath)) defaultRSFileNames[i] = tempPath;
                            }
                    }
                }
                if (!SearchDir.StartsWith(".." + DS))
                {
                    tempPath = newWDirAbs + SearchDir;
                    if (Directory.Exists(tempPath)) sDir = tempPath;
                }
                if (!GraphLayoutDir.StartsWith(".." + DS))
                {
                    tempPath = newWDirAbs + GraphLayoutDir;
                    if (Directory.Exists(tempPath)) glDir = tempPath;
                }
                if (!CustomShapesFile.StartsWith(".." + DS))
                {
                    tempPath = newWDirAbs + CustomShapesFile;
                    if (File.Exists(tempPath)) customShapesFile = tempPath;
                }
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        #endregion
    }
}