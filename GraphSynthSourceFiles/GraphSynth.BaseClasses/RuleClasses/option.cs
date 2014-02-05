/*************************************************************************
 *     This option file & class is part of the GraphSynth.BaseClasses 
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

using System.Linq;
using GraphSynth.Search;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GraphSynth.Representation
{
    /// <summary>
    ///   A rule is not enough - the Opion class captures all the details of
    ///   an decision option from one point in the search process. The list of
    ///   options are presented in the choice for which rule to apply. Option 
    ///   contains references to the location where the rule is applicable, 
    ///   the rule itself, along with its number in the ruleSet and the ruleSet's
    ///   number when there are multiple ruleSets.
    /// </summary>
    public class option
    {
        #region Fields and Properties

        /// <summary>
        ///   A list of parameters chosen and used by the apply fuctions of the rule.
        /// </summary>
        public List<double> parameters = new List<double>();

        #region Lists of Nodes and Arcs included in this graph

        /// <summary />
        List<arc> _arcs;

        /// <summary />
        List<hyperarc> _hyperarcs;

        /// <summary />
        List<node> _nodes;

        /// <summary>
        ///   Gets or sets the arcs.
        /// </summary>
        /// <value>The arcs.</value>
        public List<arc> arcs
        {
            get { return _arcs ?? (_arcs = new List<arc>()); }
            private set { _arcs = value; }
        }

        /// <summary>
        ///   Gets the nodes.
        /// </summary>
        /// <value>The nodes.</value>
        public List<node> nodes
        {
            get { return _nodes ?? (_nodes = new List<node>()); }
            private set { _nodes = value; }
        }

        /// <summary>
        ///   Gets the hyperarcs.
        /// </summary>
        /// <value>The hyperarcs.</value>
        public List<hyperarc> hyperarcs
        {
            get { return _hyperarcs ?? (_hyperarcs = new List<hyperarc>()); }
            private set { _hyperarcs = value; }
        }

        #endregion

        /// <summary>
        ///   Gets or sets the option number.
        /// </summary>
        /// <value>The option number.</value>
        public int optionNumber { get; set; }

        /// <summary>
        ///   Gets or sets the index of the rule set.
        /// </summary>
        /// <value>The index of the rule set.</value>
        public int ruleSetIndex { get; set; }

        /// <summary>
        ///   Gets or sets the rule number.
        /// </summary>
        /// <value>The rule number.</value>
        public int ruleNumber { get; set; }

        /// <summary>
        ///   Gets or sets the rule.
        /// </summary>
        /// <value>The rule.</value>
        [XmlIgnore]
        public grammarRule rule { get; set; }

        /// <summary>
        ///   Gets or sets the global label start loc.
        /// </summary>
        /// <value>The global label start loc.</value>
        public int globalLabelStartLoc { get; set; }

        /// <summary>
        ///   Gets or sets the position transform.
        /// </summary>
        /// <value>The position transform.</value>
        [XmlIgnore]
        public double[,] positionTransform { get; set; }


        /// <summary>
        ///   Gets or sets the confluence.
        /// </summary>
        /// <value>The confluence.</value>
        public List<int> confluence { get; set; }

        /// <summary>
        ///   Gets or sets the probability.
        /// </summary>
        /// <value>The probability.</value>
        public double probability { get; set; }

        /// <summary>
        /// Gets or sets the relaxations.
        /// </summary>
        /// <value>
        /// The relaxations.
        /// </value>
        [XmlIgnore]
        public Relaxation Relaxations
        {
            get { return relaxations ?? (relaxations = new Relaxation()); }
            set { relaxations = value; }
        }
        private Relaxation relaxations;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="option"/> class.
        /// </summary>
        /// <param name="rule">The rule.</param>
        public option(grammarRule rule)
        {
            var numNodes = rule.L.nodes.Count;
            var numArcs = rule.L.arcs.Count;
            var numHyperarcs = rule.L.hyperarcs.Count;
            for (int i = 0; i < numNodes; i++)
                nodes.Add(null);
            for (int i = 0; i < numArcs; i++)
                arcs.Add(null);
            for (int i = 0; i < numHyperarcs; i++)
                hyperarcs.Add(null);
            this.rule = rule;
            positionTransform = MatrixMath.Identity(3);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="option"/> class.
        /// </summary>
        private option() { }
        #endregion

        #region Methods

        /// <summary>
        ///   Applies the option to the specified host. It is essentially
        ///   a shorthand instead of calling 
        ///   option.rule.apply(option.location, host, parameters); we call
        ///   option.apply(host, parameters).
        /// </summary>
        /// <param name = "host">The host.</param>
        /// <param name = "Parameters">The parameters.</param>
        public void apply(designGraph host, double[] Parameters)
        {
            rule.apply(host, this, Parameters);
        }

        /// <summary>
        ///   Returns a copy of this instance of option. Note that location is a 
        ///   shallow copy and applies to the same host.
        /// </summary>
        /// <returns></returns>
        public option copy()
        {
            var copyOfOption = new option
                                   {
                                       globalLabelStartLoc = globalLabelStartLoc,
                                       nodes = new List<node>(nodes),
                                       arcs = new List<arc>(arcs),
                                       hyperarcs = new List<hyperarc>(hyperarcs),
                                       optionNumber = optionNumber,
                                       parameters = new List<double>(parameters),
                                       positionTransform = positionTransform,
                                       probability = probability,
                                       Relaxations = Relaxations.copy(),
                                       rule = rule,
                                       ruleNumber = ruleNumber,
                                       ruleSetIndex = ruleSetIndex
                                   };
            return copyOfOption;
        }

        internal option assignRuleInfo(int ruleIndex, int RuleSetIndex)
        {
            ruleNumber = ruleIndex;
            ruleSetIndex = RuleSetIndex;
            return this;
        }

        #endregion


        /// <summary>
        /// Finds the L mapped element.
        /// </summary>
        /// <param name="GraphElementName">Name of the graph element.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Graph element named \ + GraphElementName + \ was not found in the L-mapping of rule  + GraphElementName</exception>
        public graphElement findLMappedElement(string GraphElementName)
        {
            graphElement elt = rule.L[GraphElementName];
            if (elt != null) return findLMappedElement(elt);
            throw new Exception("Graph element named \"" + GraphElementName + "\" was not found in the L-mapping of rule " + GraphElementName);
        }
        /// <summary>
        /// Finds the L mapped element.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Graph element not found in rule's left-hand-side (GrammarRule.findMappedElement)</exception>
        public graphElement findLMappedElement(graphElement x)
        {
            if (x is hyperarc) return findLMappedHyperarc((hyperarc)x);
            if (x is node) return findLMappedNode((node)x);
            if (x is arc) return findLMappedArc((arc)x);
            throw new Exception("Graph element not found in rule's left-hand-side (GrammarRule.findMappedElement)");
        }
        /// <summary>
        /// Finds the L mapped node.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        public node findLMappedNode(node x)
        {
            return nodes[rule.L.nodes.IndexOf(x)];
        }
        /// <summary>
        /// Finds the L mapped arc.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        public arc findLMappedArc(arc x)
        {
            return arcs[rule.L.arcs.IndexOf(x)];
        }
        /// <summary>
        /// Finds the L mapped hyperarc.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        public hyperarc findLMappedHyperarc(hyperarc x)
        {
            return hyperarcs[rule.L.hyperarcs.IndexOf(x)];
        }

        #region Confluence and Invalidation Matrix- functions for comparing option to option
        /// <summary>
        /// Create lists on integers within each option that indicates what other
        /// options in that list it is confluent with. As discussed below confluence
        /// is commutative which saves a little time in this function, but it is not
        /// transitive - meaning is A is confluent with B and C. It is not necessarily
        /// true that B is confluent with C.
        /// </summary>
        /// <param name="options">The list of options to assign confluence</param>
        /// <param name="cand">The cand.</param>
        /// <param name="confluenceAnalysis">The confluence analysis.</param>
        /// <returns></returns>
        public static int[,] AssignOptionConfluence(List<option> options, candidate cand,
            ConfluenceAnalysis confluenceAnalysis)
        {
            var numOpts = options.Count;
            var invalidationMatrix = MakeInvalidationMatrix(options, cand, confluenceAnalysis);

            foreach (var o in options)
                o.confluence = new List<int>();
            for (var i = 0; i < numOpts - 1; i++)
                for (var j = i + 1; j < numOpts; j++)
                    if (confluenceAnalysis == ConfluenceAnalysis.OptimisticSimple)
                    {
                        if (invalidationMatrix[i, j] <= 0 && invalidationMatrix[j, i] <= 0)
                        {
                            options[i].confluence.Add(j);
                            options[j].confluence.Add(i);
                        }
                    }
                    else if (invalidationMatrix[i, j] < 0 && invalidationMatrix[j, i] < 0)
                    {
                        options[i].confluence.Add(j);
                        options[j].confluence.Add(i);
                    }

            return invalidationMatrix;
        }

        /// <summary>
        /// Makes the invalidation matrix.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cand">The cand.</param>
        /// <param name="confluenceAnalysis">The confluence analysis.</param>
        /// <returns></returns>
        public static int[,] MakeInvalidationMatrix(List<option> options, candidate cand, ConfluenceAnalysis confluenceAnalysis)
        {
            var numOpts = options.Count;
            var invalidationMatrix = new int[numOpts, numOpts];
            for (var i = 0; i < numOpts; i++)
                for (var j = 0; j < numOpts; j++)
                {
                    if (i == j) invalidationMatrix[i, j] = -1;
                    invalidationMatrix[i, j] = doesPInvalidateQ(options[i], options[j], cand, confluenceAnalysis);
                }
            return invalidationMatrix;
        }

        /// <summary>
        /// Predicts whether the option p is invalidates option q.
        /// This invalidata is a tricky thing. For the most part, this function
        /// has been carefully coded to handle almost all cases. The only exceptions
        /// are from what the additional recognize and apply functions require or modify.
        /// This is handled by actually testing to see if this is true.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="q">The q.</param>
        /// <param name="cand">The cand.</param>
        /// <param name="confluenceAnalysis">The confluence analysis.</param>
        /// <returns></returns>
        private static int doesPInvalidateQ(option p, option q, candidate cand, ConfluenceAnalysis confluenceAnalysis)
        {

            #region Global Labels
            var pIntersectLabels = p.rule.L.globalLabels.Intersect(p.rule.R.globalLabels);
            var pRemovedLabels = new List<string>(p.rule.L.globalLabels);
            pRemovedLabels.RemoveAll(s => pIntersectLabels.Contains(s));
            var pAddedLabels = new List<string>(p.rule.R.globalLabels);
            pAddedLabels.RemoveAll(s => pIntersectLabels.Contains(s));

            if ( /* first check that there are no labels deleted that the other depeonds on*/
                (q.rule.L.globalLabels.Intersect(pRemovedLabels).Any()) ||
                /* adding labels is problematic if the other rule was recognized under
                 * the condition of containsAllLocalLabels. */
                ((q.rule.containsAllGlobalLabels) && (pAddedLabels.Any())) ||
                /* adding labels is also problematic if you add a label that negates the
                 * other rule. */
                (pAddedLabels.Intersect(q.rule.negateLabels).Any()))
                return 1;
            #endregion

            #region Nodes
            /* first we check the nodes. If two options do not share any nodes, then 
             * the whole block of code is skipped. q is to save time if comparing many
             * options on a large graph. However, since there is some need to understand what
             * nodes are saved in rule execution, the following two lists are defined outside
             * of q condition and are used in the Arcs section below. */
            /* why are the following three parameters declared here and not in scope with the
             * other node parameters below? This is because they are used in the induced and
             * shape restriction calculations below - why calculate twice? */
            int Num_pKNodes = 0;
            string[] pNodesKNames = null;
            node[] pKNodes = null;

            var commonNodes = q.nodes.Intersect(p.nodes);
            if (commonNodes.Any())
            /* if there are no common nodes, then no need to check the details. */
            {
                /* the following arrays of nodes are within the rule not the host. */

                #region  Check whether there are nodes that p will delete that q depends upon.
                var pNodesLNames = from n in p.rule.L.nodes
                                   where !((ruleNode)n).NotExist
                                   select n.name;
                var pNodesRNames = from n in p.rule.R.nodes
                                   select n.name;
                pNodesKNames = pNodesRNames.Intersect(pNodesLNames).ToArray();
                Num_pKNodes = pNodesKNames.GetLength(0);
                pKNodes = new node[Num_pKNodes];
                for (var i = 0; i < p.rule.L.nodes.Count; i++)
                {
                    var index = Array.IndexOf(pNodesKNames, p.rule.L.nodes[i].name);
                    if (index >= 0) pKNodes[index] = p.nodes[i];
                    else if (commonNodes.Contains(p.nodes[i])) return 1;
                }
                #endregion

                #region NodesModified
                /* in the above regions where deletions are checked, we also create lists for potentially
                 * modified nodes, nodes common to both L and R. We will now check these. There are several 
                 * ways that a node can be modified:
                 * 1. labels are removed
                 * 2. labels are added (and potentially in the negabels of the other rule).
                 * 3. number of arcs connected, which affect strictDegreeMatch
                 * 4. variables are added/removed/changed
                 * 
                 * There first 3 conditions are check all at once below. For the last one, it is impossible
                 * to tell without executing extra functions that the user may have created for rule
                 * recognition. Therefore, additional functions are not check in q confluence check. */
                foreach (var commonNode in commonNodes)
                {
                    var qNodeL = (ruleNode)q.rule.L.nodes[q.nodes.IndexOf(commonNode)];
                    var pNodeL = (ruleNode)p.rule.L.nodes[p.nodes.IndexOf(commonNode)];
                    var pNodeR = (ruleNode)p.rule.R[pNodeL.name];
                    pIntersectLabels = pNodeL.localLabels.Intersect(pNodeR.localLabels);
                    pRemovedLabels = new List<string>(pNodeL.localLabels);
                    pRemovedLabels.RemoveAll(s => pIntersectLabels.Contains(s));
                    pAddedLabels = new List<string>(pNodeR.localLabels);
                    pAddedLabels.RemoveAll(s => pIntersectLabels.Contains(s));
                    if ( /* first check that there are no labels deleted that the other depeonds on*/
                        (qNodeL.localLabels.Intersect(pRemovedLabels).Any()) ||
                        /* adding labels is problematic if the other rule was recognized under
                         * the condition of containsAllLocalLabels. */
                        ((qNodeL.containsAllLocalLabels) && (pAddedLabels.Any())) ||
                        /* adding labels is also problematic if you add a label that negates the
                         * other rule. */
                        (pAddedLabels.Intersect(qNodeL.negateLabels).Any()) ||
                        /* if one rule uses strictDegreeMatch, we need to make sure the other rule
                         * doesn't change the degree. */
                        (qNodeL.strictDegreeMatch && (pNodeL.degree != pNodeR.degree)) ||
                        /* actually, the degree can also change from free-arc embedding rules. These
                         * are checked below. */
                        (qNodeL.strictDegreeMatch &&
                         (p.rule.embeddingRules.FindAll(e => (e.RNodeName.Equals(pNodeR.name))).Count > 0)))
                        return 1;
                }
                #endregion
            }

            #endregion
            #region Arcs
            var commonArcs = q.arcs.Intersect(p.arcs);
            if (commonArcs.Any())
            /* if there are no common arcs, then no need to check the details. */
            {
                /* the following arrays of arcs are within the rule not the host. */
                #region  Check whether there are arcs that p will delete that q depends upon.
                var pArcsLNames = from n in p.rule.L.arcs
                                  where !((ruleArc)n).NotExist
                                  select n.name;
                var pArcsRNames = from n in p.rule.R.arcs
                                  select n.name;
                var pArcsKNames = new List<string>(pArcsRNames.Intersect(pArcsLNames));
                var pKArcs = new arc[pArcsKNames.Count()];
                for (var i = 0; i < p.rule.L.arcs.Count; i++)
                {
                    if (pArcsKNames.Contains(p.rule.L.arcs[i].name))
                        pKArcs[pArcsKNames.IndexOf(p.rule.L.arcs[i].name)] = p.arcs[i];
                    else if (commonArcs.Contains(p.arcs[i])) return 1;
                }
                #endregion


                #region ArcsModified
                foreach (var commonArc in commonArcs)
                {
                    var qArcL = (ruleArc)q.rule.L.arcs[q.arcs.IndexOf(commonArc)];
                    var pArcL = (ruleArc)p.rule.L.arcs[p.arcs.IndexOf(commonArc)];
                    var pArcR = (ruleArc)p.rule.R[pArcL.name];
                    pIntersectLabels = pArcL.localLabels.Intersect(pArcR.localLabels);
                    pRemovedLabels = new List<string>(pArcL.localLabels);
                    pRemovedLabels.RemoveAll(s => pIntersectLabels.Contains(s));
                    pAddedLabels = new List<string>(pArcR.localLabels);
                    pAddedLabels.RemoveAll(s => pIntersectLabels.Contains(s));
                    if ( /* first check that there are no labels deleted that the other depeonds on*/
                        (qArcL.localLabels.Intersect(pRemovedLabels).Any()) ||
                        /* adding labels is problematic if the other rule was recognized under
                         * the condition of containsAllLocalLabels. */
                        ((qArcL.containsAllLocalLabels) && (pAddedLabels.Any())) ||
                        /* adding labels is also problematic if you add a label that negates the
                         * other rule. */
                        (pAddedLabels.Intersect(qArcL.negateLabels).Any()) ||
                        /* if one rule uses strictDegreeMatch, we need to make sure the other rule
                         * doesn't change the degree. */

                        /* if one rule requires that an arc be dangling for correct recognition (nullMeansNull)
                         * then we check to make sure that the other rule doesn't add a node to it. */
                        ((qArcL.nullMeansNull)
                        && (((qArcL.From == null) && (pArcR.From != null)) ||
                                ((qArcL.To == null) && (pArcR.To != null)))) ||
                        /* well, even if the dangling isn't required, we still need to ensure that p
                         * doesn't put a node on an empty end that q is expecting to belong 
                         * to some other node. */
                        ((pArcL.From == null) && (pArcR.From != null) && (qArcL.From != null)) ||
                        /* check the To direction as well */
                         ((pArcL.To == null) && (pArcR.To != null) && (qArcL.To != null)) ||

                        /* in q, the rule is not matching with a dangling arc, but we need to ensure that
                         * the rule doesn't remove or re-connect the arc to something else in the K of the rule
                         * such that the recogniation of the second rule is invalidated. q may be a tad strong 
                         * (or conservative) as there could still be confluence despite the change in connectivity.
                         * How? I have yet to imagine. But clearly the assumption here is that change in 
                         * connectivity prevent confluence. */
                        ((pArcL.From != null) &&
                                 (pNodesKNames != null && pNodesKNames.Contains(pArcL.From.name)) &&
                                 ((pArcR.From == null) || (pArcL.From.name != pArcR.From.name))) ||
                        ((pArcL.To != null) &&
                                 (pNodesKNames != null && pNodesKNames.Contains(pArcL.To.name)) &&
                                 ((pArcR.To == null) || (pArcL.To.name != pArcR.To.name))) ||

                        /* Changes in Arc Direction

                        /* finally we check that the changes in arc directionality (e.g. making
                         * directed, making doubly-directed, making undirected) do not affect 
                         * the other rule. */
                        /* Here, the directionIsEqual restriction is easier to check than the 
                         * default case where directed match with doubly-directed and undirected
                         * match with directed. */
                        ((qArcL.directionIsEqual) &&
                            ((!qArcL.directed.Equals(pArcR.directed)) ||
                             (!qArcL.doublyDirected.Equals(pArcR.doublyDirected)))) ||
                         ((qArcL.directed && !pArcR.directed) ||
                            (qArcL.doublyDirected && !pArcR.doublyDirected))
                            )
                        return 1;
                }
                #endregion
            }
            #endregion

            #region HyperArcs
            /* Onto hyperarcs! q is similiar to nodes - more so than arcs. */
            var commonHyperArcs = q.hyperarcs.Intersect(p.hyperarcs);
            if (commonHyperArcs.Any())
            {
                #region  Check whether there are hyperarcs that p will delete that q.option depends upon.

                var pHyperArcsLNames = from n in p.rule.L.hyperarcs
                                       where !((ruleHyperarc)n).NotExist
                                       select n.name;
                var pHyperArcsRNames = from n in p.rule.R.hyperarcs select n.name;
                var pHyperArcsKNames = new List<string>(pHyperArcsRNames.Intersect(pHyperArcsLNames));
                var pKHyperarcs = new hyperarc[pHyperArcsKNames.Count()];

                for (var i = 0; i < p.rule.L.hyperarcs.Count; i++)
                {
                    if (pHyperArcsKNames.Contains(p.rule.L.hyperarcs[i].name))
                        pKHyperarcs[pHyperArcsKNames.IndexOf(p.rule.L.hyperarcs[i].name)] = p.hyperarcs[i];
                    else if (commonHyperArcs.Contains(p.hyperarcs[i])) return 1;
                }
                #endregion

                #region HyperArcsModified
                foreach (var commonHyperArc in commonHyperArcs)
                {
                    var qHyperArcL = (ruleHyperarc)q.rule.L.hyperarcs[q.hyperarcs.IndexOf(commonHyperArc)];
                    var pHyperArcL = (ruleHyperarc)p.rule.L.hyperarcs[p.hyperarcs.IndexOf(commonHyperArc)];
                    var pHyperArcR = (ruleHyperarc)p.rule.R[pHyperArcL.name];
                    pIntersectLabels = pHyperArcL.localLabels.Intersect(pHyperArcR.localLabels);
                    pRemovedLabels = new List<string>(pHyperArcL.localLabels);
                    pRemovedLabels.RemoveAll(s => pIntersectLabels.Contains(s));
                    pAddedLabels = new List<string>(pHyperArcR.localLabels);
                    pAddedLabels.RemoveAll(s => pIntersectLabels.Contains(s));

                    if ( /* first check that there are no labels deleted that the other depends on*/
                        (qHyperArcL.localLabels.Intersect(pRemovedLabels).Any()) ||
                        /* adding labels is problematic if the other rule was recognized under
                         * the condition of containsAllLocalLabels. */
                        ((qHyperArcL.containsAllLocalLabels) && (pAddedLabels.Any())) ||
                        /* adding labels is also problematic if you add a label that negates the
                         * other rule. */
                        (pAddedLabels.Intersect(qHyperArcL.negateLabels).Any()) ||
                        /* if one rule uses strictDegreeMatch, we need to make sure the other rule
                         * doesn't change the degree. */
                        (qHyperArcL.strictNodeCountMatch && (pHyperArcL.degree != pHyperArcR.degree)))
                        /* actually, the degree can also change from free-arc embedding rules. These
                         * are checked below. */
                        return 1;
                }
                #endregion
            }
            #endregion

            #region now we're left with some tricky checks...
            if (commonNodes.Any())
            {
                #region if q is induced
                /* if q is induced then p will invalidate it, if it adds arcs between the 
                  * common nodes. */
                if (q.rule.induced)
                {
                    var pArcsLNames = from a in p.rule.L.arcs select a.name;
                    if ((from newArc in p.rule.R.arcs.Where(a => !pArcsLNames.Contains(a.name))
                         where newArc.To != null && newArc.From != null
                         let toName = newArc.To.name
                         let fromName = newArc.To.name
                         where pNodesKNames.Contains(toName) && pNodesKNames.Contains(fromName)
                         where commonNodes.Contains(pKNodes[Array.IndexOf(pNodesKNames, toName)])
                               && commonNodes.Contains(pKNodes[Array.IndexOf(pNodesKNames, fromName)])
                         select toName).Any())
                    {
                        return 1;
                    }
                    /* is there another situation in which an embedding rule in p may work against
                 * q being an induced rule? It doesn't seem like it would seem embedding rules
                 * reattach free-arcs. oh, what about arc duplication in embedding rules? nah. */
                }

                #endregion

                #region shape restrictions
                for (int i = 0; i < Num_pKNodes; i++)
                {
                    var pNode = pKNodes[i];
                    if (commonNodes.Contains(pNode)) continue;
                    var pname = pNodesKNames[i];
                    var lNode = (node)p.rule.L[pname];
                    var rNode = (node)p.rule.R[pname];

                    if (q.rule.UseShapeRestrictions && p.rule.TransformNodePositions &&
                        !(MatrixMath.sameCloseZero(lNode.X, rNode.X) &&
                          MatrixMath.sameCloseZero(lNode.Y, rNode.Y) &&
                          MatrixMath.sameCloseZero(lNode.Z, rNode.Z)))
                        return 1;
                    if ((q.rule.RestrictToNodeShapeMatch && p.rule.TransformNodeShapes && lNode.DisplayShape != null
                        && rNode.DisplayShape != null)
                        && !(MatrixMath.sameCloseZero(lNode.DisplayShape.Height, rNode.DisplayShape.Height) &&
                             MatrixMath.sameCloseZero(lNode.DisplayShape.Width, rNode.DisplayShape.Width) &&
                             MatrixMath.sameCloseZero(p.positionTransform[0, 0], 1) &&
                             MatrixMath.sameCloseZero(p.positionTransform[1, 1], 1) &&
                             MatrixMath.sameCloseZero(p.positionTransform[1, 0]) &&
                             MatrixMath.sameCloseZero(p.positionTransform[0, 1])))
                        return 1;
                }
                #endregion
            }

            /* you've run the gauntlet of easy check
            * except (1) if there is something caught by additional recognition functions,
            * or (2) NOTExist elements now exist. These can only be solving by an empirical
            * test, which will be expensive. 
            * So, now we switch from conditions that return false to conditions that return true.
            */

            if (q.rule.ContainsNegativeElements || q.rule.recognizeFuncs.Count > 0 || p.rule.applyFuncs.Count > 0)
                if (confluenceAnalysis == ConfluenceAnalysis.Full) return fullInvalidationCheck(p, q, cand);
                else return 0;
            return -1;

            #endregion
        }
        /// <summary>
        /// Does a full invalidation check through empirical evidence. That is, it makes
        /// a copy of the graph and tests to see if this is true.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="q">The q.</param>
        /// <param name="cand">The cand.</param>
        /// <returns></returns>
        static int fullInvalidationCheck(option p, option q, candidate cand)
        {
            var testGraph = cand.graph.copy();
            var qCopy = q.copy();
            var pCopy = p.copy();
            SearchProcess.transferLmappingToChild(testGraph, cand.graph, pCopy);
            SearchProcess.transferLmappingToChild(testGraph, cand.graph, qCopy);
            pCopy.apply(testGraph, null);
            var newOptions = q.rule.recognize(testGraph);
            if (newOptions.Any(newOpt => sameLocation(qCopy, newOpt))) return -1;
            //  find if there is an option in newOptions that is the SAME elements as q.location
            // then return false - p does NOT invalidate q
            return 1;

        }

        /// <summary>
        /// Sames the location.
        /// </summary>
        /// <param name="qOption">The q option.</param>
        /// <param name="newOption">The new option.</param>
        /// <returns></returns>
        private static bool sameLocation(option qOption, option newOption)
        {
            if (qOption.nodes.Where((t, i) => t != newOption.nodes[i]).Any())
                return false;
            if (qOption.arcs.Where((t, i) => t != newOption.arcs[i]).Any())
                return false;
            if (qOption.hyperarcs.Where((t, i) => t != newOption.hyperarcs[i]).Any())
                return false;
            return true;
        }

        #endregion
    }
}