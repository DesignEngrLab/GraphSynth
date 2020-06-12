/*************************************************************************
 *     This ruleArc file & class is part of the GraphSynth.BaseClasses 
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

namespace GraphSynth.Representation
{
    /* here we define additional qualities used only by arcs in the grammar rules. */

    /// <summary>
    ///   The ruleArc class is an inherited class from arc which includes additional details
    ///   necessary to correctly perform recognition. This mostly hinges on the "subset or equal"
    ///   Booleans.
    /// </summary>
    public class ruleArc : arc
    {
        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "ruleArc" /> class with a particular name.
        /// </summary>
        /// <param name = "newName">The new name.</param>
        public ruleArc(string newName)
            : base(newName)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "ruleArc" /> class.
        /// </summary>
        public ruleArc()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ruleArc"/> class.
        ///   Converts an arc to a ruleArc and returns it with default Booleans.
        ///   The original arc is unaffected.
        /// </summary>
        /// <param name="a">A.</param>
        public ruleArc(arc a)
            : this(a.name)
        {
            TargetType = a.GetType().ToString();
            directed = a.directed;
            DisplayShape = a.DisplayShape;
            doublyDirected = a.doublyDirected;
            From = a.From;
            To = a.To;
            localLabels.AddRange(a.localLabels);
            localVariables.AddRange(a.localVariables);
        }

        /// <summary>
        ///   Returns a copy of this instance.
        /// </summary>
        /// <returns>the copy of the arc.</returns>
        public override arc copy()
        {
            var copyOfArc = new ruleArc();
            copy(copyOfArc);
            return copyOfArc;
        }

        /// <summary>
        ///   Copies this instance into the (already intialized) copyOfArc.
        /// </summary>
        /// <param name = "copyOfArc">A new copy of arc.</param>
        public override void copy(arc copyOfArc)
        {
            base.copy(copyOfArc);
            if (copyOfArc is ruleArc)
            {
                var rcopy = (ruleArc)copyOfArc;
                rcopy.containsAllLocalLabels = containsAllLocalLabels;
                rcopy.directionIsEqual = directionIsEqual;
                rcopy.nullMeansNull = nullMeansNull;
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
        public Boolean MustExist {
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

        #region Arc Specific Conditional Attributes
        /* The following booleans capture the possible ways in which an arc may/may not be a subset 
         * (boolean set to false) or is equal (in this respective quality) to the host (boolean set 
         * to true). These are special subset or equal booleans used by recognize. For this 
         * fundamental arc classes, only these three possible conditions exist. */

        /// <summary>
        ///   Gets or sets a value indicating whether the directionality within the arc is to match
        ///   perfectly. If false then all (singly)-directed arcs
        ///   will match with doubly-directed arcs, and all undirected arcs will match with all
        ///   directed and doubly-directed arcs. Of course, a directed arc going one way will 
        ///   still not match with a directed arc going the other way.
        ///   If true, then undirected only matches with undirected, directed only with directed (again, the
        ///   actual direction must match too), and doubly-directed only with doubly-directed.
        /// </summary>
        /// <value><c>true</c> if [direction is equal]; otherwise, <c>false</c>.</value>
        public Boolean directionIsEqual { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether dangling (the null reference to node) arc are only
        ///   to match with dangling arcs.If this is set to false, then we are saying a 
        ///   null reference on an arc can be matched with a null in the graph or any node in the graph. 
        ///   Like the above, a false value is like a subset in that null is a subset of any actual node. 
        ///   And a true value means it must match exactly or in otherwords, "null means null" - null 
        ///   matches only with a null in the host. If you want the rule to be recognized only when an actual
        ///   node is present simply add a dummy node with no distinguishing characteristics. That would
        ///   in turn nullify this boolean since this boolean only applies when a null pointer exists in
        ///   the rule.
        /// </summary>
        /// <value><c>true</c> if [null means null]; otherwise, <c>false</c>.</value>
        public Boolean nullMeansNull { get; set; }
        #endregion
    }
}