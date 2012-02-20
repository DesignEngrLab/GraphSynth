using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Xml.Linq;
using GraphSynth.Representation;

namespace GraphSynth.GraphDisplay
{
    public partial class GraphGUI : InkCanvas
    {
        #region Cut Method

        public virtual void Cut()
        {
            Copy();
            Delete();
        }

        #endregion

        #region Copy Methods

        public virtual void Copy()
        {
            Clipboard.SetText(Selection.SerializeToXml());
        }

        #endregion

        #region Delete Methods

        public virtual void Delete()
        {
            foreach (var a in Selection.selectedArcs)
                RemoveArcFromGraph(a);
            foreach (var n in Selection.selectedNodes)
                RemoveNodeFromGraph(n);
            foreach (var h in Selection.selectedHyperArcs)
                RemoveHyperArcFromGraph(h);
            storeOnUndoStack();
        }

        public virtual void DisconnectArcTail(arc a)
        {
            var toDS = ((ArcShape)a.DisplayShape.Shape).ToShape;
            var fromDS = ((ArcShape)a.DisplayShape.Shape).FromShape;
            var toPt = new Point(toDS.RenderTransform.Value.OffsetX,
                                 toDS.RenderTransform.Value.OffsetY);
            var fromPt = new Point(fromDS.RenderTransform.Value.OffsetX,
                                   fromDS.RenderTransform.Value.OffsetY);

            var retract = toPt - fromPt;
            var length = retract.Length;
            retract.Normalize();

            fromPt += Math.Min(defaultLength, 0.33 * length) * retract;

            a.From = null;
            ((ArcShape)a.DisplayShape.Shape).FromShape = null;

            SetUpNewArcShape(a, fromPt);
            storeOnUndoStack();
        }

        public virtual void DisconnectArcHead(arc a)
        {
            var toDS = ((ArcShape)a.DisplayShape.Shape).ToShape;
            var fromDS = ((ArcShape)a.DisplayShape.Shape).FromShape;
            var toPt = new Point(toDS.RenderTransform.Value.OffsetX,
                                 toDS.RenderTransform.Value.OffsetY);
            var fromPt = new Point(fromDS.RenderTransform.Value.OffsetX,
                                   fromDS.RenderTransform.Value.OffsetY);

            var retract = fromPt - toPt;
            var length = retract.Length;
            retract.Normalize();
            toPt += Math.Min(defaultLength, 0.33 * length) * retract;

            a.To = null;
            ((ArcShape)a.DisplayShape.Shape).ToShape = null;

            SetUpNewArcShape(a, new Point(double.NaN, double.NaN), toPt);
            storeOnUndoStack();
        }


        public virtual void DisconnectHyperArcConnection(hyperarc h, node n)
        {
            h.DisconnectFrom(n);
            BindHyperArcToNodeShapes(h);
            storeOnUndoStack();
        }

        public virtual void FlipArc(arc a)
        {
            var tempNode = a.To;
            a.To = a.From;
            a.From = tempNode;
            SetUpNewArcShape(a);
            storeOnUndoStack();
        }


        protected void RemoveNodeFromGraph(node n)
        {
            for (int i=n.arcsTo.Count-1; i>=0; i--)
                DisconnectArcHead(n.arcsTo[i]);
            for (int i = n.arcsFrom.Count - 1; i >= 0; i--)
                DisconnectArcTail(n.arcsFrom[i]);
            var hyperArcs = n.arcs.Where(h => h is hyperarc).Cast<hyperarc>().ToArray();
            for (int i=hyperArcs.Count()-1; i>=0; i--)
                DisconnectHyperArcConnection(hyperArcs[i], n);
            var nIS = (NodeIconShape)((DisplayShape)n.DisplayShape).icon;
            nodeIcons.Remove(nIS);
            nodeShapes.Remove((Shape)n.DisplayShape.Shape);
            graph.nodes.Remove(n);
        }

        protected void RemoveArcFromGraph(arc a)
        {
            var aDS = (ArcShape)a.DisplayShape.Shape;
            var aIS = ((DisplayShape)a.DisplayShape).icon;
            a.To = null;
            a.From = null;
            if (aDS.FromShape is NullNodeIconShape)
                nullNodeIcons.Remove((IconShape)aDS.FromShape);
            if (aDS.ToShape is NullNodeIconShape)
                nullNodeIcons.Remove((IconShape)aDS.ToShape);
            // this gets rid of the deleted shape from the shape collection
            arcIcons.Remove(aIS);
            arcShapes.Remove(aDS);

            // this finally purges the node corresponding to the shape deleted
            graph.arcs.Remove(a);
        }

