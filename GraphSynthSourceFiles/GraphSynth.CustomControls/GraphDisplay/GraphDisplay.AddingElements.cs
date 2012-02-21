using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphSynth.Representation;
using GraphSynth.UI;

namespace GraphSynth.GraphDisplay
{
    public partial class GraphGUI : InkCanvas
    {
        public void InitDrawGraph(Boolean storeOnUndo = true)
        {
            foreach (var n in graph.nodes)
            {
                addNodeShape(n, false);
            }
            foreach (var a in graph.arcs)
            {
                AddArcShape(a);
                SetUpNewArcShape(a);
            }
            foreach (var h in graph.hyperarcs)
            {
                AddHyperArcShape(h);
            }
            RedrawResizeAndReposition(true);
            if (storeOnUndo) storeOnUndoStack();
        }
        #region Add New Nodes

        protected virtual void addNewNode(string shapeKey, Point point)
        {
            try
            {
                point = (Point)(point - Origin);
                if (SnapToGrid)
                    point = GoToNearestGridIntersection(point);
                var newNode = InstantiateNewNode();
                newNode.X = point.X;
                newNode.Y = point.Y;
                newNode.DisplayShape = new DisplayShape((string)Application.Current.Resources[shapeKey],
                    ShapeRepresents.Node, newNode);
                addNodeShape(newNode);
                ((DisplayShape)newNode.DisplayShape).StringNeedsUpdating = true;
                mainObject.propertyUpdate();
                storeOnUndoStack();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public void addNodeShape(node n, Boolean redraw = true)
        {
            if (!(n.DisplayShape is DisplayShape))
                GS1xCompatibility.UpdateNodeShape(n);
            var shape = (Shape)n.DisplayShape.Shape;
            nodeShapes.Add(shape);
            shape.RenderTransformOrigin = new Point(0.5, 0.5);
            if (double.IsNaN(n.X)) n.X = 0.0;
            if (double.IsNaN(n.Y)) n.Y = 0.0;
            shape.RenderTransform = new MatrixTransform(
                shape.RenderTransform.Value.M11,
                shape.RenderTransform.Value.M12,
                shape.RenderTransform.Value.M21,
                shape.RenderTransform.Value.M22,
                n.X - (shape.Width / 2) + Origin.X,
                n.Y - (shape.Height / 2) + Origin.Y);
            nodeIcons.Add(shape, n);
            if (redraw) RedrawResizeAndReposition(true);
        }

        /* this way we can use the same beginNewNode method for both rule display 
         * and graph display by just overriding this small method. */
        protected virtual node InstantiateNewNode()
        {
            var newNode = new node(graph.makeUniqueNodeName());
            graph.addNode(newNode);
            return newNode;
        }

        #endregion

        #region Add New Arcs
        public void AddArcShape(arc a)
        {
            if (!(a.DisplayShape is DisplayShape))
                GS1xCompatibility.UpdateArcShape(a);
            arcShapes.Add((ArcShape)a.DisplayShape.Shape);
            arcIcons.Add(a, (ArcShape)a.DisplayShape.Shape);
        }

        public void SetUpNewArcShape(arc a)
        {
            SetUpNewArcShape(a, new Point(double.NaN, double.NaN), new Point(double.NaN, double.NaN));
        }

        private void SetUpNewArcShape(arc a, Point fromPt)
        {
            SetUpNewArcShape(a, fromPt, new Point(double.NaN, double.NaN));
        }

        protected void SetUpNewArcShape(arc a, Point fromPt, Point toPt)
        {
            try
            {
                /* first identify local vars for the arcShape and its icon */
                var arcShape = (ArcShape)a.DisplayShape.Shape;

                /* next, store the nodes that the arc points to. Note that these trump
                 * the current shape values as seen in the following if-then statements. */
                var fromNode = a.From;
                var toNode = a.To;

                var toNodeShape = arcShape.ToShape;
                var fromNodeShape = arcShape.FromShape;

                var winWidth = double.IsNaN(Width) ? MinWidth : Width;
                var winHeight = double.IsNaN(Height) ? MinHeight : Height;

                /* in this first condition, we are connecting the from side of the arc
                 * to a null node. Since null nodes have no real position (they ideally
                 * float on the screen), we first have to figure out a good place for it.*/
                if ((fromNodeShape == null) && (fromNode == null))
                {
                    if (double.IsNaN(fromPt.X))
                        fromPt = (toNode != null)
                                ? new Point((toNode.X + Origin.X) / 2, toNode.Y)
                                : new Point(defaultLength, winHeight / 2);
                    fromNodeShape = nullNodeIcons.Add(fromPt, a, false);
                }
                /* this next condition is when a new node is to be attached to the
             * current from side of the arc. the old node is to deleted if it is null. */
                else if ((fromNode != null) && (fromNodeShape != fromNode.DisplayShape.Shape))
                    fromNodeShape = (Shape)fromNode.DisplayShape.Shape;

                /* these same conditions above are now repeated for the to side of the arc. */
                if ((toNodeShape == null) && (toNode == null))
                {
                    if (double.IsNaN(toPt.X))
                        /* there is a slight difference here for the to side in comparison to 
                           * the from-side. As a convention we put the arcs flowing left to right
                           * so the from sides are closed to the left side of the window and the
                           * to sides to the right - notice the additional this.Width in the calc. */
                        toPt = (fromNode != null)
                            ? new Point((fromNode.X + Origin.X + winWidth) / 2, fromNode.Y)
                            : new Point(winWidth - defaultLength, winHeight / 2);
                    toNodeShape = nullNodeIcons.Add(toPt, a, true);
                }
                else if ((toNode != null) && (toNodeShape != toNode.DisplayShape.Shape))
                    toNodeShape = (Shape)toNode.DisplayShape.Shape;

                arcShape.CreateShapeBindings(fromNodeShape, toNodeShape, a);
                ((DisplayShape)a.DisplayShape).StringNeedsUpdating = true;
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        /// <summary>
        ///   Begins the new arc.
        /// </summary>
        /// <param name = "shapeKey">The shape key.</param>
        /// <param name = "point">The point.</param>
        private void beginNewArc(string shapeKey, Point point)
        {
            try
            {
                var newArc = InstantiateNewArc();
                newArc.directed = true;
                //draggingArcHead = true;
                newArc.DisplayShape = new DisplayShape((string)Application.Current.Resources[shapeKey], ShapeRepresents.Arc,
                       newArc);
                AddArcShape(newArc);
                ((ArcShape)newArc.DisplayShape.Shape).ShowArrowHeads = true;
                ((DisplayShape)newArc.DisplayShape).StringNeedsUpdating = true;
                newArc.From = Selection.SelectedNode ?? getNodeFromPoint(point);

                SetUpNewArcShape(newArc, point, point);
                activeNullNode = (NullNodeIconShape)((ArcShape)newArc.DisplayShape.Shape).ToShape;
                storeOnUndoStack();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        /* this way we can use the same beginNewArc method for both rule display 
 * and graph display by just overriding this small method. */

        protected virtual arc InstantiateNewArc()
        {
            var newArc = new arc(graph.makeUniqueArcName());
            graph.addArc(newArc);
            return newArc;
        }

        /// <summary>
        ///   Completes the new arc.
        /// </summary>
        /// <param name = "point">The point.</param>
        protected virtual void completeNewArc(arc a, node nodeToAttach, Boolean attachHeadTo)
        {
            if (nodeToAttach != null)
            {
                if (attachHeadTo) a.To = nodeToAttach;
                else a.From = nodeToAttach;
                SetUpNewArcShape(a);
                nullNodeIcons.Remove(nullNodeIcons.FirstOrDefault(n => n.GraphElement == a));
                storeOnUndoStack();
            }
            activeNullNode = null;
            mainObject.propertyUpdate();
        }

        #endregion

        #region Add New HyperArcs


        protected virtual void addNewHyperArc(string shapeKey, Point point)
        {
            try
            {
                var nodesToConnect = Selection.selectedNodes;
                if (SnapToGrid)
                    point = GoToNearestGridIntersection(point);
                var newHyperarc = InstantiateNewHyperArc();
                foreach (var n in nodesToConnect)
                    newHyperarc.ConnectTo(n);
                newHyperarc.DisplayShape = new DisplayShape((string)Application.Current.Resources[shapeKey],
                    ShapeRepresents.HyperArc, newHyperarc);
                AddHyperArcShape(newHyperarc, point);
                ((DisplayShape)newHyperarc.DisplayShape).StringNeedsUpdating = true;

                mainObject.propertyUpdate();
                storeOnUndoStack();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public void AddHyperArcShape(hyperarc h, Point? point = null)
        {
            hyperarcShapes.Add((HyperArcShape)h.DisplayShape.Shape);
            if (point.HasValue)
                ((HyperArcShape)h.DisplayShape.Shape).Center = point.Value;
            hyperarcIcons.Add(h, (HyperArcShape)h.DisplayShape.Shape);
            BindHyperArcToNodeShapes(h);
        }

        protected void BindHyperArcToNodeShapes(hyperarc ha)
        {
            BindingOperations.ClearBinding((HyperArcShape)ha.DisplayShape.Shape, HyperArcShape.NodeCentersProperty);

            var multiBinding = new MultiBinding
                {
                    Mode = BindingMode.OneWay,
                    Converter = new HyperArcNodeLocationsConverter()
                };
            foreach (node n in ha.nodes)
            {
                var icon = ((DisplayShape)n.DisplayShape).icon;
                var binding = new Binding
                {
                    Source = icon,
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath(IconShape.CenterProperty)
                };
                multiBinding.Bindings.Add(binding);
            }
            ((HyperArcShape)ha.DisplayShape.Shape).SetBinding(HyperArcShape.NodeCentersProperty, multiBinding);
            if (((HyperArcShape)ha.DisplayShape.Shape).Controller is InferredHyperArcController)
                ((InferredHyperArcController)((HyperArcShape)ha.DisplayShape.Shape).Controller).BindToArcs();
        }


        /* this way we can use the same beginNewArc method for both rule display 
 * and graph display by just overriding this small method. */

        protected virtual hyperarc InstantiateNewHyperArc()
        {
            var newHA = new hyperarc(graph.makeUniqueHyperArcName());
            graph.addHyperArc(newHA);
            return newHA;
        }

        private Boolean startingNewHyperArcConnection()
        {
            var draggingHyperConnect = (HyperArcIconShape)hyperarcIcons.Where(hI =>
                   ((HyperArcIconShape)hI).IsPointContained(MouseLocation)).FirstOrDefault();
            if (draggingHyperConnect == null) return false;
            draggingHyperConnect.newConnection = activeNullNode
               = nullNodeIcons.Add(MouseLocation, draggingHyperConnect.GraphElement, false);
            return true;
        }

        protected virtual void completeNewHyperArcConnection(hyperarc h, node nodeToAttach)
        {
            if (nodeToAttach != null)
            {
                h.ConnectTo(nodeToAttach);
                BindHyperArcToNodeShapes(h);
            }
            nullNodeIcons.Remove(activeNullNode);
            activeNullNode = null;
            ((HyperArcIconShape)((DisplayShape)h.DisplayShape).icon).newConnection = null;
            mainObject.propertyUpdate();
            storeOnUndoStack();
        }
        #endregion
    }
}