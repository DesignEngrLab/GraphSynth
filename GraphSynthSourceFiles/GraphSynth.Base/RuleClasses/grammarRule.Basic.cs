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
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace GraphSynth.Representation
{
    /// <summary>
    /// </summary>
    public partial class grammarRule
    {

        /// <summary>
        /// the host graph is stored as an internal (private) field to a rule during recognition. 
        /// Since recognition just reads from it, this simplifies the need to pass it along to every
        /// recognition function. 
        /// </summary>
        private designGraph host;

        /// <summary>
        ///   any mathematical operations are fair game for the recognize and apply local variables.
        ///   At the end of a graph recognition, we check all the recognize functions, if any yield a 
        ///   positive number than the rule is infeasible. This is done in case1LocationFound.
        /// </summary>
        [XmlIgnore]
        public object DLLofFunctions;

        [XmlIgnore]
        private List<string> _applyFunctions;
        private List<embeddingRule> _embeddingRules;
        [XmlIgnore]
        private List<string> _recognizeFunctions;

        /// <summary>
        ///   a list of MethodInfo's corresponding to the strings in applyFunctions
        /// </summary>
        [XmlIgnore]
        public List<MethodInfo> applyFuncs = new List<MethodInfo>();

        /// <summary>
        ///   These are place holders when the user has clicked OrderedGlobalLabels. There may, in fact,
        ///   be multiple locations for the globalLabels to be recognized. The are determined in the 
        ///   OrderLabelsMatch function.
        /// </summary>
        protected List<int> globalLabelStartLocs = new List<int>();

        /// <summary>
        ///   this is where we store the subgraphs or locations of where the
        ///   rule can be applied. It's global to a particular L but it is invoked
        ///   only at the very bottom of the recursion tree - see the end of
        ///   recognizeRecursion().
        /// </summary>
        protected List<option> options = new List<option>();

        /// <summary>
        ///   a list of MethodInfo's corresponding to the strings in recognizeFunctions
        /// </summary>
        [XmlIgnore]
        public List<MethodInfo> recognizeFuncs = new List<MethodInfo>();

        /// <summary>
        ///   Gets or sets the name of the rule.
        /// </summary>
        /// <value>The name.</value>
        public string name { get; set; }

        /// <summary>
        ///   Gets or sets a comment about the rule.
        /// </summary>
        /// <value>The comment.</value>
        public string comment { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether this <see cref = "grammarRule" /> is termination.
        /// </summary>
        /// <value><c>true</c> if termination; otherwise, <c>false</c>.</value>
        public Boolean termination { get; set; }

        /// <summary>
        ///   Gets or sets the embedding rules.
        /// </summary>
        /// <value>The embedding rules.</value>
        public List<embeddingRule> embeddingRules
        {
            get { return _embeddingRules ?? (_embeddingRules = new List<embeddingRule>()); }
            set { _embeddingRules = value; }
        }

        /* after double pushout runs its course, we'd like to account for dangling arcs that were victims
         * of the G - (L - R) pushout. these rules are defined here following the edNCE approach (edge-directed 
         * Neighborhood Controlled Embedding) and discussed in more detail below. */

        /// <summary>
        ///   Gets or sets the additional recognize functions names.
        /// </summary>
        /// <value>The recognize functions.</value>
        public List<string> recognizeFunctions
        {
            get { return _recognizeFunctions ?? (_recognizeFunctions = new List<string>()); }
            set { _recognizeFunctions = value; }
        }

        /// <summary>
        ///   Gets or sets the apply functions.
        /// </summary>
        /// <value>The apply functions.</value>
        public List<string> applyFunctions
        {
            get { return _applyFunctions ?? (_applyFunctions = new List<string>()); }
            set { _applyFunctions = value; }
        }

        /// <summary>
        ///   Gets or sets  the left-hand-side of the rule. It is a graph that is to be 
        ///   recognized in the host graph.
        /// </summary>
        /// <value>The L.</value>
        public designGraph L { get; set; }

        /* . */

        /// <summary>
        ///   Gets or sets the right-hand-side of the rule. It is a graph that is to be 
        ///   inserted (glued) into the host graph.
        /// </summary>
        /// <value>The R.</value>
        public designGraph R { get; set; }

        #region Subset or Equal Booleans and Matching Functions

        /* these are special booleans used by recognize. In many cases, the L will be a subset of the
         * host in all respects (a proper subset - a subgraph which is anything but equal). However, 
         * there may be times when the user wants to restrict the number of recognized locations, by 
         * looking for an EQUAL conditions as opposed to simply a SUBSET. The following booleans 
         * capture the possible ways in which the subgraph may/may not be a subset (boolean set to false)
         * or is equal (in this respective quality) to the host (boolean set to true). */

        private List<string> _negateLabels;

        /// <summary>
        ///   Gets or sets a value indicating whether this <see cref = "grammarRule" /> is spanning.
        /// </summary>
        /// <value><c>true</c> if spanning; otherwise, <c>false</c>.</value>
        public Boolean spanning { get; set; }

        /* if true then all nodes in L must be in host and vice-verse - NOT a proper subset
        /* if false then proper subset. */

        /// <summary>
        ///   Gets or sets a value indicating whether this <see cref = "grammarRule" /> is induced.
        /// </summary>
        /// <value><c>true</c> if induced; otherwise, <c>false</c>.</value>
        public Boolean induced { get; set; }

        /* if true then all arcs between the nodes in L must be in host and no more 
         * - again not a proper SUBSET
         * if false then proper subset.
         * this following function is the only to use induced and is only called early
         * in the Location Found case, and only then when induced is true. As its name implies it 
         * simply checks to see if there are any arcs in the host between the nodes recognized. */

        /// <summary>
        ///   Gets or sets the negating labels - labels that must NOT exist in the host.
        /// </summary>
        /// <value>The negating labels.</value>
        public List<string> negateLabels
        {
            get { return _negateLabels ?? (_negateLabels = new List<string>()); }
            set { _negateLabels = value; }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether the host must contains only the
        ///   global labels of the rule. Said another way, the rule must contain all global labels
        ///   in the host to be a valid match. If false, then a subset of global labels is sufficient.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [contains all global labels]; otherwise, <c>false</c>.
        /// </value>
        public Boolean containsAllGlobalLabels { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether the host must contain the
        ///   global labels in the presented order. This is mainly to allow for the creation of traditional
        ///   string grammars.
        /// </summary>
        /// <value><c>true</c> if [ordered global labels]; otherwise, <c>false</c>.</value>
        public Boolean OrderedGlobalLabels { get; set; }
        #endregion

        /// <summary>
        ///   Makes a unique name of a node.
        /// </summary>
        /// <returns></returns>
        public string makeUniqueNodeName(string stub = "n")
        {
            stub = stub.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            var i = 0;
            while ((L.nodes.Exists(b => b.name.Equals(stub + i)))
                   || (R.nodes.Exists(b => b.name.Equals(stub + i))))
                i++;
            return stub + i;
        }

        /// <summary>
        ///   Makes a unique name of an arc.
        /// </summary>
        /// <returns></returns>
        public string makeUniqueArcName(string stub = "a")
        {
            stub = stub.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            var i = 0;
            while ((L.arcs.Exists(b => b.name.Equals(stub + i)))
                   || (R.arcs.Exists(b => b.name.Equals(stub + i))))
                i++;
            return stub + i;
        }

        /// <summary>
        ///   Makes a unique name of a hyperarc.
        /// </summary>
        /// <returns></returns>
        public string makeUniqueHyperarcName(string stub = "ha")
        {
            stub = stub.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            var i = 0;
            while ((L.hyperarcs.Exists(b => b.name.Equals(stub + i)))
                   || (R.hyperarcs.Exists(b => b.name.Equals(stub + i))))
                i++;
            return stub + i;
        }

        IList<int> LDegreeSequence
        {
            get
            {
                var degrees = new List<int>();
                foreach (var n in L.nodes)
                {
                    if (((ruleNode)n).MustExist)
                        degrees.Add(((ruleNode)n).degree);
                }
                degrees.Sort();
                degrees.Reverse();
                return degrees;
            }
        }
        IList<int> LHyperArcDegreeSequence
        {
            get
            {
                var degrees = new List<int>();
                foreach (var ha in L.hyperarcs)
                {
                    if (((ruleHyperarc)ha).MustExist)
                        degrees.Add(((ruleHyperarc)ha).degree);
                }
                degrees.Sort();
                degrees.Reverse();
                return degrees;
            }
        }
        //private grammarRule copy()
        //{
        //    return new grammarRule
        //               {
        //                   applyFuncs = new List<MethodInfo>(applyFuncs),
        //                   applyFunctions = new List<string>(applyFunctions),
        //                   comment = comment,
        //                   containsAllGlobalLabels = containsAllGlobalLabels,
        //                   DLLofFunctions = DLLofFunctions,
        //                   embeddingRules = new List<embeddingRule>(embeddingRules),
        //                   Flip = Flip,
        //                   globalLabelStartLocs = new List<int>(globalLabelStartLocs),
        //                   host=host,
        //                   induced = induced,
        //                   _in_parallel_ = _in_parallel_,
        //                   L = L.copy(),
        //                   name = name,
        //                   negateLabels = new List<string>(negateLabels),
        //                   ...
        //               };
        //}
    }
}