/*************************************************************************
 *     This arc file & class is part of the GraphSynth.BaseClasses Project
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
using System.Xml.Serialization;

namespace GraphSynth.Representation
{
    /// <summary>
    ///   One of the two basic classes for a graph is the arc or edge that connects
    ///   two and only two elements of the node class.
    /// </summary>
    [XmlInclude(typeof(edge))]
    [XmlInclude(typeof(ruleArc))]
    public class arc : graphElement
    {
        #region Fields & Properties

        #region to and from node connections

        /// <summary>
        ///   Each arc connects to two and only two nodes, these are stored in protected elements.
        ///   This is a field for the node the arc is coming from.
        /// </summary>
        private node from;

        /// <summary>
        ///   Each arc connects to two and only two nodes, these are stored in protected elements.
        ///   This is a field for the node the arc is going to.
        /// </summary>
        private node to;

        /* From and To do all the work of from and to. Getting the value is simple, but
         * setting a to or from involves removing or adding the arc to the elements of 
         * the node. */

        /// <summary>
        ///   Gets or sets the connected node, From.
        /// </summary>
        /// <value>the node the arc is coming from.</value>
        [XmlIgnore]
        public node From
        {
            get { return from; }
            set
            {
                if (from == value) return;
                /* if you are disconnecting an arc... */
                if ((from != null) && (from != to))
                    from.arcs.Remove(this);
                /* if you are connecting an arc to a new node...*/
                if ((value != null) && (!value.arcs.Contains(this)))
                    value.arcs.Add(this);
                from = value;
            }
        }

        /// <summary>
        ///   Gets or sets  the connected node, To.
        /// </summary>
        /// <value>the node the arc is going to.</value>
        [XmlIgnore]
        public node To
        {
            get { return to; }
            set
            {
                if (to == value) return;
                /* if you are disconnecting an arc... */
                if ((to != null) && (to != from))
                    to.arcs.Remove(this);
                /* if you are connecting an arc to a new node...*/
                if ((value != null) && (!value.arcs.Contains(this)))
                    value.arcs.Add(this);
                to = value;
            }
        }

        /// <summary>
        ///   Gets or sets the name of the node that the arc is coming from.
        ///   It is necessary to do this, otherwise the serializer would rewrite 
        ///   the actual node to the file (*.gxml file).
        /// </summary>
        /// <value>The XML from.</value>
        [XmlElement("From")]
        public string XmlFrom
        {
            get
            {
                if (from != null)
                    return from.name;
                return null;
            }
            set
            {
                if (from == null)
                    from = new node();
                from.name = value;
            }
        }

        /// <summary>
        ///   Gets or sets the  NAME of the node that the arc is going to.
        /// </summary>
        /// <value>The XML to.</value>
        [XmlElement("To")]
        public string XmlTo
        {
            get
            {
                if (to != null)
                    return to.name;
                return null;
            }
            set
            {
                if (to == null)
                    to = new node();
                to.name = value;
            }
        }

        #endregion

        #region Direction (is the arc defined as an arrow with direction).

        /* arc can have a meaningful direction or be labelled doubly-directed. These
         * protected fields are controlled by the similarly named properties below. */
        private Boolean _directed;
        private Boolean _doublyDirected;
        /* these Boolean properties manage the protected elements. The trick here
         * is that an arc cannot be doubly-directed and not directed, but it is
         * possible to be directed and not doubly-directed. */

        /// <summary>
        ///   Gets or sets a value indicating whether this <see cref = "arc" /> is directed.
        /// </summary>
        /// <value><c>true</c> if directed; otherwise, <c>false</c>.</value>
        public Boolean directed
        {
            get { return _directed; }
            set
            {
                if ((!value) && _doublyDirected)
                {
                    _directed = false;
                    _doublyDirected = false;
                }
                else _directed = value;
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether [doubly directed].
        /// </summary>
        /// <value><c>true</c> if [doubly directed]; otherwise, <c>false</c>.</value>
        public Boolean doublyDirected
        {
            get { return _doublyDirected; }
            set
            {
                if ((value) && (_directed == false))
                {
                    _doublyDirected = true;
                    _directed = true;
                }
                else _doublyDirected = value;
            }
        }

        #endregion

        /// <summary>
        ///   Gets the straightline distance between the two connecting nodes.
        /// </summary>
        /// <value>The length.</value>
        [XmlIgnore]
        public double length
        {
            get
            {
                try /* this is in case the connecting nodes are not vertices */
                {
                    var v1 = From;
                    var v2 = To;
                    return Math.Sqrt((v1.X - v2.X) * (v1.X - v2.X) + (v1.Y - v2.Y) * (v1.Y - v2.Y)
                        + (v1.Z - v2.Z) * (v1.Z - v2.Z));
                }
                catch
                {
                    return 0.0;
                }
            }
        }
        #endregion

        #region Property-like Functions

        /// <summary>
        ///   Given one connecting node, this function returns
        ///   the other node connected to this arc.
        /// </summary>
        /// <param name = "node1">One known node.</param>
        /// <returns></returns>
        public node otherNode(node node1)
        /* well, this isn't exactly a property, but it's kinda used like one.
 * here, we know one of the nodes that the arc is connected to, but not
 * the other. So, we are simply asking for the node other than the one we know.*/
        {
            if (from == node1) return to;
            if (to == node1) return from;
            return null;
        }

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="arc"/> class.
        /// </summary>
        public arc() : this("a") { }
        /// <summary>
        /// Initializes a new instance of the <see cref="arc"/> class.
        /// </summary>
        /// <param name="newName">The new name.</param>
        public arc(string newName)
        {
            name = newName;
        }

        #endregion

        #region Copy Method

        /// <summary>
        ///   Copies this instance of an arc and returns the copy.
        /// </summary>
        /// <returns>the copy of the arc.</returns>
        public virtual arc copy()
        {
            var copyOfArc = new arc();
            copy(copyOfArc);
            return copyOfArc;
        }

        /// <summary>
        ///   Copies this.arc into the argument copyOfArc.
        /// </summary>
        /// <param name = "copyOfArc">The copy of arc.</param>
        public virtual void copy(arc copyOfArc)
        {
            base.copy(copyOfArc);

            copyOfArc.directed = directed;
            copyOfArc.doublyDirected = doublyDirected;
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
    public class edge : arc
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="edge"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public edge(string name = "e") : base(name) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="edge"/> class.
        /// </summary>
        public edge() { }
        /// <summary>
        ///   Copies this instance of an arc and returns the copy.
        /// </summary>
        /// <returns>the copy of the arc.</returns>
        public override arc copy()
        {
            var copyOfEdge = new edge();
            base.copy(copyOfEdge);

            return copyOfEdge;
        }
    }
}