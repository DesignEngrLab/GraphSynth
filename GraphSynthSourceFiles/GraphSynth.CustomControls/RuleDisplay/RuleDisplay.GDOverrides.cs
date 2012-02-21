using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
using GraphSynth.Representation;
using GraphSynth.UI;

namespace GraphSynth.GraphDisplay
{
    public partial class RuleDisplay : GraphGUI
    {
        #region Fields

        /// <summary>
        ///   field to store the parent rule window to which this graph is associated
        /// </summary>
        public IRuleWindow rW
        {
            get { return OwnerWindow as IRuleWindow; }
        }

        #endregion

        #region AddNewElements
        // Nodes Methods
        protected override void addNewNode(string shapeKey, Point point)
        {
            base.addNewNode(shapeKey, point);
            if (rW.graphGUIK == this) AddKNodeToLandR(graph.nodes.Last());
        }

        protected override node InstantiateNewNode()
        {
            var newNode = new ruleNode(rW.rule.makeUniqueNodeName());
            graph.addNode(newNode);
            return newNode;
        }

        // Arc Methods
        protected override void completeNewArc(arc a, node nodeToAttach, bool attachHeadTo)
        {
            base.completeNewArc(a, nodeToAttach, attachHeadTo);
            if (nodeToAttach == null) return;
            if (propagateChange)
            {
                if (rW.graphGUIK == this)
                {
                    var Larc = (ruleArc)rW.rule.L.arcs.Find(b => (b.name == a.name));
                    var Rarc = (ruleArc)rW.rule.R.arcs.Find(b => (b.name == a.name));
                    if (Larc == null && Rarc == null) AddKArcToLandR(a);
                    else
                    {
                        var LNode = rW.rule.L.nodes.Find(b => string.Equals(b.name, nodeToAttach.name));
                        rW.graphGUIL.propagateChange = false;
                        rW.graphGUIL.completeNewArc(Larc, LNode, attachHeadTo);
                        var RNode = rW.rule.R.nodes.Find(b => string.Equals(b.name, nodeToAttach.name));
                        rW.graphGUIR.propagateChange = false;
                        rW.graphGUIR.completeNewArc(Rarc, RNode, attachHeadTo);
                    }
                }
                else if (this == rW.graphGUIL)
                {
                    var Karc = rW.graphGUIK.graph.arcs.Find(b => string.Equals(b.name, a.name));
                    var KNode = rW.graphGUIK.graph.nodes.Find(b => string.Equals(b.name, nodeToAttach.name));
                    if ((Karc != null) && (KNode != null))
                    {
                        rW.graphGUIK.propagateChange = false;
                        rW.graphGUIK.completeNewArc(Karc, KNode, attachHeadTo);
                    }
                }
            }
            else propagateChange = true;
        }

        protected override arc InstantiateNewArc()
        {
            var newArc = new ruleArc(rW.rule.makeUniqueArcName());
            graph.addArc(newArc);
            return newArc;
        }

        // Hyperarc Methods
        protected override void addNewHyperArc(string shapeKey, Point point)
        {
            base.addNewHyperArc(shapeKey, point);
            if (rW.graphGUIK == this) AddKHyperToLandR(graph.hyperarcs.Last());
        }
        protected override hyperarc InstantiateNewHyperArc()
        {
            var newHA = new ruleHyperarc(rW.rule.makeUniqueHyperarcName());
            graph.addHyperArc(newHA);
            return newHA;
        }
        protected override void completeNewHyperArcConnection(hyperarc h, node nodeToAttach)
        {
            base.completeNewHyperArcConnection(h, nodeToAttach);
            if (nodeToAttach == null) return;
            if (propagateChange)
            {
                if (rW.graphGUIK == this)
                {
                    // then need to update the arc names in both L and R
                    var LHyper = rW.rule.L.hyperarcs.Find(b => string.Equals(b.name, h.name));
                    var LNode = rW.rule.L.nodes.Find(b => string.Equals(b.name, nodeToAttach.name));
                    rW.graphGUIL.propagateChange = false;
                    rW.graphGUIL.completeNewHyperArcConnection(LHyper, LNode);
                    var RHyper = rW.rule.R.hyperarcs.Find(b => string.Equals(b.name, h.name));
                    var RNode = rW.rule.R.nodes.Find(b => string.Equals(b.name, nodeToAttach.name));
                    rW.graphGUIR.propagateChange = false;
                    rW.graphGUIR.completeNewHyperArcConnection(RHyper, RNode);
                }
                else if (this == rW.graphGUIL)
                {
                    var KHyper = rW.graphGUIK.graph.hyperarcs.Find(b => string.Equals(b.name, h.name));
                    var KNode = rW.graphGUIK.graph.nodes.Find(b => string.Equals(b.name, nodeToAttach.name));
                    if ((KHyper != null) && (KNode != null))
                    {
                        rW.graphGUIK.propagateChange = false;
                        rW.graphGUIK.completeNewHyperArcConnection(KHyper, KNode);
                    }
                }
            }
            else propagateChange = true;
        }
        #endregion

