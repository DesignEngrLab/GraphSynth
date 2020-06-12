using System;
/*************************************************************************
 *     This file includes definitions for fundamental enumerators and the
 *     OptimizeSort Comparer which are part of the GraphSynth.BaseClasses 
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
using System.Collections.Generic;


namespace GraphSynth
{
    /// <summary>
    /// Defines whether the choice method of a particular ruleset is done
    /// by some design agent (human or computer) or is automatic - meaning 
    /// once a rule is found to be recognized on a host, it is invoked.
    /// </summary>
    public enum choiceMethods
    {
        /// <summary>
        /// A set of options are first defined by an exhaustive recognition
        /// of all rules in the ruleset. The decision of which option to 
        /// choose is left to some design agent.
        /// </summary>
        Design,
        /// <summary>
        /// Whenever a rule is recognized it is invoked. Rules invoked in 
        /// the order presented in the ruleset.
        /// </summary>
        Automatic
    };

    /// <summary>
    /// Defines whether the candidates created by a particular ruleset 
    /// are feasible candidates and hence ready for evaluation, or
    /// developing candidates which are yet to completed.
    /// </summary>
    public enum feasibilityState
    {
        /// <summary/>
        Unspecified,
        /// <summary>
        /// Candidates are not yet complete, they are still 
        /// developing; not ready for evaluation.
        /// </summary>
        Developing,
        /// <summary>
        /// Candidates are feasible and ready for evaluation.
        /// </summary>
        Feasible
    };

    /// <summary>
    /// Defines how the generation process is to continue.
    /// </summary>
    public enum nextGenerationSteps
    {
        /// <summary />
        Unspecified = -5,
        /// <summary>
        /// stop the generation process
        /// </summary>
        Stop = -4,
        /// <summary>
        /// loop within current ruleset
        /// </summary>
        Loop = -3,
        /// <summary>
        /// go to the previous ruleset
        /// </summary>
        GoToPrevious = -2,
        /// <summary>
        /// go to the next ruleset
        /// </summary>
        GoToNext = -1,
        /// <summary>
        /// go to ruleset #0
        /// </summary>
        GoToRuleSet0 = 0,
        /// <summary>
        /// go to ruleset #1
        /// </summary>
        GoToRuleSet1 = 1,
        /// <summary>
        /// go to ruleset #2
        /// </summary>
        GoToRuleSet2 = 2,
        /// <summary>
        /// go to ruleset #3
        /// </summary>
        GoToRuleSet3 = 3,
        /// <summary>
        /// go to ruleset #4
        /// </summary>
        GoToRuleSet4 = 4,
        /// <summary>
        /// go to ruleset #5
        /// </summary>
        GoToRuleSet5 = 5,
        /// <summary>
        /// go to ruleset #6
        /// </summary>
        GoToRuleSet6 = 6,
        /// <summary>
        /// go to ruleset #7
        /// </summary>
        GoToRuleSet7 = 7,
        /// <summary>
        /// go to ruleset #8
        /// </summary>
        GoToRuleSet8 = 8,
        /// <summary>
        /// go to ruleset #9
        /// </summary> 
        GoToRuleSet9 = 9,
        /// <summary>
        /// go to ruleset #10
        /// </summary>
        GoToRuleSet10 = 10
    };

    /// <summary>
    /// Enumerator Declaration for How Generation Ended, GenerationStatus 
    /// </summary>
    public enum GenerationStatuses
    {
        /// <summary />
        Unspecified = -1,
        /// <summary>
        /// Following a normal cycle through the RCA loop.
        /// </summary>
        Normal,
        /// <summary>
        /// Following the choosing step of the RCA loop.
        /// </summary>
        Choice,
        /// <summary>
        /// Following the a maximum number of cycle through the RCA loop.
        /// </summary>
        CycleLimit,
        /// <summary>
        /// Following no rules having been recognized.
        /// </summary>
        NoRules,
        /// <summary>
        /// Following the application of a trigger rule.
        /// </summary>
        TriggerRule
    };

    ///<summary>
    /// Enumerator for Search functions that have generality
    /// to either minimize or maximize (e.g. PNPPS, stochasticChoose). */
    ///</summary>
    public enum optimize
    {
        /// <summary>
        /// Minimize in the search - smaller is better.
        /// </summary>
        minimize = -1,
        /// <summary>
        /// Maximize in the search - bigger is better.
        /// </summary>
        maximize = 1
    };


    /// <summary>
    /// Calculating the confluence between options is a complex task which may take an
    /// unintended amount of time to determine. In order to control this three possible
    /// analyses are defined. 
    /// </summary>
    public enum ConfluenceAnalysis
    {        
        /// <summary>
        /// A simple analysis that may produce a number of "unknown" states. Any unknown
        /// states are regarded as NOT confluent, even though they may be.
        /// </summary>
        ConservativeSimple,
        /// <summary>
        /// A simple analysis that may produce a number of "unknown" states. Any unknown
        /// states are regarded as confluent, even though they may not be.
        /// </summary>
        OptimisticSimple,
        /// <summary>
        /// The full analysis will run the empirical test for invalidation between a pair
        /// of options. This is potentially time-consuming.
        /// </summary>
        Full
    };


    /// <summary>
    /// A comparer for optimization that can be used for either 
    /// minimization or maximization.
    /// </summary>
    public class OptimizeSort : IComparer<double>
    {
        /* an integer equal to the required sort direction. */
        private readonly Boolean AllowEqualInSort;
        private readonly int direction;
        /* if using with SortedList, set AllowEqualInSorting to false, otherwise
         * it will crash when equal values are encountered. If using in Linq's 
         * OrderBy then the equal is need (AllowEqualInSorting = true) otherwise
         * the program will hang. */

        /// <summary>
        /// Initializes a new instance of the <see cref="OptimizeSort"/> class.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="AllowEqualInSort">The allow equal in sort.</param>
        public OptimizeSort(optimize direction, Boolean AllowEqualInSort = false)
        {
            this.direction = (int)direction;
            this.AllowEqualInSort = AllowEqualInSort;
        }


        #region IComparer<double> Members

        /// <summary>
        /// Compares two objects and returns a value indicating whether the 
        /// first one is better than the second one. "Better than" is defined
        /// by the optimize direction provided in the constructor. 
        /// </summary>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, 
        /// as shown in the following table. 
        /// Value | Meaning 
        /// Less than zero | <paramref name="x"/> is less than <paramref name="y"/>.
        /// Zero | <paramref name="x"/> equals <paramref name="y"/>.
        /// Greater than zero | <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
        public int Compare(double x, double y)
        {
            if (AllowEqualInSort && (x == y)) return 0;
            /* in order to avoid the collections from throwing an error, we make sure
             * that only -1 or 1 is returned. If they are equal, we return +1 (when
             * minimizing). This makes newer items to the list appear before older items.
             * It is slightly more efficient than returning -1 and conforms with the 
             * philosophy of always exploring/preferring new concepts. See: SA's Metropolis Criteria. */

            if (x < y) return direction;
            return -1 * direction;
        }
        #endregion
        /// <summary>
        /// Is x betters the than y?
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        public Boolean BetterThan(double x, double y)
        {
            return (-1 == Compare(x, y));
        }
    }


    /// <summary>
    /// Defines the constraint on how shapes/coordinates are transformed. 
    /// </summary>
    public enum transfromType
    {
        /// <summary>
        /// This type of transform is not recognized/performed.
        /// </summary>
        Prohibited=0,
        /// <summary>
        /// This type of transform is recognized/performed only in the X-direction.
        /// </summary>
        OnlyX = 1,
        /// <summary>
        /// This type of transform is recognized/performed only in the Y-direction.
        /// </summary>
        OnlyY = 2,
        /// <summary>
        /// This type of transform is recognized/performed only in the Z-direction.
        /// </summary>
        OnlyZ = 3,
        /// <summary>
        /// This type of transform is recognized/performed uniformly in X, Y, and Z.
        /// </summary>  
        XYZUniform = 4,
        /// <summary>        
        /// Deprecated. The type of transform is recognized/performed uniformly in BOTH X and Y (should use XYZUniform).
        /// </summary>
        BothUniform = 4,
        /// <summary>
        /// This type of transform is recognized/performed independently in X, Y, and Z.
        /// </summary>    
        XYZIndependent = 5,
        /// <summary>        
        /// Deprecated. The type of transform is recognized/performed independently in BOTH X and Y (should use XYZIndependent).
        /// </summary>
        BothIndependent = 5,
    };
}
