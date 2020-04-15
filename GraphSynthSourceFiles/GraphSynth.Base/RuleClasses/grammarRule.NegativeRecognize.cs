/*************************************************************************
 *     This grammarRule.NegativeRecognize.cs file partially defines the 
 *     grammarRule class (also partially defined in grammarRule.Basic.cs, 
 *     grammarRule.RecognizeApply.cs and grammarRule.ShapeMethods.cs)
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

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
        [XmlIgnore]
        internal Boolean ContainsNegativeElements
        {
            get
            {
                return ((L.nodes.Any(n => ((ruleNode)n).NotExist))
                        || (L.arcs.Any(a => ((ruleArc)a).NotExist))
                        || (L.hyperarcs.Any(h => ((ruleHyperarc)h).NotExist)));
            }
        }
        #region First Pass: Find All elements that are supposed to be in host
        /* this is similar for rules with nonexistence graph elements*/
        private void FindPositiveStartElementAvoidNegatives(option location)
        {
            #region Case #1: Location found! No empty slots left in the location
            /* this is the only way to properly exist the recursive loop. */
            if (!L.nodes.Any(n => ((ruleNode)n).MustExist && location.findLMappedNode(n) == null)
                && !L.arcs.Any(a => ((ruleArc)a).MustExist && location.findLMappedArc(a) == null)
                && !L.hyperarcs.Any(n => ((ruleHyperarc)n).MustExist && location.findLMappedHyperarc(n) == null))
            {
                /* as a recursive function, we first check how the recognition process terminates. If all nodes,
                 * hyperarcs and arcs within location have been filled with references to elements in the host, 
                 * then we've found a location...well maybe. More details are described in the LocationFound function. */
                if (!FinalRuleChecks(location) && !FinalRuleCheckRelaxed(location)) return;
                Boolean resultNegativeNotFulfilled;
                lock (AllNegativeElementsFound)
                {
                    AllNegativeElementsFound = false;
                    negativeRelaxation = location.Relaxations.copy();
                    findNegativeStartElement(location);
                    resultNegativeNotFulfilled = !(bool) AllNegativeElementsFound;
                }
                if (resultNegativeNotFulfilled)
                {
                    var locCopy = location.copy();
                    locCopy.Relaxations = negativeRelaxation;
                    lock (options) { options.Add(locCopy); }
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
            var startHyperArc = (ruleHyperarc)L.hyperarcs.FirstOrDefault(ha => ((ruleHyperarc)ha).MustExist
                && ((location.findLMappedHyperarc(ha) != null)
                && (ha.nodes.Any(n => ((ruleNode)n).MustExist
                    && (location.findLMappedNode(n) == null)))));
            if (startHyperArc != null)
            {
                var hostHyperArc = location.findLMappedHyperarc(startHyperArc);
                var newLNode = (ruleNode)startHyperArc.nodes.FirstOrDefault(n => ((ruleNode)n).MustExist
                    && (location.findLMappedNode(n) == null));
                foreach (var n in hostHyperArc.nodes.Where(n => !location.nodes.Contains(n)))
                    checkNodeAvoidNegatives(location.copy(), newLNode, n);
                return;
            }
            #endregion
            #region Case #3: build off of a node found so far - by looking for unfulfilled arcs
            /* as stated above, the quickest approach is to build from elements that have already been found.
             * Therefore, we see if there are any nodes already matched to a node in L that has an arc in L
             * that has yet to be matched with a host arc. This is more efficient than the last 3 cases 
             * because they look through the entire host, which is potentially large. */
            var startNode = (ruleNode)L.nodes.FirstOrDefault(n => ((ruleNode)n).MustExist
                && ((location.findLMappedNode( n) != null)
                && (n.arcs.Any(a =>
                    (((a is ruleHyperarc) && ((ruleHyperarc)a).MustExist)
                    || ((a is ruleArc) && ((ruleArc)a).MustExist))
                    && (location.findLMappedElement(a) == null)))));
            /* is there a node already matched (which would only occur if your recursed to get here) that has an
             * unrecognized arc attaced to it. If yes, try all possible arcs in the host with the one that needs
             * to be fulfilled in L. */
            if (startNode != null)
            {
                var newLArc = startNode.arcs.FirstOrDefault(a =>
                    (((a is ruleHyperarc) && ((ruleHyperarc)a).MustExist)
                    || ((a is ruleArc) && ((ruleArc)a).MustExist))
                    && (location.findLMappedElement( a) == null));
                if (newLArc is ruleHyperarc)
                    checkHyperArcAvoidNegatives(location, startNode, location.findLMappedNode( startNode), (ruleHyperarc)newLArc);
                else if (newLArc is ruleArc)
                    checkArcAvoidNegatives(location, startNode, location.findLMappedNode( startNode), (ruleArc)newLArc);
                return;
            }
            #endregion
            #region Case 4: Check entire host for a matching hyperarc
            /* if the above cases didn't match we try to match a hyperarc in the L to any in the host. Since the
             * prior three cases have conditions which require some non-nulls in the location, this is likely where the 
             * process will start when invoked from line 79 of recognize above. Hyperarcs are most efficient to start from 
             * since there are likely fewer hyperarcs in the host than nodes, or arcs. */
            startHyperArc = (ruleHyperarc)L.hyperarcs.FirstOrDefault(ha => ((ruleHyperarc)ha).MustExist
                && (location.findLMappedHyperarc( ha) == null));
            if (startHyperArc != null)
            {
                if (_in_parallel_)
                    Parallel.ForEach(host.hyperarcs, hostHyperArc =>
                    {
                        if (!location.hyperarcs.Contains(hostHyperArc))
                            checkHyperArcAvoidNegatives(location.copy(), startHyperArc, hostHyperArc);
                    });
                else
                    foreach (var hostHyperArc in
                        host.hyperarcs.Where(hostHyperArc => !location.hyperarcs.Contains(hostHyperArc)))
                    {
                        checkHyperArcAvoidNegatives(location.copy(), startHyperArc, hostHyperArc);
                    }
                return;
            }
            #endregion
            #region Case 5: Check entire host for a matching node
            /* If no other hyperarcs to recognize look to a unlocated node. If one gets here then none of the above
             * three conditions were met (obviously) but this also implies that there are multiple components in the
             * LHS, and we are now jumping to a new one with this. This is potentially time intensive if there are
             * a lot of nodes in the host. We allow for the possibility that this recognition can be done in parallel. */
            startNode = (ruleNode)L.nodes.FirstOrDefault(n => ((ruleNode)n).MustExist && (location.findLMappedNode( n) == null));
            if (startNode != null)
            {
                if (_in_parallel_)
                    Parallel.ForEach(host.nodes, hostNode =>
                        {
                            if (!location.nodes.Contains(hostNode))
                                checkNodeAvoidNegatives(location.copy(), startNode, hostNode);
                        });
                else foreach (var hostNode in host.nodes
                    .Where(hostNode => !location.nodes.Contains(hostNode)))
                    {
                        checkNodeAvoidNegatives(location.copy(), startNode, hostNode);
                    }
                return;
            }
            #endregion
            #region Case 6: Check entire host for a matching arc
            var looseArc = (ruleArc)L.arcs.FirstOrDefault(a => ((ruleArc)a).MustExist && (location.findLMappedArc( a) == null));
            /* the only way one can get here is if there are one or more arcs NOT connected to any nodes
             * in L - a floating arc, dangling on both sides, like an eyelash. */
            if (looseArc != null)
                if (_in_parallel_)
                    Parallel.ForEach(host.arcs, hostArc =>
                                     {
                                         if ((!location.arcs.Contains(hostArc)) && (!location.nodes.Contains(hostArc.From))
                                             && (!location.nodes.Contains(hostArc.To))
                                             && (arcMatches(looseArc, hostArc) || arcMatchRelaxed(looseArc, hostArc, location)))
                                         {
                                             var newLocation = location.copy();
                                             newLocation.arcs[L.arcs.IndexOf(looseArc)] = hostArc;
                                             FindPositiveStartElementAvoidNegatives(newLocation);
                                         }
                                     });
                else
                    foreach (var hostArc in host.arcs)
                        if ((!location.arcs.Contains(hostArc)) && (!location.nodes.Contains(hostArc.From))
                                 && (!location.nodes.Contains(hostArc.To))
                                 && (arcMatches(looseArc, hostArc) || arcMatchRelaxed(looseArc, hostArc, location)))
                        {
                            var newLocation = location.copy();
                            newLocation.arcs[L.arcs.IndexOf(looseArc)] = hostArc;
                            FindPositiveStartElementAvoidNegatives(newLocation);
                        }
            #endregion
        }

        /* this is a boxed Boolean since the lock statement requires a reference type */
        private static object AllNegativeElementsFound = false;

        private void checkNodeAvoidNegatives(option location, ruleNode LNode, node hostNode)
        {
            if (!nodeMatches(LNode, hostNode, location) && !nodeMatchRelaxed(LNode, hostNode, location))
                return;
            location.nodes[L.nodes.IndexOf(LNode)] = hostNode;
            var newLArc = LNode.arcs.FirstOrDefault(a =>
                                          (((a is ruleHyperarc) && ((ruleHyperarc)a).MustExist)
                                           || ((a is ruleArc) && ((ruleArc)a).MustExist))
                                          && (location.findLMappedElement( a) == null));
            if (newLArc == null) FindPositiveStartElementAvoidNegatives(location);
            else if (newLArc is ruleHyperarc)
                checkHyperArcAvoidNegatives(location, LNode, hostNode, (ruleHyperarc)newLArc);
            else if (newLArc is ruleArc)
                checkArcAvoidNegatives(location, LNode, hostNode, (ruleArc)newLArc);
        }


        private void checkHyperArcAvoidNegatives(option location, ruleHyperarc LHyperArc, hyperarc hostHyperArc)
        {
            if (!hyperArcMatches(LHyperArc, hostHyperArc) && !hyperArcMatchRelaxed(LHyperArc, hostHyperArc, location))
                return;
            location.hyperarcs[L.hyperarcs.IndexOf(LHyperArc)] = hostHyperArc;
            var newLNode = (ruleNode)LHyperArc.nodes.FirstOrDefault(n => ((ruleNode)n).MustExist
                                                               && (location.findLMappedNode( n) == null));
            if (newLNode == null) FindPositiveStartElementAvoidNegatives(location);
            else
                foreach (var n in hostHyperArc.nodes.Where(n => !location.nodes.Contains(n)))
                    checkNodeAvoidNegatives(location.copy(), newLNode, n);
        }

        private void checkHyperArcAvoidNegatives(option location, ruleNode fromLNode,
            node fromHostNode, ruleHyperarc newLHyperArc)
        {
            var otherConnectedNodes = (from n in newLHyperArc.nodes
                                       where ((n != fromLNode) && ((ruleNode)n).MustExist && (location.findLMappedNode( n) != null))
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
                checkHyperArcAvoidNegatives(location.copy(), newLHyperArc, hostHyperArc);
        }

        private void checkArcAvoidNegatives(option location, node fromLNode, node fromHostNode,
            ruleArc newLArc)
        {
            var currentLArcIndex = L.arcs.IndexOf(newLArc);
            /* so, currentLArcIndex now, points to a LArc that has yet to be recognized. What we do from
            * this point depends on whether that LArc points to an L node we have yet to recognize, an L
            * node we have recognized, or null. */
            var nextLNode = (ruleNode)newLArc.otherNode(fromLNode);
            /* first we must match the arc to a possible arc leaving the fromHostNode .*/
            var nextHostNode = (nextLNode == null || nextLNode.NotExist) ? null
                               : location.findLMappedNode(nextLNode);

            var neighborHostArcs = fromHostNode.arcs.FindAll(a =>
                (a is arc && !location.arcs.Contains(a))
                && (arcMatches(newLArc, (arc)a, fromHostNode, nextHostNode, (newLArc.From == fromLNode))
                || arcMatchRelaxed(newLArc, (arc)a, location, fromHostNode, nextHostNode, (newLArc.From == fromLNode)))).Cast<arc>();
            //relaxelt
            if ((nextHostNode != null) || newLArc.nullMeansNull)
                foreach (var HostArc in neighborHostArcs)
                {
                    var newLocation = location.copy();
                    newLocation.arcs[currentLArcIndex] = HostArc;
                    FindPositiveStartElementAvoidNegatives(newLocation);
                }
            else
                foreach (var HostArc in neighborHostArcs)
                {
                    nextHostNode = HostArc.otherNode(fromHostNode);
                    if (!location.nodes.Contains(nextHostNode))
                    {
                        var newLocation = location.copy();
                        newLocation.arcs[currentLArcIndex] = HostArc;
                        if (nextLNode == null || nextLNode.NotExist) FindPositiveStartElementAvoidNegatives(newLocation);
                        else checkNodeAvoidNegatives(newLocation, nextLNode, nextHostNode);
                    }
                }
        }
        #endregion

        private Relaxation negativeRelaxation;
        #region Second Pass: Find all elements that are not supposed to be in the host
        private void findNegativeStartElement(option location)
        {
            if ((bool)AllNegativeElementsFound) return; /* another sub-branch found a match to the negative elements.
                                                                 * There's no point in finding more than one, so this statement
                                                                 * aborts the search down this branch. */
            #region Case #1: Location found! No empty slots left in the location
            /* this is the only way to properly exit the recursive loop. */
            if (!location.nodes.Contains(null) && !location.arcs.Contains(null) && !location.hyperarcs.Contains(null))
            {
                if (FinalRuleChecks(location))
                    if (!InvalidateWithRelaxation(location))
                        AllNegativeElementsFound = true;
                return;
            }
            #endregion
            #region Case #2: build off of a hyperarc found so far - by looking for unfulfilled nodes
            /* the quickest approach to finding a new element in the LHS to host subgraph matching is to build
             * directly off of elements found so far. This is because we don't need to check amongst ALL elements in the
             * host (as is the case in the last three cases below). In this case we start with any hyperarcs
             * that have already been matched to one in the host, and see if it connects to any nodes that
             * have yet to be matched. */
            var startHyperArc = (ruleHyperarc)L.hyperarcs.FirstOrDefault(ha => ((location.findLMappedHyperarc( ha) != null)
                && (ha.nodes.Any(n => (location.findLMappedNode( n) == null)))));
            if (startHyperArc != null)
            {
                var hostHyperArc = location.findLMappedHyperarc( startHyperArc);
                var newLNode = (ruleNode)startHyperArc.nodes.FirstOrDefault(n => (location.findLMappedNode( n) == null));
                foreach (var n in hostHyperArc.nodes.Where(n => !location.nodes.Contains(n)))
                {
                    checkForNegativeNode(location.copy(), newLNode, n);
                    if ((bool)AllNegativeElementsFound) return;
                }
                return;
            }
            #endregion
            #region Case #2.5: build off of a partially matched arc
            /* unlike the other renditions of this function (findNewStartElement, 
             * findPositiveStartElementAvoidNegatives) this has a situation in which an arc has only been 
             * partially matched but the connected nodes were not touched because they were negative elements.*/
            var startArc = (ruleArc)L.arcs.FirstOrDefault(a => (location.findLMappedArc( a) != null) &&
                a.To != null && location.findLMappedNode( a.To) == null);
            if (startArc != null)
            {
                var hostArc = location.findLMappedArc( startArc);
                if ((hostArc.To != null) && !location.nodes.Contains(hostArc.To))
                    checkForNegativeNode(location.copy(), (ruleNode)startArc.To, hostArc.To);
                else if (!startArc.directionIsEqual && hostArc.From != null &&
                   !location.nodes.Contains(hostArc.From))
                    checkForNegativeNode(location.copy(), (ruleNode)startArc.To, hostArc.From);
                return;
            }
            startArc = (ruleArc)L.arcs.FirstOrDefault(a => (location.findLMappedArc( a) != null) &&
                a.From != null && location.findLMappedNode( a.From) == null);
            if (startArc != null)
            {
                var hostArc = location.findLMappedArc( startArc);
                if ((hostArc.From != null) && !location.nodes.Contains(hostArc.From))
                    checkForNegativeNode(location.copy(), (ruleNode)startArc.From, hostArc.From);
                else if (!startArc.directionIsEqual && hostArc.To != null &&
                   !location.nodes.Contains(hostArc.To))
                    checkForNegativeNode(location.copy(), (ruleNode)startArc.From, hostArc.To);
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
                    checkForNegativeHyperArc(location, startNode, location.findLMappedNode(startNode), (ruleHyperarc)newLArc);
                else if (newLArc is ruleArc)
                    checkForNegativeArc(location, startNode, location.findLMappedNode(startNode), (ruleArc)newLArc);
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
                foreach (var hostHyperArc in
                    host.hyperarcs.Where(hostHyperArc => !location.hyperarcs.Contains(hostHyperArc)))
                {
                    checkForNegativeHyperArc(location.copy(), startHyperArc, hostHyperArc);
                    if ((bool)AllNegativeElementsFound) return;
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
                foreach (var hostNode in host.nodes.Where(hostNode => !location.nodes.Contains(hostNode)))
                {
                    checkForNegativeNode(location.copy(), startNode, hostNode);
                    if ((bool)AllNegativeElementsFound) return;
                }
                return;
            }
            #endregion
            #region Case 6: Check entire host for a matching arc
            var looseArc = (ruleArc)L.arcs.FirstOrDefault(a => (location.findLMappedArc(a) == null));
            /* the only way one can get here is if there are one or more arcs NOT connected to any nodes
             * in L - a floating arc, dangling on both sides, like an eyelash. */
            if (looseArc != null)
                foreach (var hostArc in host.arcs)
                {
                    if (!location.arcs.Contains(hostArc) && !location.nodes.Contains(hostArc.From)
                             && !location.nodes.Contains(hostArc.To) && arcMatches(looseArc, hostArc))
                    {
                        var newLocation = location.copy();
                        newLocation.arcs[L.arcs.IndexOf(looseArc)] = hostArc;
                        findNegativeStartElement(newLocation);
                    }
                    if ((bool)AllNegativeElementsFound) return;
                }
            #endregion
        }

        private Boolean InvalidateWithRelaxation(option location)
        {
            if (negativeRelaxation.NumberAllowable == 0) return false;
            var ruleNegElts = new List<graphElement>();
            ruleNegElts.AddRange(L.nodes.FindAll(n => ((ruleNode)n).NotExist));
            ruleNegElts.AddRange(L.arcs.FindAll(a => ((ruleArc)a).NotExist));
            ruleNegElts.AddRange(L.hyperarcs.FindAll(h => ((ruleHyperarc)h).NotExist));
            foreach (var ruleElt in ruleNegElts)
            {
                var hostElt = location.findLMappedElement(ruleElt);
                if (ruleElt.localLabels.Count < hostElt.localLabels.Count)
                {
                    var rContainsAll =
                        negativeRelaxation.FirstOrDefault(
                            r => r.Matches(Relaxations.Contains_All_Local_Labels_Imposed, ruleElt));
                    if (rContainsAll != null)
                    {
                        negativeRelaxation.NumberAllowable--;
                        rContainsAll.NumberAllowed--;
                        negativeRelaxation.FulfilledItems.Add(
                            new RelaxItem(Relaxations.Contains_All_Local_Labels_Imposed, 1, ruleElt,
                                hostElt.localLabels.Count.ToString(CultureInfo.InvariantCulture)));
                        return true;
                    }
                }
                if ((ruleElt is ruleNode) && (!((ruleNode)ruleElt).strictDegreeMatch) && ((node)ruleElt).degree != ((node)hostElt).degree)
                {
                    var rStrictDegree =
                        negativeRelaxation.FirstOrDefault(r => r.Matches(Relaxations.Strict_Degree_Match_Imposed, ruleElt));
                    if (rStrictDegree != null)
                    {
                        negativeRelaxation.NumberAllowable--;
                        rStrictDegree.NumberAllowed--;
                        negativeRelaxation.FulfilledItems.Add(new RelaxItem(Relaxations.Strict_Degree_Match_Imposed, 1,
                                                                              ruleElt,
                                                                              ((node)hostElt).degree.ToString(
                                                                                  CultureInfo.InvariantCulture)));
                        return true;
                    }
                }
                if ((ruleElt is ruleHyperarc) && (!((ruleHyperarc)ruleElt).strictNodeCountMatch) && ((hyperarc)ruleElt).degree != ((hyperarc)hostElt).degree)
                {

                    var rStrictDegree =
                        negativeRelaxation.FirstOrDefault(r => r.Matches(Relaxations.Strict_Node_Count_Imposed, ruleElt));
                    if (rStrictDegree != null)
                    {
                        negativeRelaxation.NumberAllowable--;
                        rStrictDegree.NumberAllowed--;
                        negativeRelaxation.FulfilledItems.Add(new RelaxItem(Relaxations.Strict_Node_Count_Imposed, 1,
                                                                              ruleElt,
                                                                              ((hyperarc)hostElt).degree.ToString(
                                                                                  CultureInfo.InvariantCulture)));
                        return true;
                    }
                }
                if (ruleElt is arc)
                {
                    var rulearc = (ruleArc)ruleElt;
                    var hostarc = (arc)hostElt;
                    if (!rulearc.nullMeansNull && ((rulearc.To == null && hostarc.To != null) ||
                                                   (rulearc.From == null && hostarc.From != null)))
                    {
                        var rNullMeansNull =
                            negativeRelaxation.FirstOrDefault(r => r.Matches(Relaxations.Null_Means_Null_Imposed, ruleElt));
                        if (rNullMeansNull != null)
                        {
                            negativeRelaxation.NumberAllowable--;
                            rNullMeansNull.NumberAllowed--;
                            negativeRelaxation.FulfilledItems.Add(new RelaxItem(Relaxations.Null_Means_Null_Imposed, 1,
                                                                                  ruleElt,
                                                                                  (rulearc.To == null)
                                                                                      ? hostarc.To.name
                                                                                      : hostarc.From.name));
                            return true;
                        }
                    }
                    if (!rulearc.directionIsEqual &&
                        ((rulearc.doublyDirected != hostarc.doublyDirected) || (rulearc.directed != hostarc.directed)))
                    {
                        var rDir =
                            negativeRelaxation.FirstOrDefault(
                                r => r.Matches(Relaxations.Direction_Is_Equal_Imposed, ruleElt));
                        if (rDir != null)
                        {
                            negativeRelaxation.NumberAllowable--;
                            rDir.NumberAllowed--;
                            negativeRelaxation.FulfilledItems.Add(new RelaxItem(Relaxations.Direction_Is_Equal_Imposed, 1,
                                                                                  ruleElt));
                            return true;
                        }
                    }
                }
                var ruleNegLabels = (ruleElt is ruleNode)
                                        ? ((ruleNode)ruleElt).negateLabels
                                        : (ruleElt is ruleArc)
                                              ? ((ruleArc)ruleElt).negateLabels
                                              : ((ruleHyperarc)ruleElt).negateLabels;
                foreach (var negLabel in ruleNegLabels)
                {
                    var rLabel =
                        negativeRelaxation.FirstOrDefault(
                            r => r.Matches(Relaxations.Label_Imposed, ruleElt, negLabel));
                    if (rLabel != null)
                    {
                        negativeRelaxation.NumberAllowable--;
                        rLabel.NumberAllowed--;
                        negativeRelaxation.FulfilledItems.Add(new RelaxItem(Relaxations.Label_Imposed, 1, ruleElt,
                                                                              negLabel));
                        return true;
                    }
                }
                foreach (var lab in ruleElt.localLabels)
                {
                    var rNegLabel =
                        negativeRelaxation.FirstOrDefault(
                            r => r.Matches(Relaxations.Negate_Label_Imposed, ruleElt, lab));
                    if (rNegLabel != null)
                    {
                        negativeRelaxation.NumberAllowable--;
                        rNegLabel.NumberAllowed--;
                        negativeRelaxation.FulfilledItems.Add(new RelaxItem(Relaxations.Negate_Label_Imposed, 1, ruleElt,
                                                                              lab));
                        return true;
                    }
                }
            }
            var localNumAllowable = negativeRelaxation.NumberAllowable;
            var usedRelaxItems = new List<RelaxItem>();
            var usedFulfilledRelaxItems = new List<RelaxItem>();
            foreach (var elt in ruleNegElts)
            {
                var rNotExist =
                    negativeRelaxation.FirstOrDefault(r => r.Matches(Relaxations.Element_Made_Positive, elt)
                                                             &&
                                                             usedRelaxItems.Count(ur => ur == r) <
                                                             r.NumberAllowed);
                if (rNotExist == null) break;
                localNumAllowable--;
                usedRelaxItems.Add(rNotExist);
                usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Element_Made_Positive, 1, elt));
            }
            if ((localNumAllowable >= 0) && usedFulfilledRelaxItems.Count == ruleNegElts.Count)
            {
                negativeRelaxation.NumberAllowable = localNumAllowable;
                foreach (var r in usedRelaxItems) r.NumberAllowed--;
                negativeRelaxation.FulfilledItems.AddRange(usedFulfilledRelaxItems);
                return true;
            }
            return false;
        }

        private void checkForNegativeNode(option location, ruleNode LNode, node hostNode)
        {
            if (!nodeMatches(LNode, hostNode, location)) return;
            location.nodes[L.nodes.IndexOf(LNode)] = hostNode;
            var newLArc = LNode.arcs.FirstOrDefault(a => (location.findLMappedElement(a) == null));
            if (newLArc == null) findNegativeStartElement(location);
            else if (newLArc is ruleHyperarc)
                checkForNegativeHyperArc(location, LNode, hostNode, (ruleHyperarc)newLArc);
            else if (newLArc is ruleArc)
                checkForNegativeArc(location, LNode, hostNode, (ruleArc)newLArc);
        }


        private void checkForNegativeHyperArc(option location, ruleHyperarc LHyperArc, hyperarc hostHyperArc)
        {
            if (!hyperArcMatches(LHyperArc, hostHyperArc)) return;
            location.hyperarcs[L.hyperarcs.IndexOf(LHyperArc)] = hostHyperArc;
            var newLNode = (ruleNode)LHyperArc.nodes.FirstOrDefault(n => (location.findLMappedNode(n) == null));
            if (newLNode == null) findNegativeStartElement(location);
            else
            {
                foreach (var n in hostHyperArc.nodes.Where(n => !location.nodes.Contains(n)))
                {
                    checkForNegativeNode(location.copy(), newLNode, n);
                    if ((bool)AllNegativeElementsFound) return; /* another sub-branch found a match to the negative elements.
                                                         * There's no point in finding more than one, so this statement
                                                         * aborts the search down this branch. */
                }
            }
        }


        private void checkForNegativeHyperArc(option location, ruleNode fromLNode, node fromHostNode,
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
            {
                checkForNegativeHyperArc(location.copy(), newLHyperArc, hostHyperArc);
                if ((bool)AllNegativeElementsFound) return; /* another sub-branch found a match to the negative elements.
                                                             * There's no point in finding more than one, so this statement
                                                             * aborts the search down this branch. */
            }
        }


        private void checkForNegativeArc(option location, node fromLNode, node fromHostNode,
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
                && arcMatches(newLArc, (arc)a, fromHostNode, nextHostNode, (newLArc.From == fromLNode))).Cast<arc>();

            if ((nextHostNode != null) || newLArc.nullMeansNull)
                foreach (var HostArc in neighborHostArcs)
                {
                    var newLocation = location.copy();
                    newLocation.arcs[currentLArcIndex] = HostArc;
                    findNegativeStartElement(newLocation);
                    if ((bool)AllNegativeElementsFound) return; /* another sub-branch found a match to the negative elements.
                                                         * There's no point in finding more than one, so this statement
                                                         * aborts the search down this branch. */
                }
            else
                foreach (var HostArc in neighborHostArcs)
                {
                    nextHostNode = HostArc.otherNode(fromHostNode);
                    if (!location.nodes.Contains(nextHostNode))
                    {
                        var newLocation = location.copy();
                        newLocation.arcs[currentLArcIndex] = HostArc;
                        if (nextLNode == null) findNegativeStartElement(newLocation);
                        else checkForNegativeNode(newLocation, nextLNode, nextHostNode);
                    }
                    if ((bool)AllNegativeElementsFound) return; /* another sub-branch found a match to the negative elements.
                                                         * There's no point in finding more than one, so this statement
                                                         * aborts the search down this branch. */
                }
        }

        #endregion
    }

}