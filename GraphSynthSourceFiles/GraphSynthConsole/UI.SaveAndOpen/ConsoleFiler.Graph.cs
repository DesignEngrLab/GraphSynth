using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using GraphSynth.Representation;

namespace GraphSynth
{
    public partial class ConsoleFiler : BasicFiler
    {
        private const string IgnorableSetup =
            " xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\" \n"
            + "mc:Ignorable=\"GraphSynth\" xmlns:GraphSynth=\"ignorableUri\" ";

        #region Save

        public void SaveGraph(string filename, object[] saveObjects)
        {
            if (saveObjects.GetLength(0) == 1)
            {
                if (saveObjects[0] is designGraph)
                    SaveGraph(filename, (designGraph)saveObjects[0]);
            }
            else
                SaveGraph(filename, saveObjects[0] as designGraph);
        }

        private new void SaveGraph(string filename, designGraph graph)
        {
            graph.name = Path.GetFileNameWithoutExtension(filename);

            if (graph.checkForRepeatNames())
                SearchIO.output("Sorry, but you are not allowed to have repeat names. I have changed" +
                                     " these names to be unique");

            /* go build the browser page */
            string SaveString = BuildXAMLGraphPage(graph);
            /* A little manipulation is needed to stick the GraphSynth objects within the page. *
             * The IgnorableSetup string ensures that XAML viewers ignore the following      *
             * GraphSynth specific elements: the canvas data, and the graph data. This eff-  *
             * ectively separates the topology and data of the graph from the graphic elements.       */
            var insertIndex = SaveString.Contains(">") ? SaveString.IndexOf(">", StringComparison.Ordinal) : 0;
            SaveString = SaveString.Insert(insertIndex, IgnorableSetup + "Tag=\"Graph\" ");
            /* remove the ending Page tag but put it back at the end. */
            SaveString = SaveString.Replace("</Page>", "");

            /* add the canvas properties. */
            /* add the graph data. */
            SaveString += "\n\n" + AddIgnorablePrefix(SerializeGraphToXml(graph)) + "\n\n";
            /* put the closing tag back on. */
            SaveString += "</Page>";
            try
            {
                File.WriteAllText(filename, SaveString);
                SearchIO.output("**** Graph successfully saved. ****", 2);
            }
            catch (Exception E)
            {
                SearchIO.output("File Access Exception" + E.Message, 2);
            }
        }

        private string BuildXAMLGraphPage(designGraph graph)
        {
            return "";
        }

        //private Canvas BuildGraphGUICanvas(designGraph graph, CanvasProperty cp)
        //{
        //    var dtc = new DisplayTextConverter();
        //    var ptc = new PositionTextConverter();

        //    var saveCanvas = new Canvas
        //                         {
        //                             Background = cp.BackgroundColor,
        //                             HorizontalAlignment = HorizontalAlignment.Stretch,
        //                             VerticalAlignment = VerticalAlignment.Stretch,
        //                             Height = cp.CanvasHeight / cp.ScaleFactor,
        //                             Width = cp.CanvasWidth.Left / cp.ScaleFactor,
        //                             RenderTransform = new MatrixTransform(1, 0, 0,
        //                                                                   -1, 0, cp.CanvasHeight / cp.ScaleFactor)
        //                         };
        //    if (UserCancelled) return null;
        //    progress += 2;
        //    var canvString = XamlWriter.Save(saveCanvas);
        //    canvString = canvString.EndsWith("</Canvas>")
        //        ? canvString.Replace("</Canvas>", "")
        //        : canvString.Replace("/>", ">");
        //    progress += 2;
        //    var progressStart = progress;
        //    var progStep = (double)(progressEnd - progressStart) / (graph.nodes.Count
        //        + graph.arcs.Count + graph.hyperarcs.Count);
        //    var step = 1;

