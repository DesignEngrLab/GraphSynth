using GraphSynth.Search;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GraphSynth
{
    public static partial class GSApp
    {
        private static readonly List<SearchProcess> SearchAlgorithms = new List<SearchProcess>();


        public static void LoadPlugins()
        {
            Assembly searchAssembly = null;
            Type[] searchprocesses;
            var potentialAssemblies = getPotentialAssemblies(settings.SearchDirAbs);
            potentialAssemblies.Add("thisEXE");
            if (potentialAssemblies.Count == 0) return;

            foreach (string filepath in potentialAssemblies)
            {
                try
                {
                    if (filepath == "thisEXE") searchAssembly = Assembly.GetExecutingAssembly();
                    else searchAssembly = Assembly.LoadFrom(filepath);
                    searchprocesses = searchAssembly.GetExportedTypes();
                    foreach (Type spt in searchprocesses)
                        if (!spt.IsAbstract && SearchProcess.IsInheritedType(spt)
                            && !SearchAlgorithms.Any(w => w.GetType().FullName.Equals(spt.FullName)))
                            LoadSearchAlgo(spt);
                }
                catch (Exception exc)
                {
                    if (exc.Message.Equals("Assembly with same name is already loaded")) continue;
                    if (searchAssembly == null)
                        SearchIO.output("Unable to open " + filepath + ": " + exc.Message);
                    else
                        SearchIO.output("Unable to open " + searchAssembly.FullName + "(" + filepath + "): " +
                                        exc.Message);
                }
            }
            //load from this exe
            searchAssembly = Assembly.GetExecutingAssembly();
            searchprocesses = searchAssembly.GetExportedTypes();
            foreach (Type spt in searchprocesses)
                if (!spt.IsAbstract && SearchProcess.IsInheritedType(spt)
                    && !SearchAlgorithms.Any(w => w.GetType().FullName.Equals(spt.FullName)))
                    LoadSearchAlgo(spt);
        }


        private static void LoadSearchAlgo(Type spt)
        {
            try
            {
                var constructor = spt.GetConstructor(new[] { typeof(GlobalSettings) });
                var searchAlgo = (SearchProcess)constructor.Invoke(new object[] { GSApp.settings });
                SearchAlgorithms.Add(searchAlgo);
                SearchIO.output("\t" + spt.Name + " loaded successfully.", 3);
            }
            catch (Exception exc)
            {
                SearchIO.output("Unable to load " + spt.Name + ": " + exc.Message, 1);
            }
        }

        private const string defaultPluginDir = "defaultPlugins/";

        private static List<string> getPotentialAssemblies(string directory)
        {
            var potentialAssemblies = Directory.GetFiles(GlobalSettings.ExecDir, "*.dll", SearchOption.TopDirectoryOnly).ToList();
            if (Directory.Exists(GlobalSettings.ExecDir + defaultPluginDir))
                potentialAssemblies.AddRange(Directory.GetFiles(GlobalSettings.ExecDir + defaultPluginDir,
                                                                "*.dll", SearchOption.AllDirectories));
            if (Directory.Exists(directory))
                potentialAssemblies.AddRange(Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories));
            else SearchIO.output("Plugin directory" + directory + " not found.");

            potentialAssemblies.RemoveAll(fs => fs.Contains("/obj/"));
            potentialAssemblies.RemoveAll(fs => fs.Contains("GraphSynth.CustomControls.dll"));
            potentialAssemblies.RemoveAll(fs => fs.Contains("GraphSynth.BaseClasses.dll"));
            potentialAssemblies.RemoveAll(fs => fs.Contains("StarMath.dll"));
            potentialAssemblies.RemoveAll(fs => fs.Contains("OptimizationToolbox.dll"));
            return potentialAssemblies;
        }

        private static void PluginDialog()
        {
            var inactiveIndices = new List<int>();
            var numActive = 0;
            var activeIndex = 0;
            //if (SearchAlgorithms.Count == 0) SearchIO.output("=== no plugins loaded. ===");
            SearchIO.output("\n========= Plugins ==========");
            for (int i = 0; i < SearchAlgorithms.Count; i++)
            {
                var inactive = false;
                var s = SearchAlgorithms[i];
                var numString = (i < 10) ? Keys[i].ToString().Remove(0, 1) : Keys[i].ToString();
                Console.Write("{0}. {1}", numString, s.text);
                if (s.RequireSeed && settings.seed == null)
                {
                    inactive = true;
                    Console.Write(" (requires seed)");
                }
                if (s.RequiredNumRuleSets > settings.numOfRuleSets)
                {
                    inactive = true;
                    Console.Write(" (requires {0} rulesets)", s.RequiredNumRuleSets);
                }
                Console.WriteLine();
                if (inactive) inactiveIndices.Add(i);
                else
                {
                    numActive++;
                    activeIndex = i;
                }
            }
            switch (numActive)
            {
                case 0:
                    Console.WriteLine("There are no active plugins. Please fix configuration file before rerunning.");
                    break;
                case 1:
                    SearchIO.output("Press any key to start the single active plugin (F1 for other commands).");
                    Console.Write(" >");
                    break;
                default:
                    SearchIO.output("Press the key of the plugin you would like to run (F1 for other commands).");
                    Console.Write(" >");
                    break;
            }
            var readKey = Console.ReadKey().Key;
            int choice = ConvertConsoleKeyToInt(readKey);
            if (choice == 30) HelpDialog();
            else if (choice == 31) VerbosityDialog();
            else if (choice == 32) return;
            else
            {
                if (numActive == 1) choice = activeIndex;
                else if (inactiveIndices.Contains(choice)) choice = -1;

                if (choice >= 0 && choice < SearchAlgorithms.Count)
                    SearchAlgorithms[choice].RunSearchProcess();
            }
            if (choice <= 31) PluginDialog();
        }

        private static void HelpDialog()
        {
            Console.WriteLine("\n---> help for commands");
            Console.WriteLine("     V: brings up verbosity dialog");
            Console.WriteLine("     X: to exit the program\n");
        }

        private static void VerbosityDialog()
        {
            Console.WriteLine("\n---> change verbosity");
            Console.WriteLine("     (0 [most verbose] to 4 [least verbose])");
            Console.WriteLine("     currently set to: {0}", settings.DefaultVerbosity);
            Console.WriteLine("     enter new value (or any other key to keep current).");
            Console.Write("----> ");
            ConsoleKey response;
            int choice = -1;
            if (Enum.TryParse(Console.ReadKey().Key.ToString(), out response))
                choice = (int)response + 1;
            if (choice != settings.DefaultVerbosity && choice >= 0 && choice <= 4)
            {
                settings.DefaultVerbosity = SearchIO.defaultVerbosity = choice;
                Console.WriteLine("\n---> verbosity changed to {0}", settings.DefaultVerbosity);
            }
        }
        private static int ConvertConsoleKeyToInt(ConsoleKey value)
        {

            var result = Keys.Select((k, i) => new { Pos = i, ConsoleKey = k }).Where(o => o.ConsoleKey.Equals(value)).FirstOrDefault();
            if (result != null)
            {
                return result.Pos;
            }
            return -1;
        }

        private static ConsoleKey[] Keys = new[] {
            ConsoleKey.D1, ConsoleKey.D2, ConsoleKey.D3, ConsoleKey.D4, ConsoleKey.D5,
            ConsoleKey.D6, ConsoleKey.D7, ConsoleKey.D8, ConsoleKey.D9, ConsoleKey.D0,
            ConsoleKey.A, ConsoleKey.B, ConsoleKey.C, ConsoleKey.D, ConsoleKey.E,
            ConsoleKey.F, ConsoleKey.G, ConsoleKey.H, ConsoleKey.I, ConsoleKey.J,
            ConsoleKey.K, ConsoleKey.L, ConsoleKey.M, ConsoleKey.N, ConsoleKey.O,
            ConsoleKey.P, ConsoleKey.Q, ConsoleKey.R, ConsoleKey.S, ConsoleKey.T,
            ConsoleKey.F1, // 30 help
            ConsoleKey.V, // 31 verbosity
             ConsoleKey.X // exit
        };

    }
}

