/*************************************************************************
 *     This ruleHyperarc file & class is part of the GraphSynth.BaseClasses 
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

namespace GraphSynth.Representation
{
    /* here we define additional qualities used only by hyperarcs in the grammar rules. */

    /// <summary>
    ///   The ruleHyperArc class is an inherited class from hyperarc which includes additional details
    ///   necessary to correctly perform recognition. This mostly hinges on the "subset or equal"
    ///   Booleans.
    /// </summary>
    public class ruleHyperarc : hyperarc
    {
        #region Constructors & Copy

        /// <summary>
        ///   Initializes a new instance of the <see cref = "ruleHyperarc" /> class.
        /// </summary>
        /// <param name = "newName">The new name.</param>
        public ruleHyperarc(string newName)
            : base(newName)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "ruleHyperarc" /> class.
        /// </summary>
        public ruleHyperarc()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ruleHyperarc"/> class.
        /// Converts a hyperarc to a ruleHyperArc and returns it with default Booleans.
        /// The original hyperarc is unaffected.
        /// </summary>
        /// <param name="ha">The hyperarc, ha.</param>
        /// <returns></returns>
        public ruleHyperarc(hyperarc ha)
            : this(ha.name)
        {
            DisplayShape = ha.DisplayShape;
            TargetType = ha.GetType().ToString();
            localLabels.AddRange(ha.localLabels);
            localVariables.AddRange(ha.localVariables);
            nodes.AddRange(ha.nodes);
        }
        /// <summary>
        ///   Returns a copy of this instance.
        /// </summary>
        /// <returns>the copy of the arc.</returns>
        public override hyperarc copy()
        {
            var copyOfNode = new ruleHyperarc();
            copy(copyOfNode);
            return copyOfNode;
        }

        /// <summary>
        ///   Copies this instance into the (already intialized) copyOfHyperArc.
        /// </summary>
        /// <param name = "copyOfHyperArc">The copy of node.</param>
        public override void copy(hyperarc copyOfHyperArc)
        {
            base.copy(copyOfHyperArc);
            if (copyOfHyperArc is ruleHyperarc)
            {
                var rcopy = (ruleHyperarc)copyOfHyperArc;
                rcopy.containsAllLocalLabels = containsAllLocalLabels;
                rcopy.strictNodeCountMatch = strictNodeCountMatch;
                foreach (var label in negateLabels)
                    rcopy.negateLabels.Add(label);
            }
        }

        #endregion

        #region Conditional Attributes (of all graph elements)
        /// <summary>
        ///   Gets the negating labels. The labels that must not exist for correct recognition.
        /// </summary>
        /// <value>The negate labels.</value>
        public List<string> negateLabels
        {
            get { return _negateLabels ?? (_negateLabels = new List<string>()); }
        }
        private List<string> _negateLabels;

        /// <summary>
        /// Gets or sets a value indicating whether the element should not exist in the
        /// host graph.
        /// </summary>
        /// <value><c>true</c> if [not exist]; otherwise, <c>false</c>.</value>
        public Boolean NotExist { get; set; }


        /// <summary>
        /// Gets the value indicating whether the element SHOULD exist in the
        /// host graph. It is just the opposite (true/false) or NotExist.
        /// </summary>
        /// <value><c>true</c> if [not exist]; otherwise, <c>false</c>.</value>
        public Boolean MustExist
        {
            get { return !NotExist; }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether arc must contain all the local labels of the matching element.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [contains all local labels]; otherwise, <c>false</c>.
        /// </value>
        public Boolean containsAllLocalLabels { get; set; }
        /* if true then all the localLabels in the rule element much match with those in the host 
         * element, if false then the rule element labels only need to be a subset on host elt. localLabels. */

        /// <summary>
        ///   Gets or sets the type (as a string) for the matching graph element.
        /// </summary>
        /// <value>The string describing the type of graph element.</value>
        public string TargetType
        {
            get { return _targetType; }
            set
            {
                Type t = null;
                if (value != null) t = Type.GetType(value);
                /* if the user typed a Type but we can't find it, it is likely that
                 * * it is being compiled within GraphSynth, so prepend with various
                 * * namespaces. */
                if (t == null)
                    t = Type.GetType("GraphSynth." + value);
                if (t == null)
                    t = Type.GetType("GraphSynth.Representation." + value);
                if (t != null)
                    _targetType = t.ToString();
                else _targetType = value;
                //    throw new Exception("The Type: "+value+ " is not known.");
            }
        }
        private string _targetType = "";
        #endregion
        #region Hyperarc Specific Conditional Attributes
        /// <summary>
        /// Gets or sets a value indicating whether [strict node count match].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [strict node count match]; otherwise, <c>false</c>.
        /// </value>
        public Boolean strictNodeCountMatch { get; set; }

        /* this boolean is to distinguish that a particular hyperarc
         * of L has all of the nodes of the host hyperarc. Again,
         * if true then use equal if false then use subset */

        /// <summary>
        /// Gets the degree of the hyperarcs - the number of nodes that it connects to.
        ///  A slight difference exists for ruleNode since we don't want to count "NotExist" arcs.
        /// </summary>
        /// <value>The degree.</value>
        public new int degree
        {
            get
            {
                return nodes.Count(n => ((ruleNode)n).MustExist);
            }
        }
        #endregion
    }
}