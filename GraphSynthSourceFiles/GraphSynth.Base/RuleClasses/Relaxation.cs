/*************************************************************************
 *     This candidate file & class is part of the GraphSynth.BaseClasses
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GraphSynth.Representation
{
    /// <summary>
    /// 
    /// </summary>
    public class Relaxation : IEnumerable<RelaxItem>
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Relaxation"/> class.
        /// </summary>
        /// <param name="prescribedItems">The prescribed items.</param>
        /// <param name="NumberAllowable">The number allowable.</param>
        public Relaxation(List<RelaxItem> prescribedItems, int NumberAllowable = 0)
        {
            items = prescribedItems;
            InitialAllowableRelaxes = prescribedItems.Select(r => r.NumberAllowed).ToArray();
            if (NumberAllowable >= 0) this.NumberAllowable = NumberAllowable;
            else this.NumberAllowable = initialNumberAllowable = InitialAllowableRelaxes.Sum();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Relaxation"/> class.
        /// </summary>
        /// <param name="NumberAllowable">The number allowable.</param>
        public Relaxation(int NumberAllowable = 0)
        {
            this.NumberAllowable = initialNumberAllowable = NumberAllowable;
            if (NumberAllowable > 0)
            {
                items = new List<RelaxItem> { new RelaxItem(Relaxations.Any, NumberAllowable) };
            }
        }

        #endregion
        /// <summary>
        /// Gets the allowable relaxes.
        /// </summary>
        public int[] InitialAllowableRelaxes { get; private set; }

        /// <summary>
        /// Gets the number allowable relaxations that are left (not initially prescribed).
        /// </summary>
        public int NumberAllowable { get; internal set; }

        private int initialNumberAllowable;

        /// <summary>
        /// The prescribed relaxation items.
        /// </summary>
        private List<RelaxItem> items;
        /// <summary>
        /// Gets the fulfilled items.
        /// </summary>
        public List<RelaxItem> FulfilledItems
        {
            get { return fulfilledItems ?? (fulfilledItems = new List<RelaxItem>()); }
        }

        private List<RelaxItem> fulfilledItems;

        /// <summary>
        /// Gets the summary of relaxation that were used to make the match.
        /// </summary>
        public string RelaxationSummary
        {
            get
            {
                var result = "";
                foreach (var f in fulfilledItems)
                {
                    result += "\n" + f.RelaxationType.ToString().Replace('_', ' ');
                    result += " on the ";
                    if (f.GraphElement == null) result += "LHS graph";
                    else
                        result += f.GraphElement.GetType().BaseType.Name + " named " + f.GraphElement.name;
                    if (f.Datum != null) result += ": " + f.Datum;
                }
                result += ".\n";
                return result.Remove(0, 1);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<RelaxItem> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Copies this instance.
        /// </summary>
        /// <returns></returns>
        public Relaxation copy()
        {
            items = items ?? (items = new List<RelaxItem>());
            return new Relaxation
            {
                fulfilledItems = new List<RelaxItem>(FulfilledItems),
                InitialAllowableRelaxes = InitialAllowableRelaxes,
                items = items.Select(r => new RelaxItem(r.RelaxationType, r.NumberAllowed, r.GraphElement, r.Datum)).ToList(),
                NumberAllowable = NumberAllowable,
                initialNumberAllowable = initialNumberAllowable,
            };
        }

        /// <summary>
        /// Resets the relaxation back to the way it was originally defined.
        /// </summary>
        public void Reset()
        {
            NumberAllowable = initialNumberAllowable;
            fulfilledItems = null;
            for (int i = 0; i < InitialAllowableRelaxes.GetLength(0); i++)
                items[i].NumberAllowed = InitialAllowableRelaxes[i];
        }
    }
    /// <summary>
    /// The RelaxItem describes the manner in which one can relax a rule or ruleset.
    /// A list of these is defined for the Relaxation class.
    /// </summary>
    public class RelaxItem
    {
        private readonly string prefixForAltered;
        private readonly Boolean bothRevokeAndImpose;

        /// <summary>
        /// Gets the elt.
        /// </summary>
        public graphElement GraphElement { get; private set; }

        /// <summary>
        /// Gets the type of the relaxation.
        /// </summary>
        /// <value>
        /// The type of the relaxation.
        /// </value>
        public Relaxations RelaxationType { get; private set; }

        /// <summary>
        /// Gets the datum.
        /// </summary>
        public string Datum { get; private set; }

        /// <summary>
        /// Gets the applies to.
        /// </summary>
        RelaxAppliesTo AppliesTo { get; set; }
        enum RelaxAppliesTo
        {
            graph, node, arc, hyperarc, element
        }
        /// <summary>
        /// Gets or sets the number of relaxations of this type that are allowed.
        /// </summary>
        /// <value>
        /// The number allowed.
        /// </value>
        public int NumberAllowed { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelaxItem"/> class.
        /// </summary>
        /// <param name="RelaxationType">Type of the relaxation.</param>
        /// <param name="NumberAllowed">The number allowed.</param>
        /// <param name="GraphElement">The graph element.</param>
        /// <param name="Datum">The datum.</param>
        public RelaxItem(Relaxations RelaxationType, int NumberAllowed, graphElement GraphElement = null, string Datum = null)
        {
            this.GraphElement = GraphElement;
            this.RelaxationType = RelaxationType;
            bothRevokeAndImpose = RelaxationType.ToString().EndsWith("_Altered");
            if (bothRevokeAndImpose)
                prefixForAltered = RelaxationType.ToString().Replace("_Altered","");
            this.Datum = Datum;
            this.NumberAllowed = NumberAllowed;
            if (GraphElement != null)
                AppliesTo = (RelaxAppliesTo)Enum.Parse(typeof(RelaxAppliesTo), GraphElement.GetType().BaseType.Name, true);
            else if (RelaxationType == Relaxations.Additional_Functions_Revoked
                    || RelaxationType == Relaxations.Contains_All_Global_Labels_Revoked
                    || RelaxationType == Relaxations.Ordered_Global_Labels_Revoked
                    || RelaxationType == Relaxations.Global_Label_Revoked
                    || RelaxationType == Relaxations.Negate_Global_Label_Revoked
                    || RelaxationType == Relaxations.Induced_Revoked
                    || RelaxationType == Relaxations.Shape_Restriction_Revoked)
                AppliesTo = RelaxAppliesTo.graph;
            else if (RelaxationType == Relaxations.Direction_Is_Equal_Revoked
                || RelaxationType == Relaxations.Null_Means_Null_Revoked)
                AppliesTo = RelaxAppliesTo.arc;
            else if (RelaxationType == Relaxations.Strict_Degree_Match_Revoked
                    || RelaxationType == Relaxations.HyperArc_Preclusion_Revoked)
                AppliesTo = RelaxAppliesTo.node;
            else if (RelaxationType == Relaxations.Strict_Node_Count_Revoked)
                AppliesTo = RelaxAppliesTo.hyperarc;
            else AppliesTo = RelaxAppliesTo.element;
        }

        internal bool Matches(Relaxations rType, graphElement g = null, string datum = "")
        {
            if ((!string.IsNullOrWhiteSpace(Datum)) && (Datum != datum)) return false;
            if (NumberAllowed <= 0) return false;
            if (RelaxationType == Relaxations.Any) return true;
            if (bothRevokeAndImpose)
            {
                if (!rType.ToString().StartsWith(prefixForAltered)) return false;
            }
            else if (RelaxationType != rType) return false;
            if (g == null) return true;
            if (GraphElement != null) return (GraphElement == g);
            /* in the remaining cases, g is not null, but GraphElement is, so we need to look at "Applies To" */
            if (AppliesTo == RelaxAppliesTo.element) return true;
            return ((g is node && AppliesTo == RelaxAppliesTo.node)
                    || (g is arc && AppliesTo == RelaxAppliesTo.arc)
                    || (g is hyperarc && AppliesTo == RelaxAppliesTo.hyperarc));
        }
    }
    /// <summary>
    /// The enumerator of the Relaxation Types
    /// </summary>
    public enum Relaxations
    {
        /// <summary>
        /// Any is a wildcard for any of the following specific types of relaxations
        /// </summary>
        Any,
        #region Initial Graph Conditions
        /// <summary>
        /// the contains all global labels condition is revoked
        /// </summary>
        Contains_All_Global_Labels_Revoked,
        /// <summary>
        /// the ordered global labels condition is revoked 
        /// </summary>
        Ordered_Global_Labels_Revoked,
        /// <summary>
        /// a global label as indicated by |DATUM| is revoked.
        /// </summary>
        Global_Label_Revoked,
        /// <summary>
        /// a global negating label as indicated by |DATUM| is revoked.
        /// </summary>
        Negate_Global_Label_Revoked,
        /// <summary>
        /// the spanning restriction is revoked
        /// </summary>
        Spanning_Revoked,
        #endregion
        #region Final Graph Conditions
        /// <summary>
        /// A particular additional functions as indicated by |DATUM| is revoked.
        /// </summary>
        Additional_Functions_Revoked,
        /// <summary>
        /// all shape restrictions are revoked
        /// </summary>
        Shape_Restriction_Revoked,
        /// <summary>
        /// the induced condition is revoked (set to false)
        /// </summary>
        Induced_Revoked,
        #endregion
        #region Element Specific
        /// <summary>
        /// the contains all local labels condition of the |GraphElement| is revoked
        /// </summary>
        Contains_All_Local_Labels_Revoked,
        /// <summary>
        /// the contains all local labels condition of the |GraphElement| is imposed 
        /// to prevent a match for negative (NOT_EXIST) elements.
        /// </summary>
        Contains_All_Local_Labels_Imposed,
        /// <summary>
        /// the contains all local labels condition of the |GraphElement| can be
        /// either revoked (for positivie elements) or imposed (for negative elements).
        /// </summary>
        Contains_All_Local_Labels_Altered,
        /// <summary>
        /// a label of the |GraphElement| as named in |DATUM| is revoked.
        /// </summary>
        Label_Revoked,
        /// <summary>
        /// a label of the |GraphElement| as named in |DATUM| is imposed 
        /// to prevent a match for negative (NOT_EXIST) elements. 
        /// </summary>
        Label_Imposed,
        /// <summary>
        /// a label of the |GraphElement| as named in |DATUM| can be
        /// either revoked (for positivie elements) or imposed (for negative elements).
        /// </summary>
        Label_Altered,
        /// <summary>
        /// the negating label of the |GraphElement| as named in |DATUM| is revoked.
        /// </summary>
        Negate_Label_Revoked,
        /// <summary>
        /// the negating label of the |GraphElement| as named in |DATUM| is imposed 
        /// to prevent a match for negative (NOT_EXIST) elements.
        /// </summary>
        Negate_Label_Imposed,
        /// <summary>
        /// the negating label of the |GraphElement| as named in |DATUM| can be
        /// either revoked (for positivie elements) or imposed (for negative elements).
        /// </summary>
        Negate_Label_Altered,
        /// <summary>
        /// the NullMeansNull condition of the arc indicated by |GraphElement| is revoked
        /// (treated as false).
        /// </summary>
        Null_Means_Null_Revoked,
        /// <summary> 
        /// the NullMeansNull condition of the arc indicated by |GraphElement| is imposed 
        /// (set as true) to prevent a match for negative (NOT_EXIST) elements.
        /// </summary>
        Null_Means_Null_Imposed,
        /// <summary>
        /// the NullMeansNull condition of the arc indicated by |GraphElement| can be
        /// either revoked (for positivie elements) or imposed (for negative elements).
        /// </summary>
        Null_Means_Null_Altered,
        /// <summary>
        /// the DirectionIsEqual condition of the arc indicated by |GraphElement| is revoked
        /// </summary>
        Direction_Is_Equal_Revoked,
        /// <summary>
        /// the DirectionIsEqual condition of the arc indicated by |GraphElement| is imposed 
        /// to prevent a match for negative (NOT_EXIST) elements.
        /// </summary>
        Direction_Is_Equal_Imposed,
        /// <summary>
        /// the DirectionIsEqual condition of the arc indicated by |GraphElement| can be
        /// either revoked (for positivie elements) or imposed (for negative elements).
        /// </summary>
        Direction_Is_Equal_Altered,
        /// <summary>
        /// Strict Degree Match of the node indicated by |GraphElement| is revoked
        /// </summary>
        Strict_Degree_Match_Revoked,
        /// <summary>
        /// Strict Degree Match of the node indicated by |GraphElement| is imposed 
        /// to prevent a match for negative (NOT_EXIST) elements.
        /// </summary>
        Strict_Degree_Match_Imposed,
        /// <summary>
        /// Strict Degree Match of the node indicated by |GraphElement| can be
        /// either revoked (for positivie elements) or imposed (for negative elements).
        /// </summary>
        Strict_Degree_Match_Altered,
        /// <summary>
        /// Strict Node Count of the hyperarc indicated by |GraphElement| is revoked
        /// </summary>
        Strict_Node_Count_Revoked,
        /// <summary>
        /// Strict Node Count of the hyperarc indicated by |GraphElement| is imposed 
        /// to prevent a match for negative (NOT_EXIST) elements.
        /// </summary>
        Strict_Node_Count_Imposed,
        /// <summary>
        /// Strict Node Count of the hyperarc indicated by |GraphElement| can be
        /// either revoked (for positivie elements) or imposed (for negative elements).
        /// </summary>
        Strict_Node_Count_Altered,
        /// <summary>
        /// hyperarc preclusion is revoked, meaning that a node actually
        /// connects to a hyperarc (as indicated by |DATUM|) in L (even 
        /// though L shows them disconnected).
        /// </summary>
        HyperArc_Preclusion_Revoked,
        /// <summary>
        /// the target type of the |GraphElement| is revoked
        /// </summary>
        Target_Type_Revoked,
        #endregion
        #region Graph element addition or removal
        /// <summary>
        /// the NOTEXIST condition of |GraphElement| is revoked, but it must be found
        /// as if a positive element.
        /// </summary>
        Element_Made_Positive,
        /// <summary>
        /// the element as indicated by |GraphElement| is removed from L
        /// </summary>
        Element_Made_Negative
        #endregion
    }


}