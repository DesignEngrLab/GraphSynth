using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using GraphSynth.GraphDisplay;
using GraphSynth.Representation;
using GraphSynth.UI;
using Path = System.IO.Path;

namespace GraphSynth
{
    public partial class WPFFiler : BasicFiler
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
                    SaveGraph(filename, (designGraph)saveObjects[0], null);
                else if (saveObjects[0] is graphWindow)
                    SaveGraph(filename, (graphWindow)saveObjects[0]);
            }
            else
                SaveGraph(filename, saveObjects[0] as designGraph, saveObjects[1] as CanvasProperty);
        }

        private void SaveGraph(string filename, graphWindow gW)
        {
            var canvProp = gW.canvasProps;
            dispatch.Invoke((ThreadStart)delegate
                                              {
                                                  canvProp.WindowLeft = gW.Left;
                                                  canvProp.WindowTop = gW.Top;
                                              });
            if (string.IsNullOrEmpty(filename) && (!string.IsNullOrEmpty(gW.filename)))
                filename = gW.filename;
            SaveGraph(filename, gW.graph, gW.canvasProps);
        }

        private void SaveGraph(string filename, designGraph graph, CanvasProperty canvProp)
        {
            if (UserCancelled) return;
            progress = 5;
            graph.name = Path.GetFileNameWithoutExtension(filename);
            removeNullWhiteSpaceEmptyLabels(graph);

            if ((graph.checkForRepeatNames()) && !suppressWarnings)
                progWindow.QueryUser("Sorry, but you are not allowed to have repeat names. I have changed" +
                                     " these names to be unique", 5000, "OK", "", false);

            progress = 7;
            if (UserCancelled) return;
            progressEnd = 80;
            /* go build the browser page */
            string SaveString = null;
            dispatch.Invoke((ThreadStart)delegate
                                              {
                                                  var xamlPage = BuildXAMLGraphPage(graph, canvProp);
                                                  progress = 80;
                                                  SaveString = XamlWriter.Save(xamlPage);
                                              });
            progress = 87;
            if (UserCancelled) return;
            SaveString = MyXamlHelpers.CleanOutxNulls(SaveString);
            /* A little manipulation is needed to stick the GraphSynth objects within the page. *
             * The IgnorableSetup string ensures that XAML viewers ignore the following      *
             * GraphSynth specific elements: the canvas data, and the graph data. This eff-  *
             * ectively separates the topology and data of the graph from the graphic elements.       */
            SaveString = SaveString.Insert(SaveString.IndexOf(">"),
                                           IgnorableSetup + "Tag=\"Graph\" ");
            /* remove the ending Page tag but put it back at the end. */
            SaveString = SaveString.Replace("</Page>", "");

            /* add the canvas properties. */
            if (canvProp != null)
                dispatch.Invoke(
                    (ThreadStart)
                    delegate { SaveString += "\n\n" + AddIgnorablePrefix(CanvasProperty.SerializeCanvasToXml(canvProp)); });
            progress = 95;
            if (UserCancelled) return;
            /* add the graph data. */
            SaveString += "\n\n" + AddIgnorablePrefix(SerializeGraphToXml(graph)) + "\n\n";
            if (UserCancelled) return;
            /* put the closing tag back on. */
            SaveString += "</Page>";
            try
            {
                File.WriteAllText(filename, SaveString);
                progress = 100;
                if ((progWindow != null) && !suppressWarnings)
                    progWindow.QueryUser("\n                     **** Graph successfully saved. ****", 1000, "OK", "",
                                         false);
                else SearchIO.output("**** Graph successfully saved. ****", 2);
            }
            catch (Exception E)
            {
                if ((progWindow != null) && !suppressWarnings)
                    progWindow.QueryUser("File Access Exception" + E.Message, 10000, "", "Cancel", false);
                else SearchIO.output("File Access Exception" + E.Message, 2);
            }
        }

        private FrameworkElement BuildXAMLGraphPage(designGraph graph, CanvasProperty cp)
        {
            if (cp == null) cp = new CanvasProperty();
            return new Page
                       {
                           Background = Brushes.Black,
                           Content = new Border
                                         {
                                             BorderThickness = new Thickness(1),
                                             BorderBrush = Brushes.DarkGray,
                                             HorizontalAlignment = HorizontalAlignment.Center,
                                             VerticalAlignment = VerticalAlignment.Center,
                                             Child = new Viewbox
                                                         {
                                                             HorizontalAlignment = HorizontalAlignment.Stretch,
                                                             VerticalAlignment = VerticalAlignment.Stretch,
                                                             StretchDirection = StretchDirection.Both,
                                                             Child = BuildGraphGUICanvas(graph, cp)
                                                         }
                                         }
                       };
        }

        private Canvas BuildGraphGUICanvas(designGraph graph, CanvasProperty cp)
        {
            var dtc = new DisplayTextConverter();
            var ptc = new PositionTextConverter();

            var saveCanvas = new Canvas
                                 {
                                     Background = cp.BackgroundColor,
                                     HorizontalAlignment = HorizontalAlignment.Stretch,
                                     VerticalAlignment = VerticalAlignment.Stretch,
                                     Height = cp.CanvasHeight / cp.ScaleFactor,
                                     Width = cp.CanvasWidth.Left / cp.ScaleFactor,
                                     RenderTransform = new MatrixTransform(1, 0, 0,
                                                                           -1, 0, cp.CanvasHeight / cp.ScaleFactor)
                                 };
            if (UserCancelled) return null;
            progress += 2;
            var canvString = XamlWriter.Save(saveCanvas);
            canvString = canvString.EndsWith("</Canvas>")
                ? canvString.Replace("</Canvas>", "")
                : canvString.Replace("/>", ">");
            progress += 2;
            var progressStart = progress;
            var progStep = (double)(progressEnd - progressStart) / (graph.nodes.Count
                + graph.arcs.Count + graph.hyperarcs.Count);
            var step = 1;

            foreach (node n in graph.nodes)
            {
                if (n.DisplayShape == null) GS1xCompatibility.UpdateNodeShape(n);
                canvString += ((DisplayShape)n.DisplayShape).String;
                var nodeText = (FormattedText)dtc.Convert(new object[]
                                                                {
                                                                    cp.ShowNodeName,
                                                                    cp.ShowNodeLabel, cp.NodeFontSize
                                                                }, null, n, null);
                if (nodeText != null)
                {
                    var textPos = (Point)ptc.Convert(new object[]
                                                           {
                                                               cp.NodeTextDistance,
                                                               cp.NodeTextPosition, nodeText
                                                           }, null, null, null);
                    var tb = new TextBlock
                                 {
                                     FontSize = cp.NodeFontSize,
                                     Text = nodeText.Text,
                                     VerticalAlignment = VerticalAlignment.Center,
                                     HorizontalAlignment = HorizontalAlignment.Center,
                                     RenderTransform = new MatrixTransform(1, 0, 0, -1,
                                                                           n.DisplayShape.ScreenX + textPos.X,
                                                                           n.DisplayShape.ScreenY - textPos.Y)
                                 };
                    canvString += RemoveXAMLns(XamlWriter.Save(tb));
                }
                if (UserCancelled) return null;
                progress = progressStart + (int)(progStep * step++);
            }
            foreach (arc a in graph.arcs)
            {
                if (a.DisplayShape == null) GS1xCompatibility.UpdateArcShape(a);
                canvString += ((DisplayShape)a.DisplayShape).String;
                var arcText = (FormattedText)dtc.Convert(new object[]
                                                              {
                                                                  cp.ShowArcName,
                                                                  cp.ShowArcLabel, cp.ArcFontSize
                                                              }, null, a, null);
                if (arcText != null)
                {
                    AbstractController ac;
                    if ((a.DisplayShape != null) && (a.DisplayShape.Shape != null)
                        && (((ArcShape)a.DisplayShape.Shape).Controller != null))
                        ac = ((ArcShape)a.DisplayShape.Shape).Controller;
                    else ac = new StraightArcController((Shape)a.DisplayShape.Shape);
                    var textPos = (Point)ptc.Convert(new object[] { cp.ArcTextDistance, cp.ArcTextPosition, arcText },
                                                      null, ac, null);
                    var tb = new TextBlock
                                 {
                                     FontSize = cp.ArcFontSize,
                                     Text = arcText.Text,
                                     RenderTransform = new MatrixTransform(1, 0, 0, -1, textPos.X, textPos.Y)
                                 };
                    canvString += RemoveXAMLns(XamlWriter.Save(tb));
                }
            }
            foreach (hyperarc h in graph.hyperarcs)
            {
                canvString += ((DisplayShape)h.DisplayShape).String;
                var hatext = (FormattedText)dtc.Convert(new object[]
                                                               {
                                                                   cp.ShowHyperArcName,
                                                                   cp.ShowHyperArcLabel, cp.HyperArcFontSize
                                                               }, null, h, null);
                if (hatext != null)
                {
                    AbstractController ac;
                    if ((h.DisplayShape != null) && (h.DisplayShape.Shape != null)
                        && (((HyperArcShape)h.DisplayShape.Shape).Controller != null))
                        ac = ((HyperArcShape)h.DisplayShape.Shape).Controller;
                    else ac = new CircleHyperArcController((Shape)h.DisplayShape.Shape,
                        new[] { 25.0 - 1.0 });
                    var textPos = (Point)ptc.Convert(new object[] { cp.HyperArcTextDistance,
                        cp.HyperArcTextPosition, hatext },
                                                       null, ac, null);
                    var tb = new TextBlock
                                 {
                                     FontSize = cp.HyperArcFontSize,
                                     Text = hatext.Text,
                                     RenderTransform = new MatrixTransform(1, 0, 0, -1, textPos.X, textPos.Y)
                                 };
                    canvString += RemoveXAMLns(XamlWriter.Save(tb));
                }
                if (UserCancelled) return null;
                progress = progressStart + (int)(progStep * step++);
            }
            canvString += "</Canvas>";
            return (Canvas)MyXamlHelpers.Parse(canvString);
        }

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
                if (UserCancelled) return null;
                progress = 5;
                //("{http://schemas.microsoft.com/winfx/2006/xaml/presentation}" + "Viewbox").
                //Element("{http://schemas.microsoft.com/winfx/2006/xaml/presentation}" + "Canvas");
                CanvasProperty canvas = null;
                dispatch.Invoke((ThreadStart)delegate { canvas = LoadCanvasProperty(XGraphAndCanvas); });
                if (UserCancelled) return null;
                progress = 20;

                var temp = XGraphAndCanvas.Element("{ignorableUri}" + "designGraph");
                var newDesignGraph = new designGraph();
                if (temp != null)
                    newDesignGraph = DeSerializeGraphFromXML(RemoveXAMLns(RemoveIgnorablePrefix(temp.ToString())));
                if (UserCancelled) return null;
                progress = 60;
                RestoreDisplayShapes(shapes, newDesignGraph.nodes, newDesignGraph.arcs, newDesignGraph.hyperarcs);
                if (UserCancelled) return null;
                progress = 95;

                if ((string.IsNullOrWhiteSpace(newDesignGraph.name)) || (newDesignGraph.name == "Untitled"))
                    newDesignGraph.name = Path.GetFileNameWithoutExtension(filename);

                if (UserCancelled) return null;
                progress = 100;
                return new object[] { newDesignGraph, canvas, filename };
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
                return null;
            }
            finally
            {
                xR.Close();
            }
        }

        public CanvasProperty LoadCanvasProperty(XElement xfile)
        {
            XElement temp = null;
            if (xfile.Name.LocalName.Equals("CanvasProperty")) temp = xfile;
            if (temp == null) temp = xfile.Element("{ignorableUri}" + "CanvasProperty");
            if (temp == null) temp = xfile.Element("CanvasProperty");
            /* from Jan 2009 to Sept. 2009, the Canvas Properties were stored as GraphSynth:Canvas
             * the next line of code is a check for this in case one wasn't found in the above line .*/
            if (temp == null) temp = xfile.Element("{ignorableUri}" + "Canvas");
            var canvas = new CanvasProperty();
            if (temp != null)
                canvas = CanvasProperty.DeSerializeFromXML(RemoveIgnorablePrefix(temp.ToString()));

            return canvas;
        }

        internal void RestoreDisplayShapes(XElement shapes, List<node> nodes, List<arc> arcs, List<hyperarc> hyperarcs)
        {
            //
            // how to get to the icons
            //
            var progressStart = progress;
            var progStep = (double)(progressEnd - progressStart) / (nodes.Count + arcs.Count + hyperarcs.Count);
            var step = 0;
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
                {
                    SearchIO.output("Node: " + n.name + " does not have a shape description in the" +
                                    " file. A default shape is added");
                    dispatch.Invoke((ThreadStart)delegate
                    {
                        n.DisplayShape = new DisplayShape((string)Application.Current.Resources["SmallCircleNode"],
                            ShapeRepresents.Node, n);
                        n.DisplayShape.Tag = n.name;
                    });
                }
                if (UserCancelled) return;
                progress = progressStart + (int)(progStep * step++);
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
                {
                    SearchIO.output("Arc: " + a.name + " does not have a shape description in the" +
                                    " file. A default shape is added");
                    dispatch.Invoke((ThreadStart)delegate
                    {
                        a.DisplayShape =
                        new DisplayShape((string)Application.Current.Resources["StraightArc"],
                            ShapeRepresents.Arc, a);
                    });
                }
                if (UserCancelled) return;
                progress = progressStart + (int)(progStep * step++);
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
                else
                {
                    SearchIO.output("HyperArc: " + h.name + " does not have a shape description in the" +
                                    " file. A default shape is added");
                    dispatch.Invoke((ThreadStart)delegate
                    {
                        h.DisplayShape =
                            new DisplayShape((string)Application.Current.Resources["StarHyper"],
                                ShapeRepresents.HyperArc, h);
                    });
                }
                if (UserCancelled) return;
                progress = progressStart + (int)(progStep * step++);
            }
        }

        #endregion
    }
}