        //    foreach (node n in graph.nodes)
        //    {
        //        if (n.DisplayShape == null) GS1xCompatibility.UpdateNodeShape(n);
        //        canvString += ((DisplayShape)n.DisplayShape).String;
        //        var nodeText = (FormattedText)dtc.Convert(new object[]
        //                                                        {
        //                                                            cp.ShowNodeName,
        //                                                            cp.ShowNodeLabel, cp.NodeFontSize
        //                                                        }, null, n, null);
        //        if (nodeText != null)
        //        {
        //            var textPos = (Point)ptc.Convert(new object[]
        //                                                   {
        //                                                       cp.NodeTextDistance,
        //                                                       cp.NodeTextPosition, nodeText
        //                                                   }, null, null, null);
        //            var tb = new TextBlock
        //                         {
        //                             FontSize = cp.NodeFontSize,
        //                             Text = nodeText.Text,
        //                             VerticalAlignment = VerticalAlignment.Center,
        //                             HorizontalAlignment = HorizontalAlignment.Center,
        //                             RenderTransform = new MatrixTransform(1, 0, 0, -1,
        //                                                                   n.DisplayShape.ScreenX + textPos.X,
        //                                                                   n.DisplayShape.ScreenY - textPos.Y)
        //                         };
        //            canvString += RemoveXAMLns(XamlWriter.Save(tb));
        //        }
        //        if (UserCancelled) return null;
        //        progress = progressStart + (int)(progStep * step++);
        //    }
        //    foreach (arc a in graph.arcs)
        //    {
        //        if (a.DisplayShape == null) GS1xCompatibility.UpdateArcShape(a);
        //        canvString += ((DisplayShape)a.DisplayShape).String;
        //        var arcText = (FormattedText)dtc.Convert(new object[]
        //                                                      {
        //                                                          cp.ShowArcName,
        //                                                          cp.ShowArcLabel, cp.ArcFontSize
        //                                                      }, null, a, null);
        //        if (arcText != null)
        //        {
        //            AbstractController ac;
        //            if ((a.DisplayShape != null) && (a.DisplayShape.Shape != null)
        //                && (((ArcShape)a.DisplayShape.Shape).Controller != null))
        //                ac = ((ArcShape)a.DisplayShape.Shape).Controller;
        //            else ac = new StraightArcController((Shape)a.DisplayShape.Shape);
        //            var textPos = (Point)ptc.Convert(new object[] { cp.ArcTextDistance, cp.ArcTextPosition, arcText },
        //                                              null, ac, null);
        //            var tb = new TextBlock
        //                         {
        //                             FontSize = cp.ArcFontSize,
        //                             Text = arcText.Text,
        //                             RenderTransform = new MatrixTransform(1, 0, 0, -1, textPos.X, textPos.Y)
        //                         };
        //            canvString += RemoveXAMLns(XamlWriter.Save(tb));
        //        }
        //    }
        //    foreach (hyperarc h in graph.hyperarcs)
        //    {
        //        canvString += ((DisplayShape)h.DisplayShape).String;
        //        var hatext = (FormattedText)dtc.Convert(new object[]
        //                                                       {
        //                                                           cp.ShowHyperArcName,
        //                                                           cp.ShowHyperArcLabel, cp.HyperArcFontSize
        //                                                       }, null, h, null);
        //        if (hatext != null)
        //        {
        //            AbstractController ac;
        //            if ((h.DisplayShape != null) && (h.DisplayShape.Shape != null)
        //                && (((HyperArcShape)h.DisplayShape.Shape).Controller != null))
        //                ac = ((HyperArcShape)h.DisplayShape.Shape).Controller;
        //            else ac = new CircleHyperArcController((Shape)h.DisplayShape.Shape,
        //                new[] { 25.0 - 1.0 });
        //            var textPos = (Point)ptc.Convert(new object[] { cp.HyperArcTextDistance,
        //                cp.HyperArcTextPosition, hatext },
        //                                               null, ac, null);
        //            var tb = new TextBlock
        //                         {
        //                             FontSize = cp.HyperArcFontSize,
        //                             Text = hatext.Text,
        //                             RenderTransform = new MatrixTransform(1, 0, 0, -1, textPos.X, textPos.Y)
        //                         };
        //            canvString += RemoveXAMLns(XamlWriter.Save(tb));
        //        }
        //        if (UserCancelled) return null;
        //        progress = progressStart + (int)(progStep * step++);
        //    }
        //    canvString += "</Canvas>";
        //    return (Canvas)MyXamlHelpers.Parse(canvString);
        //}

