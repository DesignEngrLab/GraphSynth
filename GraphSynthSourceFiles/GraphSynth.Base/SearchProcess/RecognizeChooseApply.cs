/*************************************************************************
 *     This RecognizeChooseApply file & class is part of the GraphSynth.
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphSynth.Representation;

namespace GraphSynth.Search
{
    /// <summary>
    ///   this is the main file and class of generation. The model adopted is that one should
    ///   create an inherited class of this ABSTRACT class, as has been done for randomChoose, 
    ///   chooseViaHumanGui, etc. This file has gone through a lot of revision to make it 
    ///   general to a wide variety of problems. It should not have to be altered, rather one
    ///   can control aspects of the execution through the ruleSets
    /// </summary>
    public abstract class RecognizeChooseApply
    {
        #region Fields
        /// <summary>
        ///   An array of rulesets used in this generation process.
        /// </summary>
        protected readonly ruleSet[] Rulesets;

        /// <summary>
        /// The number of rule sets
        /// </summary>
        protected readonly int NumOfRuleSets;


        /// <summary>
        /// Gets or sets the host. Often the same as the seed used as a starting point for the 
        /// generation process. That seed is stored here as a accessible propoerty of the class.
        /// </summary>
        /// <value>
        /// The host.
        /// </value>
        public candidate Host { get; protected set; }

        /// <summary>
        ///   The array of the maximum number of calls to make in each ruleset.
        ///   This is not cumulative - if you return to a previous ruleset, the counter is 
        ///   reset. It is copied to the numOfCallsLeft at the beginning of the RCACycle into 
        ///   the numOfCallsLeft slot.
        /// </summary>
        /// <value>The max num of calls.</value>
        protected readonly int[] MaxNumOfCalls;


        /// <summary>
        /// A simple Boolean used for debugging or interactive generation. If true then
        /// the host will be replotted after each apply action.
        /// </summary>
        /// <value><c>true</c> if display; otherwise, <c>false</c>.</value>
        protected readonly Boolean Display;

        /// <summary>
        /// A value indicating whether [in parallel].
        /// </summary>
        /// <value><c>true</c> if [in parallel]; otherwise, <c>false</c>.</value>
        protected readonly Boolean InParallel;


        #endregion

        #region RecognizeChooseApplyCycle -- the Generation Process Defined

        /// <summary>
        ///   Recognizes the choose apply cycle. Here is the main Recognize, Choose, and 
        ///   Apply Generation Cycle. It accepts the host candidate (not graph), the index
        ///   of what ruleSet to invoke, and an array of size equal to the number of ruleSets.
        ///   At the end of the process, it returns the updated candidate. The three step
        ///   process may, however exit at any of five places in the loop, these are described below.
        ///   1. the ruleSet invoked may not have any calls left. This will cause the GenerationStatus
        ///   to be CycleLimit, and the process will execute what is stored in the 3rd position of 
        ///   generationSteps, ruleSet->nextGenerationStep[2], either Stop, Loop, GoToPrevious(ruleSet),
        ///   GoToNext(ruleSet), or GoToRuleSet#
        ///   2. the choice operation has sent a STOP message, or more precisely a negative # or
        ///   a number greater than the list of option. This results in a GenerationStatus of Choice
        ///   and the execution of ruleSet->nextGenerationStep[1] (any of the options stated above).
        ///   3. there are no rules recognized for the graph. This results in a GenerationStatus of
        ///   NoRules and the execution of ruleSet->nextGenerationStep[3] (any of the options above).
        ///   4. A trigger rule has been applied. This results in a GenerationStatus of TriggerRule
        ///   and the execution of ruleSet->nextGenerationStep[4] (any of the options stated above).
        ///   5. the recognize, choose, and apply cycle performed as intended - no abnormal activites.
        ///   This results in a GenerationStatus of Normal and the execution of 
        ///   ruleSet->nextGenerationStep[0] (any of the options stated above).*/
        /// </summary>
        /// <param name = "host">The host.</param>
        /// <param name = "ruleSetIndex">Index of the rule set.</param>
        /// <param name = "numOfCallsLeft">The num of calls left.</param>
        protected void RecognizeChooseApplyCycle(candidate host, int ruleSetIndex, int[] numOfCallsLeft)
        {
            while ((ruleSetIndex >= 0) && (ruleSetIndex < NumOfRuleSets))
            {
                host.activeRuleSetIndex = ruleSetIndex;
                SearchIO.output("Active Rule Set = " + ruleSetIndex, 4);

                #region terminate immediately if there are no cycles left

                if (numOfCallsLeft[ruleSetIndex] == 0)
                {
                    /* it is possible that another ruleset intends to invoke this one, but your
                     * number of calls for this set has hit its limit. */
                    host.GenerationStatus[ruleSetIndex] = GenerationStatuses.CycleLimit;
                    ruleSetIndex = nextRuleSet(ruleSetIndex, GenerationStatuses.CycleLimit);
                    SearchIO.output("cycle limit reached", 4);
                    continue;
                }

                #endregion

                #region ***** RECOGNIZE *****

                
                SearchIO.output("begin RCA loop for RuleSet #" + ruleSetIndex, 4);
                var options = Rulesets[ruleSetIndex].recognize(host.graph, InParallel);

                SearchIO.output("There are " + options.Count + " rule choices.", 4);
                if (options.Count == 0)
                {
                    /* There are no rules to recognize, exit here. */
                    host.GenerationStatus[ruleSetIndex] = GenerationStatuses.NoRules;
                    var newRSIndex = nextRuleSet(ruleSetIndex, GenerationStatuses.NoRules);
                    if (newRSIndex == ruleSetIndex)
                        throw new Exception("Same ruleset chosen when no rules are recognized.");
                    ruleSetIndex = newRSIndex;
                    continue;
                }
                if (SearchIO.GetTerminateRequest(Thread.CurrentThread.ManagedThreadId)) return;

                #endregion

                #region ***** CHOOSE *****

                if (Rulesets[ruleSetIndex].choiceMethod == choiceMethods.Automatic)
                    choice = new[] { 0 };
                else choice = choose(options, host);
                if (choice[0] == -1)
                {
                    host.undoLastRule();
                    if (Display)
                        SearchIO.addAndShowGraphWindow(host.graph.copy(),
                                                       "Revert to after calling " + host.numRulesCalled + " rules");
                    continue;
                }
                if ((choice == null) || (choice[0] < 0) || (choice[0] >= options.Count))
                {
                    SearchIO.output("Choice = #" + IntCollectionConverter.Convert(choice), 4);
                    /* the overloaded choice function may want to communicate to the loop that it
                     * should finish the process. */
                    SearchIO.output("Choice received a STOP request", 4);
                    host.GenerationStatus[ruleSetIndex] = GenerationStatuses.Choice;
                    ruleSetIndex = nextRuleSet(ruleSetIndex, GenerationStatuses.Choice);
                    continue;
                }
                if (SearchIO.GetTerminateRequest(Thread.CurrentThread.ManagedThreadId)) return;

                #endregion

                #region ***** APPLY *****
                host.saveCurrent();
                foreach (var c in choice)
                {
                    options[c].apply(host.graph, choose(options[c], host));
                    host.addToRecipe(options[c]);
                    SearchIO.output("Rule sucessfully applied", 4);
                }
                if (Display && Rulesets[ruleSetIndex].choiceMethod == choiceMethods.Design)
                    SearchIO.addAndShowGraphWindow(host.graph.copy(),
                                                   "After calling " + host.numRulesCalled + " rules");
                if (SearchIO.GetTerminateRequest(Thread.CurrentThread.ManagedThreadId)) return;

                #endregion

                #region Check to see if loop is done

                /* First thing we do is reduce the number of calls left. Note that if you start with
                 * a negative number, the process will continue to make it more negative - mimicking
                 * no cycle limit. It is safer to use the globalvar, maxRulesToApply though.*/
                if (this is LindenmayerChooseRCA) numOfCallsLeft[ruleSetIndex]--;
                else numOfCallsLeft[ruleSetIndex] = numOfCallsLeft[ruleSetIndex] - choice.GetLength(0);

                if (choice.Any(c => (options[c].ruleNumber == Rulesets[ruleSetIndex].TriggerRuleNum)))
                {
                    /* your ruleset loops until a trigger rule and the trigger rule was just called. */
                    SearchIO.output("The trigger rule has been chosen.", 4);
                    host.GenerationStatus[ruleSetIndex] = GenerationStatuses.TriggerRule;
                    ruleSetIndex = nextRuleSet(ruleSetIndex, GenerationStatuses.TriggerRule);
                }
                else
                {
                    /* Normal operation */
                    SearchIO.output("RCA loop executed normally.", 4);
                    host.GenerationStatus[ruleSetIndex] = GenerationStatuses.Normal;
                    ruleSetIndex = nextRuleSet(ruleSetIndex, GenerationStatuses.Normal);
                }

                #endregion
            }
        }


        /* A helper function to RecognizeChooseApplyCycle. This function returns what the new ruleSet
         * will be. Here the enumerator nextGenerationSteps and GenerationStatuses is used to great
         * affect. Understand that if a negative number is returned, the cycle will be stopped. */

        private int nextRuleSet(int ruleSetIndex, GenerationStatuses status)
        {
            switch (Rulesets[ruleSetIndex].nextGenerationStep[(int)status])
            {
                case nextGenerationSteps.Loop:
                    return ruleSetIndex;
                case nextGenerationSteps.GoToNext:
                    return ++ruleSetIndex;
                case nextGenerationSteps.GoToPrevious:
                    return --ruleSetIndex;
                default:
                    return (int)Rulesets[ruleSetIndex].nextGenerationStep[(int)status];
            }
        }

        #endregion

        #region Invoking the RecognizeChooseApplyCycle

        /// <summary>
        ///   Calls  one rule on candidate - the rule is determined by the choose function.
        /// </summary>
        /// <param name = "cand">The cand.</param>
        /// <param name = "startingRuleSet">The starting rule set.</param>
        /// <returns></returns>
        public virtual candidate CallOneRuleOnCandidate(candidate cand = null, int startingRuleSet = -1)
        {
            if (cand == null) cand = Host;
            if (startingRuleSet == -1) startingRuleSet = cand.activeRuleSetIndex;
            /* the RecognizeChooseApplyCycle requires an array of ruleSet limits,
             * since we only intend to make one call on the activeRuleSet we make
             * an array (it should initialize to all zeros) of the proper length
             * and set its one value at the activeRuleSetIndex to 1. */
            var numOfCalls = new int[NumOfRuleSets];
            numOfCalls[cand.activeRuleSetIndex] = 1;

            var newCand = cand.copy();
            /* here the main cycle is invoked. First, we must pass a copy of the candidate
             * to the RCA cycle since the apply set will modify it, and then move the prevoius
             * state onto the prevStates under the candidate. It is not incorrect to state
             * merely the candidate here, but the prevStates will not be stored correctly.*/
            RecognizeChooseApplyCycle(newCand, startingRuleSet, numOfCalls);
            return newCand;
        }

        /// <summary>
        ///   Generates all neighbors of the current.
        /// </summary>
        /// <param name = "current">The current.</param>
        /// <param name = "IncludingParent">if set to <c>true</c> [including parent].</param>
        /// <param name = "MaxNumber">The max number.</param>
        /// <returns></returns>
        public virtual List<candidate> GenerateAllNeighbors(candidate current=null, Boolean IncludingParent = false,
                                                            int MaxNumber = int.MaxValue)
        {
            if (current == null) current = Host;
            var rand = new Random();
            var neighbors = new List<candidate>();
            var options = Rulesets[current.activeRuleSetIndex].recognize(current.graph, InParallel);
            while (MaxNumber < options.Count)
            {
                var i = rand.Next(options.Count);
                options.RemoveAt(i);
            }
            if (IncludingParent)
            {
                var parent = current.copy();
                parent.undoLastRule();
                neighbors.Add(parent);
            }
            if (InParallel)
                Parallel.ForEach(options, opt =>
                                                  {
                                                      var child = current.copy();
                                                      SearchProcess.transferLmappingToChild(child.graph, current.graph, opt);
                                                      opt.apply(child.graph, null);
                                                      child.addToRecipe(opt);
                                                      neighbors.Add(child);
                                                  });
            else foreach (var opt in options)
                {
                    var child = current.copy();
                    SearchProcess.transferLmappingToChild(child.graph, current.graph, opt);
                    opt.apply(child.graph, null);
                    child.addToRecipe(opt);
                    neighbors.Add(child);
                }
            return neighbors;
        }

        /// <summary>
        ///   Generates one candidate. A simple function for invoking the RecognizeChooseApplyCycle.
        ///   That function is protected so we invoke it through a function like this.
        /// </summary>
        /// <param name = "cand">The cand to build upon (if null, then the seed will be used).</param>
        /// <param name = "startingRuleSet">The starting rule set (if unspecified then the candidate's active ruleset will be used.</param>
        /// <returns></returns>
        public virtual candidate GenerateOneCandidate(candidate cand = null, int startingRuleSet = -1)
        {
            if (cand == null) cand = Host;
            if (startingRuleSet == -1) startingRuleSet = cand.activeRuleSetIndex;
            var numOfCalls = (int[])MaxNumOfCalls.Clone();
            /* this copy set is needed because array are reference types and the RCA cycle
             * will modify the numOfCalls inside of it. */

            var newCand = cand.copy();
            RecognizeChooseApplyCycle(newCand, startingRuleSet, numOfCalls);
            return newCand;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RecognizeChooseApply" /> class.
        /// </summary>
        /// <param name="host">The _seed.</param>
        /// <param name="rulesets">The _rulesets.</param>
        /// <param name="maxNumOfCalls">The _max num of calls.</param>
        /// <param name="display">if set to <c>true</c> [_display].</param>
        /// <param name="inParallel">The process should be invoked in parallel where possible.</param>
        protected RecognizeChooseApply(designGraph host, ruleSet[] rulesets, int[] maxNumOfCalls = null, Boolean display = false,
            Boolean inParallel=true)
        {
            SearchIO.output("initializing RCA generation:", 4);
            Rulesets = (ruleSet[]) rulesets.Clone();
            NumOfRuleSets = rulesets.GetLength(0);
            SearchIO.output("There are " + NumOfRuleSets + " rule sets.", 4);
            Host = new candidate(host, NumOfRuleSets);
            SearchIO.output("Host = " + host.name, 4);

            MaxNumOfCalls = new int[rulesets.GetLength(0)];
            for (var i = 0; i < this.MaxNumOfCalls.GetLength(0); i++)
                if (maxNumOfCalls == null) this.MaxNumOfCalls[i] = -1;
                else if (maxNumOfCalls.GetLength(0) <= i) this.MaxNumOfCalls[i] = maxNumOfCalls[0];
                else this.MaxNumOfCalls[i] = maxNumOfCalls[i];

            Display = display;
            SearchIO.output("It is " + display + " that the SearchIO will be displayed.", 4);

            InParallel = inParallel;
        }

        #endregion


        #region Handling Choose Functionality
        /* Here we outline what an inherited class must contain. Basically it should have 
         * methods for the 2 types of decisions that are made - decisions on what option
         * to invoke and decisions for the variables required for the process. */

        /// <summary>
        ///   Chooses the specified options. Given the list of options and the candidate, 
        ///   determine what option to invoke. Return the integer index of this option from the list.
        /// </summary>
        /// <param name = "options">The options.</param>
        /// <param name = "cand">The cand.</param>
        /// <returns></returns>
        public abstract int[] choose(List<option> options, candidate cand);

        /// <summary>
        ///   Gets or sets the integer choice which is used in the main loop as well as in 
        ///   various implementation of choose. As a result we define it as global to the class.
        /// </summary>
        /// <value>The choice.</value>
        protected int[] choice { get; set; }

        /// <summary>
        ///   Chooses the specified option. Given that the rule has now been chosen, determine
        ///   the values needed by the rule to properly apply it to the candidate, cand. The 
        ///   array of doubles is to be determined by parametric apply rules written in 
        ///   complement C# files for the ruleSet being used.
        /// </summary>
        /// <param name = "opt">The opt.</param>
        /// <param name = "cand">The cand.</param>
        /// <returns></returns>
        public virtual double[] choose(option opt, candidate cand)
        {
            var unusedArraryOfParameters = new double[0];
            foreach (var a in unusedArraryOfParameters)
                opt.parameters.Add(a);
            /* this foreach statement does nothing here, but should be included in order to
             * put the parameters into the recipe. */
            return unusedArraryOfParameters;
        }

        #endregion
    }
}