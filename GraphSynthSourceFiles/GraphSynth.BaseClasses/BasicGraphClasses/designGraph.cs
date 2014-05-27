/*************************************************************************
 *     This designGraph file & class is part of the GraphSynth.BaseClasses 
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
    ///   The quintessential class in all of this research. The graph of nodes,
    ///   arcs, and hyperarcs is called a designGraph. The use of the word design
    ///   is a carry-over from other research, but indicates that GraphSynth is really
    ///   about designing with graphs.
    /// </summary>
    public class designGraph
    {
        #region Fields & Properties

        /// <summary>
        ///   Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string name { get; set; }

        /// <summary>
        ///   Gets or sets the comment.
        /// </summary>
        /// <value>The comment.</value>
        public string comment { get; set; }

        #region Labels and Variables

        /* just like a node, an arc can contain both characterizing strings, known as globalLabels
         * and numbers, stored as globalVariables. */

        /// <summary />
        protected List<string> _globalLabels;

        /// <summary />
        protected List<double> _globalVariables;

        /// <summary>
        ///   Gets the global labels.
        /// </summary>
        /// <value>The global labels.</value>
        public List<string> globalLabels
        {
            get { return _globalLabels ?? (_globalLabels = new List<string>()); }
        }

        /// <summary>
        ///   Gets the global variables.
        /// </summary>
        /// <value>The global variables.</value>
        public List<double> globalVariables
        {
            get { return _globalVariables ?? (_globalVariables = new List<double>()); }
        }

        #endregion

        #region Lists of Nodes and Arcs included in this graph

        /// <summary />
        protected List<arc> _arcs;

        /// <summary />
        protected List<hyperarc> _hyperarcs;

        /// <summary />
        protected List<node> _nodes;

        /// <summary>
        ///   Gets or sets the arcs.
        /// </summary>
        /// <value>The arcs.</value>
        public List<arc> arcs
        {
            get { return _arcs ?? (_arcs = new List<arc>()); }
            //set { arcs = value; }
        }

        /// <summary>
        ///   Gets the nodes.
        /// </summary>
        /// <value>The nodes.</value>
        public List<node> nodes
        {
            get { return _nodes ?? (_nodes = new List<node>()); }
        }

        /// <summary>
        ///   Gets the hyperarcs.
        /// </summary>
        /// <value>The hyperarcs.</value>
        public List<hyperarc> hyperarcs
        {
            get { return _hyperarcs ?? (_hyperarcs = new List<hyperarc>()); }
        }

        #endregion

        #region Helper Properties

        /// <summary>
        /// Gets the degree sequence.
        /// </summary>
        /// <value>The degree sequence.</value>
        [XmlIgnore]
        public List<int> DegreeSequence
        {
            get { return new List<int>(from n in nodes orderby n.degree descending select n.degree); }
        }
        /// <summary>
        /// Gets the hyper arc degree sequence.
        /// </summary>
        /// <value>The hyper arc degree sequence.</value>
        [XmlIgnore]
        public List<int> HyperArcDegreeSequence
        {
            get { return new List<int>(from ha in hyperarcs orderby ha.degree descending select ha.degree); }
        }
        #endregion

        #region Iterator for Nodes and Arcs, and Hyperarcs

        /// <summary>
        ///   Gets the <see cref = "GraphSynth.Representation.graphElement" /> with the specified name.
        ///   This indexer is to make it easier to find a particular node, arc, or hyperArc. Note 
        ///   that it only returns a graphElement, so the user must explicitly cast it as a node,
        ///   arc, or hyperArc.
        /// </summary>
        /// <value></value>
        [XmlIgnore]
        public graphElement this[string eltName]
        {
            get
            {
                var gE = (nodes.Find(a => (a.name == eltName)) ??
                                   (graphElement)arcs.Find(a => (a.name == eltName))) ??
                                  hyperarcs.Find(h => (h.name == eltName));
                return gE;
            }
        }

        #endregion

        #endregion

        #region Add and Remove Nodes and Arcs Methods
        /* Here is a series of important graph management functions
         * while it would be easy to just call, for example, ".arcs.add",
         * the difficulty comes in properly linking the nodes 
         * likewise with the nodes and their dangling arcs. */
        #region addArc

        /// <summary>
        /// Creates and Adds a new arc to the graph, and connects it between
        /// the fromNode and the toNode
        /// </summary>
        /// <param name="fromNode">From node.</param>
        /// <param name="toNode">To node.</param>
        /// <param name="newName">The name.</param>
        /// <param name="arcType">Type of the arc.</param>
        public void addArc(node fromNode, node toNode, string newName = "", Type arcType = null)
        {
            arc newArc;
            if (string.IsNullOrWhiteSpace(newName)) newName = makeUniqueArcName();
            if (arcType == null || arcType == typeof(arc))
                newArc = new arc(newName);
            else
            {
                var types = new[] { typeof(string), typeof(node), typeof(node) };
                var arcConstructor = arcType.GetConstructor(types);

                var inputs = new object[] { newName, fromNode, toNode };
                newArc = (arc)arcConstructor.Invoke(inputs);
            }
            addArc(newArc, fromNode, toNode);
        }

        /// <summary>
        ///   Adds the arc to the graph and connects it between these two nodes.
        /// </summary>
        /// <param name = "newArc">The new arc.</param>
        /// <param name = "fromNode">From node.</param>
        /// <param name = "toNode">To node.</param>
        public void addArc(arc newArc, node fromNode, node toNode)
        {
            newArc.From = fromNode;
            newArc.To = toNode;
            arcs.Add(newArc);
        }

        #endregion

        #region removeArc
        /// <summary>
        ///   Removes the arc and references to it in the nodes.
        /// </summary>
        /// <param name = "arcToRemove">The arc to remove.</param>
        public void removeArc(arc arcToRemove)
        {
            if (arcToRemove.From != null)
                arcToRemove.From.arcs.Remove(arcToRemove);
            if (arcToRemove.To != null)
                arcToRemove.To.arcs.Remove(arcToRemove);
            arcs.Remove(arcToRemove);
        }

        #endregion

        #region addNode
        /// <summary>
        ///   Creates and Adds a new node of called newName of type nodeType.
        /// </summary>
        /// <param name = "newName">The new name.</param>
        /// <param name = "nodeType">Type of the node.</param>
        public void addNode(string newName = "", Type nodeType = null)
        {
            if (string.IsNullOrWhiteSpace(newName))
                newName = makeUniqueNodeName();
            if (nodeType == null || nodeType == typeof(node))
                addNode(new node(newName));
            else
            {
                var types = new Type[1];
                types[0] = typeof(string);
                var nodeConstructor = nodeType.GetConstructor(types);

                var inputs = new object[1];
                inputs[0] = newName;
                addNode((node)nodeConstructor.Invoke(inputs));
            }
        }

        /// <summary>
        ///   Adds the node to the graph. This is very simple, and is in fact identical to
        ///   doing graph.nodes.Add(n);
        /// </summary>
        /// <param name = "n">The n.</param>
        public void addNode(node n)
        {
            nodes.Add(n);
        }

        #endregion

        #region removeNode

        /// <summary>
        ///   Removes the node. Removing a node is a little more complicated than removing arcs
        ///   since we need to decide what to do with dangling arcs. As a result there are two 
        ///   booleans that specify how to handle the arcs. removeArcToo will simply delete the
        ///   attached arcs if true, otherwise it will leave them dangling (default is false).
        ///   removeNodeRef will change the references within the attached arcs to null if set 
        ///   to true, or will leave them if false (default is true).
        /// </summary>
        /// <param name = "nodeToRemove">The node to remove.</param>
        /// <param name = "removeNodeRef">if set to <c>true</c> remove reference to this node in the arcs.</param>
        public void removeNode(node nodeToRemove, Boolean removeNodeRef = true)
        {
            if (removeNodeRef)
            {
                var connectedArcs = new List<graphElement>(nodeToRemove.arcs);
                foreach (var connectedArc in connectedArcs)
                    if (connectedArc is arc)
                        if (((arc)connectedArc).From == nodeToRemove)
                            ((arc)connectedArc).From = null;
                        else ((arc)connectedArc).To = null;
                    else if (connectedArc is hyperarc)
                        ((hyperarc)connectedArc).nodes.Remove(nodeToRemove);
                nodes.Remove(nodeToRemove);
            }
            else nodes.Remove(nodeToRemove);
        }
        #endregion

        #region addHyperArc
        /// <summary>
        /// Creates and Adds a new hyperarc to the graph, and connects it
        /// to the stated nodes.
        /// </summary>
        /// <param name="attachedNodes">The nodes.</param>
        /// <param name="newName">The new name.</param>
        /// <param name="hyperarcType">The t.</param>
        public void addHyperArc(List<node> attachedNodes, string newName = "", Type hyperarcType = null)
        {
            hyperarc newArc;
            if (string.IsNullOrWhiteSpace(newName)) newName = makeUniqueHyperArcName();
            if (hyperarcType == null || hyperarcType == typeof(hyperarc))
                newArc = new hyperarc(newName);
            else
            {
                var types = new[] { typeof(string), typeof(node), typeof(node) };
                var arcConstructor = hyperarcType.GetConstructor(types);

                var inputs = new object[] { newName, attachedNodes };
                newArc = (hyperarc)arcConstructor.Invoke(inputs);
            }
            addHyperArc(newArc, attachedNodes);
        }

        /// <summary>
        /// Adds the arc to the graph and connects it between these nodes.
        /// </summary>
        /// <param name="newArc">The new arc.</param>
        /// <param name="attachedNodes">The nodes.</param>
        public void addHyperArc(hyperarc newArc, List<node> attachedNodes = null)
        {
            if (attachedNodes != null)
                foreach (var n in attachedNodes)
                    newArc.ConnectTo(n);
            hyperarcs.Add(newArc);
        }
        #endregion

        #region removeHyperArc
        /// <summary>
        /// Removes the hyper arc.
        /// </summary>
        /// <param name="arcToRemove">The arc to remove.</param>
        public void removeHyperArc(hyperarc arcToRemove)
        {
            for (var i = arcToRemove.nodes.Count - 1; i >= 0; i--)
                arcToRemove.DisconnectFrom(arcToRemove.nodes[i]);
            hyperarcs.Remove(arcToRemove);
        }
        #endregion
        #endregion

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "designGraph" /> class.
        /// </summary>
        public designGraph() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="designGraph"/> class.
        /// currently this constructor is used within the recognize function of the
        /// grammar rule to establish each of the recognized locations.
        /// </summary>
        /// <param name="newNodes">The new nodes.</param>
        /// <param name="newArcs">The new arcs.</param>
        /// <param name="newHyperArcs">The new hyper arcs.</param>
        public designGraph(IEnumerable<node> newNodes, IEnumerable<arc> newArcs = null,
            IEnumerable<hyperarc> newHyperArcs = null)
        {
            _nodes = new List<node>(newNodes);
            if (newArcs != null) _arcs = new List<arc>(newArcs);
            if (newHyperArcs != null) _hyperarcs = new List<hyperarc>(newHyperArcs);
        }

        /// <summary>
        /// Creates a random graph that takes two parameters: the number of nodes, 
        /// and the average degree. Note: that there is no guarantee that the graph 
        /// will be connected.
        /// </summary>
        /// <param name = "numNodes">The number of nodes.</param>
        /// <param name = "aveDegree">The average degree.</param>
        /// <returns></returns>
        public static designGraph CreateRandomGraph(int numNodes, int aveDegree)
        {
            var randomGraph = new designGraph
                                  {
                                      name = "RandomGraph_with_" + numNodes + "_nodes_and_degree_of_" + aveDegree
                                  };
            var arcProb = (double)aveDegree / (numNodes + 1);
            var rnd = new Random();
            for (var i = 0; i != numNodes; i++)
                randomGraph.addNode();

            for (var i = 0; i != numNodes; i++)
                for (var j = i + 1; j != numNodes; j++)
                    if ((double)rnd.Next(1000) / 1000 <= arcProb)
                        randomGraph.addArc(randomGraph.nodes[i], randomGraph.nodes[j]);
            return randomGraph;
        }

        /// <summary>
        ///   Creates a complete graph where every node is connected to every
        ///   other node by an arc.
        /// </summary>
        /// <param name="numNodes">The number of nodes.</param>
        /// <returns></returns>
        public static designGraph CreateCompleteGraph(int numNodes)
        {
            var numArcs = numNodes * (numNodes - 1) / 2;
            var completeGraph = new designGraph
                                    {
                                        name = "CompleteGraph_with_" + numNodes + "_nodes_and_" + numArcs + "_arcs"
                                    };
            for (var i = 0; i != numNodes; i++)
                completeGraph.addNode();
            for (var i = 0; i != numNodes; i++)
                for (var j = i + 1; j != numNodes; j++)
                    completeGraph.addArc(completeGraph.nodes[i], completeGraph.nodes[j]);
            return completeGraph;
        }

        /// <summary>
        /// Creates an empty location graph used in recognition.
        /// </summary>
        /// <param name="numNodes">The num nodes.</param>
        /// <param name="numArcs">The num arcs.</param>
        /// <param name="numHyperArcs">The num hyper arcs.</param>
        /// <returns></returns>
        public static designGraph CreateEmptyLocationGraph(int numNodes, int numArcs, int numHyperArcs = 0)
        {
            var emptyGraph = new designGraph();
            for (var i = 0; i != numNodes; i++)
                emptyGraph.nodes.Add(null);
            for (var i = 0; i != numArcs; i++)
                emptyGraph.arcs.Add(null);
            for (var i = 0; i != numHyperArcs; i++)
                emptyGraph.hyperarcs.Add(null);
            return emptyGraph;
        }

        #endregion

        #region misc. Methods

        /// <summary>
        ///   Copies the specified make deep copy.
        /// </summary>
        /// <param name = "MakeDeepCopy">if set to <c>true</c> [make deep copy].</param>
        /// <returns></returns>
        public designGraph copy(Boolean MakeDeepCopy = true)
        {
            /* at times we want to copy a graph and not refer to the same objects. This happens mainly
             * (rather initially what inspired this function) when the seed graph is copied into a candidate.*/
            var copyOfGraph = new designGraph { name = name };

            foreach (var label in globalLabels)
                copyOfGraph.globalLabels.Add(label);
            foreach (var v in globalVariables)
                copyOfGraph.globalVariables.Add(v);
            foreach (var origNode in nodes)
                copyOfGraph.addNode(MakeDeepCopy ? origNode.copy() : origNode);
            foreach (var origArc in arcs)
            {
                if (MakeDeepCopy)
                {
                    var copyOfArc = origArc.copy();
                    var toIndex = nodes.FindIndex(a => (a == origArc.To));
                    var fromIndex = nodes.FindIndex(b => (b == origArc.From));
                    node fromNode = null;
                    if (fromIndex > -1) fromNode = copyOfGraph.nodes[fromIndex];
                    node toNode = null;
                    if (toIndex > -1) toNode = copyOfGraph.nodes[toIndex];
                    copyOfGraph.addArc(copyOfArc, fromNode, toNode);
                }
                else copyOfGraph.arcs.Add(origArc);
            }
            foreach (var origHyperArc in hyperarcs)
            {
                if (MakeDeepCopy)
                {
                    var copyOfHyperArc = origHyperArc.copy();
                    var attachedNodes = new List<node>();
                    foreach (var n in origHyperArc.nodes)
                    {
                        var index = nodes.FindIndex(a => (a == n));
                        attachedNodes.Add(copyOfGraph.nodes[index]);
                    }
                    copyOfGraph.addHyperArc(copyOfHyperArc, attachedNodes);
                }
                else copyOfGraph.hyperarcs.Add(origHyperArc);
            }
            return copyOfGraph;
        }

        /// <summary>
        ///   Makes a unique name for a node.
        /// </summary>
        /// <param name = "stub">The stub.</param>
        /// <returns></returns>
        public string makeUniqueNodeName(string stub = "n")
        {
            stub = stub.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            var i = 0;
            while (nodes.Exists(b => b.name.Equals(stub + i)))
                i++;
            return stub + i;
        }

        /// <summary>
        ///   Makes a unique name for an arc.
        /// </summary>
        /// <param name = "stub">The stub.</param>
        /// <returns></returns>
        public string makeUniqueArcName(string stub = "a")
        {
            stub = stub.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            var i = 0;
            while (arcs.Exists(b => b.name.Equals(stub + i)))
                i++;
            return stub + i;
        }


        /// <summary>
        /// Makes the name of the unique hyper arc.
        /// </summary>
        /// <param name="stub">The stub.</param>
        /// <returns></returns>
        public string makeUniqueHyperArcName(string stub = "ha")
        {
            stub = stub.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            var i = 0;
            while (hyperarcs.Exists(b => b.name.Equals(stub + i)))
                i++;
            return stub + i;
        }


        /// <summary>
        ///   Internally connects the graph.
        /// </summary>
        public void internallyConnectGraph()
        {
            for (var i = nodes.Count - 1; i >= 0; i--)
            {
                if (nodes[i].name.StartsWith("null"))
                    nodes.RemoveAt(i);
            }
            foreach (var a in arcs)
            {
                if ((a.From == null) || a.From.name.StartsWith("null"))
                    a.From = null;
                else
                {
                    var fromNode = nodes.Find(b => (b.name == a.From.name));
                    if (fromNode == null) throw new Exception("Arc, " + a.name + ", was to connect to node, " + a.From.name +
                      ", but the node did not load as part of the graph.");
                    a.From = fromNode;
                }

                if ((a.To == null) || a.To.name.StartsWith("null"))
                    a.To = null;
                else
                {
                    var toNode = nodes.Find(b => (b.name == a.To.name));
                    if (toNode == null) throw new Exception("Arc, " + a.name + ", was to connect to node, " + a.To.name +
                      ", but the node did not load as part of the graph.");
                    a.To = toNode;
                }
            }
            foreach (var h in hyperarcs)
                for (var i = h.nodes.Count - 1; i >= 0; i--)
                {
                    if (h.nodes[i] != null)
                    {
                        var attachedNode = nodes.Find(b => (b.name == h.nodes[i].name));
                        if (attachedNode == null)
                            throw new Exception("Hyperarc, " + h.name + ", was to connect to node, "
                                                + h.nodes[i].name + ", but the node did not load as part of the graph.");
                        h.nodes[i] = attachedNode;
                        attachedNode.arcs.Add(h);
                    }
                }
            foreach (var n in nodes.Where(n => n.name.Contains("Not_set")))
                n.name = makeUniqueNodeName();

            foreach (var a in arcs.Where(a => a.name.StartsWith("Not_set")))
                a.name = makeUniqueArcName();

        }

        /// <summary>
        /// Replaces the type of the node with inherited.
        /// </summary>
        /// <param name="origNode">The orig node.</param>
        /// <param name="newType">The new type.</param>
        public void replaceNodeWithInheritedType(node origNode, Type newType)
        {
            addNode(origNode.name, newType);
            origNode.copy(nodes.Last());
            nodes.Last().DisplayShape = origNode.DisplayShape;
            for (var i = 0; i != origNode.arcsFrom.Count; i++)
                origNode.arcsFrom[i].From = nodes.Last();
            for (var i = 0; i != origNode.arcsTo.Count; i++)
                origNode.arcsTo[i].To = nodes.Last();

            removeNode(origNode);
        }

        /// <summary>
        /// Replaces the type of the arc with inherited.
        /// </summary>
        /// <param name="origArc">The orig arc.</param>
        /// <param name="newType">The new type.</param>
        public void replaceArcWithInheritedType(arc origArc, Type newType)
        {
            addArc(origArc.From, origArc.To, origArc.name, newType);
            origArc.copy(arcs.Last());
            arcs.Last().DisplayShape = origArc.DisplayShape;
            removeArc(origArc);
        }

        /// <summary>
        /// Replaces the type of the arc with inherited.
        /// </summary>
        /// <param name="origArc">The orig arc.</param>
        /// <param name="newType">The new type.</param>
        public void replaceHyperArcWithInheritedType(hyperarc origArc, Type newType)
        {
            addHyperArc(origArc.nodes, origArc.name, newType);
            origArc.copy(hyperarcs.Last());
            hyperarcs.Last().DisplayShape = origArc.DisplayShape;
            removeHyperArc(origArc);
        }

        #region CheckForRepeatNames

        /// <summary>
        ///   Checks for repeat names.
        /// </summary>
        /// <returns></returns>
        public Boolean checkForRepeatNames()
        {
            var numberChar = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            var anyNameChanged = false;
            if (nodes.Count > 0)
            {
                var sortedNodes = nodes.OrderBy(n => n.name).ToList();
                for (var i = 0; i != sortedNodes.Count - 1; i++)
                    if (sortedNodes[i].name == sortedNodes[i + 1].name)
                    {
                        sortedNodes[i].name = makeUniqueNodeName(sortedNodes[i].name.Trim(numberChar));
                        anyNameChanged = true;
                    }
            }
            if (arcs.Count > 0)
            {
                var sortedArcs = arcs.OrderBy(a => a.name).ToList();
                for (var i = 0; i != sortedArcs.Count - 1; i++)
                    if (sortedArcs[i].name == sortedArcs[i + 1].name)
                    {
                        sortedArcs[i].name = makeUniqueArcName(sortedArcs[i].name.Trim(numberChar));
                        anyNameChanged = true;
                    }
            }
            if (hyperarcs.Count > 0)
            {
                var sortedHypers = hyperarcs.OrderBy(a => a.name).ToList();
                for (var i = 0; i != sortedHypers.Count - 1; i++)
                    if (sortedHypers[i].name == sortedHypers[i + 1].name)
                    {
                        sortedHypers[i].name = makeUniqueArcName(sortedHypers[i].name.Trim(numberChar));
                        anyNameChanged = true;
                    }
            }
            return anyNameChanged;
        }

        #endregion

        /// <summary>
        /// Overrides the object method to check all details of the graphs to see
        /// if they are identical. It is potentially time-consuming as it makes
        /// rules and assigns the graphs as the L of the rule, and then performs
        /// the "recognize" function on the other graph.
        /// </summary>
        /// <param name="obj">The other graph to compare to this one.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj, false);
        }

        /// <summary>
        /// Overrides the object method to check all details of the graphs to see
        /// if they are identical. It is potentially time-consuming as it makes
        /// rules and assigns the graphs as the L of the rule, and then performs
        /// the "recognize" function on the other graph.
        /// </summary>
        /// <param name="obj">The other graph to compare to this one.</param>
        /// <param name="contentsOfGraphAreEqual">if set to <c>true</c> then check that contents of graph are equal even though they occupy different memory.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(object obj, bool contentsOfGraphAreEqual)
        {
            if (base.Equals(obj)) return true;
            if (!contentsOfGraphAreEqual) return false;
            if (!(obj is designGraph)) return false;
            var g = (designGraph)obj;
            if (nodes.Count != g.nodes.Count) return false;
            if (arcs.Count != g.arcs.Count) return false;
            if (hyperarcs.Count != g.hyperarcs.Count) return false;
            for (int i = 0; i < nodes.Count; i++)
                if (DegreeSequence[i] != g.DegreeSequence[i]) return false;
            var maxDegree = DegreeSequence[0];
            var dummyRule = new grammarRule();
            #region put g's nodes, arcs and hyperarcs into the LHS of the rule
            dummyRule.L = new designGraph();
            foreach (var n in g.nodes)
            {
                if (n.degree == maxDegree) dummyRule.L.nodes.Insert(0, new ruleNode(n));
                else dummyRule.L.nodes.Add(new ruleNode(n));
            }
            foreach (var n in g.arcs)
                dummyRule.L.arcs.Add(new ruleArc(n));
            foreach (var n in g.hyperarcs)
                dummyRule.L.hyperarcs.Add(new ruleHyperarc(n));
            dummyRule.L.internallyConnectGraph();
            #endregion
            if (dummyRule.recognize(this).Count < 1) return false;

            #region put this's nodes, arcs and hyperarcs into the LHS of the rule
            dummyRule.L = new designGraph();
            foreach (var n in this.nodes)
            {
                if (n.degree == maxDegree) dummyRule.L.nodes.Insert(0, new ruleNode(n));
                dummyRule.L.nodes.Add(new ruleNode(n));
            }
            foreach (var n in this.arcs)
                dummyRule.L.arcs.Add(new ruleArc(n));
            foreach (var n in this.hyperarcs)
                dummyRule.L.hyperarcs.Add(new ruleHyperarc(n));
            dummyRule.L.internallyConnectGraph();
            #endregion
            if (dummyRule.recognize(g).Count < 1) return false;
            return true;
        }
        #endregion

    }
}