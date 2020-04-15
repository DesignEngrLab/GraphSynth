/*************************************************************************
 *     This ruleNode file & class is part of the GraphSynth.BaseClasses 
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
    /* here we define additional qualities used only by nodes in the grammar rules. */

    /// <summary>
    ///   The ruleNode class is an inherited class from node which includes additional details
    ///   necessary to correctly perform recognition. This mostly hinges on the "subset or equal"
    ///   Booleans.
    /// </summary>
    public class ruleNode : node
    {
        #region Constructors and Copy

        /// <summary>
        ///   Initializes a new instance of the <see cref = "ruleNode" /> class with a particular name.
        /// </summary>
        /// <param name = "newName">The new name.</param>
        public ruleNode(string newName)
            : base(newName)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "ruleNode" /> class.
        /// </summary>
        public ruleNode()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ruleNode"/> class.
        /// Up-casts the node to a ruleNode and returns it with default Booleans.
        /// The original node is unaffected.
        /// </summary>
        /// <param name="n">The node.</param>
        public ruleNode(node n)
            : this(n.name)
        {
            DisplayShape = n.DisplayShape;
            TargetType = n.GetType().ToString();
            localLabels.AddRange(n.localLabels);
            localVariables.AddRange(n.localVariables);
            X = n.X;
            Y = n.Y;
            Z = n.Z;
        }

        /// <summary>
        ///   Returns a copy of this instance.
        /// </summary>
        /// <returns></returns>
        public override node copy()
        {
            var copyOfNode = new ruleNode();
            copy(copyOfNode);
            return copyOfNode;
        }

        /// <summary>
        ///   Copies this instance into the (already intialized) copyOfNode.
        /// </summary>
        /// <param name = "copyOfNode">A new copy of node.</param>
        public override void copy(node copyOfNode)
        {
            base.copy(copyOfNode);
            if (copyOfNode is ruleNode)
            {
                var rcopy = (ruleNode)copyOfNode;
                rcopy.containsAllLocalLabels = containsAllLocalLabels;
                rcopy.strictDegreeMatch = strictDegreeMatch;
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

        #region Node Specific Conditional Attributes
        /// <summary>
        ///   Gets or sets a value indicating whether [strict degree match] is required for recognition.
        /// </summary>
        /// <value><c>true</c> if [strict degree match]; otherwise, <c>false</c>.</value>
        public Boolean strictDegreeMatch { get; set; }

        /* this boolean is to distinguish that a particular node
         * of L has all of the arcs of the host node. Again,
         * if true then use equal
         * if false then use subset 
         * NOTE: this is commonly misunderstood to be the same as induced. The difference is that this
         * applies to each node in the LHS and includes arcs that reference nodes not found on the LHS*/

        /* In GraphSynth 1.8, I added these to ruleNode, ruleArc, grammarRule (as global Negabels) and 
         * embedding rule (both for freeArc and NeighborNode) classes. This is a simple fix and useful in 
         * many domains. If the host item, contains a negabel then it is not a valid match. */

        /// <summary>
        /// Gets the degree. The degree or valence of a node is the number of arcs connecting to it.
        /// Currently this is used in recognition of a rule when the strictDegreeMatch is checked.
        /// A slight difference exists for ruleNode since we don't want to count "NotExist" arcs.
        /// </summary>
        /// <value>The degree.</value>
        public new int degree
        {
            get
            {
                return arcs.Count(a =>
                               //   ((a is ruleHyperarc) && ((ruleHyperarc)a).MustExist) ||
                                    ((a is ruleArc) && ((ruleArc)a).MustExist));
            }
        }
        #endregion
    }
}