using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var w = new StreamWriter(fs, Encoding.Default);

            w.Write("using System;\nusing System.Collections.Generic;\n");
            w.Write("using GraphSynth;\nusing GraphSynth.Representation;\n");
            w.Write("\nnamespace GraphSynth.ParamRules\n{\n");
            w.Write("public partial class ParamRules\n{\n");
            w.Write("/* here are parametric rules written as part of the ruleSet.\n");
            w.Write("* these are compiled at runtime into a .dll indicated in the\n");
            w.Write("* App.gsconfig file. */\n");
            w.Write("#region Parametric Recognition Rules\n");
            w.Write("/* Parametric recognition rules receive as input:\n");
            w.Write("         * 1. the option which includes the rule, 3x3 transformation matrix to transform node\n" +
                    "              positions from L to host (T[,]), the location of the nodes, arcs, and hyperarcs\n" +
                    "              of the LHS, and some extraneous data.\n");
            w.Write("         * 2. the entire host graph (host) */ \n");
            w.Write("#endregion\n\n\n");
            w.Write("#region Parametric Application Rules\n");
            w.Write("/* Parametric application rules receive as input:\n");
            w.Write("         * 1. the location designGraph indicating the nodes&arcs of host that match with L (Lmapping)\n");
            w.Write("         * 2. the entire host graph (host)\n");
            w.Write("         * 3. the location of the nodes in the host that R matches to (Rmapping).\n");
            w.Write("         * 4. the parameters chosen for instantiating elements of Rmapping (parameters). */\n");
            w.Write("#endregion\n\n\n");
            w.Write("}\n}\n");
            w.Flush();
            w.Close();
            fs.Close();
        }

        #endregion

        #region Creating Functions within files
        public static void checkForFunctions(Boolean isThisRecognize, grammarRule SelectedRule,
                                             List<string> newFunctions)
        {
            var numNewFuncs = newFunctions.Count;
            if (numNewFuncs == 0) return;
            var filesWithFunc = new string[numNewFuncs];
            var found = new Boolean[numNewFuncs];
            var sourceFiles = Directory.GetFiles(GSApp.settings.RulesDirAbs, "*.cs");

            for (int i = 0; i < numNewFuncs; i++)
            {
                found[i] = false;
                var funcName = newFunctions[i];
                foreach (string file in sourceFiles)
                {
                    var r = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read), Encoding.Default);
                    var funcString = "public ";
                    if (isThisRecognize) funcString += "double ";
                    else funcString += "void ";
                    funcString += funcName + "(";
                    if (r.ReadToEnd().Contains(funcString))
                    {
                        filesWithFunc[i] = file;
                        found[i] = true;
                        r.Close();
                        break;
                    }
                    else r.Close();
                }
                if (!found[i] && !isThisRecognize)
                {
                    foreach (string file in sourceFiles)
                    {
                        var r = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read), Encoding.Default);
                        var funcString = "public designGraph " + funcName + "(";
                        var fileString = r.ReadToEnd();
                        r.Close();
                        var position = fileString.IndexOf(funcString, StringComparison.Ordinal);
                        if (position > 0)
                        {
                            filesWithFunc[i] = file;
                            found[i] = true;
                            fileString = fileString.Remove(position + 7, 11);
                            fileString = fileString.Insert(position + 7, "void");
                            position = fileString.IndexOf("return host;", position, StringComparison.Ordinal);
                            fileString = fileString.Remove(position, 12);

                            var w = new StreamWriter(new FileStream(file, FileMode.Create, FileAccess.Write), Encoding.Default);
                            w.Write(fileString);
                            w.Flush();
                            w.Close();
                            break;
                        }
                    }
                }
            }
            AdditionalFunctionToFileDialog.Show(isThisRecognize, newFunctions, filesWithFunc, found, sourceFiles);
            if (filesWithFunc.Any(f => f == null)) return;
            for (int i = 0; i < numNewFuncs; i++)
            {
                if (found[i]) continue;
                if (isThisRecognize)
                    createRecognizeFunctionTemplate(filesWithFunc[i], SelectedRule, newFunctions[i]);
                else createApplyFunctionTemplate(filesWithFunc[i], SelectedRule, newFunctions[i]);
            }
        }

        public static void checkForFunctionsOLD(Boolean isThisRecognize, grammarRule SelectedRule,
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
                    var r = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read), Encoding.Default);
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
                        createRecognizeFunctionTemplate(sourceFiles[i], SelectedRule, funcName);
                        functionNames.Add(funcName);
                    }
                    else
                    {
                        createApplyFunctionTemplate(sourceFiles[i], SelectedRule, funcName);
                        functionNames.Add(funcName);
                    }
                }
            }
        }

        private static void createRecognizeFunctionTemplate(string path, grammarRule rule, string funcName)
        {
            var r = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read), Encoding.Default);
            var fileString = r.ReadToEnd();
            /** find place to insert new function **/
            var startOfApplyFuncsIndex = fileString.IndexOf("#region Parametric Application Rules", 0, StringComparison.Ordinal);
            var startBackABitIndex = startOfApplyFuncsIndex;
            var position = -1;
            do
            {
                startBackABitIndex -= 10;
                position = fileString.IndexOf("#endregion", startBackABitIndex, StringComparison.Ordinal);
            } while (position > startOfApplyFuncsIndex);
            /***************************************/
            var sb = new StringBuilder("");
            r.Close();

            sb.Append("\n/* This is RECOGNIZE for the rule entitled: ");
            sb.Append(rule.name);
            sb.Append(" */");
            sb.Append("\npublic double ");
            sb.Append(funcName);
            sb.Append("(option opt, designGraph host)\n{\n");
            sb.Append("#region Define Mapped Elements\n");
            sb.Append("/* the following variables are declared for your convenience. They are the mapped elements" +
                      " of the LHS in the host graph. */\n");
            for (int i = 0; i < rule.L.nodes.Count; i++)
            {
                var n = (ruleNode)rule.L.nodes[i];
                if (n.NotExist) continue;
                if (n.localLabels.Count > 0)
                    sb.Append("/* " + n.name + " is the node in L that has labels: " +
                              StringCollectionConverter.convert(n.localLabels) + ";");
                else
                    sb.Append("/* " + n.name + " is the node in L that has no labels;");
                if (n.negateLabels.Count > 0)
                    sb.Append(" has the negating labels: " +
                              StringCollectionConverter.convert(n.negateLabels) + ";");
                sb.Append("\n * is connected to: " + StringCollectionConverter.convert(n.arcs.Select(a => a.name)) +
                          ";");
                sb.Append(" and is located at [" + n.X + ", " + n.Y + ", " + n.Z + "]. */\n");
                sb.Append("var " + n.name + " = opt.nodes[" + i + "];\n\n");
            }
            for (int i = 0; i < rule.L.arcs.Count; i++)
            {
                var a = (ruleArc)rule.L.arcs[i];
                if (a.NotExist) continue;
                if (a.localLabels.Count > 0)
                    sb.Append("/* " + a.name + " is the arc in L that has labels: " +
                              StringCollectionConverter.convert(a.localLabels) + ";");
                else
                    sb.Append("/* " + a.name + " is the arc in L that has no labels;");
                if (a.negateLabels.Count > 0)
                    sb.Append(" has the negating labels: " + StringCollectionConverter.convert(a.negateLabels) + ";");
                sb.Append("\n * and is connected from " + a.From.name + " to " + a.To.name + ". */\n");
                sb.Append("var " + a.name + " = opt.arcs[" + i + "];\n\n");
            }
            for (int i = 0; i < rule.L.hyperarcs.Count; i++)
            {
                var ha = (ruleHyperarc)rule.L.hyperarcs[i];
                if (ha.NotExist) continue;
                if (ha.localLabels.Count > 0)
                    sb.Append("/* " + ha.name + " is the hypearc in L that has labels: " +
                              StringCollectionConverter.convert(ha.localLabels) + ";");
                else
                    sb.Append("/* " + ha.name + " is the hyperarc in L that no labels;");
                if (ha.negateLabels.Count > 0)
                    sb.Append(" has the negating labels: " +
                              StringCollectionConverter.convert(ha.negateLabels) + ";\n");
                sb.Append("\n * and is connected to: " + StringCollectionConverter.convert(ha.nodes.Select(n => n.name)) +
                          ". */\n");
                sb.Append("var " + ha.name + " = opt.hyperarcs[" + i + "];\n\n");
            }
            sb.Append("#endregion\n\n\n");
            sb.Append("\n\n/* here is where the code for the RECOGNIZE function is to be located.\n");
            sb.Append(" * please remember that returning a positive real (double) is equivalent to\n");
            sb.Append(" * a constraint violation. Zero and negative numbers are feasible. */\n");
            sb.Append("return 0.0;\n}\n");
            fileString = fileString.Insert(position, sb.ToString());

            var w = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write), Encoding.Default);
            w.Write(fileString);
            w.Flush();
            w.Close();
        }

        private static void createApplyFunctionTemplate(string path, grammarRule rule, string funcName)
        {
            var r = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read), Encoding.Default);
            var fileString = r.ReadToEnd();
            /** find place to insert new function **/
            var position = fileString.IndexOf("#region Parametric Application Rules", 0, StringComparison.Ordinal);
            while (fileString.IndexOf("#endregion", position + 1, StringComparison.Ordinal) > 0)
            {
                position = fileString.IndexOf("#endregion", position + 1, StringComparison.Ordinal);
            }
            /***************************************/
            var sb = new StringBuilder("");
            r.Close();

            sb.Append("\n/* This is APPLY for the rule entitled: ");
            sb.Append(rule.name);
            sb.Append(" */");
            sb.Append("\npublic void ");
            sb.Append(funcName);
            sb.Append(
                "(option opt, designGraph host, designGraph Rmapping, double[] parameters)\n{\n");


            sb.Append("#region Define Mapped Elements\n");
            sb.Append("/* the following variables are declared for your convenience. They are the mapped elements" +
                      " of the L, K, and R elements\n" +
                      " * in the host graph. The convention that is used is when the element is only in L, then the\n" +
                      " * variable name is followed by \"_deleted\", when the element is only in R, then it is appended\n" +
                      " * with \"_added\". If it is in both (in K), then no post-script is added. */\n");
            for (int i = 0; i < rule.L.nodes.Count; i++)
            {
                var n = (ruleNode)rule.L.nodes[i];
                if (n.NotExist) continue;
                var name = (rule.R.nodes.Any(rn => rn.name.Equals(n.name))) ? n.name : n.name + "_deleted";
                var where = (rule.R.nodes.Any(rn => rn.name.Equals(n.name))) ? "K" : "L";
                if (n.localLabels.Count > 0)
                    sb.Append("/* " + name + " is the node in " + where + " that has labels: " +
                              StringCollectionConverter.convert(n.localLabels) + ";");
                else
                    sb.Append("/* " + name + " is the node in " + where + " that has no labels;");
                if (n.negateLabels.Count > 0)
                    sb.Append(" has the negating labels: " +
                              StringCollectionConverter.convert(n.negateLabels) + ";");
                sb.Append("\n * is connected to: " + StringCollectionConverter.convert(n.arcs.Select(a => a.name)) +
                          ";");
                sb.Append(" and is located at [" + n.X + ", " + n.Y + ", " + n.Z + "]. */\n");
                sb.Append("var " + name + " = opt.nodes[" + i + "];\n\n");
            }
            for (int i = 0; i < rule.R.nodes.Count; i++)
            {
                var n = (ruleNode)rule.R.nodes[i];
                if (rule.L.nodes.Any(ln => ln.name.Equals(n.name))) continue;
                if (n.localLabels.Count > 0)
                    sb.Append("/* " + n.name + "_added is the node in R that has labels: " +
                              StringCollectionConverter.convert(n.localLabels) + ";");
                else
                    sb.Append("/* " + n.name + "_added is the node in R that has no labels;");
                sb.Append("\n * is connected to: " + StringCollectionConverter.convert(n.arcs.Select(a => a.name)) +
                          ";");
                sb.Append(" and is located at [" + n.X + ", " + n.Y + ", " + n.Z + "]. */\n");
                sb.Append("var " + n.name + "_added = Rmapping.nodes[" + i + "];\n\n");
            }
            for (int i = 0; i < rule.L.arcs.Count; i++)
            {
                var a = (ruleArc)rule.L.arcs[i];
                if (a.NotExist) continue;
                var name = (rule.R.arcs.Any(ra => ra.name.Equals(a.name))) ? a.name : a.name + "_deleted";
                var where = (rule.R.arcs.Any(ra => ra.name.Equals(a.name))) ? "K" : "L";
                if (a.localLabels.Count > 0)
                    sb.Append("/* " + name + " is the arc in " + where + " that has labels: " +
                              StringCollectionConverter.convert(a.localLabels) + ";");
                else sb.Append("/* " + name + " is the arc in " + where + " that has no labels;");
                if (a.negateLabels.Count > 0)
                    sb.Append(" has the negating labels: " + StringCollectionConverter.convert(a.negateLabels) + ";");
                var fromName = a.From == null ? "nothing" : a.From.name;
                var toName = a.To == null ? "nothing" : a.To.name;
                sb.Append("\n * and is connected from " + fromName + " to " + toName + ". */\n");
                sb.Append("var " + name + " = opt.arcs[" + i + "];\n\n");
            }
            for (int i = 0; i < rule.R.arcs.Count; i++)
            {
                var a = (ruleArc)rule.R.arcs[i];
                if (rule.L.arcs.Any(ln => ln.name.Equals(a.name))) continue;
                if (a.localLabels.Count > 0)
                    sb.Append("/* " + a.name + "_added is the arc in R that has labels: " +
                              StringCollectionConverter.convert(a.localLabels) + ";");
                else sb.Append("/* " + a.name + "_added is the arc in R that has no labels;");
                var fromName = a.From == null ? "nothing" : a.From.name;
                var toName = a.To == null ? "nothing" : a.To.name;
                sb.Append("\n * and is connected from " + fromName + " to " + toName + ". */\n");
                sb.Append("var " + a.name + "_added = Rmapping.arcs[" + i + "];\n\n");
            }
            for (int i = 0; i < rule.L.hyperarcs.Count; i++)
            {
                var ha = (ruleHyperarc)rule.L.hyperarcs[i];
                if (ha.NotExist) continue;
                var name = (rule.R.hyperarcs.Any(ra => ra.name.Equals(ha.name))) ? ha.name : ha.name + "_deleted";
                var where = (rule.R.hyperarcs.Any(ra => ra.name.Equals(ha.name))) ? "K" : "L";
                if (ha.localLabels.Count > 0)
                    sb.Append("/* " + name + " is the hypearc in " + where + " that has labels: " +
                              StringCollectionConverter.convert(ha.localLabels) + ";");
                else
                    sb.Append("/* " + name + " is the hyperarc in " + where + " that no labels;");
                if (ha.negateLabels.Count > 0)
                    sb.Append(" has the negating labels: " +
                              StringCollectionConverter.convert(ha.negateLabels) + ";\n");
                sb.Append("\n * and is connected to: " + StringCollectionConverter.convert(ha.nodes.Select(n => n.name)) +
                          ". */\n");
                sb.Append("var " + name + " = opt.hyperarcs[" + i + "];\n\n");
            }
            for (int i = 0; i < rule.R.hyperarcs.Count; i++)
            {
                var a = (ruleHyperarc)rule.R.hyperarcs[i];
                if (rule.L.hyperarcs.Any(ln => ln.name.Equals(a.name))) continue;
                if (a.localLabels.Count > 0)
                    sb.Append("/* " + a.name + "_added is the hyperarc in R that has labels: " +
                              StringCollectionConverter.convert(a.localLabels) + ";");
                else sb.Append("/* " + a.name + "_added is the hyperarc in R that has no labels;");
                sb.Append("\n * and is connected to: " + StringCollectionConverter.convert(a.nodes.Select(n => n.name)) +
                          ". */\n");
                sb.Append("var " + a.name + "_added = Rmapping.hyperarcs[" + i + "];\n\n");
            }
            sb.Append("#endregion\n\n\n");

            sb.Append("\n\n/* here is where the code for the APPLY function is to be located.\n");
            sb.Append("* please modify host (or located nodes) with the input from parameters. */\n");
            sb.Append("\n}\n");

            fileString = fileString.Insert(position, sb.ToString());

            var w = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write), Encoding.Default);
            w.Write(fileString);
            w.Flush();
            w.Close();
        }

        #endregion
    }
}