using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GraphSynth;
using GraphSynth.Search;

namespace GraphSynth
{
    public static partial class GSApp
    {
        private static readonly List<SearchProcess> SearchAlgorithms = new List<SearchProcess>();


        public static void LoadPlugins()
        {
            Assembly searchAssembly = null;
            Type[] searchprocesses = null;
            var potentialAssemblies = getPotentialAssemblies(settings.SearchDirAbs);
            if (potentialAssemblies.Count == 0) return;

            foreach (string filepath in potentialAssemblies)
            {
                try
                {
                    searchAssembly = Assembly.LoadFrom(filepath);
                    searchprocesses = searchAssembly.GetExportedTypes();
                    foreach (Type spt in searchprocesses)
                        if (!spt.IsAbstract && (spt.BaseType == typeof(SearchProcess))
                            && !SearchAlgorithms.Any(w => w.GetType().FullName.Equals(spt.FullName)))
                            LoadSearchAlgo(spt);
                }
                catch (Exception exc)
                {
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
                if (!spt.IsAbstract && (spt.BaseType == typeof(SearchProcess))
                    && !SearchAlgorithms.Any(w => w.GetType().FullName.Equals(spt.FullName)))
                    LoadSearchAlgo(spt);
        }


        private static void LoadSearchAlgo(Type spt)
        {
            try
            {
                var constructor = spt.GetConstructor(new Type[0]);
                SearchProcess searchAlgo = (SearchProcess)constructor.Invoke(null);
                searchAlgo.settings = GSApp.settings;
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
            potentialAssemblies.RemoveAll(fs => fs.Contains("GraphSynth.FundamentalClassesAndInterfaces.dll"));
            potentialAssemblies.RemoveAll(fs => fs.Contains("GraphSynth.Representation.dll"));
            potentialAssemblies.RemoveAll(fs => fs.Contains("StarMath.dll"));
            potentialAssemblies.RemoveAll(fs => fs.Contains("OptimizationToolbox.dll"));
            return potentialAssemblies;
        }

        private static void PluginDialog()
        {
            List<int> inactiveIndices = new List<int>();
            int numActive = 0;
            int activeIndex = 0;
            int choice = -1;
            //if (SearchAlgorithms.Count == 0) SearchIO.output("=== no plugins loaded. ===");
            SearchIO.output("\n========= Plugins ==========");
            for (int i = 0; i < SearchAlgorithms.Count; i++)
            {
                var inactive = false;
                var s = SearchAlgorithms[i];
                var numString = (i < 10) ? ((Key)i).ToString().Remove(0, 1) : ((Key)i).ToString();
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
                    SearchIO.output("Push any key to start the single active plugin (F1 for other commands).");
                    Console.Write(" >");
                    break;
                default:
                    SearchIO.output("Push the key of the plugin you would like to run (F1 for other commands).");
                    Console.Write(" >");
                    break;
            }
            Key response;
            if (Enum.TryParse(Console.ReadKey().Key.ToString(), out response))
                choice = (int)response;
            else choice = -1;
            Console.Write("\n");
            if (choice == 30) HelpDialog();
            else if (choice == 31) VerbosityDialog();
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
            Key response;
            int choice = -1;
            if (Enum.TryParse(Console.ReadKey().Key.ToString(), out response))
                choice = (int)response + 1;
            if (choice != settings.DefaultVerbosity && choice >= 0 && choice <= 4)
            {
                settings.DefaultVerbosity = SearchIO.defaultVerbosity = choice;
                Console.WriteLine("\n---> verbosity changed to {0}", settings.DefaultVerbosity);
            }
        }
    }
    public enum Key
    {
        D1, D2, D3, D4, D5, D6, D7, D8, D9, D0,
        A, B, C, D, E, F, G, H, I, J,
        K, L, M, N, O, P, Q, R, S, T,
        F1, V, X
    }
}
