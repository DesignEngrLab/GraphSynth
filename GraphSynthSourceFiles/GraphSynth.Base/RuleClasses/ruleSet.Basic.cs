/*************************************************************************
 *     This ruleSet.Basic.cs file partially defines the ruleset class (also
 *     partially defined in ruleSet.File.cs) and is part of the 
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
using System.Linq;
using System.Xml.Serialization;

namespace GraphSynth.Representation
{
    /* As far as I can tell, this is the first time the idea of a rule set
     * has been developed to this degree. In many applications we find that 
     * different sets of rules are needed. Many of these characteristics
     * are built into our current generation process. */

    public partial class ruleSet
    {
        #region Fields & Properties

        /* A ruleSet can have one rule set to the triggerRule. If there is no
         * triggerRule, then this should stay at negative one (or any negative
         * number). When the trigger rule is applied, the generation process, will
         * exit to the specified generationStep (as described below). */
        private choiceMethods _choiceMethod = choiceMethods.Design;
        private feasibilityState _finalCandidates = feasibilityState.Unspecified;
        private feasibilityState _interimCandidates = feasibilityState.Unspecified;
        private int _triggerRuleNum = -1;


        /// <summary>
        ///   For a particular set of rules, we need to specify what generation should
        ///   do if any of five conditions occur during the recognize->choose->apply
        ///   cycle. The enumerator, nextGenerationSteps, listed in globalSettings.cs
        ///   indicates what to do. The five correspond directly to the five elements
        ///   of another enumerator called GenerationStatuses. These five possibilties are:
        ///   Normal, Choice, CycleLimit, NoRules, TriggerRule. So, following normal operation 
        ///   of RCA (normal), we perform the first operation stated below, nextGenerationStep[0]
        ///   this will likely be to LOOP and contine apply rules. Defaults for these are
        ///   specified in App.gsconfig.           
        [XmlIgnore]
        /// </summary>
        public nextGenerationSteps[] nextGenerationStep;

        /// <summary>
        ///   Represents a list of the rule file names.
        /// </summary>
        public List<string> ruleFileNames = new List<string>();

        /// <summary>
        ///   Represents a list of the rules included within the ruleset.
        ///   The rules are clearly part of the set, but these are not stored
        ///   in the rsxml file, only the ruleFileNames.
        /// </summary>
        [XmlIgnore]
        public List<grammarRule> rules = new List<grammarRule>();

        private List<string> _recognizeSourceFiles = new List<string>();
        private List<string> _applySourceFiles = new List<string>();

        /// <summary>
        ///   Gets or sets the name for the ruleSet - usually set to the filename
        /// </summary>
        /// <value>The name.</value>
        public string name { get; set; }

        /// <summary>
        ///   Gets or sets the trigger rule num. Note: the rule numbers start at 1
        ///   not zero. Here we keep track by using a zero-based private field with
        ///   this property (as a way to remember. I know it sounds strange, but it
        ///   works).
        /// </summary>
        /// <value>The trigger rule num.</value>
        public int TriggerRuleNum
        {
            get { return _triggerRuleNum + 1; }
            set { _triggerRuleNum = value - 1; }
        }

        /// <summary>
        ///   Gets or sets the choice method - either automatic or by design.
        /// </summary>
        /// <value>The choice method.</value>
        public choiceMethods choiceMethod
        {
            get { return _choiceMethod; }
            set { _choiceMethod = value; }
        }

        /* Often when multiple ruleSets are used, some will produce feasible candidates, 
         * while others will only produce steps towards a feasible candidate. Here, we
         * classify a particular ruleSet as one of these. */

        /// <summary>
        ///   Gets or sets the feasibility state of the interim candidates.
        /// </summary>
        /// <value>The interim candidates.</value>
        public feasibilityState interimCandidates
        {
            get { return _interimCandidates; }
            set { _interimCandidates = value; }
        }

        /// <summary>
        ///   Gets or sets the feasibility state of the final candidates.
        /// </summary>
        /// <value>The final candidates.</value>
        public feasibilityState finalCandidates
        {
            get { return _finalCandidates; }
            set { _finalCandidates = value; }
        }

        /* For multiple ruleSets, a value to store its place within the set of ruleSets
         * proves a useful indicator. */

        /// <summary>
        ///   Gets or sets the index of the rule set.
        /// </summary>
        /// <value>The index of the rule set.</value>
        [XmlIgnore]
        public int RuleSetIndex { get; set; }

        /* a C# file can be custom created to correspond to special recognize or apply
         * instructions that may exist. These '.cs' are stored here.  */

        /// <summary>
        ///   Gets or sets the recognize source file names (string paths).
        /// </summary>
        /// <value>The recognize source files.</value>
        public List<string> recognizeSourceFiles
        {
            get { return _recognizeSourceFiles; }
            set { _recognizeSourceFiles = value; }
        }

        /// <summary>
        ///   Gets or sets the apply source file names (string paths).
        /// </summary>
        /// <value>The apply source files.</value>
        public List<string> applySourceFiles
        {
            get { return _applySourceFiles; }
            set { _applySourceFiles = value; }
        }

        /// <summary>
        ///   Retrieves the index of the next rule set.A helper function to RecognizeChooseApplyCycle.
        ///   This function returns what the new ruleSet will be. Here the enumerator nextGenerationSteps
        ///   and GenerationStatuses is used to great affect. Understand that if a negative number is
        ///   returned, the cycle will be stopped.
        /// </summary>
        /// <param name = "status">The status.</param>
        /// <returns></returns>
        public int nextRuleSet(GenerationStatuses status)
        {
            switch (nextGenerationStep[(int)status])
            {
                case nextGenerationSteps.Loop:
                    return RuleSetIndex;
                case nextGenerationSteps.GoToNext:
                    return RuleSetIndex + 1;
                case nextGenerationSteps.GoToPrevious:
                    return RuleSetIndex - 1;
                default:
                    return (int)nextGenerationStep[(int)status];
            }
        }

        #endregion

        #region Constructor
        /// <summary>
        ///   Initializes a new instance of the <see cref = "ruleSet" /> class.
        /// </summary>
        /// <param name = "defaultRulesDir">The default rules dir.</param>
        public ruleSet(string defaultRulesDir)
        {
            rulesDir = defaultRulesDir;
            nextGenerationStep = new[]
                {
                    nextGenerationSteps.Loop,
                    nextGenerationSteps.Stop,
                    nextGenerationSteps.Stop,
                    nextGenerationSteps.Stop,
                    nextGenerationSteps.Stop
                };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ruleSet"/> class.
        /// </summary>
        public ruleSet() : this("") { }

        #endregion

        #region Methods

        /// <summary>
        /// This is the recognize function called within the RCA generation. It is
        /// fairly straightforward method that basically invokes the more complex
        /// recognize function for each rule within it, and returns a list of
        /// options.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="InParallel">if set to <c>true</c> [in parallel].</param>
        /// <param name="RelaxationTemplate">The relaxation template.</param>
        /// <returns></returns>
        public List<option> recognize(designGraph host, Boolean InParallel = true, Relaxation RelaxationTemplate = null)
        {
            var options = new List<option>();
            if (rules.Count == 0) return options;
            if (choiceMethod == choiceMethods.Automatic)
            {
                for (var i = 0; i != rules.Count; i++)
                {
                    var ruleOpts = rules[i].recognize(host, InParallel, (generationAfterNoRules == nextGenerationSteps.Stop) ? RelaxationTemplate : null);
                    if (ruleOpts.Count > 0)
                    {
                        var r0 = ruleOpts[0];
                        r0.assignRuleInfo(i + 1, RuleSetIndex);
                        return new List<option> { r0 };
                    }
                }
            }
            else if (InParallel)/* new parallel rule check */
                options = rules.SelectMany((rule, ruleIndex) =>
                                               rule.recognize(host, true, (generationAfterNoRules == nextGenerationSteps.Stop) ? RelaxationTemplate : null)
                                               .Select(o => o.assignRuleInfo(ruleIndex + 1, RuleSetIndex))).AsParallel().ToList();
            else /* do in series */
                options = rules.SelectMany((rule, ruleIndex) =>
                                               rule.recognize(host, false, (generationAfterNoRules == nextGenerationSteps.Stop) ? RelaxationTemplate : null)
                                               .Select(o => o.assignRuleInfo(ruleIndex + 1, RuleSetIndex))).ToList();
            for (var i = 0; i < options.Count; i++)
                options[i].optionNumber = i;
            return options;
        }

        /* simple functions to add and remove rules from the ruleSet */

        /// <summary>
        ///   Adds the specified new rule.
        /// </summary>
        /// <param name = "newRule">The new rule.</param>
        public void Add(grammarRule newRule)
        {
            rules.Add(newRule);
        }

        /// <summary>
        ///   Removes the specified remove rule.
        /// </summary>
        /// <param name = "removeRule">The remove rule.</param>
        public void Remove(grammarRule removeRule)
        {
            rules.Remove(removeRule);
        }

        /// <summary>
        ///   Returns a copy of this instance.
        /// </summary>
        /// <returns></returns>
        public ruleSet copy()
        {
            var copyOfRuleSet = new ruleSet();
            foreach (var a in applySourceFiles)
                copyOfRuleSet.applySourceFiles.Add(a);
            foreach (var a in recognizeSourceFiles)
                copyOfRuleSet.recognizeSourceFiles.Add(a);
            copyOfRuleSet.choiceMethod = choiceMethod;
            copyOfRuleSet.finalCandidates = finalCandidates;
            copyOfRuleSet.generationAfterChoice = generationAfterChoice;
            copyOfRuleSet.generationAfterCycleLimit = generationAfterCycleLimit;
            copyOfRuleSet.generationAfterNormal = generationAfterNormal;
            copyOfRuleSet.generationAfterNoRules = generationAfterNoRules;
            copyOfRuleSet.generationAfterTriggerRule = generationAfterTriggerRule;
            copyOfRuleSet.interimCandidates = interimCandidates;
            copyOfRuleSet.name = name;
            foreach (var a in ruleFileNames)
                copyOfRuleSet.ruleFileNames.Add(a);
            foreach (var a in rules)
                copyOfRuleSet.rules.Add(a);
            copyOfRuleSet.rulesDir = rulesDir;
            copyOfRuleSet.RuleSetIndex = RuleSetIndex;
            copyOfRuleSet.TriggerRuleNum = TriggerRuleNum;
            return copyOfRuleSet;
        }

        #endregion

    }
}