        #region Paste and Delete Overrides

        public override void Paste()
        {
            var ClipboardString = Clipboard.GetText();
            var copiedSelection = SelectionClass.DeSerializeClipboardFormatFromXML(ClipboardString);
            RestoreDisplayShapes(copiedSelection.ReadInXmlShapes, copiedSelection.selectedNodes,
                copiedSelection.selectedArcs, copiedSelection.selectedHyperArcs);
            var newSelection = new List<UIElement>();
            if (copiedSelection != null)
            {
                var tempGraph = new designGraph();
                foreach (node n in copiedSelection.selectedNodes)
                {
                    if (n is ruleNode)
                        tempGraph.addNode(n);
                    else tempGraph.addNode(ruleNode.ConvertFromNode(n));
                }
                foreach (arc a in copiedSelection.selectedArcs)
                {
                    if (a is ruleArc)
                        tempGraph.addArc(a);
                    else tempGraph.addArc(ruleArc.ConvertFromArc(a));
                }
                foreach (hyperarc a in copiedSelection.selectedHyperArcs)
                {
                    if (a is ruleHyperarc)
                        tempGraph.addHyperArc(a);
                    else tempGraph.addHyperArc(ruleHyperarc.ConvertFromHyperArc(a));
                }
                tempGraph.internallyConnectGraph();

                foreach (node n in tempGraph.nodes)
                {
                    var mousePos = Mouse.GetPosition(this);
                    n.X = mousePos.X + n.X - copiedSelection.ReferencePoint.X - Origin.X;
                    n.Y = mousePos.Y + n.Y - copiedSelection.ReferencePoint.Y - Origin.Y;
                    n.name = rW.rule.makeUniqueNodeName(n.name);
                    addNodeShape(n);
                    graph.addNode(n);
                    newSelection.Add((Shape)n.DisplayShape.Shape);

                    if (rW.graphGUIK == this) AddKNodeToLandR(n);
                }
                foreach (arc a in tempGraph.arcs)
                {
                    a.name = rW.rule.makeUniqueArcName(a.name);
                    graph.addArc(a);
                    AddArcShape(a);
                    SetUpNewArcShape(a);
                    newSelection.Add((ArcShape)a.DisplayShape.Shape);

                    if (rW.graphGUIK == this) AddKArcToLandR(a);
                }
                foreach (hyperarc h in tempGraph.hyperarcs)
                {
                    h.name = rW.rule.makeUniqueHyperarcName(h.name);
                    graph.addHyperArc(h);
                    AddHyperArcShape(h);
                    newSelection.Add((HyperArcShape)h.DisplayShape.Shape);

                    if (rW.graphGUIK == this) AddKHyperToLandR(h);
                }
                Select(newSelection);
            }
        }

        public override void Delete()
        {
            if (this == rW.graphGUIK) // deleted in K- remove nodes and arcs from both L and R
            {
                var Kdialog = new KDeleteDialog();
                Kdialog.ShowDialog();
                switch (Kdialog.result)
                {
                    case -1:
                        return;
                    case 2:
                        rW.graphGUIR.Delete(Selection.FindCommonSelection(rW.graphGUIR));
                        break;
                    case 1:
                        rW.graphGUIL.Delete(Selection.FindCommonSelection(rW.graphGUIL));
                        break;
                    case 0:
                        rW.graphGUIR.Delete(Selection.FindCommonSelection(rW.graphGUIR));
                        rW.graphGUIL.Delete(Selection.FindCommonSelection(rW.graphGUIL));
                        break;
                }
                Delete(Selection);
            }
            else if (this == rW.graphGUIL)
            {
                Delete(Selection);
                rW.graphGUIK.Delete(Selection.FindCommonSelection(rW.graphGUIK));
            }
            else if (this == rW.graphGUIR)
            {
                Delete(Selection);
                rW.graphGUIK.Delete(Selection.FindCommonSelection(rW.graphGUIK));
            }
        }

