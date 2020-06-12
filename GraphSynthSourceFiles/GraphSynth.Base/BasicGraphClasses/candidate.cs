/*************************************************************************
 *     This candidate file & class is part of the GraphSynth.BaseClasses
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
using System.Linq;
using System.Xml.Serialization;

namespace GraphSynth.Representation
{
    /// <summary>
    ///   The candidate class is a wrapper to designGraph. While the graph is
    ///   essentially what we are interested in, the candidate also includes
    ///   some other essential information. For example, what is the worth
    ///   of the graph (performance parameters), and what is the recipe, or
    ///   list of options that were called to create the graph (recipe).
    /// </summary>
    public class candidate
    {
        #region Constructor

        /* a candidate can be made with nothing or by passing the graph that will be set
         * to its current state. */

        /// <summary>
        ///   Initializes a new instance of the <see cref = "candidate" /> class.
        /// </summary>
        public candidate()
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "candidate" /> class.
        /// </summary>
        /// <param name = "_graph">The _graph.</param>
        /// <param name = "numRuleSets">The num rule sets.</param>
        public candidate(designGraph _graph, int numRuleSets)
        {
            graph = _graph;
            for (var i = 0; i != numRuleSets; i++)
                GenerationStatus.Add(GenerationStatuses.Unspecified);
        }

        #endregion

        #region Fields & Properties
        /// <summary>
        /// All the previous states of the graph are stored within a candidate. This makes candidate a
        ///   'heavy' class, but it allows us to go back to how it existed quickly.
        /// </summary>
        [XmlIgnore]
        public readonly List<designGraph> prevStates = new List<designGraph>();

        /// <summary>
        ///   Just like the discussion for activeRuleSetIndex, GenerationStatus stores what has
        ///   happened during the RCA generation loop. The is one for each ruleSet as each ruleSet
        ///   may have ended in a different way.
        /// </summary>
        public List<GenerationStatuses> GenerationStatus = new List<GenerationStatuses>();


        /// <summary>
        ///   a list of numbers used to define a candidate's worth. While this is a public field, 
        ///   it may be less buggy to write code using the properties f0, f1, f2, f3, and f4 stored
        /// </summary>
        public List<double> performanceParams = new List<double>();

        /// <summary>
        ///   the recipe is a list of all the options that were chosen to create the candidate.
        ///   Option is stored under representation. Each option contains, the rulesetindex,
        ///   the number of the rule, a reference to the rule, and the location of where the rule
        ///   was applied.
        /// </summary>
        public List<option> recipe = new List<option>();

        /// <summary>
        ///   Gets or sets the graph. Stating a candidate's graph is simply it's current state. 
        ///   However, if this property is used in to set the graph to a new one, then we move 
        ///   the current state onto the prevStates list.
        /// </summary>
        /// <value>The graph.</value>
        [XmlIgnore]
        public designGraph graph { get; set; }

        /// <summary>
        ///   Gets or sets the name of the graph file.
        /// </summary>
        /// <value>The name of the graph file.</value>
        [XmlElement("graph")]
        public string graphFileName { get; set; }

        /// <summary>
        ///   Gets or sets the index of the active rule set. the activeRuleSetIndex is set during 
        ///   the recognize->choose->apply generation. It is very similar to the candidate property, 
        ///   lastRuleSetIndex however in certain subtle yet important occasions the two will differ. 
        ///   This will happen if an RCA loop starts but doesn't complete apply. This happens if 
        ///   max number of calls is reached, if choice is STOP, or no rules are recognized.
        /// </summary>
        /// <value>The index of the active rule set.</value>
        public int activeRuleSetIndex { get; set; }

        /// <summary>
        ///   Gets or sets the age. This is an arbitrary value set by the search process. 
        ///   Likely it will be set to the # of iterations the candidate has existed in.
        /// </summary>
        /// <value>The age.</value>
        public int age { get; set; }


        /// <summary>
        ///   Gets the number of rules called (same of length of recipe.
        /// </summary>
        /// <value>The num rules called.</value>
        [XmlIgnore]
        public int numRulesCalled
        {
            get { return recipe.Count; }
        }

        /// <summary>
        ///   Gets the rule set index of the last option.
        /// </summary>
        /// <value>The last index of the rule set.</value>
        [XmlIgnore]
        public int lastRuleSetIndex
        {
            get
            {
                if (recipe.Count == 0) return -1;
                return recipe.Last().ruleSetIndex;
            }
        }

        /* the following five properties simply make the performance
         * parameters easier to code. we simply refer to the as f0, f1,
         * f2, f3, and f4. In rare cases will you need more than these. 
         * If they have yet to be defined, they return Not-A-Number 
         * (a quality that C# double understands). */

        /// <summary>
        ///   Gets or sets the first performance parameter.
        /// </summary>
        /// <value>The f0.</value>
        [XmlIgnore]
        public double f0
        {
            get
            {
                if (performanceParams.Count < 1)
                    return Double.NaN;
                return performanceParams[0];
            }
            set
            {
                if (performanceParams.Count < 1)
                    performanceParams.Add(value);
                else performanceParams[0] = value;
            }
        }

        /// <summary>
        ///   Gets or sets the second performance parameter.
        /// </summary>
        /// <value>The f1.</value>
        [XmlIgnore]
        public double f1
        {
            get
            {
                if (performanceParams.Count < 2)
                    return Double.NaN;
                return performanceParams[1];
            }
            set
            {
                if (performanceParams.Count < 2)
                {
                    f0 = f0;
                    performanceParams.Add(value);
                }
                else performanceParams[1] = value;
            }
        }

        /// <summary>
        ///   Gets or sets the third performance parameter.
        /// </summary>
        /// <value>The f2.</value>
        [XmlIgnore]
        public double f2
        {
            get
            {
                if (performanceParams.Count < 3)
                    return Double.NaN;
                return performanceParams[2];
            }
            set
            {
                if (performanceParams.Count < 3)
                {
                    f0 = f0;
                    f1 = f1;
                    performanceParams.Add(value);
                }
                else performanceParams[2] = value;
            }
        }

        /// <summary>
        ///   Gets or sets the fourth performance parameter.
        /// </summary>
        /// <value>The f3.</value>
        [XmlIgnore]
        public double f3
        {
            get
            {
                if (performanceParams.Count < 4)
                    return Double.NaN;
                return performanceParams[3];
            }
            set
            {
                if (performanceParams.Count < 4)
                {
                    f0 = f0;
                    f1 = f1;
                    f2 = f2;
                    performanceParams.Add(value);
                }
                else performanceParams[3] = value;
            }
        }

        /// <summary>
        ///   Gets or sets the fifth performance parameter.
        /// </summary>
        /// <value>The f4.</value>
        [XmlIgnore]
        public double f4
        {
            get
            {
                if (performanceParams.Count < 5)
                    return Double.NaN;
                return performanceParams[4];
            }
            set
            {
                if (performanceParams.Count < 5)
                {
                    f0 = f0;
                    f1 = f1;
                    f2 = f2;
                    f3 = f3;
                    performanceParams.Add(value);
                }
                else performanceParams[4] = value;
            }
        }


        /// <summary>
        ///   Gets the rule numbers in recipe as an array of integers.
        /// </summary>
        /// <value>The rule numbers in recipe.</value>
        [XmlIgnore]
        public int[] ruleNumbersInRecipe
        {
            get
            {
                var rNIR = new int[recipe.Count];
                for (var i = 0; i != recipe.Count; i++)
                    rNIR[i] = recipe[i].ruleNumber;
                return rNIR;
            }
        }

        /// <summary>
        ///   Gets the rule set indices in recipe as an array of integers.
        /// </summary>
        /// <value>The rule set indices in recipe.</value>
        [XmlIgnore]
        public int[] ruleSetIndicesInRecipe
        {
            get
            {
                var rSIIR = new int[recipe.Count];
                for (var i = 0; i != recipe.Count; i++)
                    rSIIR[i] = recipe[i].ruleSetIndex;
                return rSIIR;
            }
        }

        /// <summary>
        ///   Gets the option numbers in recipe as an array of integers.
        /// </summary>
        /// <value>The option numbers in recipe.</value>
        [XmlIgnore]
        public int[] optionNumbersInRecipe
        {
            get
            {
                var oNIR = new int[recipe.Count];
                for (var i = 0; i != recipe.Count; i++)
                    oNIR[i] = recipe[i].optionNumber;
                return oNIR;
            }
        }

        /// <summary>
        ///   Gets the parameter decisions of the recipe. This is a List the
        ///   same length as recipe, but each element is an array of double values.
        /// </summary>
        /// <value>The parameters in recipe.</value>
        [XmlIgnore]
        public List<double>[] parametersInRecipe
        {
            get
            {
                var pIR = new List<double>[recipe.Count];
                for (var i = 0; i != recipe.Count; i++)
                    if (recipe[i].parameters.Count > 0)
                        pIR[i].AddRange(recipe[i].parameters);
                return pIR;
            }
        }

        #endregion

        /// <summary>
        ///   a list of numbers used to define a candidate's design or decision variables. This is
        ///   typically used to define parameters within the graph.
        /// </summary>
        public List<double> designParameters = new List<double>();

        #region Misc Methods

        /// <summary>
        ///   Saves a copy of the current state to the list of previous states.
        /// </summary>
        public virtual void saveCurrent()
        {
            if (graph != null) prevStates.Add(graph.copy());
        }

        /// <summary>
        ///   Adds to recipe. This is called (currently only) from the RCA loop. This happens
        ///   directly after the rule is APPLIED. A rule application updates
        ///   the currentstate, so this correspondingly adds the option to the recipe.
        /// </summary>
        /// <param name = "currentOpt">The currentrule.</param>
        public virtual void addToRecipe(option currentOpt)
        {
            recipe.Add(currentOpt.copy());
        }


        /// <summary>
        ///   Undoes the last rule. This is perhaps the whole reason previous states are used.
        ///   Rules cannot be guaranteed to work in reverse as they work
        ///   forward, so this simply resets the candidate to how it looked
        ///   prior to calling the last rule.
        /// </summary>
        public virtual void undoLastRule()
        {
            if (prevStates.Count <= 0) return;
            activeRuleSetIndex = lastRuleSetIndex;
            graph = prevStates.Last();
            prevStates.RemoveAt(prevStates.Count - 1);
            recipe.RemoveAt(recipe.Count - 1);
            for (var i = 0; i != performanceParams.Count; i++)
                performanceParams[i] = double.NaN;
            age = 0;
        }


        /// <summary>
        ///   Copies this instance of a candidate. Very similar to designGraph copy.
        ///   We make sure to not do a shallow copy (ala Clone) since we are unsure
        ///   how each candidate may be changed in the future.
        /// </summary>
        /// <returns></returns>
        public virtual candidate copy()
        {
            var copyOfCand = new candidate
                                 {
                                     activeRuleSetIndex = activeRuleSetIndex,
                                     graph = graph.copy()
                                 };

            foreach (var d in prevStates)
                copyOfCand.prevStates.Add(d.copy());
            foreach (var opt in recipe) 
                copyOfCand.recipe.Add(opt.copy());
            foreach (var f in performanceParams)
                copyOfCand.performanceParams.Add(f);
            foreach (var f in designParameters)
                copyOfCand.designParameters.Add(f);
            foreach (var a in GenerationStatus)
                copyOfCand.GenerationStatus.Add(a);


            return copyOfCand;
        }

        #endregion
    }
}