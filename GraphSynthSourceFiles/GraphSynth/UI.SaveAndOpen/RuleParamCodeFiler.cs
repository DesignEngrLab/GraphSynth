using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    public static class RuleParamCodeFiler
    {
        #region Creating Source Files

        public static void checkForRuleFile(ruleSet rs, List<string> ruleFiles, string str)
        {
            if (!str.EndsWith(".cs")) str += ".cs";
            if (ruleFiles.Contains(str))
                SearchIO.output(rs.name + " already contains a reference to code file, " + str + ".");

            else if (!File.Exists(rs.rulesDir + str))
            {
                var result =
                    MessageBox.Show("Source File " + rs.rulesDir + str
                                    + " not found. Would you like to create it?", "File not found.",
                                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    createRulesSourceFile(rs.rulesDir + str);
                    ruleFiles.Add(str);
                }
            }
            else
            {
                SearchIO.output("Code file, " + str + " found: Adding file reference to ruleset.");
                ruleFiles.Add(str);
            }
        }

        private static void createRulesSourceFile(string path)
        {
            var fs = new FileStream(path, FileMode.Create);
            var w = new StreamWriter(fs, Encoding.UTF8);

            w.WriteLine("using System;\nusing System.Collections.Generic;");
            w.WriteLine("using GraphSynth;\nusing GraphSynth.Representation;");
            w.WriteLine("\nnamespace GraphSynth.ParamRules\n{");
            w.WriteLine("public partial class ParamRules\n{");
            w.WriteLine("/* here are parametric rules written as part of the ruleSet.");
            w.WriteLine("* these are compiled at runtime into a .dll indicated in the");
            w.WriteLine("* App.gsconfig file. */");
            w.WriteLine("#region Parametric Recognition Rules");
            w.WriteLine("/* Parametric recognition rules receive as input:");
            w.WriteLine("* 1. the left hand side of the rule (L)");
            w.WriteLine("* 2. the entire host graph (host)");
            w.WriteLine("* 3. the location of the nodes in the host that L matches to (locatedNodes).");
            w.WriteLine("* 4. the location of the arcs in the host that L matches to (locatedArcs). */");
            w.WriteLine("#endregion\n\n");
            w.WriteLine("#region Parametric Application Rules");
            w.WriteLine("/* Parametric application rules receive as input:");
            w.WriteLine("* 1. the location designGraph indicating the nodes&arcs of host that match with L (Lmapping)");
            w.WriteLine("* 2. the entire host graph (host)");
            w.WriteLine("* 3. the location of the nodes in the host that R matches to (Rmapping).");
            w.WriteLine("* 4. the parameters chosen for instantiating elements of Rmapping (parameters).");
            w.WriteLine("* 5. the entire rule that is being invoked (rule). */");
            w.WriteLine("#endregion\n\n");
            w.WriteLine("}\n}");
            w.Flush();
            w.Close();
            fs.Close();
        }

        #endregion

        #region Creating Functions within files

        public static void checkForFunctions(Boolean isThisRecognize, grammarRule SelectedRule,
                                             List<string> newFunctions)
        {
            var found = false;
            var filesWithFunc = new List<string>();
            var functionNames = new List<string>();
            var i = 0;

            var sourceFiles = Directory.GetFiles(GSApp.settings.RulesDirAbs, "*.cs");
            foreach (string funcName in newFunctions)
            {
                found = false;
                foreach (string file in sourceFiles)
                {
                    var r = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read), Encoding.UTF8);
                    var funcString = "public ";
                    if (isThisRecognize) funcString += "double ";
                    else funcString += "designGraph ";
                    if (r.ReadToEnd().Contains(funcString + funcName + "("))
                    {
                        found = true;
                        filesWithFunc.Add(file);
                    }
                    r.Close();
                }
                if (found)
                {
                    var message = "Function, " + funcName + ", found in ";
                    foreach (string a in filesWithFunc)
                    {
                        message += Path.GetFileName(a);
                        message += ", ";
                    }
                    MessageBox.Show(message, "Function Found", MessageBoxButton.OK, MessageBoxImage.Information);
                    functionNames.Add(funcName);
                }
                else
                {
                    var result = MessageBoxResult.Cancel;
                    if (sourceFiles.GetLength(0) == 1)
                    {
                        result =
                            MessageBox.Show("Function, " + funcName + " not found. Would you like to add it to "
                                            + Path.GetFileName(sourceFiles[0]) + "?", "Function Not Found",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Information);
                    }
                    else if (sourceFiles.GetLength(0) > 1)
                    {
                        for (i = 0; i != sourceFiles.GetLength(0); i++)
                        {
                            result =
                                MessageBox.Show("Function, " + funcName + " not found. Would you like to add it to "
                                                + Path.GetFileName(sourceFiles[i]) +
                                                "? (You can add to any source file - I will ask you about more after this...)",
                                                "Function Not Found", MessageBoxButton.YesNo,
                                                MessageBoxImage.Information);
                            if (result == MessageBoxResult.Yes) break;
                        }
                    }
                    if (result != MessageBoxResult.Yes)
                    {
                        MessageBox.Show("You must first create a C# source file to add the function, "
                                        + funcName +
                                        " to. This can be done by adding a name of a file to the ruleSet display.",
                                        "No Parameter Rule File Exists.", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else if (isThisRecognize)
                    {
                        createRecognizeFunctionTemplate(sourceFiles[i], SelectedRule.name, funcName);
                        functionNames.Add(funcName);
                    }
                    else
                    {
                        createApplyFunctionTemplate(sourceFiles[i], SelectedRule.name, funcName);
                        functionNames.Add(funcName);
                    }
                }
            }
        }

        private static void createRecognizeFunctionTemplate(string path, string ruleName, string funcName)
        {
            var r = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read), Encoding.UTF8);
            var fileString = r.ReadToEnd();
            var position = fileString.IndexOf("#endregion", 0);
            var sb = new StringBuilder("");
            r.Close();

            sb.Append("\n/* This is RECOGNIZE for the rule entitled: ");
            sb.Append(ruleName);
            sb.Append(" */");
            sb.Append("\npublic double ");
            sb.Append(funcName);
            sb.Append(
                "(grammarRule rule, designGraph host, designGraph location, double[,] T)\n");
            sb.Append("{\n/* here is where the code for the RECOGNIZE function is to be located.\n");
            sb.Append("* please remember that returning a positive real (double) is equivalent to\n");
            sb.Append("* a constraint violation. Zero and negative numbers are feasible. */\n");
            sb.Append("return 0.0;\n}\n");
            fileString = fileString.Insert(position, sb.ToString());

            var w = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write), Encoding.UTF8);
            w.Write(fileString);
            w.Flush();
            w.Close();
        }

        private static void createApplyFunctionTemplate(string path, string ruleName, string funcName)
        {
            var r = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read), Encoding.UTF8);
            var fileString = r.ReadToEnd();
            var position = fileString.IndexOf("#endregion", 0);
            position = fileString.IndexOf("#endregion", position + 1);
            var sb = new StringBuilder("");
            r.Close();

            sb.Append("\n/* This is APPLY for the rule entitled: ");
            sb.Append(ruleName);
            sb.Append(" */");
            sb.Append("\npublic designGraph ");
            sb.Append(funcName);
            sb.Append(
                "(designGraph Lmapping, designGraph host, designGraph Rmapping, double[] parameters, grammarRule rule)\n");
            sb.Append("{\n/* here is where the code for the APPLY function is to be located.\n");
            sb.Append("* please modify host (or located nodes) with the input from parameters. */\n");
            sb.Append("return host;\n}\n");

            fileString = fileString.Insert(position, sb.ToString());

            var w = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write), Encoding.UTF8);
            w.Write(fileString);
            w.Flush();
            w.Close();
        }

        #endregion
    }
}