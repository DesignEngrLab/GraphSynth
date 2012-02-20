using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using GraphSynth.Representation;
using System.Collections.Generic;

namespace GraphSynth.GraphDisplay
{
    public partial class GraphGUI : InkCanvas
    {
        public void SelectAll()
        {
            var SelectedShapes = nodeShapes.Cast<UIElement>().ToList();
            SelectedShapes.AddRange(arcShapes);
            SelectedShapes.AddRange(hyperarcShapes);
            Select(SelectedShapes);
        }

        protected void nudge(double xDiff, double yDiff)
        {
            if (Selection.selectedNodes.Count > 0)
            {
                foreach (var n in Selection.selectedNodes)
                {
                    n.X = n.X + xDiff;
                    n.Y = n.Y + yDiff;
                    MoveShapeToXYNodeCoordinates(n);
                }
                storeOnUndoStack();
            }
        }

        public virtual void NodePropertyChanged(node n)
        {
            storeOnUndoStack();
        }

        public virtual void ArcPropertyChanged(arc a)
        {
            storeOnUndoStack();
        }


        public virtual void HyperArcPropertyChanged(hyperarc h)
        {
            storeOnUndoStack();
        }

        public void ClearShapeBanks()
        {
            Selection.Clear();
            arcIcons.Clear();
            arcShapes.Clear();
            nodeIcons.Clear();
            nullNodeIcons.Clear();
            nodeShapes.Clear();
            hyperarcShapes.Clear();
            hyperarcIcons.Clear();
        }

        protected void nudgeDown()
        {
            nudge(0, -12);
        }

        protected void nudgeUp()
        {
            nudge(0, 12);
        }

        protected void nudgeLeft()
        {
            nudge(-12, 0);
        }

        protected void nudgeRight()
        {
            nudge(12, 0);
        }

        public virtual void UpdateXYCoordinatesInNodes(node Node)
        {
            if (Node != null)
            {
                Node.X = Node.DisplayShape.ScreenX - Origin.X;
                Node.Y = Node.DisplayShape.ScreenY - Origin.Y;
            }
            RedrawResizeAndReposition(true);
            storeOnUndoStack();
        }

        public void MoveShapesToXYNodeCoordinates()
        {
            foreach (var n in graph.nodes)
            {
                n.DisplayShape.ScreenX = n.X + Origin.X;
                n.DisplayShape.ScreenY = n.Y + Origin.Y;
            }
            RedrawResizeAndReposition(true);
            storeOnUndoStack();
        }

        public virtual void MoveShapeToXYNodeCoordinates(node n)
        {
            n.DisplayShape.ScreenX = n.X + Origin.X;
            n.DisplayShape.ScreenY = n.Y + Origin.Y;
            RedrawResizeAndReposition(true);
            storeOnUndoStack();
        }


        public void ApplyNodeFormatting(node victim, node datum, Boolean ChangeDimensions,
                                        Boolean ChangeShape, Boolean IncludeTransform)
        {
            var nDS = victim.DisplayShape;
            var nIS = ((DisplayShape)victim.DisplayShape).icon;
            var origH = victim.DisplayShape.Height;
            var origW = victim.DisplayShape.Width;
            var origX = victim.X;
            var origY = victim.Y;
            var origZ = victim.Z;
            if (ChangeShape)
            {
                nodeIcons.Remove(nIS);
                nodeShapes.Remove((Shape)nDS.Shape);
                victim.DisplayShape = datum.DisplayShape.Copy(victim);
                addNodeShape(victim);
                foreach (var a in victim.arcs)
                    if (a is arc) SetUpNewArcShape((arc)a);
                    else if (a is hyperarc) BindHyperArcToNodeShapes((hyperarc)a);
            }
            else
            {
                victim.DisplayShape.StrokeThickness = datum.DisplayShape.StrokeThickness;
                victim.DisplayShape.Stroke = ((Brush)datum.DisplayShape.Stroke).Clone();
                victim.DisplayShape.Fill = ((Brush)datum.DisplayShape.Fill).Clone();
            }
            if (ChangeDimensions)
            {
                victim.DisplayShape.Height = datum.DisplayShape.Height;
                victim.DisplayShape.Width = datum.DisplayShape.Width;
            }
            else
            {
                victim.DisplayShape.Height = origH;
                victim.DisplayShape.Width = origW;
            }
            if (IncludeTransform)
            {
                victim.DisplayShape.TransformMatrix = (double[,])datum.DisplayShape.TransformMatrix.Clone();
                victim.X = datum.X;
                victim.Y = datum.Y;
                victim.Z = datum.Z;
            }
            else
            {
                /* if the position was not to change, then reset to the original position */
                victim.X = origX;
                victim.Y = origY;
                victim.Z = origZ;
            }
            MoveShapeToXYNodeCoordinates(victim);
            RedrawResizeAndReposition();
            storeOnUndoStack();
        }

