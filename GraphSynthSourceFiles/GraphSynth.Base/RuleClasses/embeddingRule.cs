/*************************************************************************
 *     This embeddingRule file & class is part of the GraphSynth.BaseClasses 
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
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace GraphSynth.Representation
{
    /// <summary>
    ///   The embedding rule defines what to do with arcs that are freed (one or both of its connecting
    ///   nodes is deleted) by the enclosing grammar rule.
    /// </summary>
    public class embeddingRule
    {
        #region Properties and Fields

        /// <summary>
        ///   The list of labels that the free arc must have for this embedding rule to be recognized.
        /// </summary>
        public List<string> freeArcLabels = new List<string>();

        /// <summary>
        ///   The list of labels that the free arc must NOT have for this embedding rule to be recognized.
        /// </summary>
        public List<string> freeArcNegabels = new List<string>();

        /// <summary>
        ///   The list of labels that the remaining neighboring node 
        ///   must have for this embedding rule to be recognized.
        /// </summary>
        public List<string> neighborNodeLabels = new List<string>();

        /// <summary>
        ///   The list of labels that the remaining neighboring node 
        ///   must NOT have for this embedding rule to be recognized.
        /// </summary>
        public List<string> neighborNodeNegabels = new List<string>();

        /// <summary>
        ///   In order to be backwards compatible, this field captures the old fields and converts them
        ///   to proper places. See use in BasicFiler.cs
        /// </summary>
        [XmlAnyElement]
        public XElement[] oldLabels;

        /// <summary>
        ///   Gets or sets the name of the L node that was attached to the 
        ///   free arc. This is NOT the name of the node in the host graph, 
        ///   but in the L of the rule.
        /// </summary>
        /// <value>The name of the L node.</value>
        public string LNodeName { get; set; }

        /// <summary>
        ///   Gets or sets the name of the R node to connect this arc to. There is no need to include 
        ///   any other defining character - of course we still need to find the corresponding node 
        ///   in H1 to connect it to. Note, this is also the main quality that distinguishes the 
        ///   approach as NCE or NLC, as the control is given to the each individual of R-L (or the
        ///   daughter graph in the NCE lingo) as opposed to simply a label based method.
        /// </summary>
        /// <value>The name of the R node.</value>
        public string RNodeName { get; set; }

        /// <summary>
        ///   Gets or sets the original direction that must be free for this embedding rule to be recognized.
        ///   in order to give the edNCE approach the "ed" quality, we must allow for the possibility of
        ///   recognizing arcs having a particular direction. The original direction can be either +1 meaning
        ///   "to", or -1 meaning "from", or 0 meaning no imposed direction - this indicates what side of the 
        ///   arc is dangling. Furthermore, the newDirection, can specify a new direction of the arc ("to",
        ///   or "from" being the new connection) or "" (unspecified) for updating the arc. This allows us 
        ///   to change the direction of the arc, or keep it as is.
        /// </summary>
        /// <value>The original direction.</value>
        public sbyte originalDirection { get; set; }

        /// <summary>
        ///   Gets or sets the new direction of the arc. Yes, the arc can actually be flipped by the embedding rule.
        /// </summary>
        /// <value>The new direction.</value>
        public sbyte newDirection { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether [allow arc duplication].
        /// </summary>
        /// <value><c>true</c> if [allow arc duplication]; otherwise, <c>false</c>.</value>
        public Boolean allowArcDuplication { get; set; }

        /* if allowArcDuplication is true then for each rule that matches with the arc the arc will be 
         * duplicated. */

        #endregion

        #region Methods

        internal static Boolean hyperArcIsFree(hyperarc dangleHyperArc, designGraph host, out List<node> neighborNodes)
        {
            neighborNodes = dangleHyperArc.nodes.Where(n => !host.nodes.Contains(n)).ToList();
            return (neighborNodes.Count > 0);
        }
        /// <summary>
        ///   Is the arc a free arc from this grammar rule?
        /// </summary>
        /// <param name = "a">A.</param>
        /// <param name = "host">The host.</param>
        /// <param name = "freeEndIdentifier">The free end identifier.</param>
        /// <param name = "neighborNode">The neighbor node.</param>
        /// <returns></returns>
        public static Boolean arcIsFree(arc a, designGraph host, out sbyte freeEndIdentifier, out node neighborNode)
        {
            if (a.From != null && a.To != null &&
                !host.nodes.Contains(a.From) && !host.nodes.Contains(a.To))
            {
                freeEndIdentifier = 0;
                /* if the nodes on either end of the freeArc are pointing to previous nodes 
                 * that were deleted in the first pushout then neighborNode is null (and as
                 * a result any rules using the neighborNodeLabel will not apply) and the 
                 * freeEndIdentifier is zero. */
                neighborNode = null;
                return true;
            }
            if (a.From != null && !host.nodes.Contains(a.From))
            {
                freeEndIdentifier = -1;
                /* freeEndIdentifier set to -1 means that the From end of the arc must be the free end.*/
                neighborNode = a.To;
                return true;
            }
            if (a.To != null && !host.nodes.Contains(a.To))
            {
                freeEndIdentifier = +1;
                /* freeEndIdentifier set to +1 means that the To end of the arc must be the free end.*/
                neighborNode = a.From;
                return true;
            }
            /* else, the arc is not a free arc after all and we simply break out 
                 * of this loop and try the next arc. */
            freeEndIdentifier = 0;
            neighborNode = null;
            return false;
        }


        /// <summary>
        ///   Is the rule recognized on the given inputs?
        /// </summary>
        /// <param name = "freeEndIdentifier">The free end identifier.</param>
        /// <param name = "freeArc">The free arc.</param>
        /// <param name = "neighborNode">The neighbor node.</param>
        /// <param name = "nodeRemoved">The node removed.</param>
        /// <returns></returns>
        internal Boolean ruleIsRecognized(sbyte freeEndIdentifier, arc freeArc,
                                        node neighborNode, node nodeRemoved)
        {
            if (freeEndIdentifier * originalDirection < 0) return false;
            /* this one is a little bit of enigmatic but clever coding if I do say so myself. Both
                * of these variables can be either +1, 0, -1. If in multiplying the two together you 
                * get -1 then this is the only incompability. Combinations of +1&+1, or +1&0, or 
                * -1&-1 all mean that the arc has a free end on the requested side (From or To). */

            List<string> neighborlabels = null;
            if (neighborNode != null) neighborlabels = neighborNode.localLabels;
            return ((labelsMatch(freeArc.localLabels, neighborlabels))
                    &&
                    ((nodeRemoved == null) ||
                     ((freeArc.To == nodeRemoved) && (freeEndIdentifier >= 0)) ||
                     ((freeArc.From == nodeRemoved) && (freeEndIdentifier <= 0))));
        }




        internal Boolean ruleIsRecognized(hyperarc dangleHyperArc, List<node> neighborNodes, node nodeRemoved)
        {
            IEnumerable<string> neighborlabels = null ;
            neighborlabels = neighborNodes.Aggregate(neighborlabels, (current, n) => current.Union(n.localLabels));
            return ((labelsMatch(dangleHyperArc.localLabels, neighborlabels.ToList()))
                   && ((nodeRemoved == null) || (dangleHyperArc.nodes.Contains(nodeRemoved))));
        }

        private Boolean labelsMatch(List<string> hostFreeArcLabels, List<string> hostNeighborLabels)
        {
            if (hostFreeArcLabels == null) hostFreeArcLabels = new List<string>();
            if (hostNeighborLabels == null) hostNeighborLabels = new List<string>();
            if (freeArcNegabels.Any(label => !label.Equals("<none>") && hostFreeArcLabels.Contains(label)))
                return false;

            if (neighborNodeNegabels.Any(label => !label.Equals("<none>") && hostNeighborLabels.Contains(label)))
                return false;

            /* next, set up a tempLabels so that we don't change the 
             * host's actual labels. We delete an instance of the label. 
             * this is new in version 1.8. It's important since one may
             * have multiple identical labels. */
            var tempLabels = new List<string>();
            if (!freeArcLabels.Contains("<any>"))
            {
                tempLabels.AddRange(hostFreeArcLabels);

                foreach (var label in freeArcLabels)
                {
                    if (tempLabels.Contains(label)) tempLabels.Remove(label);
                    else return false;
                }
            }

            tempLabels.Clear();
            if (!neighborNodeLabels.Contains("<any>"))
            {
                tempLabels.AddRange(hostNeighborLabels);

                foreach (var label in neighborNodeLabels)
                {
                    if (tempLabels.Contains(label)) tempLabels.Remove(label);
                    else return false;
                }
            }


            return true;
        }

        #endregion
    }
}