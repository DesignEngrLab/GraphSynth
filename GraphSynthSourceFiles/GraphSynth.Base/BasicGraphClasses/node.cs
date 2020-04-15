/*************************************************************************
 *     This node file & class is part of the GraphSynth.BaseClasses Project
 *     which is the foundation of the GraphSynth Application.
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
    /* in order to "show" xml serialization that these are inherited from node and arc,
     * we need to add these two XmlInclude lines to the declaration of node and arc.
     * If you are developing your own inherited classes, remember to add this. NOTE: there
     * sometimes can be a problem with compilation if the compilers sees this before
     * it sees node and arc (or something like that - might also be interference with 
     * ruleNode and ruleArc. */

    /// <summary>
    ///   One of the two basic classes for a graph is the node or vertex.
    /// </summary>
    [XmlInclude(typeof(vertex))]
    [XmlInclude(typeof(ruleNode))]
    public class node : graphElement
    {
        #region Fields & Properties

        #region List of arcs connected to the node

        /// <summary />
        protected List<graphElement> _arcs;

        /// <summary>
        ///   Gets the arcs connected to the node. This includes both arcs and hyperArcs.
        /// </summary>
        /// <value>The arcs.</value>
        [XmlIgnore]
        public List<graphElement> arcs
        {
            get { return _arcs ?? (_arcs = new List<graphElement>()); }
        }

        /* additionally these are divided into arcs coming into the 
         * node - those in which the head or TO of the arc connects 
         * to the node (arcsto), and those leaving the node, tail of 
         * arc, FROM of the arc . */

        /// <summary>
        ///   Gets the arcs entering this node - head (or to) of the
        ///   arc is connected to this node.
        /// </summary>
        /// <value>The arcs to.</value>
        [XmlIgnore]
        public List<arc> arcsTo
        {
            get
            {
                return arcs.Where(a => (a is arc)
                    && ((arc)a).To == this)
                    .Cast<arc>().ToList();
            }
        }

        /// <summary>
        ///   Gets the arcs leaving this node - tail (or from) of the
        ///   arc is connected to this node.
        /// </summary>
        /// <value>The arcs from.</value>
        [XmlIgnore]
        public List<arc> arcsFrom
        {
            get
            {
                return arcs.Where(a => (a is arc) && ((arc)a).From == this)
                    .Cast<arc>().ToList();
            }
        }

        /* The decision to ignore these  in the XML is to make the 
         * xml-file more compact, and avoid infinite loops in the 
         * (de-)serialization. The arcs will contain to To and From 
         * nodes to indicate how the graph is connected. */

        #endregion

        /* In an effort to move towards shape grammars, I have decided to make the X, Y, and
         * Z positions of a node permanent members of the node class. This transition will not
         * affect any existing graph grammars, but will allow future graph grammars to take 
         * advantage of relative positioning of new nodes. Additionally, it solves the problem
         * of getting X, Y, and Z into the ruleNode class. */

        /// <summary>
        ///   Gets or sets the X coordinate.
        /// </summary>
        /// <value>The X coordinate.</value>
        public double X { get; set; }

        /// <summary>
        ///   Gets or sets the Y coordinate.
        /// </summary>
        /// <value>The Y coordinate.</value>
        public double Y { get; set; }

        /// <summary>
        ///   Gets or sets the Z coordinate.
        /// </summary>
        /// <value>The Z coordinate.</value>
        public double Z { get; set; }


        /// <summary>
        ///   Gets the degree. The degree or valence of a node is the number of arcs connecting to it.
        ///   Currently this is used in recognition of a rule when the strictDegreeMatch is checked.
        /// </summary>
        /// <value>The degree.</value>
        public int degree
        {
            get { return arcs.Count(a => (a is arc)); }
        }
        #endregion

        #region Constructors

        /* either make new node with a prescribed name, or give it a name never seen before. */
        /// <summary>
        /// Initializes a new instance of the <see cref="node"/> class.
        /// </summary>
        public node() : this("n") { }
        /// <summary>
        ///   Initializes a new instance of the <see cref = "node" /> class.
        /// </summary>
        /// <param name = "newName">The new name.</param>
        public node(string newName)
        {
            name = newName;
        }



        #endregion

        #region Copy Method

        /// <summary>
        ///   Copies this instance.
        /// </summary>
        /// <returns></returns>
        public virtual node copy()
        {
            var copyOfNode = new node();
            copy(copyOfNode);
            return copyOfNode;
        }

        /// <summary>
        ///   Copies the specified copy of node.
        /// </summary>
        /// <param name = "copyOfNode">The copy of node.</param>
        public virtual void copy(node copyOfNode)
        {
            base.copy(copyOfNode);

            copyOfNode.X = X;
            copyOfNode.Y = Y;
            copyOfNode.Z = Z;
        }

        #endregion
    }

    /// <summary>
    ///   Originally, I created a separate edge and vertex class to allow for the future expansion
    ///   of GraphSynth into shape grammars. I now have decided that the division is not useful, 
    ///   since it simply deprived nodes of X,Y,Z positions. Many consider edge and arc, and vertex
    ///   and node to be synonymous anyway but I prefer to think of edges and vertices as arcs and 
    ///   nodes with spatial information. At any rate there is no need to have these inherited 
    ///   classes, but I keep them for backwards-compatible purposes.
    /// </summary>
    public class vertex : node
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref = "vertex" /> class.
        /// </summary>
        /// <param name = "name">The name.</param>
        public vertex(string name)
            : base(name)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "vertex" /> class.
        /// </summary>
        public vertex()
        {
        }

        /// <summary>
        ///   Copies this instance.
        /// </summary>
        /// <returns></returns>
        public override node copy()
        {
            var copyOfVertex = new vertex(name);
            base.copy(copyOfVertex);
            return copyOfVertex;
        }
    }
}