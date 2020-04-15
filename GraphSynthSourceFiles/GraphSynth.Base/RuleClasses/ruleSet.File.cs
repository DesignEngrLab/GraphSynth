/*************************************************************************
 *     This ruleSet.File.cs file partially defines the ruleset class (also
 *     partially defined in ruleSet.Basic.cs) and is part of the 
 *     GraphSynth.BaseClasses Project which is the foundation of the 
 *     GraphSynth Application.
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
#if NETSTANDARD2_0
#else
using Microsoft.CSharp;
using System.CodeDom.Compiler;
#endif

namespace GraphSynth.Representation
{
    /// <summary>
    ///   The ruleset class represents one of the three main file types of GraphSynth. A ruleset is saved as a .rsxml. It represents a
    ///   language of rules that operate in concert. The included rules are all loaded at once, and used to populate a list of
    ///   options which make changes to a host.
    /// </summary>
    public partial class ruleSet
    {
#region Fields


        /// <summary>
        ///   Gets or sets the filer.
        /// </summary>
        /// <value>The filer.</value>
        [XmlIgnore]
        public BasicFiler filer { get; set; }

        /// <summary>
        ///   Gets or sets the rules dir.
        /// </summary>
        /// <value>The rules dir.</value>
        [XmlIgnore]
        public string rulesDir { get; set; }

        /// <summary>
        ///   Gets or sets the generation method after normal.
        /// </summary>
        /// <value>The generation after normal.</value>
        public nextGenerationSteps generationAfterNormal
        {
            get { return nextGenerationStep[0]; }
            set { nextGenerationStep[0] = value; }
        }

        /// <summary>
        ///   Gets or sets the generation method after choice.
        /// </summary>
        /// <value>The generation after choice.</value>
        public nextGenerationSteps generationAfterChoice
        {
            get { return nextGenerationStep[1]; }
            set { nextGenerationStep[1] = value; }
        }

        /// <summary>
        ///   Gets or sets the generation method after cycle limit.
        /// </summary>
        /// <value>The generation after cycle limit.</value>
        public nextGenerationSteps generationAfterCycleLimit
        {
            get { return nextGenerationStep[2]; }
            set { nextGenerationStep[2] = value; }
        }

        /// <summary>
        ///   Gets or sets the generation method after no rules.
        /// </summary>
        /// <value>The generation after no rules.</value>
        public nextGenerationSteps generationAfterNoRules
        {
            get { return nextGenerationStep[3]; }
            set { nextGenerationStep[3] = value; }
        }

        /// <summary>
        ///   Gets or sets the generation method after trigger rule.
        /// </summary>
        /// <value>The generation after trigger rule.</value>
        public nextGenerationSteps generationAfterTriggerRule
        {
            get { return nextGenerationStep[4]; }
            set { nextGenerationStep[4] = value; }
        }

#endregion

#region Load and compile Source Files

        /// <summary>
        ///   Loads and compiles the source files.
        /// </summary>
        /// <param name = "rulesets">The rulesets.</param>
        /// <param name = "recompileRules">if set to <c>true</c> [recompile rules].</param>
        /// <param name = "compiledparamRules">The compiledparam rules.</param>
        /// <param name = "execDir">The exec dir.</param>
        public static void loadAndCompileSourceFiles(ruleSet[] rulesets, Boolean recompileRules,
                                                     string compiledparamRules, string execDir)
        {
#if NETSTANDARD2_0
            throw new NotImplementedException("There is currently no way to compile parametric rules in this version of GraphSynth.");
#else
            if (rulesets.GetLength(0) == 0) return;
            Assembly assem = null;
            var allSourceFiles = new List<string>();
            var rulesDirectory = rulesets[0].rulesDir;

            if (!recompileRules && (compiledFunctionsAlreadyLoaded(rulesets))) return;
            if (recompileRules && FindSourceFiles(rulesets, allSourceFiles, rulesDirectory))
            {
                if (allSourceFiles.Count == 0)
                    SearchIO.output("No additional code files to compile.", 4);
                else
                {
                    CompilerResults cr;
                    if (CompileSourceFiles(rulesets, allSourceFiles, out cr,
                                           rulesDirectory, execDir, compiledparamRules))
                        assem = cr.CompiledAssembly;
                }
            }
            var filenames = new string[] { };
            if (assem == null)
            {
                /* load .dll since compilation crashed */
                filenames = Directory.GetFiles(rulesDirectory, "*" + compiledparamRules + "*");
                if (filenames.GetLength(0) > 1)
                    SearchIO.MessageBoxShow("More than one compiled library (*.dll) similar to "
                                        + compiledparamRules + "in" + rulesDirectory);
                if (filenames.GetLength(0) == 0)
                {
                    SearchIO.MessageBoxShow("Compiled library: "+ compiledparamRules + " not found in\n" 
                        + rulesDirectory + ".\n Attempting to recompile.");
                    CompilerResults cr;
                    if (CompileSourceFiles(rulesets, allSourceFiles, out cr,
                                           rulesDirectory, execDir, compiledparamRules))
                        assem = cr.CompiledAssembly;
                }
                else assem = Assembly.LoadFrom(filenames[0]);
            }
            try
            {
                if (assem != null)
                {
                    var compiledFunctions = assem.CreateInstance("GraphSynth.ParamRules.ParamRules");
                    foreach (var rule in rulesets.SelectMany(set => set.rules))
                    {
                        rule.DLLofFunctions = compiledFunctions;
                        rule.recognizeFuncs.Clear();
                        foreach (var functionName in rule.recognizeFunctions)
                        {
                            var func = compiledFunctions.GetType().GetMethod(functionName);
                            if (func != null) rule.recognizeFuncs.Add(func);
                            else
                                SearchIO.MessageBoxShow("Unable to locate function, " + functionName + ", in assembly, " + filenames[0] + ".");
                        }
                        rule.applyFuncs.Clear();
                        foreach (var functionName in rule.applyFunctions)
                        {
                            var func = compiledFunctions.GetType().GetMethod(functionName);
                            if (func != null) rule.applyFuncs.Add(func);
                            else
                                SearchIO.MessageBoxShow("Unable to locate function, " + functionName + ", in assembly, " + filenames[0] + ".");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SearchIO.MessageBoxShow("Compilation Error :" + ErrorLogger.MakeErrorString(e, false),
                                        "Error Compiling Additional Rule Functions", "Error");
            }
#endif
        }

        /// <summary>
        ///   Finds the source files.
        /// </summary>
        /// <param name = "rulesets">The rulesets.</param>
        /// <param name = "allSourceFiles">All source files.</param>
        /// <param name = "rulesDirectory">The rules directory.</param>
        /// <returns></returns>
        public static Boolean FindSourceFiles(ruleSet[] rulesets, List<string> allSourceFiles,
                                              string rulesDirectory)
        {
            var filesFound = true;

            foreach (var a in rulesets.Where(a => a != null))
            {
                foreach (var file in a.recognizeSourceFiles)
                {
                    var fileLower = file.ToLower();
                    if (File.Exists(rulesDirectory + fileLower))
                    {
                        if (!allSourceFiles.Contains(rulesDirectory + fileLower))
                            allSourceFiles.Add(rulesDirectory + fileLower);
                    }
                    else
                    {
                        SearchIO.MessageBoxShow("Missing source file: " + fileLower +
                                                ". Cancelling compilation of C# recognize source file.",
                                                "Missing File", "Error");
                        filesFound = false;
                        break;
                    }
                }
                foreach (var file in a.applySourceFiles)
                {
                    var fileLower = file.ToLower();
                    if (File.Exists(rulesDirectory + fileLower))
                    {
                        if (!allSourceFiles.Contains(rulesDirectory + fileLower))
                            allSourceFiles.Add(rulesDirectory + fileLower);
                    }
                    else
                    {
                        SearchIO.MessageBoxShow("Missing source file: " + fileLower +
                                                ". Cancelling compilation of C# apply source file.",
                                                "Missing File", "Error");
                        filesFound = false;
                        break;
                    }
                }
            }
            return filesFound;
        }

#if NETSTANDARD2_0
#else
        /// <summary>
        ///   Compiles the source files.
        /// </summary>
        /// <param name = "rulesets">The rulesets.</param>
        /// <param name = "allSourceFiles">All source files.</param>
        /// <param name = "cr">The cr.</param>
        /// <param name = "rulesDir">The rules dir.</param>
        /// <param name = "execDir">The exec dir.</param>
        /// <param name = "compiledparamRules">The compiledparam rules.</param>
        /// <returns></returns>
        public static Boolean CompileSourceFiles(ruleSet[] rulesets, List<string> allSourceFiles,
                                                 out CompilerResults cr, string rulesDir, string execDir,
                                                 string compiledparamRules)
        {
            cr = null;
            try
            {
                var c = new CSharpCodeProvider();
                // c.CreateCompiler();
                //                ICodeCompiler icc = c.CreateCompiler();
                var cp = new CompilerParameters();

                cp.ReferencedAssemblies.Add("system.dll");
                cp.ReferencedAssemblies.Add("system.xml.dll");
                cp.ReferencedAssemblies.Add("system.data.dll");
                cp.ReferencedAssemblies.Add("system.windows.forms.dll");
                //cp.ReferencedAssemblies.Add(execDir + "GraphSynth.exe");
                cp.ReferencedAssemblies.Add(execDir + "GraphSynth.BaseClasses.dll");

                cp.CompilerOptions = "/t:library";
                cp.GenerateInMemory = true;
                cp.OutputAssembly = rulesDir + compiledparamRules;
                var allSourceFilesArray = allSourceFiles.ToArray();

                cr = c.CompileAssemblyFromFile(cp, allSourceFilesArray);

                //cr = icc.CompileAssemblyFromFileBatch(cp, allSourceFilesArray);
                if (cr.Errors.HasErrors) throw new Exception();
                return true;
            }
            catch
            {
                SearchIO.MessageBoxShow("Error Compiling C# recognize and apply source files.",
                                        "Compilation Error", "Error");
                foreach (CompilerError e in cr.Errors)
                    SearchIO.output(e.ToString());
                return false;
            }
        }
#endif
        private static Boolean compiledFunctionsAlreadyLoaded(IEnumerable<ruleSet> rulesets)
        {
            return rulesets
                .Where(set => set != null)
                .All(set =>
                    !(set.rules.Any(rule => rule.recognizeFuncs.Count + rule.applyFuncs.Count
                        != rule.recognizeFunctions.Count + rule.applyFunctions.Count)));
        }

#endregion


    }
}