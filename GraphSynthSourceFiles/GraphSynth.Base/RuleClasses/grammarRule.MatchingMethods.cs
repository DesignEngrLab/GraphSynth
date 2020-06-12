/*************************************************************************
 *     This grammarRule.Basic.cs file partially defines the grammarRule 
 *     class (also partially defined in grammarRule.ShapeMethods.cs, 
 *     grammarRule.RecognizeApply.cs and grammarRule.NegativeRecognize.cs)
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

namespace GraphSynth.Representation
{
    /// <summary>
    /// All of these functions are static Booleans functions that match the graph or graph elements between host and rule.
    /// </summary>
    public partial class grammarRule
    {
        #region Generic Graph Element methods (Type and Label Match)
        private static Boolean IntendedTypesMatch(string TargetType, graphElement hostElt)
        {
            if (string.IsNullOrWhiteSpace(TargetType) || Type.GetType(TargetType) == null)
                return true;
            var t = Type.GetType(TargetType);
            return (hostElt.GetType().Equals(t) || hostElt.GetType().IsSubclassOf(t));
        }

        internal static Boolean LabelsMatch(IEnumerable<string> hostLabels, IEnumerable<string> positiveLabels, IEnumerable<string> negateLabels,
            Boolean containsAllLocalLabels)
        {
            /* first an easy check to see if any negating labels exist
             * in the hostLabels. If so, immediately return false. */
            if (negateLabels != null && negateLabels.Any() && negateLabels.Intersect(hostLabels).Any())
                return false;
            /* next, set up a tempLabels so that we don't change the 
             * host's actual labels. We delete an instance of the label. 
             * this is new in version 1.8. It's important since one may
             * have multiple identical labels. */
            var tempLabels = new List<string>(hostLabels);

            foreach (var label in positiveLabels)
            {
                if (tempLabels.Contains(label)) tempLabels.Remove(label);
                else return false;
            }

            /* this new approach actually simplifies and speeds up the containAllLabels
             * check. If there are no more tempLabels than the two match completely - else
             * return false. */
            if (containsAllLocalLabels && tempLabels.Any()) return false;
            return true;
        }
        #endregion

        #region Node Matching

        private Boolean nodeMatches(ruleNode LNode, node hostNode, option location)
        {
            return (!LNode.strictDegreeMatch || LNode.degree == hostNode.degree)
                   && LNode.degree <= hostNode.degree
                   && LabelsMatch(hostNode.localLabels, LNode.localLabels, LNode.negateLabels, LNode.containsAllLocalLabels)
                   && IntendedTypesMatch(LNode.TargetType, hostNode)
                   && HyperArcPreclusionCheckForSingleNode(LNode, hostNode, location.hyperarcs);
        }
        private Boolean nodeMatchRelaxed(ruleNode LNode, node hostNode, option location)
        {
            if (location.Relaxations.NumberAllowable == 0) return false;
            var localNumAllowable = location.Relaxations.NumberAllowable;
            var usedRelaxItems = new List<RelaxItem>();
            var usedFulfilledRelaxItems = new List<RelaxItem>();
            if (LNode.strictDegreeMatch && LNode.degree != hostNode.degree)
            {
                var rStrictDegree =
                     location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Strict_Degree_Match_Revoked, LNode)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                if (rStrictDegree == null) return false;
                localNumAllowable--;
                usedRelaxItems.Add(rStrictDegree);
                usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Strict_Degree_Match_Revoked, 1, LNode,
                    hostNode.degree.ToString(CultureInfo.InvariantCulture)));
            }
            if (!IntendedTypesMatch(LNode.TargetType, hostNode))
            {
                var rType =
                      location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Target_Type_Revoked, LNode)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                if (rType == null) return false;
                localNumAllowable--;
                usedRelaxItems.Add(rType);
                usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Target_Type_Revoked, 1, LNode, hostNode.GetType().ToString()));
            }
            if (!HyperArcPreclusionCheckForSingleNode(LNode, hostNode, location.hyperarcs))
            {
                var rHyperArcPreclusion =
                       location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.HyperArc_Preclusion_Revoked, LNode)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                if (rHyperArcPreclusion == null) return false;
                localNumAllowable--;
                usedRelaxItems.Add(rHyperArcPreclusion);
                usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.HyperArc_Preclusion_Revoked, 1, LNode));
            }
            foreach (var nl in LNode.negateLabels)
            {
                if (hostNode.localLabels.Contains(nl))
                {
                    var rNegLabel =
                        location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Negate_Label_Revoked, LNode, nl)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                    if (rNegLabel == null) return false;
                    localNumAllowable--;
                    usedRelaxItems.Add(rNegLabel);
                    usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Negate_Label_Revoked, 1, LNode, nl));
                }
            }
            var tempLabels = new List<string>(hostNode.localLabels);
            foreach (var label in LNode.localLabels)
            {
                if (tempLabels.Contains(label)) tempLabels.Remove(label);
                else
                {
                    var rLabel =
                        location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Label_Revoked, LNode, label)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                    if (rLabel == null) return false;
                    localNumAllowable--;
                    usedRelaxItems.Add(rLabel);
                    usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Label_Revoked, 1, LNode, label));
                    tempLabels.Remove(label);
                }
            }
            if (LNode.containsAllLocalLabels && tempLabels.Any())
            {
                var rContainsAll =
                     location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Contains_All_Local_Labels_Revoked, LNode)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                if (rContainsAll == null) return false;
                localNumAllowable--;
                usedRelaxItems.Add(rContainsAll);
                usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Contains_All_Local_Labels_Revoked, 1, LNode,
                    hostNode.localLabels.Count.ToString(CultureInfo.InvariantCulture)));
            }
            if (localNumAllowable < 0) return false; /* don't make any reductions to the relaxations list - there are not
                                                      * enough to make this work. */
            location.Relaxations.NumberAllowable = localNumAllowable;
            foreach (var r in usedRelaxItems)
                r.NumberAllowed--;
            location.Relaxations.FulfilledItems.AddRange(usedFulfilledRelaxItems);
            return true;
        }

        //private node nodeRelaxExistence(option location, ruleNode LNode)
        //{
        //    if (location.Relaxations.NumberAllowable == 0) return null;
        //    var rEltRemove = location.Relaxations.FirstOrDefault(r =>
        //        r.Matches(Relaxations.Element_Made_Negative, LNode) && r.NumberAllowed > 0);
        //    if (rEltRemove == null) return null;
        //    location.Relaxations.NumberAllowable--;
        //    location.Relaxations.FulfilledItems.Add(new RelaxItem(Relaxations.Element_Made_Negative, 1, LNode));
        //    node standinNode;
        //    Type nodeType = Type.GetType(LNode.TargetType, false);
        //    if (nodeType == null || nodeType == typeof(node))
        //        standinNode = new node();
        //    else
        //    {
        //        var nodeConstructor = nodeType.GetConstructor(new Type[0]);
        //        if (nodeConstructor == null)
        //            standinNode = new node();
        //        else standinNode = (node)nodeConstructor.Invoke(new object[0]);
        //    }
        //    LNode.copy(standinNode);
        //    return standinNode;
        //}

        #endregion

        #region Hyperarc Matching
        private static Boolean hyperArcMatches(ruleHyperarc Lha, hyperarc Hha)
        {
            if (Lha.strictNodeCountMatch && (Lha.degree != Hha.degree)) return false;
            if (Lha.degree > Hha.degree) return false;
            return (LabelsMatch(Hha.localLabels, Lha.localLabels,
               Lha.negateLabels, Lha.containsAllLocalLabels) &&
                IntendedTypesMatch(Lha.TargetType, Hha));
        }

        private static Boolean hyperArcMatchRelaxed(ruleHyperarc Lha, hyperarc Hha, option location)
        {
            if (location.Relaxations.NumberAllowable == 0) return false;
            var localNumAllowable = location.Relaxations.NumberAllowable;
            var usedRelaxItems = new List<RelaxItem>();
            var usedFulfilledRelaxItems = new List<RelaxItem>();
            if (Lha.strictNodeCountMatch && Lha.degree != Hha.degree)
            {
                var rStrictNodeCount =
                     location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Strict_Node_Count_Revoked, Lha)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                if (rStrictNodeCount == null) return false;
                localNumAllowable--;
                usedRelaxItems.Add(rStrictNodeCount);
                usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Strict_Node_Count_Revoked, 1, Lha,
                    Hha.degree.ToString(CultureInfo.InvariantCulture)));
            }
            if (!IntendedTypesMatch(Lha.TargetType, Hha))
            {
                var rType =
                      location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Target_Type_Revoked, Lha)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                if (rType == null) return false;
                localNumAllowable--;
                usedRelaxItems.Add(rType);
                usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Target_Type_Revoked, 1, Lha, Hha.GetType().ToString()));
            }
            foreach (var nl in Lha.negateLabels)
            {
                if (Hha.localLabels.Contains(nl))
                {
                    var rNegLabel =
                        location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Negate_Label_Revoked, Lha, nl)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                    if (rNegLabel == null) return false;
                    localNumAllowable--;
                    usedRelaxItems.Add(rNegLabel);
                    usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Negate_Label_Revoked, 1, Lha, nl));
                }
            }
            var tempLabels = new List<string>(Hha.localLabels);
            foreach (var label in Lha.localLabels)
            {
                if (tempLabels.Contains(label)) tempLabels.Remove(label);
                else
                {
                    var rLabel =
                        location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Label_Revoked, Lha, label)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                    if (rLabel == null) return false;
                    localNumAllowable--;
                    usedRelaxItems.Add(rLabel);
                    tempLabels.Remove(label);
                    usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Label_Revoked, 1, Lha, label));
                }
            }
            if (Lha.containsAllLocalLabels && tempLabels.Any())
            {
                var rContainsAll =
                     location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Contains_All_Local_Labels_Revoked, Lha)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                if (rContainsAll == null) return false;
                localNumAllowable--;
                usedRelaxItems.Add(rContainsAll);
                usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Contains_All_Local_Labels_Revoked, 1, Lha,
                    Hha.localLabels.Count.ToString(CultureInfo.InvariantCulture)));
            }
            if (localNumAllowable < 0) return false; /* don't make any reductions to the relaxations list - there are not
                                                      * enough to make this work. */
            location.Relaxations.NumberAllowable = localNumAllowable;
            foreach (var r in usedRelaxItems)
                r.NumberAllowed--;
            location.Relaxations.FulfilledItems.AddRange(usedFulfilledRelaxItems);
            return true;
        }


        //private hyperarc hyperarcRelaxExistence(option location, ruleHyperarc LHyper)
        //{
        //    if (location.Relaxations.NumberAllowable == 0) return null;
        //    var rEltRemove = location.Relaxations.FirstOrDefault(r =>
        //        r.Matches(Relaxations.Element_Made_Negative, LHyper) && r.NumberAllowed > 0);
        //    if (rEltRemove == null) return null;
        //    location.Relaxations.NumberAllowable--;
        //    location.Relaxations.FulfilledItems.Add(new RelaxItem(Relaxations.Element_Made_Negative, 1, LHyper));
        //    hyperarc standinNode;
        //    Type hyperarcType = Type.GetType(LHyper.TargetType, false);
        //    if (hyperarcType == null || hyperarcType == typeof(hyperarc))
        //        standinNode = new hyperarc();
        //    else
        //    {
        //        var nodeConstructor = hyperarcType.GetConstructor(new Type[0]);
        //        if (nodeConstructor == null)
        //            standinNode = new hyperarc();
        //        else standinNode = (hyperarc)nodeConstructor.Invoke(new object[0]);
        //    }
        //    LHyper.copy(standinNode);
        //    return standinNode;
        //}
        #endregion

        #region Arc Matching
        /// <summary>
        /// Returns a true/false based on if the host arc matches with this ruleArc.
        /// </summary>
        /// <param name="LArc">The L arc.</param>
        /// <param name="hostArc">The host arc.</param>
        /// <param name="fromHostNode">From host node.</param>
        /// <param name="nextHostNode">The next host node.</param>
        /// <param name="LTraversesForward">if set to <c>true</c> [traverse forward].</param>
        /// <returns></returns>
        private static Boolean arcMatches(ruleArc LArc, arc hostArc, node fromHostNode = null, node nextHostNode = null, bool LTraversesForward = false)
        {
            if ((nextHostNode != null || LArc.nullMeansNull) && hostArc.otherNode(fromHostNode) != nextHostNode)
                return false;

            var hostTraversesForward = (hostArc.From != null) && (hostArc.From == fromHostNode);
            if (LArc.directionIsEqual && (LArc.doublyDirected != hostArc.doublyDirected)) return false;
            if (LArc.directionIsEqual && (LArc.directed != hostArc.directed)) return false;
            if (LArc.directed && !hostArc.directed) return false;
            if (LArc.doublyDirected && !hostArc.doublyDirected) return false;
            if (LArc.directed  // if this rule arc is directed
                && (hostTraversesForward != LTraversesForward))
                return false;

            return (LabelsMatch(hostArc.localLabels, LArc.localLabels, LArc.negateLabels, LArc.containsAllLocalLabels)
                && IntendedTypesMatch(LArc.TargetType, hostArc));
        }

        private static Boolean arcMatchRelaxed(ruleArc LArc, arc hostArc, option location, node fromHostNode = null,
            node nextHostNode = null, bool LTraversesForward = false)
        {
            if (location.Relaxations.NumberAllowable == 0) return false;
            var localNumAllowable = location.Relaxations.NumberAllowable;
            var usedRelaxItems = new List<RelaxItem>();
            var usedFulfilledRelaxItems = new List<RelaxItem>();


            if ((nextHostNode != null) && hostArc.otherNode(fromHostNode) != nextHostNode) return false;
            //relaxelt although we could look to make nextHostNode NOTEXIST(?)

            if (LArc.nullMeansNull && hostArc.otherNode(fromHostNode) != null)
            {
                var rNullMeansNull =
                      location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Null_Means_Null_Revoked, LArc)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                if (rNullMeansNull == null) return false;
                localNumAllowable--;
                usedRelaxItems.Add(rNullMeansNull);
                usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Null_Means_Null_Revoked, 1, LArc,
                                              (nextHostNode != null) ? nextHostNode.name : ""));
            }
            var hostTraversesForward = (hostArc.From != null) && (hostArc.From == fromHostNode);
            if ((LArc.directionIsEqual && (LArc.doublyDirected != hostArc.doublyDirected))
            || (LArc.directionIsEqual && (LArc.directed != hostArc.directed))
            || (LArc.directed && !hostArc.directed)
            || (LArc.doublyDirected && !hostArc.doublyDirected)
            || (LArc.directed && (hostTraversesForward != LTraversesForward)))
            {
                var rDir =
                      location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Direction_Is_Equal_Revoked, LArc)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                if (rDir == null) return false;
                localNumAllowable--;
                usedRelaxItems.Add(rDir);
                usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Direction_Is_Equal_Revoked, 1, LArc, hostArc.name));
            }

            if (!IntendedTypesMatch(LArc.TargetType, hostArc))
            {
                var rType =
                      location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Target_Type_Revoked, LArc)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                if (rType == null) return false;
                localNumAllowable--;
                usedRelaxItems.Add(rType);
                usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Target_Type_Revoked, 1, LArc, hostArc.GetType().ToString()));
            }
            foreach (var nl in LArc.negateLabels)
            {
                if (hostArc.localLabels.Contains(nl))
                {
                    var rNegLabel =
                        location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Negate_Label_Revoked, LArc, nl)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                    if (rNegLabel == null) return false;
                    localNumAllowable--;
                    usedRelaxItems.Add(rNegLabel);
                    usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Negate_Label_Revoked, 1, LArc, nl));
                }
            }
            var tempLabels = new List<string>(hostArc.localLabels);
            foreach (var label in LArc.localLabels)
            {
                if (tempLabels.Contains(label)) tempLabels.Remove(label);
                else
                {
                    var rLabel =
                        location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Label_Revoked, LArc, label)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                    if (rLabel == null) return false;
                    localNumAllowable--;
                    usedRelaxItems.Add(rLabel);
                    tempLabels.Remove(label);
                    usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Label_Revoked, 1, LArc, label));
                }
            }
            if (LArc.containsAllLocalLabels && tempLabels.Any())
            {
                var rContainsAll =
                     location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Contains_All_Local_Labels_Revoked, LArc)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                if (rContainsAll == null) return false;
                localNumAllowable--;
                usedRelaxItems.Add(rContainsAll);
                usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Contains_All_Local_Labels_Revoked, 1, LArc,
                    hostArc.localLabels.Count.ToString(CultureInfo.InvariantCulture)));
            }
            if (localNumAllowable < 0) return false; /* don't make any reductions to the relaxations list - there are not
                                                      * enough to make this work. */
            location.Relaxations.NumberAllowable = localNumAllowable;
            foreach (var r in usedRelaxItems)
                r.NumberAllowed--;
            location.Relaxations.FulfilledItems.AddRange(usedFulfilledRelaxItems);
            return true;
        }
        #endregion

        #region Initial Rule Check (global labels, spanning)
        private Boolean InitialRuleCheck()
        {
            return (!spanning || (L.nodes.Count(rn => ((ruleNode)rn).MustExist) == host.nodes.Count))
                   && ((OrderedGlobalLabels && OrderLabelsMatch(host.globalLabels))
                       || (!OrderedGlobalLabels && LabelsMatch(host.globalLabels, L.globalLabels, negateLabels, containsAllGlobalLabels)))
                   && hasLargerOrEqualDegreeSeqence(host.DegreeSequence, LDegreeSequence)
                   && hasLargerOrEqualDegreeSeqence(host.HyperArcDegreeSequence, LHyperArcDegreeSequence)
                   && (host.arcs.Count >= L.arcs.Count(a => ((ruleArc)a).MustExist));
        }

        private Boolean InitialRuleCheckRelaxed(option location)
        {
            if (location.Relaxations.NumberAllowable == 0) return false;
            var localNumAllowable = location.Relaxations.NumberAllowable;
            var usedRelaxItems = new List<RelaxItem>();
            var usedFulfilledRelaxItems = new List<RelaxItem>();
            if (spanning && (L.nodes.Count != host.nodes.Count))
            {
                var rSpanning =
                     location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Spanning_Revoked)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                if (rSpanning == null) return false;
                localNumAllowable--;
                usedRelaxItems.Add(rSpanning);
                usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Spanning_Revoked, 1, null, host.nodes.Count.ToString(CultureInfo.InvariantCulture)));
            }
            foreach (var nl in negateLabels)
            {
                if (host.globalLabels.Contains(nl))
                {
                    var rNegLabel =
                        location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Negate_Global_Label_Revoked)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                    if (rNegLabel == null) return false;
                    localNumAllowable--;
                    usedRelaxItems.Add(rNegLabel);
                    usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Negate_Global_Label_Revoked, 1, null, nl));
                }
            }
            if (!OrderedGlobalLabels || !OrderLabelsMatch(host.globalLabels))
            {
                var tempLabels = new List<string>(host.globalLabels);
                foreach (var label in L.globalLabels)
                {
                    if (tempLabels.Contains(label)) tempLabels.Remove(label);
                    else
                    {
                        var rLabel =
                            location.Relaxations.FirstOrDefault(
                                r => r.Matches(Relaxations.Global_Label_Revoked, null, label)
                                     && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                        if (rLabel == null) return false;
                        localNumAllowable--;
                        usedRelaxItems.Add(rLabel);
                        usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Global_Label_Revoked, 1, null, label));
                        tempLabels.Remove(label);
                    }
                }
                if (OrderedGlobalLabels)
                {
                    var rOrdered =
                        location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Ordered_Global_Labels_Revoked)
                                                                 &&
                                                                 usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                    if (rOrdered == null) return false;
                    localNumAllowable--;
                    usedRelaxItems.Add(rOrdered);
                    usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Ordered_Global_Labels_Revoked, 1));
                }
                if (containsAllGlobalLabels && tempLabels.Any())
                {
                    var rContainsAll =
                        location.Relaxations.FirstOrDefault(
                            r => r.Matches(Relaxations.Contains_All_Global_Labels_Revoked)
                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                    if (rContainsAll == null) return false;
                    localNumAllowable--;
                    usedRelaxItems.Add(rContainsAll);
                    usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Contains_All_Local_Labels_Revoked, 1, null,
                                                              host.globalLabels.Count.ToString(CultureInfo.InvariantCulture)));
                }
            }
            if (localNumAllowable < 0)
                return false;
            /* don't make any reductions to the relaxations list - there are not
                                          * enough to make this work. */
            location.Relaxations.NumberAllowable = localNumAllowable;
            foreach (var r in usedRelaxItems)
                r.NumberAllowed--;
            location.Relaxations.FulfilledItems.AddRange(usedFulfilledRelaxItems);
            return true;
        }

        private Boolean OrderLabelsMatch(ICollection<string> hostLabels)
        {
            /* first an easy check to see if any negating labels exist
             * in the hostLabels. If so, immediately return false. */
            if (negateLabels.Any() && negateLabels.Intersect(hostLabels).Any())
                return false;
            if (containsAllGlobalLabels)
            {
                if (hostLabels.SequenceEqual(L.globalLabels))
                {
                    globalLabelStartLocs.Add(0);
                    return true;
                }
                return false;
            }

            var AnyFound = false;
            for (var i = 0; i < hostLabels.Count - L.globalLabels.Count + 1; i++)
            {
                var subList = new string[L.globalLabels.Count];
                Array.Copy(hostLabels.ToArray(), i, subList, 0, L.globalLabels.Count);
                if (L.globalLabels.SequenceEqual(subList))
                {
                    globalLabelStartLocs.Add(i);
                    AnyFound = true;
                }
            }
            return AnyFound;
        }
        /// <summary>
        /// Determines whether A has larger or equal degree seqence to B.
        /// </summary>
        /// <param name="ASequence">The A sequence.</param>
        /// <param name="BSequence">The B sequence.</param>
        /// <returns>
        ///   <c>true</c> if  A [has larger or equal degree seqence] [the specified B sequence]; otherwise, <c>false</c>.
        /// </returns>
        private static Boolean hasLargerOrEqualDegreeSeqence(IList<int> ASequence, IList<int> BSequence)
        {
            if (BSequence.Count > ASequence.Count) return false;
            return !BSequence.Where((Ldegree, i) => Ldegree > ASequence[i]).Any();
        }
        #endregion

        #region Final Rule Check (induced, hyperarc preclusion, shape restriction, additional functions)
        private Boolean FinalRuleChecks(option location)
        {
            if (L.nodes.Where((t, i) => location.nodes[i] != null
                                        && !HyperArcPreclusionCheckForSingleNode((ruleNode)L.nodes[i],
                                         location.nodes[i], location.hyperarcs)).Any())
                return false; // not a valid option

            /* a complete subgraph has been found. However, there is three more conditions to check:
             * induced, shape transform, and additional recognize functions written as C# code */
            if (induced && otherArcsInHost(host, location))
                return false; // not a valid option
            /* The induced boolean indicates that if there are any arcs in the host between the 
            * nodes of the subgraph that are not in L then this is not a valid location.  */

            var firstNotExistIndex = L.nodes.FindIndex(n => ((ruleNode)n).NotExist);
            double[,] T;
            Boolean validTransform;
            if (firstNotExistIndex < 0)
            {
                validTransform = findTransform(location.nodes, out T);
                if (UseShapeRestrictions && (!validTransform || !otherNodesComply(T, location.nodes)))
                    return false; // not a valid option - the transform does not have a correct transformation
            }
            else
            {
                var positiveNodes = new List<node>(location.nodes);
                positiveNodes.RemoveRange(firstNotExistIndex, (positiveNodes.Count - firstNotExistIndex));
                validTransform = findTransform(positiveNodes, out T);
                if (UseShapeRestrictions && (!validTransform || !otherNodesComply(T, positiveNodes)))
                    return false; // not a valid option
            }

            foreach (var recognizeFunction in recognizeFuncs)
            {
                try
                {
                    object[] recognizeArguments;
                    // newest approach #6
                    if (recognizeFunction.GetParameters().GetLength(0) == 2)
                        recognizeArguments = new object[] { location, host };
                    // oldest approach #1
                    else if (recognizeFunction.GetParameters()[0].ParameterType == typeof(designGraph))
                        recognizeArguments = new object[] { L, host, location.nodes, location.arcs };
                    // newer approach #5
                    else if ((recognizeFunction.GetParameters().GetLength(0) == 3)
                        && (recognizeFunction.GetParameters()[2].ParameterType == typeof(option)))
                        recognizeArguments = new object[] { this, host, location };
                    // new approach #4
                    else if ((recognizeFunction.GetParameters().GetLength(0) == 4)
                        && (recognizeFunction.GetParameters()[2].ParameterType == typeof(designGraph)))
                        recognizeArguments = new object[] { this, host, new designGraph(location.nodes,
                            location.arcs, location.hyperarcs), T };
                    // older approach #2
                    else if (recognizeFunction.GetParameters().GetLength(0) == 4)
                        recognizeArguments = new object[] { this, host, location.nodes, location.arcs };
                    //  approach #3
                    else recognizeArguments = new object[] { this, host, location.nodes, location.arcs, T };

                    if ((double)recognizeFunction.Invoke(DLLofFunctions, recognizeArguments) > 0)
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    SearchIO.MessageBoxShow("Error in additional recognize function: " + recognizeFunction.Name +
                                            ".\nSee output bar for details.", "Error in  " + recognizeFunction.Name, "Error");
                    SearchIO.output("Error in function: " + recognizeFunction.Name);
                    SearchIO.output("Exception in : " + e.InnerException.TargetSite.Name);
                    SearchIO.output("Error              : " + e.InnerException.Message);
                    SearchIO.output("Stack Trace     	: " + e.InnerException.StackTrace);
                    return false;
                }
            }
            location.positionTransform = T;
            return true;
        }

        private Boolean FinalRuleCheckRelaxed(option location)
        {
            if (location.Relaxations.NumberAllowable == 0) return false;
            var localNumAllowable = location.Relaxations.NumberAllowable;
            var usedRelaxItems = new List<RelaxItem>();
            var usedFulfilledRelaxItems = new List<RelaxItem>();
            if (induced && otherArcsInHost(host, location))
            {
                var rInduced =
                      location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Induced_Revoked)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                if (rInduced == null) return false;
                localNumAllowable--;
                usedRelaxItems.Add(rInduced);
                usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Induced_Revoked, 1));
            }
            var numNodes = L.nodes.Count;
            for (var i = 0; i < numNodes; i++)
                if (location.nodes[i] != null
                    && !HyperArcPreclusionCheckForSingleNode((ruleNode)L.nodes[i], location.nodes[i], location.hyperarcs))
                {
                    var rHyperArcPreclusion =
                           location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.HyperArc_Preclusion_Revoked, location.nodes[i])
                                                                     && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                    if (rHyperArcPreclusion == null) return false;
                    localNumAllowable--;
                    usedRelaxItems.Add(rHyperArcPreclusion);
                    usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.HyperArc_Preclusion_Revoked, 1, location.nodes[i]));
                }

            var firstNotExistIndex = L.nodes.FindIndex(n => ((ruleNode)n).NotExist);
            double[,] T;
            Boolean validTransform;
            if (firstNotExistIndex < 0) validTransform = findTransform(location.nodes, out T);
            else
            {
                var positiveNodes = new List<node>(location.nodes);
                positiveNodes.RemoveRange(firstNotExistIndex, (positiveNodes.Count - firstNotExistIndex));
                validTransform = findTransform(positiveNodes, out T);
            }
            if (UseShapeRestrictions && (!validTransform || !otherNodesComply(T, location.nodes)))
            {
                var rShape =
                      location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Shape_Restriction_Revoked)
                                                                 && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                if (rShape == null) return false;
                localNumAllowable--;
                usedRelaxItems.Add(rShape);
                usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Shape_Restriction_Revoked, 1));
            }

            foreach (var recognizeFunction in recognizeFuncs)
            {
                try
                {
                    object[] recognizeArguments;
                    // oldest approach: 1
                    if (recognizeFunction.GetParameters()[0].ParameterType == typeof(designGraph))
                        recognizeArguments
                            = new object[] { L, host, location.nodes, location.arcs };
                    // newest approach: 5
                    else if ((recognizeFunction.GetParameters().GetLength(0) == 3)
                        && (recognizeFunction.GetParameters()[2].ParameterType == typeof(option)))
                        recognizeArguments = new object[] { this, host, location };
                    // newer approach: 4
                    else if ((recognizeFunction.GetParameters().GetLength(0) == 4)
                        && (recognizeFunction.GetParameters()[2].ParameterType == typeof(designGraph)))
                        recognizeArguments = new object[] { this, host, new designGraph(location.nodes,
                            location.arcs, location.hyperarcs), T };
                    // older approach: 2
                    else if (recognizeFunction.GetParameters().GetLength(0) == 4)
                        recognizeArguments = new object[] { this, host, location.nodes, location.arcs };
                    // new approach:3
                    else
                        recognizeArguments = new object[] { this, host, location.nodes, location.arcs, T };
                    var gValue = (double)recognizeFunction.Invoke(DLLofFunctions, recognizeArguments);
                    if (gValue > 0)
                    {
                        var rAddnlFunction =
                              location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Additional_Functions_Revoked, null, recognizeFunction.Name)
                                                                         && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                        if (rAddnlFunction == null) return false;
                        localNumAllowable--;
                        usedRelaxItems.Add(rAddnlFunction);
                        usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Additional_Functions_Revoked, 1, null,
                            recognizeFunction.Name + " = " + gValue));
                    }
                }
                catch (Exception e)
                {
                    SearchIO.MessageBoxShow("Error in additional recognize function: " + recognizeFunction.Name +
                                            ".\nSee output bar for details.", "Error in  " + recognizeFunction.Name, "Error");
                    SearchIO.output("Error in function: " + recognizeFunction.Name);
                    SearchIO.output("Exception in : " + e.InnerException.TargetSite.Name);
                    SearchIO.output("Error              : " + e.InnerException.Message);
                    SearchIO.output("Stack Trace     	: " + e.InnerException.StackTrace);
                    var rAddnlFunction =
                          location.Relaxations.FirstOrDefault(r => r.Matches(Relaxations.Additional_Functions_Revoked, null, recognizeFunction.Name)
                                                                     && usedRelaxItems.Count(ur => ur == r) < r.NumberAllowed);
                    if (rAddnlFunction == null) return false;
                    localNumAllowable--;
                    usedRelaxItems.Add(rAddnlFunction);
                    usedFulfilledRelaxItems.Add(new RelaxItem(Relaxations.Additional_Functions_Revoked, 1, null,
                        recognizeFunction.Name + " = Error: " + e.InnerException.Message));
                }
            }
            if (localNumAllowable < 0) return false; /* don't make any reductions to the relaxations list - there are not
                                                      * enough to make this work. */
            location.Relaxations.NumberAllowable = localNumAllowable;
            foreach (var r in usedRelaxItems)
                r.NumberAllowed--;
            location.Relaxations.FulfilledItems.AddRange(usedFulfilledRelaxItems);
            location.positionTransform = T;
            return true;
        }

        /// <summary>
        /// This function is used when checking for an induced subgraph (near line 300 of this file under
        /// the function case1LocationFound). I have placed it here near the induced property because that's
        /// a logical place as any in such a big file.
        /// </summary>
        /// <param name="host">The host graph.</param>
        /// <param name="location">The location.</param>
        /// <returns>
        /// true - if no other arcs exist between the locatedNodes.
        /// </returns>
        private static Boolean otherArcsInHost(designGraph host, option location)
        {
            /* Check each arc of the host. If an arc is NOT in located Nodes but connects two located
             * nodes then return false. */
            if (host.arcs.Any(a =>
                              !location.arcs.Contains(a)
                              && location.nodes.Contains(a.From)
                              && location.nodes.Contains(a.To)))
                return true;

            return host.hyperarcs.Any(a =>
                                      !location.hyperarcs.Contains(a)
                                      && a.nodes.All(n => location.nodes.Contains(n)));
        }
        #endregion

        #region Hyperarc Preclusion/Inclusion
        /// <summary>
        /// Hyperarc preclusion check for single node. Actually also "inclusion" check. Checks that host has
        /// nodes connected in the same way to the hyperarcs as they are in the rule's LHS.
        /// </summary>
        /// <param name="LNode">The L node.</param>
        /// <param name="hostNode">The host node.</param>
        /// <param name="hostHyperarcs">The host hyperarcs.</param>
        /// <returns>
        /// true if preclusions are correct for this node (it is not included
        /// in hyperarcs it was intentionally precluded from.
        /// </returns>
        private Boolean HyperArcPreclusionCheckForSingleNode(ruleNode LNode, node hostNode, IList<hyperarc> hostHyperarcs)
        {
            /* after two other versions - which were correct but hard to follow - I settled on this one, which is both
             * the easiest to understand and the fastest (for loop finds both hyperarcs without extra lookup function. */
            for (var i = 0; i < L.hyperarcs.Count; i++)
            {
                var ruleHyperarc = (ruleHyperarc)L.hyperarcs[i];
                var hostHyperarc = hostHyperarcs[i];
                if (hostHyperarc != null && host.hyperarcs.Contains(hostHyperarc)
                    /* since this function is called before the end (as an early check on nodes
                     * in nodeMatches) it's possible that there are yet-to-be-found hyperarcs, 
                     * which we need to skip at this point, or the hostHyperarc was a "stand-in" if
                     * the problem was relaxed s.t. the hyperarc doesn't really exist in the host. */
                    && (LNode.NotExist || ruleHyperarc.MustExist)
                    /* this one is tricky. Basically, we don't want to check preclusion/inclusion between LNodes that
                     * are supposed to exist and hyperarcs that are not (supposed to exist). The converse of this is
                     * check if it is a NotExist L-node (regardless of the hyperarc) or if the hyperarc is to exist. */
                    && LNode.arcs.Contains(ruleHyperarc) != hostNode.arcs.Contains(hostHyperarc))
                    /* finally, if the hyperarc is connected to the LNode (i.e. contains is true) then it should 
                     * also connect in the host between the same matched elements. OR if the ruleHyperarc precludes 
                     * the L-node (i.e. contains is false), then it should be false in the host as well. */
                    return false;
                /* if they are not the same (true != false) then we return false. */
            }
            return true;
        }
        #endregion

    }
}