        private void Delete(SelectionClass newSelect)
        {
            Selection = newSelect;
            base.Delete();
        }

        public override void DisconnectArcHead(arc a)
        {
            base.DisconnectArcHead(a);
            if (propagateChange)
            {
                if (rW.graphGUIK == this)
                {
                    // then need to update the arc names in both L and R
                    var Larc = rW.rule.L.arcs.Find(b => string.Equals(b.name, a.name));
                    rW.graphGUIL.propagateChange = false;
                    rW.graphGUIL.DisconnectArcHead(Larc);
                    var Rarc = rW.rule.R.arcs.Find(b => string.Equals(b.name, a.name));
                    rW.graphGUIR.propagateChange = false;
                    rW.graphGUIR.DisconnectArcHead(Rarc);
                }
                else if (this == rW.graphGUIL)
                {
                    var Karc = rW.graphGUIK.graph.arcs.Find(b => string.Equals(b.name, a.name));
                    if (Karc != null)
                    {
                        rW.graphGUIK.propagateChange = false;
                        rW.graphGUIK.DisconnectArcHead(Karc);
                    }
                }
            }
            else propagateChange = true;
        }

        public override void DisconnectArcTail(arc a)
        {
            base.DisconnectArcTail(a);
            if (propagateChange)
            {
                if (rW.graphGUIK == this)
                {
                    // then need to update the arc names in both L and R
                    var Larc = rW.rule.L.arcs.Find(b => string.Equals(b.name, a.name));
                    rW.graphGUIL.propagateChange = false;
                    rW.graphGUIL.DisconnectArcTail(Larc);
                    var Rarc = rW.rule.R.arcs.Find(b => string.Equals(b.name, a.name));
                    rW.graphGUIR.propagateChange = false;
                    rW.graphGUIR.DisconnectArcTail(Rarc);
                }
                else if (this == rW.graphGUIL)
                {
                    var Karc = rW.graphGUIK.graph.arcs.Find(b => string.Equals(b.name, a.name));
                    if (Karc != null)
                    {
                        rW.graphGUIK.propagateChange = false;
                        rW.graphGUIK.DisconnectArcTail(Karc);
                    }
                }
            }
            else propagateChange = true;
        }

        public override void FlipArc(arc a)
        {
            base.FlipArc(a);
            if (propagateChange)
            {
                if (rW.graphGUIK == this)
                {
                    // then need to update the arc names in both L and R
                    var Larc = rW.rule.L.arcs.Find(b => string.Equals(b.name, a.name));
                    rW.graphGUIL.propagateChange = false;
                    rW.graphGUIL.FlipArc(Larc);
                    var Rarc = rW.rule.R.arcs.Find(b => string.Equals(b.name, a.name));
                    rW.graphGUIR.propagateChange = false;
                    rW.graphGUIR.FlipArc(Rarc);
                }
                else if (this == rW.graphGUIL)
                {
                    var Karc = rW.graphGUIK.graph.arcs.Find(b => string.Equals(b.name, a.name));
                    if (Karc != null)
                    {
                        rW.graphGUIK.propagateChange = false;
                        rW.graphGUIK.FlipArc(Karc);
                    }
                }
            }
            else propagateChange = true;
        }

        public override void DisconnectHyperArcConnection(hyperarc h, node n)
        {
            base.DisconnectHyperArcConnection(h, n);
            if (propagateChange)
            {
                if (rW.graphGUIK == this)
                {
                    // then need to update the arc names in both L and R
                    var LHyper = rW.rule.L.hyperarcs.Find(b => string.Equals(b.name, h.name));
                    var LNode = rW.rule.L.nodes.Find(b => string.Equals(b.name, n.name));
                    rW.graphGUIL.propagateChange = false;
                    rW.graphGUIL.DisconnectHyperArcConnection(LHyper, LNode);
                    var RHyper = rW.rule.R.hyperarcs.Find(b => string.Equals(b.name, h.name));
                    var RNode = rW.rule.R.nodes.Find(b => string.Equals(b.name, n.name));
                    rW.graphGUIR.propagateChange = false;
                    rW.graphGUIR.DisconnectHyperArcConnection(RHyper, RNode);
                }
                else if (this == rW.graphGUIL)
                {
                    var KHyper = rW.graphGUIK.graph.hyperarcs.Find(b => string.Equals(b.name, h.name));
                    var KNode = rW.graphGUIK.graph.nodes.Find(b => string.Equals(b.name, n.name));
                    if ((KHyper != null) && (KNode != null))
                    {
                        rW.graphGUIK.propagateChange = false;
                        rW.graphGUIK.DisconnectHyperArcConnection(KHyper, KNode);
                    }
                }
            }
            else propagateChange = true;
        }