        protected void RemoveHyperArcFromGraph(hyperarc a)
        {
            var aDS = (HyperArcShape)a.DisplayShape.Shape;
            var aIS = ((DisplayShape)a.DisplayShape).icon;
            hyperarcIcons.Remove(aIS);
            hyperarcShapes.Remove(aDS);

            // this finally purges the node corresponding to the shape deleted
            graph.removeHyperArc(a);
        }

        #endregion

        #region Paste Methods

        public new virtual void Paste()
        {
            var ClipboardString = Clipboard.GetText();
            var copiedSelection = SelectionClass.DeSerializeClipboardFormatFromXML(ClipboardString);
            RestoreDisplayShapes(copiedSelection.ReadInXmlShapes, copiedSelection.selectedNodes,
                copiedSelection.selectedArcs, copiedSelection.selectedHyperArcs);
            var newSelection = new List<UIElement>();

            var copiedData = new designGraph(copiedSelection.selectedNodes,
                copiedSelection.selectedArcs, copiedSelection.selectedHyperArcs);
            copiedData.internallyConnectGraph();


            foreach (var n in copiedData.nodes)
            {
                n.name = graph.makeUniqueNodeName(n.name);
                n.X = n.X + MouseLocation.X - copiedSelection.ReferencePoint.X - Origin.X;
                n.Y = n.Y + MouseLocation.Y - copiedSelection.ReferencePoint.Y - Origin.Y;
                addNodeShape(n);
                graph.addNode(n);
                newSelection.Add((Shape)n.DisplayShape.Shape);
            }
            foreach (var a in copiedData.arcs)
            {
                a.name = graph.makeUniqueArcName(a.name);
                graph.addArc(a);
                AddArcShape(a);
                SetUpNewArcShape(a);
                newSelection.Add((ArcShape)a.DisplayShape.Shape);
            }
            foreach (var h in copiedData.hyperarcs)
            {
                h.name = graph.makeUniqueHyperArcName(h.name);
                graph.addHyperArc(h); //note that the list of nodes is not sent because it is
                //already connected to those from copiedData
                AddHyperArcShape(h);
                newSelection.Add((HyperArcShape)h.DisplayShape.Shape);
            }
            Select(newSelection);
            storeOnUndoStack();
        }

        #endregion

        /// <summary>
        ///   Restores the display shapes for cut, copy and paste. It started as identical to the
        ///   function with the same name in the GraphSynth.exe.WPFFiler class. However, that class 
        ///   took on the progress update and cross-threading calls that slowed down a simple cut and
        ///   paste. Therefore this has been recreated here. Also, this is  necessary because this now
        ///   exists in a separate DLL to the WPFFiler
        /// </summary>
        /// <param name = "shapes">The shapes.</param>
        /// <param name = "nodes">The nodes.</param>
        /// <param name = "arcs">The arcs.</param>
        /// <param name="hyperarcs"></param>
        protected void RestoreDisplayShapes(XElement shapes, List<node> nodes, List<arc> arcs, List<hyperarc> hyperarcs)
        {
            foreach (node n in nodes)
            {
                XElement x
                = shapes.Elements().FirstOrDefault(p => ((p.Attribute("Tag") != null) &&
                                                           p.Attribute("Tag").Value.StartsWith(n.name)));
                if (x != null)
                {
                    n.DisplayShape = new DisplayShape(x.ToString(), ShapeRepresents.Node, n);
                    x.Remove();
                }
                else
                    n.DisplayShape = new DisplayShape((string)Application.Current.Resources["SmallCircleNode"],
                                ShapeRepresents.Node, n);
            }
            foreach (arc a in arcs)
            {
                XElement x = shapes.Elements().FirstOrDefault(p =>
                    ((p.Attribute("Tag") != null) && p.Attribute("Tag").Value.StartsWith(a.name)));
                if (x != null)
                {
                    a.DisplayShape = new DisplayShape(x.ToString(), ShapeRepresents.Arc, a);
                    x.Remove();
                }
                else
                    a.DisplayShape =
                    new DisplayShape((string)Application.Current.Resources["StraightArc"],
                        ShapeRepresents.Arc, a);
            }
            foreach (hyperarc h in hyperarcs)
            {
                XElement x = shapes.Elements().FirstOrDefault(p =>
                    ((p.Attribute("Tag") != null) && p.Attribute("Tag").Value.StartsWith(h.name)));
                if (x != null)
                {
                    h.DisplayShape = new DisplayShape(x.ToString(), ShapeRepresents.HyperArc, h);
                    x.Remove();
                }
                else h.DisplayShape =
                            new DisplayShape((string)Application.Current.Resources["StarHyper"],
                                ShapeRepresents.HyperArc, h);
            }
        }
    }
}