        #endregion

        #region Open

        public object[] OpenGraphAndCanvas(string filename)
        {
            XmlReader xR = null;
            try
            {
                xR = XmlReader.Create(filename);
                var XGraphAndCanvas = XElement.Load(xR);
                var shapes =
                    XGraphAndCanvas.Element("{http://schemas.microsoft.com/winfx/2006/xaml/presentation}" + "Border");
                if (shapes == null)
                    /* from Jan 2009 to Sept. 2009, the Shape Properties were stored directly in UICanvas
                     * the next line of code is a check for this in case one wasn't found in the above line .*/
                    shapes =
                        XGraphAndCanvas.Element("{http://schemas.microsoft.com/winfx/2006/xaml/presentation}" + "Canvas");
                else shapes = shapes.Elements().First().Elements().First();

                //("{http://schemas.microsoft.com/winfx/2006/xaml/presentation}" + "Viewbox").
                //Element("{http://schemas.microsoft.com/winfx/2006/xaml/presentation}" + "Canvas");

                var temp = XGraphAndCanvas.Element("{ignorableUri}" + "designGraph");
                var newDesignGraph = new designGraph();
                if (temp != null)
                    newDesignGraph = DeSerializeGraphFromXML(RemoveXAMLns(RemoveIgnorablePrefix(temp.ToString())));

                RestoreDisplayShapes(shapes, newDesignGraph.nodes, newDesignGraph.arcs, newDesignGraph.hyperarcs);


                if ((string.IsNullOrWhiteSpace(newDesignGraph.name)) || (newDesignGraph.name == "Untitled"))
                    newDesignGraph.name = Path.GetFileNameWithoutExtension(filename);

                return new object[] { newDesignGraph, filename };
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
                return null;
            }
            finally
            {
                if (xR != null) xR.Close();
            }
        }

        internal void RestoreDisplayShapes(XElement shapes, List<node> nodes, List<arc> arcs, List<hyperarc> hyperarcs)
        {
            //
            // how to get to the icons
            //
            foreach (node n in nodes)
            {
                XElement x
                = shapes.Elements().FirstOrDefault(p => ((p.Attribute("Tag") != null) &&
                                                           p.Attribute("Tag").Value.StartsWith(n.name)));
                if (x != null)
                {
                    n.DisplayShape = new ShapeData(x.ToString(), n);
                    x.Remove();
                }
                else
                {
                    SearchIO.output("Node: " + n.name + " does not have a shape description in the" +
                                    " file. A default shape is added");
                    n.DisplayShape = new ShapeData(GetShapeReourceString.get("SmallCircleNode"),
                            n) { Tag = n.name };
                }
            }
            foreach (arc a in arcs)
            {
                XElement x = shapes.Elements().FirstOrDefault(p =>
                    ((p.Attribute("Tag") != null) && p.Attribute("Tag").Value.StartsWith(a.name)));
                if (x != null)
                {
                    a.DisplayShape = new ShapeData(x.ToString(), a);
                    x.Remove();
                }
                else
                {
                    SearchIO.output("Arc: " + a.name + " does not have a shape description in the" +
                                    " file. A default shape is added");
                    a.DisplayShape = new ShapeData(GetShapeReourceString.get("StraightArc"), a) { Tag = a.name };
                }
            }

            foreach (hyperarc h in hyperarcs)
            {
                XElement x = shapes.Elements().FirstOrDefault(p =>
                    ((p.Attribute("Tag") != null) && p.Attribute("Tag").Value.StartsWith(h.name)));
                if (x != null)
                {
                    h.DisplayShape = new ShapeData(x.ToString(), h);
                    x.Remove();
                }
                else
                {
                    SearchIO.output("HyperArc: " + h.name + " does not have a shape description in the" +
                                    " file. A default shape is added");
                    h.DisplayShape = new ShapeData(GetShapeReourceString.get("StarHyper"), h) { Tag = h.name };
                }
            }
        }

        #endregion
    }
}