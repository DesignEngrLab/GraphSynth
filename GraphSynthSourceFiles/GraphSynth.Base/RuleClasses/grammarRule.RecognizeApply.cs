/*************************************************************************
 *     This grammarRule.RecognizeApply.cs file partially defines the 
 *     grammarRule class (also partially defined in grammarRule.Basic.cs, 
 *     grammarRule.ShapeMethods.cs and grammarRule.NegativeRecognize.cs)
 *     and is part of the GraphSynth.BaseClasses Project which is the 
 *     foundation of the GraphSynth Application.
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GraphSynth.Representation
{
    /* Get ready, this file is complicated. All the recognize and apply functions are found
     * here. There is a recognize function in ruleSet, and an apply in option but those are simply 
     * macros for the functions found here within grammarRule. */

    /// <summary>
    /// A partial description of the grammar rule class. In addition to storing designGraphs
    /// for both the left and right hand sides, there are a variety of functions for describing
    /// how a rule recognizes on a host, and how it makes changes via apply.
    /// </summary>
    public partial class grammarRule
    {
        private Boolean _in_parallel_;
        #region Recognize Methods
        // The next 300 lines define the recognize functions.

        /// <summary>
        /// Determines locations where the rule is recognized on the specified host.
        /// here is the big one! Although it looks fairly short, a lot of time can be spent in
        /// the recursion that it invokes. Before we get to that, we want to make sure that
        /// our time there is well spent. As a result, we try to rule out whether the rule
        /// can even be applied at first -- hence the series of if-thens. If you don't
        /// meet the first, leave now! likewise for the second. The third is a little trickier.
        /// if there are no nodes or arcs in this rule, then it has already proven to be valid
        /// by the global labels - thus return a single location.
        /// The real work happens in the findNewStartElement which is time-consuming so we first
        /// do some simply counting to see if the host is bigger than the LHS.
        /// When findNewStartElement recurses down and divides, a number of options may be created
        /// in the final method (LocationFound). If there are multiple locations within the
        /// global labels then we merge the two together.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="InParallel">if set to <c>true</c> [in parallel].</param>
        /// <param name="RelaxationTemplate">The relaxation template.</param>
        /// <returns></returns>
        public List<option> recognize(designGraph host, Boolean InParallel = true, Relaxation RelaxationTemplate = null)
        {
            this.host = host;
            _in_parallel_ = InParallel;
            var location = new option(this);
            if (RelaxationTemplate != null) location.Relaxations = RelaxationTemplate.copy();
            options.Clear();

            if (!InitialRuleCheck() && !InitialRuleCheckRelaxed(location)) return new List<option>();

            if (ContainsNegativeElements) FindPositiveStartElementAvoidNegatives(location);
            else findNewStartElement(location);
            /* if OrderedGlobalLabels is checked and there are multiple locations in the 
             * string of labels then we need to convolve the two set of locations together. */
            if (OrderedGlobalLabels)
            {
                var origLocs = new List<option>(options);
                options.Clear();
                for (var i = globalLabelStartLocs.Count - 1; i >= 0; i--)
                {
                    foreach (var opt in origLocs)
                    {
                        var localOption = opt;
                        if (i > 0) localOption = opt.copy();
                        localOption.globalLabelStartLoc = globalLabelStartLocs[i];
                        lock (options) { options.Add(localOption); }
                    }
                }
            }
            return options;
        }
        private void findNewStartElement(option location)
        {
            #region Case #1: Location found! No empty slots left in the location
            /* this is the only way to properly exit the recursive loop. */
            if (!location.nodes.Contains(null) && !location.arcs.Contains(null) && !location.hyperarcs.Contains(null))
            {
                /* as a recursive function, we first check how the recognition process terminates. If all nodes,
                 * hyperarcs and arcs within location have been filled with references to elements in the host, 
                 * then we've found a location...well maybe. More details are described in the LocationFound function. */
                if (FinalRuleChecks(location) || FinalRuleCheckRelaxed(location))
                {
                    var locCopy = location.copy();
                    lock (options)
                    {
                        options.Add(locCopy);
                    }
                }
                return;
            }
            #endregion
            #region Case #2: build off of a hyperarc found so far - by looking for unfulfilled nodes
            /* the quickest approach to finding a new element in the LHS to host subgraph matching is to build
             * directly off of elements found so far. This is because we don't need to check amongst ALL elements in the
             * host (as is the case in the last three cases below). In this case we start with any hyperarcs
             * that have already been matched to one in the host, and see if it connects to any nodes that
             * have yet to be matched. */
            var startHyperArc = (ruleHyperarc)L.hyperarcs.FirstOrDefault(ha => ((location.findLMappedHyperarc(ha) != null)
                && (ha.nodes.Any(n => (location.findLMappedNode(n) == null)))));
            if (startHyperArc != null)
            {
                var hostHyperArc = location.findLMappedHyperarc(startHyperArc);
                var newLNode = (ruleNode)startHyperArc.nodes.FirstOrDefault(n => (location.findLMappedNode(n) == null));
                foreach (var n in hostHyperArc.nodes.Where(n => !location.nodes.Contains(n)))
                    checkNode(location.copy(), newLNode, n);
                return;
            }
            #endregion
            #region Case #3: build off of a node found so far - by looking for unfulfilled arcs
            /* as stated above, the quickest approach is to build from elements that have already been found.
             * Therefore, we see if there are any nodes already matched to a node in L that has an arc in L
             * that has yet to be matched with a host arc. This is more efficient than the last 3 cases 
             * because they look through the entire host, which is potentially large. */
            var startNode = (ruleNode)L.nodes.FirstOrDefault(n => ((location.findLMappedNode(n) != null)
                && (n.arcs.Any(a => (location.findLMappedElement(a) == null)))));
            /* is there a node already matched (which would only occur if your recursed to get here) that has an
             * unrecognized arc attaced to it. If yes, try all possible arcs in the host with the one that needs
             * to be fulfilled in L. */
            if (startNode != null)
            {
                var newLArc = startNode.arcs.FirstOrDefault(a => (location.findLMappedElement(a) == null));
                if (newLArc is ruleHyperarc)
                    checkHyperArc(location, startNode, location.findLMappedNode(startNode), (ruleHyperarc)newLArc);
                else if (newLArc is ruleArc)
                    checkArc(location, startNode, location.findLMappedNode(startNode), (ruleArc)newLArc);
                return;
            }
            #endregion
            #region Case 4: Check entire host for a matching hyperarc
            /* if the above cases didn't match we try to match a hyperarc in the L to any in the host. Since the
             * prior three cases have conditions which require some non-nulls in the location, this is likely where the 
             * process will start when invoked from line 87 of recognize above. Hyperarcs are most efficient to start from 
             * since there are likely fewer hyperarcs in the host than nodes, or arcs. */
            startHyperArc = (ruleHyperarc)L.hyperarcs.FirstOrDefault(ha => (location.findLMappedHyperarc(ha) == null));
            if (startHyperArc != null)
            {
                if (_in_parallel_)
                    Parallel.ForEach(host.hyperarcs, hostHyperArc =>
                    {
                        if (!location.hyperarcs.Contains(hostHyperArc))
                            checkHyperArc(location.copy(), startHyperArc, hostHyperArc);
                    });
                else
                    foreach (var hostHyperArc in
                        host.hyperarcs.Where(hostHyperArc => !location.hyperarcs.Contains(hostHyperArc)))
                    {
                        checkHyperArc(location.copy(), startHyperArc, hostHyperArc);
                    }
                return;
            }
            #endregion
            #region Case 5: Check entire host for a matching node
            /* If no other hyperarcs can be recognized, then look to a unlocated node. If one gets here then none of the above
             * three conditions were met (obviously) but this also implies that there are multiple components in the
             * LHS, and we are now jumping to a new one with this. This is potentially time intensive if there are
             * a lot of nodes in the host. We allow for the possibility that this recognition can be done in parallel. */
            startNode = (ruleNode)L.nodes.FirstOrDefault(n => (location.findLMappedNode(n) == null));
            if (startNode != null)
            {
                if (_in_parallel_)
                    Parallel.ForEach(host.nodes, hostNode =>
                        {
                            if (!location.nodes.Contains(hostNode))
                                checkNode(location.copy(), startNode, hostNode);
                        });
                else foreach (var hostNode in
                    host.nodes.Where(hostNode => !location.nodes.Contains(hostNode)))
                    {
                        checkNode(location.copy(), startNode, hostNode);
                    }
                return;
            }
            #endregion
            #region Case 6: Check entire host for a matching arc
            var looseArc = (ruleArc)L.arcs.FirstOrDefault(a => (location.findLMappedArc(a) == null));
            /* the only way one can get here is if there are one or more arcs NOT connected to any nodes
             * in L - a floating arc, dangling on both sides, like an eyelash. */
            if (looseArc != null)
                if (_in_parallel_)
                    Parallel.ForEach(host.arcs, hostArc =>
                                     {
                                         if ((!location.arcs.Contains(hostArc)) && (!location.nodes.Contains(hostArc.From))
                                             && (!location.nodes.Contains(hostArc.To))
                                             && (arcMatches(looseArc, hostArc) || arcMatchRelaxed(looseArc, hostArc, location)))
                                         { //relaxelt
                                             var newLocation = location.copy();
                                             newLocation.arcs[L.arcs.IndexOf(looseArc)] = hostArc;
                                             findNewStartElement(newLocation);
                                         }
                                     });
                else
                    foreach (var hostArc in host.arcs)
                        if ((!location.arcs.Contains(hostArc)) && (!location.nodes.Contains(hostArc.From))
                                 && (!location.nodes.Contains(hostArc.To))
                                 && (arcMatches(looseArc, hostArc) || arcMatchRelaxed(looseArc, hostArc, location)))
                        { //relaxelt
                            var newLocation = location.copy();
                            newLocation.arcs[L.arcs.IndexOf(looseArc)] = hostArc;
                            findNewStartElement(newLocation);
                        }
            #endregion
        }
        private void checkNode(option location, ruleNode LNode, node hostNode)
        {
            if (!nodeMatches(LNode, hostNode, location) && !nodeMatchRelaxed(LNode, hostNode, location))
                return;
            location.nodes[L.nodes.IndexOf(LNode)] = hostNode;
            var newLArc = LNode.arcs.FirstOrDefault(a => (location.findLMappedElement(a) == null));
            if (newLArc == null) findNewStartElement(location);
            else if (newLArc is ruleHyperarc)
                checkHyperArc(location, LNode, hostNode, (ruleHyperarc)newLArc);
            else if (newLArc is ruleArc)
                checkArc(location, LNode, hostNode, (ruleArc)newLArc);
        }
        private void checkHyperArc(option location, ruleHyperarc LHyperArc, hyperarc hostHyperArc)
        {
            if (!hyperArcMatches(LHyperArc, hostHyperArc) && !hyperArcMatchRelaxed(LHyperArc, hostHyperArc, location))
                return;
            location.hyperarcs[L.hyperarcs.IndexOf(LHyperArc)] = hostHyperArc;
            var newLNode = (ruleNode)LHyperArc.nodes.FirstOrDefault(n => (location.findLMappedNode(n) == null));
            if (newLNode == null) findNewStartElement(location);
            else
                foreach (var n in hostHyperArc.nodes.Where(n => !location.nodes.Contains(n)))
                    checkNode(location.copy(), newLNode, n);
        }

        private void checkHyperArc(option location, ruleNode fromLNode, node fromHostNode,
            ruleHyperarc newLHyperArc)
        {
            var otherConnectedNodes = (from n in newLHyperArc.nodes
                                       where ((n != fromLNode) && (location.findLMappedNode(n) != null))
                                       select (location.findLMappedNode(n)));
            var oCNNum = otherConnectedNodes.Count();

            var hostHyperArcs = (from ha in fromHostNode.arcs
                                 where (ha is hyperarc
                           && !location.hyperarcs.Contains(ha)
                           && (oCNNum == otherConnectedNodes.Intersect(((hyperarc)ha).nodes).Count()))
                                 select ha).Cast<hyperarc>();
            /* at this stage hostHyperArcs are hyperarcs connected to fromHostNode, the same way that
             * newLHyperArc is connected to fromLNode. However, this is not enough! What about nodes also
             * connected to newLHyperArc that have already been recognized. We need to remove any instances
             * from hostHyperArcs which don't connect to mappings of these already recognized nodes. */
            foreach (var hostHyperArc in hostHyperArcs)
                checkHyperArc(location.copy(), newLHyperArc, hostHyperArc);
        }


        private void checkArc(option location, node fromLNode, node fromHostNode,
            ruleArc newLArc)
        {
            var currentLArcIndex = L.arcs.IndexOf(newLArc);
            /* so, currentLArcIndex now, points to a LArc that has yet to be recognized. What we do from
            * this point depends on whether that LArc points to an L node we have yet to recognize, an L
            * node we have recognized, or null. */
            var nextLNode = (ruleNode)newLArc.otherNode(fromLNode);
            /* first we must match the arc to a possible arc leaving the fromHostNode .*/
            node nextHostNode = (nextLNode == null) ? null : location.findLMappedNode(nextLNode);

            var neighborHostArcs = fromHostNode.arcs.FindAll(a =>
                (a is arc && !location.arcs.Contains(a))
                && (arcMatches(newLArc, (arc)a, fromHostNode, nextHostNode, (newLArc.From == fromLNode))
                || arcMatchRelaxed(newLArc, (arc)a, location, fromHostNode, nextHostNode, (newLArc.From == fromLNode)))).Cast<arc>();
            //relaxelt
            if ((nextHostNode != null) || nextLNode == null)
                foreach (var HostArc in neighborHostArcs)
                {
                    var newLocation = location.copy();
                    newLocation.arcs[currentLArcIndex] = HostArc;
                    findNewStartElement(newLocation);
                }
            else
                foreach (var HostArc in neighborHostArcs)
                {
                    nextHostNode = HostArc.otherNode(fromHostNode);
                    if (!location.nodes.Contains(nextHostNode))
                    {
                        var newLocation = location.copy();
                        newLocation.arcs[currentLArcIndex] = HostArc;
                        if (nextLNode == null) findNewStartElement(newLocation);
                        else checkNode(newLocation, nextLNode, nextHostNode);
                    }
                }
        }

        #endregion

        #region Find Mapped Elements
        private graphElement findRMappedElement(designGraph RMapping, string GraphElementName)
        {
            var elt = R[GraphElementName];
            if (elt is hyperarc) return RMapping.hyperarcs[R.hyperarcs.IndexOf((hyperarc)elt)];
            if (elt is node) return RMapping.nodes[R.nodes.IndexOf((node)elt)];
            if (elt is arc) return RMapping.arcs[R.arcs.IndexOf((arc)elt)];
            throw new Exception("Graph element not found in rule's right-hand-side (GrammarRule.findMappedElement)");
        }

        private node findRMappedNode(designGraph location, node n)
        {
            return location.nodes[R.nodes.IndexOf(n)];
        }



        #endregion

        #region Apply Methods
        /// <summary>
        ///   Applies the rule to the specified host.
        /// </summary>
        /// <param name = "host">The host.</param>
        /// <param name = "Lmapping">The lmapping.</param>
        /// <param name = "parameters">The parameters.</param>
        public void apply(designGraph host, option Lmapping, double[] parameters)
        {
            /* First, update the global labels and variables. */
            if (OrderedGlobalLabels)
                updateOrderedGlobalLabels(Lmapping.globalLabelStartLoc, L.globalLabels,
                                          R.globalLabels, host.globalLabels);
            else updateLabels(L.globalLabels, R.globalLabels, host.globalLabels);
            updateVariables(L.globalVariables, R.globalVariables, host.globalVariables);

            /* Second set up the Rmapping, which is a list of nodes within the host
             * that corresponds in length and position to the nodes in R, just as 
             * Lmapping contains lists of nodes and arcs in the order they are 
             * referred to in L. */
            var Rmapping = designGraph.CreateEmptyLocationGraph(
                R.nodes.Count, R.arcs.Count, R.hyperarcs.Count);
            List<graphElement> danglingNeighbors;
            removeLdiffKfromHost(Lmapping, host, out danglingNeighbors);
            var newElements = addRdiffKtoD(Lmapping, host, Rmapping, Lmapping.positionTransform);
            /* these two lines correspond to the two "pushouts" of the double pushout algorithm. 
             *     L <--- K ---> R     this is from freeArc embedding (aka edNCE)
             *     |      |      |        |      this is from the parametric update
             *     |      |      |        |       |
             *   host <-- D ---> H1 ---> H2 ---> H3
             * The first step is to create D by removing the part of L not found in K (the commonality).
             * Second, we add the elements of R not found in K to D to create the updated host, H. Note, 
             * that in order to do this, we must know what subgraph of the host we are manipulating - this
             * is the location mapping found by the recognize function. */

            newElements.AddRange(freeArcEmbedding(Lmapping, host, Rmapping,
                                 danglingNeighbors.Where(x => (x is arc)).Cast<arc>()));
            newElements.AddRange(freeArcEmbedding(Lmapping, host, Rmapping,
                               danglingNeighbors.Where(x => (x is hyperarc)).Cast<hyperarc>()));
            /* however, there may still be a need to embed the graph with other arcs left dangling,
             * as in the "edge directed Node Controlled Embedding approach", which considers the neighbor-
             * hood of nodes and arcs of the recognized Lmapping. */
            updateAdditionalFunctions(Lmapping, host, Rmapping, parameters);
            foreach (var elt in newElements)
                if (elt is node && host.nodes.Any(n => n.name.Equals(elt.name)))
                    elt.name = host.makeUniqueNodeName(elt.name);
                else if (elt is arc && host.arcs.Any(a => a.name.Equals(elt.name)))
                    elt.name = host.makeUniqueArcName(elt.name);
                else if (elt is hyperarc && host.hyperarcs.Any(h => h.name.Equals(elt.name)))
                    elt.name = host.makeUniqueHyperArcName(elt.name);
        }

        private static void updateOrderedGlobalLabels(int stringStart, ICollection LLabels,
                                                      IEnumerable<string> RLabels, List<string> hostLabels)
        {
            hostLabels.RemoveRange(stringStart, LLabels.Count);
            hostLabels.InsertRange(stringStart, RLabels);
        }

        private static void updateVariables(IEnumerable<double> Lvariables, IEnumerable<double> Rvariables,
                                            List<double> hostvariables)
        {
            foreach (var a in Lvariables) /* do the same now, for the variables. */
                hostvariables.Remove(a); /* removing the labels in L but not in R...*/
            hostvariables.AddRange(Rvariables);
        }

        private static void updateLabels(IEnumerable<string> Llabels, IEnumerable<string> Rlabels,
                                         List<string> hostlabels)
        {
            foreach (var a in Llabels)
                hostlabels.Remove(a);
            hostlabels.AddRange(Rlabels);
        }

        private void removeLdiffKfromHost(option Lmapping, designGraph host, out List<graphElement> danglingNeighbors)
        {
            /* foreach node in L - see if it "is" also in R - if it is in R than it "is" part of the 
             * commonality subgraph K, and thus should not be deleted as it is part of the connectivity
             * information for applying the rule. Note that what we mean by "is" is that there is a
             * node with the same name. The name tag in a node is not superficial - it contains
             * useful connectivity information. We use it as a stand in for referencing the same object
             * this is different than the local lables which are used for recognition and the storage
             * any important design information. */
            danglingNeighbors = new List<graphElement>();
            foreach (var n in L.nodes.Where(n => ((ruleNode)n).MustExist && (!R.nodes.Exists(b => (b.name == n.name)))))
            {
                var nodeToRemove = Lmapping.findLMappedNode(n);
                danglingNeighbors = danglingNeighbors.Union(nodeToRemove.arcs).ToList();
                host.removeNode(nodeToRemove, false);
            }

            /* if a node with the same name does not exist in R, then it is safe to remove it.
             * The removeNode should is invoked with the "false false" switches of this function. 
             * This causes the arcs to be unaffected by the deletion of a connecting node. Why 
             * do this? It is important in the edNCE approach that is appended to the DPO approach
             * (see the function freeArcEmbedding) in connecting up a new R to the elements of L 
             * a node was connected to. */

            /* arcs and hyperarcs are removed in a similar way. */
            foreach (var a in L.arcs.Where(a => ((ruleArc)a).MustExist && (!R.arcs.Exists(b => (b.name == a.name)))))
                host.removeArc(Lmapping.findLMappedArc(a));
            foreach (var h in L.hyperarcs.Where(h => ((ruleHyperarc)h).MustExist && (!R.hyperarcs.Exists(b => (b.name == h.name)))))
                host.removeHyperArc(Lmapping.findLMappedHyperarc(h));
        }

        private List<graphElement> addRdiffKtoD(option Lmapping, designGraph D, designGraph Rmapping,
                                  double[,] positionT)
        {
            var newElements = new List<graphElement>();
            /* in this adding and gluing function, we are careful to distinguish
             * the Lmapping or recognized subgraph of L in the host - heretofore
             * known as Lmapping - from the mapping of new nodes and arcs of the
             * graph, which we call Rmapping. This is a complex function that goes
             * through 4 key steps:
             * 1. add the new nodes that are in R but not in L.
             * 2. update the remaining nodes common to L&R (aka K nodes) that might
             *    have had some label changes.
             * 3. add the new arcs that are in R but not in L. These may connect to
             *    either the newly connected nodes from step 1 or from the updated nodes
             *    of step 2. Also do this for the hyperarcs
             * 4. update the arcs common to L&R (aka K arcs) which might now be connected
             *    to new nodes created in step 1 (they are already connected to 
             *    nodes in K). Also make sure to update their labels just as K nodes were
             *    updated in step 2.*/
            for (var i = 0; i != R.nodes.Count; i++)
            {
                var rNode = (ruleNode)R.nodes[i];
                #region Step 1. add new nodes to D
                if (!L.nodes.Exists(b => (b.name == rNode.name)))
                {
                    var newNode = D.addNode(null, Type.GetType(rNode.TargetType, false)); /* create a new node. */
                    Rmapping.nodes[i] = newNode; /* make sure it's referenced in Rmapping. */
                    /* labels cannot be set equal, since that merely sets the reference of this list
                     * to the same value. So, we need to make a complete copy. */
                    rNode.copy(newNode);
                    /* give that new node a name and labels to match with the R. */
                    newElements.Add(newNode);
                    /* add the new node to the list of newElements that is returned by this function.*/
                    TransformPositionOfNode(newNode, positionT, rNode);
                    if (TransformNodeShapes && newNode.DisplayShape != null)
                        TransfromShapeOfNode(newNode, positionT);
                }
                #endregion
                #region Step 2. update K nodes

                else
                {
                    /* else, we may need to modify or update the node. In the pure graph
                     * grammar sense this is merely changing the local labels. In a way, 
                     * this is a like a set grammar. We need to find the labels in L that 
                     * are no longer in R and delete them, and we need to add the new labels
                     * that are in R but not already in L. The ones common to both are left
                     * alone. */
                    var LNode = L.nodes.FirstOrDefault(n => (rNode.name.Equals(n.name)));
                    /* find index of the common node in L...*/
                    var KNode = Lmapping.findLMappedNode(LNode); /*...and then set Knode to the actual node in D.*/
                    Rmapping.nodes[i] = KNode; /*also, make sure that the Rmapping is to this same node.*/
                    updateLabels(LNode.localLabels, rNode.localLabels, KNode.localLabels);
                    updateVariables(LNode.localVariables, rNode.localVariables,
                                    KNode.localVariables);
                    if (TransformNodePositions)
                        TransformPositionOfNode(KNode, positionT, rNode);
                    if (rNode.DisplayShape != null && TransformNodeShapes)
                    {
                        if (KNode.DisplayShape == null
                            || rNode.DisplayShape.GetType() == KNode.DisplayShape.GetType())
                            KNode.DisplayShape = rNode.DisplayShape.Copy(KNode);
                        TransfromShapeOfNode(KNode, positionT);
                    }
                }

                #endregion
            }

            /* now moving onto the arcs (a little more challenging actually). */
            for (var i = 0; i != R.arcs.Count; i++)
            {
                var rArc = (ruleArc)R.arcs[i];

                #region Step 3. add new arcs to D

                if (!L.arcs.Exists(b => (b.name == rArc.name)))
                {
                    #region setting up where arc comes from

                    node from;
                    if (rArc.From == null)
                        from = null;
                    else if (L.nodes.Exists(b => (b.name == rArc.From.name)))
                    /* if the arc is coming from a node that is in K, then it must've been
             * part of the location (or Lmapping) that was originally recognized.*/
                    {
                        var LNode = L.nodes.FirstOrDefault(b => (rArc.From.name == b.name));
                        /* therefore we need to find the position/index of that node in L. */

                        from = Lmapping.findLMappedNode(LNode);
                        /* and that index1 will correspond to its image in Lmapping. Following,
                         * the Lmapping reference, we get to the proper node reference in D. */
                    }
                    else
                    /* if not in K then the arc connects to one of the new nodes that were 
             * created at the beginning of this function (see step 1) and is now
             * one of the references in Rmapping. */
                    {
                        var RNode = R.nodes.FirstOrDefault(b => (rArc.From.name == b.name));
                        from = findRMappedNode(Rmapping, RNode);
                    }

                    #endregion

                    #region setting up where arc goes to

                    /* this code is the same of "setting up where arc comes from - except here
                     * we do the same for the to connection of the arc. */
                    node to;
                    if (rArc.To == null)
                        to = null;
                    else if (L.nodes.Exists(b => (b.name == rArc.To.name)))
                    {
                        var LNode = L.nodes.FirstOrDefault(b => (rArc.To.name == b.name));
                        to = Lmapping.findLMappedNode(LNode);
                    }
                    else
                    {
                        var RNode = R.nodes.FirstOrDefault(b => (rArc.To.name == b.name));
                        to = findRMappedNode(Rmapping, RNode);
                    }

                    #endregion

                    var newArc = D.addArc(from, to, rArc.name, Type.GetType(rArc.TargetType, false));
                    Rmapping.arcs[i] = newArc;
                    rArc.copy(newArc);
                    newElements.Add(newArc);
                    /* add the new arc to the list of newElements that is returned by this function.*/
                }
                #endregion
                #region Step 4. update K arcs

                else
                {
                    /* first find the position of the same arc in L. */
                    var currentLArc = (ruleArc)L.arcs.FirstOrDefault(b => (rArc.name == b.name));
                    var mappedArc = Lmapping.findLMappedArc(currentLArc);
                    /* then find the actual arc in D that is to be changed.*/
                    /* one very subtle thing just happend here! (07/06/06) if the direction is reversed, then
                     * you might mess-up this Karc. We need to establish a boolean so that references 
                     * incorrectly altered. */
                    var KArcIsReversed =
                        ((Lmapping.nodes.IndexOf(mappedArc.From) != L.nodes.IndexOf(currentLArc.From)) &&
                         (Lmapping.nodes.IndexOf(mappedArc.To) != L.nodes.IndexOf(currentLArc.To)));


                    Rmapping.arcs[i] = mappedArc;
                    /*similar to Step 3., we first find how to update the from and to. */
                    if ((currentLArc.From != null) && (rArc.From == null))
                    {
                        /* this is a rare case in which you actually want to break an arc from its attached 
                         * node. If the corresponding L arc is not null only! if it is null then it may be 
                         * actually connected to something in the host, and we are in no place to remove it. */
                        if (KArcIsReversed) mappedArc.To = null;
                        else mappedArc.From = null;
                    }
                    else if (rArc.From != null)
                    {
                        var RNode = R.nodes.FirstOrDefault(b => (rArc.From.name == b.name));
                        /* find the position of node that this arc is supposed to connect to in R */
                        if (KArcIsReversed) mappedArc.To = findRMappedNode(Rmapping, RNode);
                        else mappedArc.From = findRMappedNode(Rmapping, RNode);
                    }
                    /* now do the same for the To connection. */
                    if ((currentLArc.To != null) && (rArc.To == null))
                    {
                        if (KArcIsReversed) mappedArc.From = null;
                        else mappedArc.To = null;
                    }
                    else if (rArc.To != null)
                    {
                        var RNode = R.nodes.FirstOrDefault(b => (rArc.To.name == b.name));
                        if (KArcIsReversed) mappedArc.From = findRMappedNode(Rmapping, RNode);
                        else mappedArc.To = findRMappedNode(Rmapping, RNode);
                    }
                    /* just like in Step 2, we may need to update the labels of the arc. */
                    updateLabels(currentLArc.localLabels, rArc.localLabels, mappedArc.localLabels);
                    updateVariables(currentLArc.localVariables, rArc.localVariables,
                                    mappedArc.localVariables);
                    if (!mappedArc.directed || (mappedArc.directed && currentLArc.directionIsEqual))
                        mappedArc.directed = rArc.directed;
                    /* if the KArc is currently undirected or if it is and direction is equal
                     * then the directed should be inherited from R. */
                    if (!mappedArc.doublyDirected || (mappedArc.doublyDirected && currentLArc.directionIsEqual))
                        mappedArc.doublyDirected = rArc.doublyDirected;
                    if (rArc.DisplayShape != null)
                        mappedArc.DisplayShape = rArc.DisplayShape.Copy(mappedArc);
                }

                #endregion
            }

            /* finally, the hyperarcs . */
            for (var i = 0; i != R.hyperarcs.Count; i++)
            {
                var rHyperarc = (ruleHyperarc)R.hyperarcs[i];
                var lHyperarc = L.hyperarcs.FirstOrDefault(b => (b.name == rHyperarc.name));

                #region Step 5. add new hyperarcs to D
                if (lHyperarc == null)
                {
                    var mappedNodes =
                        rHyperarc.nodes.Select(a => findRMappedElement(Rmapping, a.name)).Cast<node>().ToList();

                    var newHa = D.addHyperArc(mappedNodes, rHyperarc.name, Type.GetType(rHyperarc.TargetType, false));
                    Rmapping.hyperarcs[i] = newHa;
                    rHyperarc.copy(newHa);
                    newElements.Add(newHa);
                    /* add the new hyperarc to the list of newElements that is returned by this function.*/
                }
                #endregion
                #region Step 6. update K hyperarcs
                else
                {
                    var mappedHyperarc = Lmapping.findLMappedHyperarc(lHyperarc);
                    var intersectNodeNames = lHyperarc.nodes.Select(a => a.name);
                    intersectNodeNames = intersectNodeNames.Intersect(rHyperarc.nodes.Select(a => a.name));
                    foreach (var n in lHyperarc.nodes.Where(n => !intersectNodeNames.Contains(n.name)))
                    {
                        mappedHyperarc.DisconnectFrom(Lmapping.findLMappedNode(n));
                    }
                    foreach (var n in rHyperarc.nodes.Where(n => !intersectNodeNames.Contains(n.name)))
                    {
                        mappedHyperarc.ConnectTo(findRMappedNode(Rmapping, n));
                    }

                    Rmapping.hyperarcs[i] = mappedHyperarc;
                    /* just like in Step 2 and 4, we may need to update the labels of the hyperarc. */
                    updateLabels(lHyperarc.localLabels, rHyperarc.localLabels, mappedHyperarc.localLabels);
                    updateVariables(lHyperarc.localVariables, rHyperarc.localVariables,
                                    mappedHyperarc.localVariables);
                    if (rHyperarc.DisplayShape != null)
                        mappedHyperarc.DisplayShape = rHyperarc.DisplayShape.Copy(mappedHyperarc);
                }
                #endregion
            }
            return newElements;
        }

        private IEnumerable<graphElement> freeArcEmbedding(option Lmapping, designGraph host, designGraph Rmapping, IEnumerable<hyperarc> danglingNeighbors)
        {
            var newElements = new List<graphElement>();
            foreach (var dangleHyperArc in danglingNeighbors)
            {
                List<node> neighborNodes;
                if (embeddingRule.hyperArcIsFree(dangleHyperArc, host, out neighborNodes))
                {
                    #region dangleArc is a hyperArc
                    foreach (var eRule in embeddingRules)
                    {
                        var newNodeToConnect = string.IsNullOrWhiteSpace(eRule.RNodeName)
                            ? null
                            : (node)findRMappedElement(Rmapping, eRule.RNodeName);
                        var nodeRemovedinLdiffRDeletion = string.IsNullOrWhiteSpace(eRule.LNodeName)
                            ? null
                            : (node)Lmapping.findLMappedElement(eRule.LNodeName);

                        if (eRule.ruleIsRecognized(dangleHyperArc, neighborNodes, nodeRemovedinLdiffRDeletion))
                        {
                            if (eRule.allowArcDuplication)
                            {
                                var newNeighborNodes = new List<node>(neighborNodes);
                                newNeighborNodes.Add(newNodeToConnect);
                                var newHa = dangleHyperArc.copy();
                                host.addHyperArc(newHa, newNeighborNodes);
                                newElements.Add(newHa);
                                /* add the new hyperarc to the list of newElements that is returned by this function.*/
                            }
                            else
                            {
                                dangleHyperArc.ConnectTo(newNodeToConnect);
                                break;
                            }
                        }
                    }

                    #endregion
                }
            }

            #region clean up (i.e. delete) any freeArcs that are still in host.arcs
            foreach (var dangleArc in danglingNeighbors)
            {
                dangleArc.nodes.RemoveAll(n => !host.nodes.Contains(n));
                if (dangleArc.nodes.Count == 0)
                    host.removeHyperArc(dangleArc);
            }
            #endregion
            return newElements;
        }

        private IEnumerable<graphElement> freeArcEmbedding(option Lmapping, designGraph host, designGraph Rmapping, IEnumerable<arc> danglingNeighbors)
        {
            /* There are nodes in host which may have been left dangling due to the fact that their 
             * connected nodes were part of the L-R deletion. These now need to be either 1) connected
             * up to their new nodes, 2) their references to old nodes need to be changed to null if 
             * intentionally left dangling, or 3) the arcs are to be removed. In the function 
             * removeLdiffKfromHost we remove old nodes but leave their references intact on their 
             * connected arcs. This allows us to find the list of freeArcs that are candidates 
             * for the embedding rules. Essentially, we are capturing the neighborhood within the host 
             * for the rule application, that is the arcs that are affected by the deletion of the L-R
             * subgraph. Should one check non-dangling non-neighborhood arcs? No, this would seem to 
             * cause a duplication of such an arc. Additionally, what node in host should the arc remain 
             * attached to?  There seems to be no rigor in applying these more global (non-neighborhood) 
             * changes within the literature as well for the general edNCE method. */
            var newElements = new List<graphElement>();

            foreach (var dangleArc in danglingNeighbors)
            {
                sbyte freeEndIdentifier;
                node neighborNode;

                if (embeddingRule.arcIsFree(dangleArc, host, out freeEndIdentifier, out neighborNode))
                {
                    #region dangleArc is an arc
                    /* For each of the embedding rules, we see if it is applicable to the identified freeArc.
                     * The rule then modifies the arc by simply pointing it to the new node in R as indicated
                     * by the embedding Rule's RNodeName. NOTE: the order of the rules are important. If two
                     * rules are 'recognized' with the same freeArc only the first one will modify it, as it 
                     * will then remove it from the freeArc list. This is useful in that rules may have precedence
                     * to one another. There is an exception if the rule has allowArcDuplication set to true, 
                     * since this would simply create a copy of the arc. */
                    foreach (var eRule in embeddingRules)
                    {
                        var newNodeToConnect = string.IsNullOrWhiteSpace(eRule.RNodeName)
                            ? null
                            : (node)findRMappedElement(Rmapping, eRule.RNodeName);
                        var nodeRemovedinLdiffRDeletion = string.IsNullOrWhiteSpace(eRule.LNodeName)
                            ? null
                            : (node)Lmapping.findLMappedElement(eRule.LNodeName);

                        if (eRule.ruleIsRecognized(freeEndIdentifier, dangleArc,
                                                   neighborNode, nodeRemovedinLdiffRDeletion))
                        {
                            #region  set up new connection points

                            node toNode;
                            node fromNode;
                            if (freeEndIdentifier >= 0)
                            {
                                if (eRule.newDirection >= 0)
                                {
                                    toNode = newNodeToConnect;
                                    fromNode = dangleArc.From;
                                }
                                else
                                {
                                    toNode = dangleArc.From;
                                    fromNode = newNodeToConnect;
                                }
                            }
                            else
                            {
                                if (eRule.newDirection <= 0)
                                {
                                    fromNode = newNodeToConnect;
                                    toNode = dangleArc.To;
                                }
                                else
                                {
                                    fromNode = dangleArc.To;
                                    toNode = newNodeToConnect;
                                }
                            }

                            #endregion

                            #region if making a copy of arc, duplicate it and all the characteristics

                            if (eRule.allowArcDuplication)
                            {
                                /* under the allowArcDuplication section, we will be making a copy of the 
                                 * freeArc. This seems a little error-prone at first, since if there is only
                                 * one rule that applies to freeArc then we will have good copy and the old
                                 * bad copy. However, at the end of this function, we go through the arcs again
                                 * and remove any arcs that still appear free. This also serves the purpose to 
                                 * delete any dangling nodes that were not recognized in any rules. */
                                var newArc = dangleArc.copy();
                                host.addArc(newArc, fromNode, toNode);
                                newElements.Add(newArc);
                                /* add the new arc to the list of newElements that is returned by this function.*/
                            }
                            #endregion

                            #region else, just update the old freeArc

                            else
                            {
                                dangleArc.From = fromNode;
                                dangleArc.To = toNode;
                                break; /* skip to the next arc */
                                /* this is done so that no more embedding rules will be checked with this freeArc.*/
                            }

                            #endregion
                        }
                    }
                    #endregion
                }
            }

            #region clean up (i.e. delete) any freeArcs that are still in host.
            foreach (var dangleArc in
                danglingNeighbors.Where(dangleArc =>
                    ((dangleArc.From != null && !host.nodes.Contains(dangleArc.From))
                    || (dangleArc.To != null && !host.nodes.Contains(dangleArc.To)))))
            {
                host.removeArc(dangleArc);
            }
            #endregion
            return newElements;
        }

        /// <summary>
        ///   The final update step is to invoke additional functions for the rule. 
        ///   These are traditionally called the Parametric Application Functions, but
        ///   they can do any custom modifications to the host.
        /// </summary>
        /// <param name = "Lmapping">The lmapping.</param>
        /// <param name = "host">The host.</param>
        /// <param name = "Rmapping">The rmapping.</param>
        /// <param name = "parameters">The parameters.</param>
        private void updateAdditionalFunctions(option Lmapping, designGraph host,
                                               designGraph Rmapping, IEnumerable parameters)
        {
            /* If you get an error in this function, it is most likely due to 
             * an error in the compilted DLLofFunctions. Open your code for the
             * rules and leave this untouched - it's simply the messenger. */
            foreach (var applyFunction in applyFuncs)
                try
                {
                    object[] applyArguments;
                    if (applyFunction.GetParameters().GetLength(0) == 2)
                        applyArguments = new object[] { Lmapping, host };
                    else if (applyFunction.GetParameters().GetLength(0) == 3)
                        applyArguments = new object[] { Lmapping, host, Rmapping };
                    else if (applyFunction.GetParameters().GetLength(0) == 4)
                        applyArguments = new object[] { Lmapping, host, Rmapping, parameters };
                    else applyArguments = new object[] { Lmapping, host, Rmapping, parameters, this };
                    applyFunction.Invoke(DLLofFunctions, applyArguments);
                }
                catch (Exception e)
                {
                    SearchIO.MessageBoxShow("Error in additional apply function: " + applyFunction.Name +
                                            ".\nSee output bar for details.", "Error in  " + applyFunction.Name, "Error");
                    SearchIO.output("Error in function: " + applyFunction.Name);
                    //+ "\n" +ErrorLogger.MakeErrorString(e, false));
                    SearchIO.output("Exception in : " + e.InnerException.TargetSite.Name);
                    SearchIO.output("Error              : " + e.InnerException.Message);
                    SearchIO.output("Stack Trace     	: " + e.InnerException.StackTrace);
                }

        #endregion
        }

    }
}