        #endregion

        #region OnEvents and Related Methods

        public Boolean propagateChange = true;

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            rW.rule.ResetRegularizationMatrix();
        }

        protected override void OnSelectionChanged(EventArgs e)
        {
            base.OnSelectionChanged(e);
            mainObject.propertyUpdate(this);
        }

        public override void UpdateXYCoordinatesInNodes(node Node)
        {
            var u = (Shape)Node.DisplayShape.Shape;
            u.SetValue(LeftProperty, double.NaN);
            u.SetValue(TopProperty, double.NaN);
            u.SetValue(RightProperty, double.NaN);
            u.SetValue(BottomProperty, double.NaN);
            base.UpdateXYCoordinatesInNodes(Node);
            NodePropertyChanged(Node);
        }

        public override void MoveShapeToXYNodeCoordinates(node Node)
        {
            var u = (Shape)Node.DisplayShape.Shape;
            u.SetValue(LeftProperty, double.NaN);
            u.SetValue(TopProperty, double.NaN);
            u.SetValue(RightProperty, double.NaN);
            u.SetValue(BottomProperty, double.NaN);
            Node.DisplayShape.ScreenX = Node.X + Origin.X;
            Node.DisplayShape.ScreenY = Node.Y + Origin.Y;
            NodePropertyChanged(Node);
        }

        public override void NodePropertyChanged(node n)
        {
            base.NodePropertyChanged(n);
            if (propagateChange)
            {
                if (rW.graphGUIK == this)
                {
                    // then need to update the node names in both L and R
                    var Lnode = rW.rule.L.nodes.Find(b => string.Equals(b.name, n.name));
                    rW.graphGUIL.propagateChange = false;
                    rW.graphGUIL.ApplyNodeFormatting(Lnode, n, true, true, true);
                    var Rnode = rW.rule.R.nodes.Find(b => string.Equals(b.name, n.name));
                    rW.graphGUIR.propagateChange = false;
                    rW.graphGUIR.ApplyNodeFormatting(Rnode, n, true, true, true);
                }
                else if (this == rW.graphGUIL)
                {
                    var Knode = rW.graphGUIK.graph.nodes.Find(b => string.Equals(b.name, n.name));
                    if (Knode != null)
                    {
                        rW.graphGUIK.propagateChange = false;
                        rW.graphGUIK.ApplyNodeFormatting(Knode, n, true, true, true);
                    }
                }
            }
            else propagateChange = true;
        }

        public override void ArcPropertyChanged(arc a)
        {
            base.ArcPropertyChanged(a);
            if (rW.graphGUIK == this)
            {
                // then need to update the arc names in both L and R
                var Larc = rW.rule.L.arcs.Find(b => string.Equals(b.name, a.name));
                if (Larc != null)
                    rW.graphGUIL.ApplyArcFormatting(Larc, a, true, true);
                var Rarc = rW.rule.R.arcs.Find(b => string.Equals(b.name, a.name));
                if (Rarc != null)
                    rW.graphGUIR.ApplyArcFormatting(Rarc, a, true, true);
            }
            else if (this == rW.graphGUIL)
            {
                var Karc = rW.graphGUIK.graph.arcs.Find(b => string.Equals(b.name, a.name));
                if (Karc != null)
                    rW.graphGUIK.ApplyArcFormatting(Karc, a, true, true);
            }
        }
        public override void HyperArcPropertyChanged(hyperarc h)
        {
            base.HyperArcPropertyChanged(h);
            if (propagateChange)
            {
                if (rW.graphGUIK == this)
                {
                    // then need to update the node names in both L and R
                    var LHyper = rW.rule.L.hyperarcs.Find(b => string.Equals(b.name, h.name));
                    rW.graphGUIL.propagateChange = false;
                    rW.graphGUIL.ApplyHyperArcFormatting(LHyper, h, true);
                    var RHyper = rW.rule.R.hyperarcs.Find(b => string.Equals(b.name, h.name));
                    rW.graphGUIR.propagateChange = false;
                    rW.graphGUIR.ApplyHyperArcFormatting(RHyper, h, true);
                }
                else if (this == rW.graphGUIL)
                {
                    var Knode = rW.graphGUIK.graph.hyperarcs.Find(b => string.Equals(b.name, h.name));
                    if (Knode != null)
                    {
                        rW.graphGUIK.propagateChange = false;
                        rW.graphGUIK.ApplyHyperArcFormatting(Knode, h, true);
                    }
                }
            }
            else propagateChange = true;
        }

