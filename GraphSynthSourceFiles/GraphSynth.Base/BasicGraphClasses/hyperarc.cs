/*************************************************************************
 *     This hyperarc file & class is part of the GraphSynth.BaseClasses 
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
    ///   In the spring of 2010, it was decided to introduce a third basic element
    ///   to the graph: a hyperarc. A hyperarc is an arc that connects an arbitrary
    ///   number of nodes. It does not have a sense of direction like an arc (i.e. all
    ///   hyperarcs are undirected), it does not have a head/tail or to/from distinction
    ///   and it does not have a length. In a sense it has more in common with the node
    ///   than the arc - as such it was decided not to override the arc class.
    /// </summary>
    [XmlInclude(typeof(ruleHyperarc))]
    public class hyperarc : graphElement
    {
        #region Fields & Properties

        #region List of nodes connected to the hyperarc

        /* the list of nodes connecting to this hyperarc are stored here. */

        /// <summary />
        protected List<node> _nodes;

        /// <summary>
        ///   Gets the list of attaced nodes.
        /// </summary>
        /// <value>The nodes.</value>
        [XmlIgnore]
        public List<node> nodes
        {
            get { return _nodes ?? (_nodes = new List<node>()); }
        }

        /// <summary>
        ///   Gets or sets the name of the node that the arc is coming from.
        ///   It is necessary to do this, otherwise the serializer would rewrite 
        ///   the actual node to the file (*.gxml file).
        /// </summary>
        /// <value>The XML from.</value>
        [XmlElement("node")]
        public string[] XmlNodes
        {
            get
            {
                return nodes.Select(n => n.name).ToArray();
            }
            set
            {
                if (value == null) return;
                var names = value;
                _nodes = new List<node>();
                foreach (var a in names)
                    nodes.Add(new node(a));
            }
        }

        #endregion

        /* the degree or valence of a node is the number of arcs connecting to it.
         * Currently this is used in recognition of a rule when the strictDegreeMatch
         * is checked. */

        /// <summary>
        ///   Gets the degree of the hyperarcs - the number of nodes that it connects to.
        /// </summary>
        /// <value>The degree.</value>
        public int degree
        {
            get { return nodes.Count; }
        }
        /// <summary>
        /// Gets the arcs that connect to and from nodes within this hyperarc.
        /// </summary>
        /// <value>The intra-connected arcs.</value>
        [XmlIgnore]
        public List<arc> IntraArcs
        {
            get
            {
                var pathArcsEnumerable = nodes.SelectMany(n => n.arcsFrom);
                pathArcsEnumerable = pathArcsEnumerable.Intersect(nodes.SelectMany(n => n.arcsTo));
                return pathArcsEnumerable.ToList();
            }
        }
        
        #endregion

        #region Property-like Functions
        /// <summary>
        /// Connects the hyperarc to a new node.
        /// </summary>
        /// <param name="newNode">The new node.</param>
        public void ConnectTo(node newNode)
        {
            if (!nodes.Contains(newNode))
            {
                nodes.Add(newNode);
                newNode.arcs.Add(this);
            }
        }
        /// <summary>
        /// Disconnects the hyperarc from a node.
        /// </summary>
        /// <param name="removeNode">The remove node.</param>
        public void DisconnectFrom(node removeNode)
        {
            if (nodes.Contains(removeNode))
            {
                nodes.Remove(removeNode);
                removeNode.arcs.Remove(this);
            }
        }
        #endregion

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "hyperarc" /> class.
        /// </summary>
        /// <param name = "newName">The new name.</param>
        /// <param name = "attachedNodes">The attached nodes.</param>
        public hyperarc(string newName, IEnumerable<node> attachedNodes)
        {
            name = newName;
            if (attachedNodes == null) return;
            _nodes = new List<node>();
            foreach (var n in attachedNodes)
                ConnectTo(n);
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "hyperarc" /> class.
        /// </summary>
        /// <param name = "newName">The new name.</param>
        public hyperarc(string newName)
            : this(newName, null)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "hyperarc" /> class.
        /// </summary>
        public hyperarc()
            : this("_", null)
        {
        }

        #endregion

        #region Copy Method

        /// <summary>
        ///   Copies this instance of an arc and returns the copy.
        /// </summary>
        /// <returns>the copy of the arc.</returns>
        public virtual hyperarc copy()
        {
            var copyOfArc = new hyperarc();
            copy(copyOfArc);
            return copyOfArc;
        }

        /// <summary>
        ///   Copies this.arc into the argument copyOfArc.
        /// </summary>
        /// <param name = "copyOfArc">The copy of arc.</param>
        public virtual void copy(hyperarc copyOfArc)
        {
            base.copy(copyOfArc);
        }

        #endregion
    }
}