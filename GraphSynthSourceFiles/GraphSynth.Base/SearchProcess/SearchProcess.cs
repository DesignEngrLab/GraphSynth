/*************************************************************************
 *     This SearchProcess file & class is part of the GraphSynth.
 *     BaseClasses Project which is the foundation of the GraphSynth Ap-
 *     plication. GraphSynth.BaseClasses is protected and copyright under 
 *     the MIT License.
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
using System.Collections;
using System.Collections.Generic;
using GraphSynth.Representation;

namespace GraphSynth.Search
{
    /// <summary>
    ///   The abstract class that must be inherited in the Search plugins.
    /// </summary>
    public abstract class SearchProcess
    {
        private int _requireNumRuleSets;
        private Boolean _requireSeed = true;
        private Boolean numReqRuleSetsSet;

        /// <summary>
        ///   Initializes a new instance of the <see cref = "SearchProcess" /> class.
        /// </summary>
        /// <param name = "settings">The settings.</param>
        protected SearchProcess(GlobalSettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "SearchProcess" /> class.
        /// </summary>
        protected SearchProcess()
        {
        }

        /// <summary>
        ///   Gets the seed graph defined in settings.
        /// </summary>
        /// <value>The seed graph.</value>
        protected designGraph seedGraph
        {
            get { return settings.seed; }
        }

        /// <summary>
        ///   Gets the seed candidate.
        /// </summary>
        /// <value>The seed candidate.</value>
        protected candidate seedCandidate
        {
            get { return new candidate(seedGraph, settings.numOfRuleSets); }
        }

        /// <summary>
        /// Gets the number of RuleSets.
        /// </summary>
        /// <value>
        /// The num of rule sets.
        /// </value>
        protected int numOfRuleSets { get { return settings.numOfRuleSets; } }

        /// <summary>
        ///   Gets the rulesets defined in the settings as an array.
        /// </summary>
        /// <value>The rulesets.</value>
        protected ruleSet[] rulesets { get; private set; }

        /// <summary>
        ///   Gets or sets a value indicating whether [require seed].
        /// </summary>
        /// <value><c>true</c> if [require seed]; otherwise, <c>false</c>.</value>
        public Boolean RequireSeed
        {
            get { return _requireSeed; }
            protected set { _requireSeed = value; }
        }

        /// <summary>
        ///   Gets or sets the required num rule sets.
        /// </summary>
        /// <value>The required num rule sets.</value>
        public int RequiredNumRuleSets
        {
            get
            {
                if (numReqRuleSetsSet) return _requireNumRuleSets;
                return Math.Max(settings.numOfRuleSets, 1);
            }
            protected set
            {
                _requireNumRuleSets = value;
                numReqRuleSetsSet = true;
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether [auto play].
        /// </summary>
        /// <value><c>true</c> if [auto play]; otherwise, <c>false</c>.</value>
        public Boolean AutoPlay { get; set; }

        /// <summary>
        ///   Gets the settings defined for the problem (as saved in GraphSynthSettings.gsconfig
        ///   and as specified by the user in Edit->Settings).
        /// </summary>
        /// <value>The settings.</value>
        public GlobalSettings settings { get; set; }

        /// <summary>
        ///   Gets the text describing that is displayed in the menu. It must be overridden in the methods.
        /// </summary>
        /// <value>The text.</value>
        public abstract string text { get; }

        /// <summary>
        ///   Runs the search process.
        /// </summary>
        public void RunSearchProcess()
        {
            if ((numOfRuleSets > 0) && (RequiredNumRuleSets > 0))
            {
                rulesets = new ruleSet[settings.numOfRuleSets];
                for (var i = 0; i < settings.numOfRuleSets; i++)
                    rulesets[i] = settings.rulesets[i];
                ruleSet.loadAndCompileSourceFiles(rulesets, settings.RecompileRuleConditions,
                                                  settings.CompiledRuleFunctions, GlobalSettings.ExecDir);
            }
            Run();
            rulesets = null;
        }

        /// <summary>
        ///   Runs this instance.
        /// </summary>
        protected abstract void Run();


        /// <summary>
        ///   A necessary function when multiple (more than one) application of a rule is applied
        ///   to a host. The function reads in the child graph (often a copy of the current), 
        ///   the current graph, and the Lmapping. The Lmapping is changed but the child and current are
        ///   unaffected.
        /// </summary>
        /// <param name = "child">The child.</param>
        /// <param name = "current">The current.</param>
        /// <param name = "Lmapping">The lmapping.</param>
        public static void transferLmappingToChild(designGraph child, designGraph current, option Lmapping)
        {
            transferLmappingToChild(child, current, Lmapping.nodes, Lmapping.arcs, Lmapping.hyperarcs);
        }
        /// <summary>
        /// A necessary function when multiple (more than one) application of a rule is applied
        /// to a host. The function reads in the child graph (often a copy of the current),
        /// the current graph, and the Lmapping. The Lmapping is changed but the child and current are
        /// unaffected.
        /// </summary>
        /// <param name="child">The child.</param>
        /// <param name="current">The current.</param>
        /// <param name="LMappedNodes">The L mapped nodes.</param>
        /// <param name="LMappedArcs">The L mapped arcs.</param>
        /// <param name="LMappedHyperarcs">The L mapped hyperarcs.</param>
        public static void transferLmappingToChild(designGraph child, designGraph current, List<node> LMappedNodes, List<arc> LMappedArcs,
            List<hyperarc> LMappedHyperarcs)
        {
            /* this is a subtle issue with recognize-choose-apply in a Tree Search.
             * The locations within each option are pointing to nodes and arcs within the current.graph,
             * but we would like to retain the current so we make a deep copy of it. This is fine but now
             * the locations need to be transfered to the child. That is why this function was created. */
            for (var i = 0; i != LMappedArcs.Count; i++)
            {
                var Larc = LMappedArcs[i];
                LMappedArcs[i] = (Larc == null) ? null : child.arcs[current.arcs.IndexOf(Larc)];
            }
            for (var i = 0; i != LMappedNodes.Count; i++)
            {
                var Lnode = LMappedNodes[i];
                LMappedNodes[i] = (Lnode == null) ? null : child.nodes[current.nodes.IndexOf(Lnode)];
            }
            for (var i = 0; i != LMappedHyperarcs.Count; i++)
            {
                var Lhyperarc = LMappedHyperarcs[i];
                LMappedHyperarcs[i] = (Lhyperarc == null) ? null : child.hyperarcs[current.hyperarcs.IndexOf(Lhyperarc)];
            }
        }

        /// <summary>
        ///   This function returns what the new ruleSet
        ///   will be. Here the enumerator nextGenerationSteps and GenerationStatuses is used to great
        ///   affect. Understand that if a negative number is returned, the cycle will be stopped.
        /// </summary>
        /// <param name="ruleSetIndex">Index of the rule set.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        protected int nextRuleSet(int ruleSetIndex, GenerationStatuses status)
        {
            return NextRuleSet(rulesets, ruleSetIndex, status);
        }
        /// <summary>
        /// This function returns what the new ruleSet
        /// will be. Here the enumerator nextGenerationSteps and GenerationStatuses is used to great
        /// affect. Understand that if a negative number is returned, the cycle will be stopped.
        /// </summary>
        /// <param name="RuleSets">The rule sets.</param>
        /// <param name="ruleSetIndex">Index of the rule set.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public static int NextRuleSet(ruleSet[] RuleSets,int ruleSetIndex, GenerationStatuses status)
        {
            switch (RuleSets[ruleSetIndex].nextGenerationStep[(int)status])
            {
                case nextGenerationSteps.Loop:
                    return ruleSetIndex;
                case nextGenerationSteps.GoToNext:
                    return ++ruleSetIndex;
                case nextGenerationSteps.GoToPrevious:
                    return --ruleSetIndex;
                default:
                    return (int)RuleSets[ruleSetIndex].nextGenerationStep[(int)status];
            }
        }

        #region Common Search Functions
        /// <summary>
        /// Determines whether [the specified type] is inherited from SearchProcess.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static Boolean IsInheritedType(Type t)
        {
            while (t != typeof(object))
            {
                if (t == typeof(SearchProcess)) return true;
                t = t.BaseType;
            }
            return false;
        }
        /// <summary>
        ///   Adds the child to sorted candidate list based on the value of f0 (performanceParams[0]).
        ///   The OptimizeDirection is not used as the list is always sorted from lowest to highest.
        /// </summary>
        /// <param name = "candidates">The candidates.</param>
        /// <param name = "child">The child.</param>
        protected void addChildToSortedCandList(List<candidate> candidates, candidate child)
        {
            throw new NotSupportedException("This method is no longer supported. It is more efficient"
                                            +
                                            "to use a SortedList or Dictionary, as this insertion into the list is inefficient.");
        }

        #region Pareto Functions
        /// <summary>
        ///   Adds the new candidate to the pareto set.
        /// </summary>
        /// <param name = "c">The c.</param>
        /// <param name = "ParetoCands">The pareto cands.</param>
        protected static void addNewCandtoPareto(candidate c, List<candidate> ParetoCands)
        {
            for (var i = ParetoCands.Count - 1; i >= 0; i--)
            {
                var pc = ParetoCands[i];
                if (dominates(c, pc))
                    ParetoCands.Remove(pc);
                else if (dominates(pc, c)) return;
            }
            ParetoCands.Add(c);
        }


        /// <summary>
        ///   Does c1 dominate c2? assuming this is a minimization of all objectives 
        /// </summary>
        /// <param name = "c1">the subject candidate, c1 (does this dominate...).</param>
        /// <param name = "c2">the object candidate, c2 (is dominated by).</param>
        /// <returns></returns>
        protected static Boolean dominates(candidate c1, candidate c2)
        {
            var length = Math.Min(c1.performanceParams.Count, c2.performanceParams.Count);
            var optvector = new optimize[length];
            for (var i = 0; i < length; i++)
                optvector[i] = optimize.minimize;
            return dominates(c1, c2, optvector);
        }

        /// <summary>
        ///   Does candidate, c1, dominate c2?
        /// </summary>
        /// <param name = "c1">The c1.</param>
        /// <param name = "c2">The c2.</param>
        /// <param name = "optDirections">The opt directions.</param>
        /// <returns></returns>
        protected static Boolean dominates(candidate c1, candidate c2, optimize[] optDirections)
        {
            var length = Math.Min(Math.Min(c1.performanceParams.Count, c2.performanceParams.Count),
                                  optDirections.GetLength(0));
            for (var i = 0; i < length; i++)
                if (((int)optDirections[i]) * c1.performanceParams[i] <
                    ((int)optDirections[i]) * c2.performanceParams[i])
                    return false;
            return true;
        }
        #endregion

        #endregion

        /// <summary>
        /// Gets or sets the input directory.
        /// </summary>
        /// <value>The input directory.</value>
        public string inputDirectory
        {
            get { return settings.InputDirAbs; }
            set { throw new Exception("Cannot set input directory from inside plugin. Please change in settings window."); }
        }

        /// <summary>
        /// Gets or sets the output directory.
        /// </summary>
        /// <value>The output directory.</value>
        public string outputDirectory
        {
            get { return settings.OutputDirAbs; }
            set { throw new Exception("Cannot set output directory from inside plugin. Please change in settings window."); }
        }

        /// <summary>
        /// Gets or sets the rules directory.
        /// </summary>
        /// <value>The rules directory.</value>
        public string rulesDirectory
        {
            get { return settings.RulesDirAbs; }
            set { throw new Exception("Cannot set rules directory from inside plugin. Please change in settings window."); }
        }

        /// <summary>
        /// Saves the specified filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="o">The o.</param>
        /// <param name="SuppressWarnings">if set to <c>true</c> [suppress warnings].</param>
        public void Save(string filename, object o, bool SuppressWarnings = false)
        {
            settings.filer.Save(filename, o, SuppressWarnings);
        }

        /// <summary>
        /// Saves the candidates.
        /// </summary>
        /// <param name="filename">The filename base, a unique number is added for each
        /// candidate (plus the timestamp, if true).</param>
        /// <param name="candidates">The candidates.</param>
        /// <param name="SaveToOutputDir">if set to <c>true</c> [save to output dir].</param>
        /// <param name="timeStamp">if set to <c>true</c> [time stamp].</param>
        public void SaveCandidates(string filename, IList candidates, bool SaveToOutputDir, bool timeStamp)
        {
            settings.filer.SaveCandidates(filename, candidates, SaveToOutputDir, timeStamp);
        }

        /// <summary>
        /// Opens the specified filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="SuppressWarnings">if set to <c>true</c> [suppress warnings].</param>
        /// <returns></returns>
        public object[] Open(string filename, bool SuppressWarnings = false)
        {
            return settings.filer.Open(filename, SuppressWarnings);
        }
    }
}