        #endregion

        /// <summary>
        ///   This method updates the L, K, and R graphs to reflect a change in node name.
        /// </summary>
        /// <param name = "n">node that was renamed</param>
        /// <param name = "NewName">The new name.</param>
        public override void NodeNameChanged(node n, string NewName)
        {
            if (!string.Equals(n.name, NewName))
            {
                #region RenamedNodeInK

                // then need to update the node names in both L and R
                if (rW.graphGUIK == this)
                {
                    var Lnode = rW.rule.L.nodes.Find(b => string.Equals(b.name, n.name));
                    Lnode.name = NewName;
                    var nI = ((DisplayShape)Lnode.DisplayShape).icon;
                    var mbe = BindingOperations.GetMultiBindingExpression(nI, IconShape.DisplayTextProperty);
                    mbe.UpdateTarget();
                    var Rnode = rW.rule.R.nodes.Find(b => string.Equals(b.name, n.name));
                    Rnode.name = NewName;
                    nI = ((DisplayShape)Rnode.DisplayShape).icon;
                    mbe = BindingOperations.GetMultiBindingExpression(nI, IconShape.DisplayTextProperty);
                    mbe.UpdateTarget();
                }
                #endregion

                else
                {
                    #region RenamedNodeInLorR - CheckForBreakingACommonNodeRelationship

                    // does renaming breaks a common nodes relationship then need to deleted the node from K
                    var Knode = rW.graphGUIK.graph.nodes.Find(b => string.Equals(b.name, n.name));
                    if (Knode != null)
                        rW.graphGUIK.RemoveNodeFromGraph(Knode);

                    #endregion

                    #region RenamedNodeInLorR - CheckForCreatingACommonNodeRelationship

                    // does renaming results in a common node then need to copy it into K
                    var match = false;

                    // finding the active graph canvas
                    if (rW.graphGUIL == this)
                        match = rW.rule.R.nodes.Exists(b => string.Equals(b.name, NewName));
                    else if (rW.graphGUIR == this)
                        match = rW.rule.L.nodes.Exists(b => string.Equals(b.name, NewName));

                    if (match)
                    {
                        var Lnode = n;
                        if (rW.graphGUIR == this)
                            Lnode = rW.rule.L.nodes.Find(b => string.Equals(b.name, NewName));

                        var tempNode = (ruleNode)(Lnode).copy();
                        tempNode.name = NewName;
                        //add it to K graph
                        // the node need to be added to K in the same location as it was in L graph
                        rW.graphGUIK.addNodeShape(tempNode);
                        rW.graphGUIK.graph.addNode(tempNode);
                    }

                    #endregion
                }
                mainObject.propertyUpdate(this);
            }
            base.NodeNameChanged(n, NewName);
        }