        public void ApplyArcFormatting(arc victim, arc datum, Boolean IncludeController,
                                       Boolean IncludeDirection)
        {
            /* Update Common Properties */
            victim.DisplayShape.Stroke = datum.DisplayShape.Stroke;
            victim.DisplayShape.Fill = datum.DisplayShape.Fill;
            victim.DisplayShape.StrokeThickness = datum.DisplayShape.StrokeThickness;
            ((ArcShape)victim.DisplayShape.Shape).ShowArrowHeads
                = ((ArcShape)datum.DisplayShape.Shape).ShowArrowHeads;

            #region Arc Direction
            if (IncludeDirection)
            {
                victim.directed = datum.directed;
                ((ArcShape)victim.DisplayShape.Shape).directed
                    = ((ArcShape)datum.DisplayShape.Shape).directed;
                victim.doublyDirected = datum.doublyDirected;
                ((ArcShape)victim.DisplayShape.Shape).doublyDirected
                    = ((ArcShape)datum.DisplayShape.Shape).doublyDirected;
            }
            #endregion

            if (IncludeController)
            {
                if (((ArcShape)victim.DisplayShape.Shape).Controller.GetType().Equals(
                                   ((ArcShape)datum.DisplayShape.Shape).Controller.GetType()))


                    ((ArcShape)datum.DisplayShape.Shape).Controller.copyValueTo(
                        ((ArcShape)victim.DisplayShape.Shape).Controller);
                else
                {
                    ((ArcShape)victim.DisplayShape.Shape).Controller
                        = ((ArcShape)datum.DisplayShape.Shape).Controller.copy((Shape)victim.DisplayShape.Shape);
                }
                ((ArcShape)victim.DisplayShape.Shape).Controller.Redraw();
                RedrawResizeAndReposition();
            }
            storeOnUndoStack();
        }

        public void ApplyHyperArcFormatting(hyperarc victim, hyperarc datum, Boolean IncludeController)
        {
            /* Update Common Properties */

            victim.DisplayShape.Stroke = datum.DisplayShape.Stroke;
            victim.DisplayShape.Fill = datum.DisplayShape.Fill;
            victim.DisplayShape.StrokeThickness = datum.DisplayShape.StrokeThickness;

            if (IncludeController)
            {
                if (((HyperArcShape)victim.DisplayShape.Shape).Controller.GetType().Equals(
                                   ((HyperArcShape)datum.DisplayShape.Shape).Controller.GetType()))
                    ((HyperArcShape)datum.DisplayShape.Shape).Controller.copyValueTo(
                        ((HyperArcShape)victim.DisplayShape.Shape).Controller);
                else
                {
                    ((HyperArcShape)victim.DisplayShape.Shape).Controller
                        = ((HyperArcShape)datum.DisplayShape.Shape).Controller.copy((HyperArcShape)victim.DisplayShape.Shape);
                }
                ((HyperArcShape)victim.DisplayShape.Shape).Controller.Redraw();
                RedrawResizeAndReposition();
            }
            storeOnUndoStack();
        }

        public void Rotate(node SelectedNode, double degree)
        {
            var u = (Shape)SelectedNode.DisplayShape.Shape;
            if (u != null)
            {
                var RotTransform = new RotateTransform();
                RotTransform.Angle = degree;
                u.RenderTransform = new MatrixTransform(RotTransform.Value.M11, RotTransform.Value.M12,
                                                        RotTransform.Value.M21,
                                                        RotTransform.Value.M22, u.RenderTransform.Value.OffsetX,
                                                        u.RenderTransform.Value.OffsetY);
                NodePropertyChanged(SelectedNode);
                storeOnUndoStack();
            }
        }

        public virtual void NodeNameChanged(node n, string NewName)
        {
            if (n != null) n.name = NewName;
            storeOnUndoStack();
        }

        public virtual void ArcNameChanged(arc a, string NewName)
        {
            if (a != null) a.name = NewName;
            storeOnUndoStack();
        }

        public virtual void HyperArcNameChanged(hyperarc a, string NewName)
        {
            if (a != null) a.name = NewName;
            storeOnUndoStack();
        }

        #region relate Shape to Node and Arc

        /// <summary>
        ///   Gets the node from graph that matches the selected shape. Since the shape is any XAML shape for nodes
        ///   this function is needed. For arcs and hyperarcs, all such objects inherit from a base class (ArcShape.cs
        ///   or HyperArcShape.cs), which include properties to point back to the graphElement. 
        /// </summary>
        /// <param name = "u">The selected object.</param>
        /// <returns>The node from the graph that owns the shape or icon that is the selectedObject.</returns>
        public node getNodeFromShape(UIElement u)
        {
            return graph.nodes.FirstOrDefault(n => n.DisplayShape.Shape == u);
        }
        public node getNodeFromPoint(Point p)
        {
            var icon = getNodeIconFromPoint(p);
            if (icon == null) return null;
            return (node)icon.GraphElement;
        }
        protected NodeIconShape getNodeIconFromPoint(Point p)
        {
            return (NodeIconShape)nodeIcons.FirstOrDefault(t => ((NodeIconShape)t).IsPointContained(p));
        }
        #endregion

        #region Undo & Redo
        void storeOnUndoStack()
        {
            if (doIndex > 0) doStates.RemoveRange(0, doIndex);
            doIndex = 0;
            doStates.Insert(0, graph.copy());
            if (doStates.Count > doLimit) doStates.RemoveAt(doLimit);
        }
        public void Undo()
        {
            ClearShapeBanks();
            graph = doStates[++doIndex].copy();
            InitDrawGraph(false);
            RedrawResizeAndReposition(true);
        }
        public void Redo()
        {
            ClearShapeBanks();
            graph = doStates[--doIndex].copy();
            InitDrawGraph(false);
            RedrawResizeAndReposition(true);
        }
        List<designGraph> doStates = new List<designGraph>();
        int doIndex;
        const int doLimit = 25;


        public bool UndoCanExecute()
        {
            return (doIndex < doStates.Count - 1);
        }
        public bool RedoCanExecute()
        {
            return (doIndex > 0);
        }
        #endregion
    }
}