        /// <summary>
        /// This method updates the L, K, and R graphs to reflect a change in arc name.
        /// </summary>
        /// <param name="a">arc, a.</param>
        /// <param name="NewName">The new name.</param>
        public override void ArcNameChanged(arc a, string NewName)
        {
            if (!string.Equals(a.name, NewName))
            {
                #region RenamedArcInK

                // then need to update the node names in both L and R
                if (rW.graphGUIK == this)
                {
                    var Larc = rW.rule.L.arcs.Find(b => string.Equals(b.name, a.name));
                    Larc.name = NewName;
                    var nI = ((DisplayShape)Larc.DisplayShape).icon;
                    var mbe = BindingOperations.GetMultiBindingExpression(nI, IconShape.DisplayTextProperty);
                    mbe.UpdateTarget();
                    var Rarc = rW.rule.R.arcs.Find(b => string.Equals(b.name, a.name));
                    Rarc.name = NewName;
                    nI = ((DisplayShape)Rarc.DisplayShape).icon;
                    mbe = BindingOperations.GetMultiBindingExpression(nI, IconShape.DisplayTextProperty);
                    mbe.UpdateTarget();
                }
                #endregion

                else
                {
                    #region RenamedArcInLorR - CheckForBreakingACommonArcRelationship

                    // does renaming breaks a common arcs relationship then need to deleted the arc from K
                    var Karc = rW.graphGUIK.graph.arcs.Find(b => string.Equals(b.name, a.name));
                    if (Karc != null)
                        rW.graphGUIK.RemoveArcFromGraph(Karc);

                    #endregion

                    #region RenamedArcInLorR - CheckForCreatingACommonArcRelationship

                    // does renaming results in a common arc then need to copy it into K
                    var match = false;

                    // finding the active graph canvas
                    if (rW.graphGUIL == this)
                        match = rW.rule.R.arcs.Exists(b => string.Equals(b.name, NewName));
                    else if (rW.graphGUIR == this)
                        match = rW.rule.L.arcs.Exists(b => string.Equals(b.name, NewName));

                    if (match)
                    {
                        var Larc = a;
                        if (rW.graphGUIR == this)
                            Larc = rW.rule.L.arcs.Find(b => string.Equals(b.name, NewName));
                        Karc = (ruleArc)(Larc).copy();
                        Karc.name = NewName;
                        Karc.From =
                            rW.graphGUIK.graph.nodes.Find(
                                b => string.Equals(b.name, Larc.From.name));
                        Karc.To =
                            rW.graphGUIK.graph.nodes.Find(
                                b => string.Equals(b.name, Larc.To.name));

                        rW.graphGUIK.graph.addArc(Karc, Karc.From, Karc.To);
                        rW.graphGUIK.AddArcShape(Karc);
                        rW.graphGUIK.SetUpNewArcShape(Karc);
                    }
                }

                    #endregion

                base.ArcNameChanged(a, NewName);
            }
        }

        public override void HyperArcNameChanged(hyperarc h, string NewName)
        {
            if (!string.Equals(h.name, NewName))
            {
                #region RenamedNodeInK

                // then need to update the hyperarc names in both L and R
                if (rW.graphGUIK == this)
                {
                    var Lhyperarc = rW.rule.L.hyperarcs.Find(b => string.Equals(b.name, h.name));
                    Lhyperarc.name = NewName;
                    var hI = ((DisplayShape)Lhyperarc.DisplayShape).icon;
                    var mbe = BindingOperations.GetMultiBindingExpression(hI, IconShape.DisplayTextProperty);
                    mbe.UpdateTarget();
                    var Rhyperarc = rW.rule.R.hyperarcs.Find(b => string.Equals(b.name, h.name));
                    Rhyperarc.name = NewName;
                    hI = ((DisplayShape)Rhyperarc.DisplayShape).icon;
                    mbe = BindingOperations.GetMultiBindingExpression(hI, IconShape.DisplayTextProperty);
                    mbe.UpdateTarget();
                }
                #endregion

                else
                {
                    #region RenamedNodeInLorR - CheckForBreakingACommonNodeRelationship

                    // does renaming breaks a common hyperarcs relationship then need to deleted the hyperarc from K
                    var Khyperarc = rW.graphGUIK.graph.hyperarcs.Find(b => string.Equals(b.name, h.name));
                    if (Khyperarc != null)
                        rW.graphGUIK.RemoveHyperArcFromGraph(Khyperarc);

                    #endregion

                    #region RenamedNodeInLorR - CheckForCreatingACommonNodeRelationship

                    // does renaming results in a common hyperarc then need to copy it into K
                    var match = false;

                    // finding the active graph canvas
                    if (rW.graphGUIL == this)
                        match = rW.rule.R.hyperarcs.Exists(b => string.Equals(b.name, NewName));
                    else if (rW.graphGUIR == this)
                        match = rW.rule.L.hyperarcs.Exists(b => string.Equals(b.name, NewName));

                    if (match)
                    {
                        var Lhyperarc = h;
                        if (rW.graphGUIR == this)
                            Lhyperarc = rW.rule.L.hyperarcs.Find(b => string.Equals(b.name, NewName));

                        var tempHyper = (ruleHyperarc)Lhyperarc.copy();
                        tempHyper.name = NewName;
                        //add it to K graph
                        // the hyperarc need to be added to K in the same location as it was in L graph
                        rW.graphGUIK.AddHyperArcShape(tempHyper);
                        rW.graphGUIK.graph.addHyperArc(tempHyper);
                    }

                    #endregion
                }
                mainObject.propertyUpdate(this);
                //mainObject.property.FreeArcEmbedRulePrpt.Update(rW.rule);
            }
            base.HyperArcNameChanged(h, NewName);
        }
    }
}