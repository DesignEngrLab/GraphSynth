using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using GraphSynth.Representation;
using GraphSynth.UI;

namespace GraphSynth
{
    public partial class WPFFiler : BasicFiler
    {
        #region Save

        public void SaveRule(string filename, object[] saveObjects)
        {
            lock (fileTransfer)
            {
                if (saveObjects.GetLength(0) == 1)
                {
                    if (saveObjects[0] is grammarRule)
                        SaveRule(filename, (grammarRule)saveObjects[0], null, null);
                    else if (saveObjects[0] is ruleWindow)
                        SaveRule(filename, (ruleWindow)saveObjects[0]);
                }
                else
                    SaveRule(filename, saveObjects[0] as grammarRule, null, saveObjects[1] as CanvasProperty);
            }
        }

        private void SaveRule(string filename, ruleWindow rW)
        {
            dispatch.Invoke((ThreadStart)delegate
                                              {
                                                  rW.canvasProps.WindowLeft = rW.Left;
                                                  rW.canvasProps.WindowTop = rW.Top;
                                              });
            if (string.IsNullOrWhiteSpace(filename) && (!string.IsNullOrWhiteSpace(rW.filename)))
                filename = rW.filename;
            SaveRule(filename, rW.rule, rW.graphCanvasK.graph, rW.canvasProps);
        }

        private void SaveRule(string filename, grammarRule rule, designGraph graphK, CanvasProperty canvProp)
        {
            if (UserCancelled) return;
            progress = 3;
            rule.name = Path.GetFileNameWithoutExtension(filename);

            if (checkRule(rule))
            {
                // progress is now at 13 
                string SaveString = null;
                dispatch.Invoke((ThreadStart)delegate
                                                  {
                                                      var xamlPage = BuildXAMLRulePage(rule, graphK, canvProp);
                                                      SaveString = XamlWriter.Save(xamlPage);
                                                  });
                progress = 85;
                SaveString = MyXamlHelpers.CleanOutxNulls(SaveString);

                /* A little manipulation is needed to stick the GraphSynth objects within the page. *
                 * The IgnorableSetup string ensures that XAML viewers ignore the following      *
                 * GraphSynth specific elements: the canvas data, and the graph data. This eff-  *
                 * ectively separates the topology and data of the graph from the graphic elements.       */
                SaveString = SaveString.Insert(SaveString.IndexOf(">"),
                                               IgnorableSetup + "Tag=\"Rule\" ");
                /* remove the ending Page tag but put it back at the end. */
                SaveString = SaveString.Replace("</Page>", "");

                /* add the canvas properties. */
                if (canvProp != null)
                    dispatch.Invoke(
                        (ThreadStart)
                        delegate { SaveString += "\n\n" + AddIgnorablePrefix(CanvasProperty.SerializeCanvasToXml(canvProp)); });
                progress = 92;
                /* add the graph data. */
                SaveString += "\n\n" + AddIgnorablePrefix(SerializeRuleToXml(rule)) + "\n\n";
                /* put the closing tag back on. */
                SaveString += "</Page>";
                try
                {
                    File.WriteAllText(filename, SaveString);
                }
                catch (Exception E)
                {
                    if (!suppressWarnings)
                        progWindow.QueryUser("File Access Exception" + E.Message, 10000, "", "Cancel", false);
                }
                progress = 100;
                if ((progWindow != null) && (!suppressWarnings))
                    progWindow.QueryUser("\n                     **** Rule successfully saved. ****", 1000, "OK", "",
                                         false);
                else SearchIO.output("**** Rule successfully saved. ****", 2);
            }
        }

        private FrameworkElement BuildXAMLRulePage(grammarRule rule, designGraph graphK, CanvasProperty canvProp)
        {
            if (canvProp == null) canvProp = new CanvasProperty();
            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(canvProp.CanvasWidth.Left) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(canvProp.CanvasWidth.Top) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
            var rWidth = canvProp.CanvasWidth.Bottom - 12 - canvProp.CanvasWidth.Left - canvProp.CanvasWidth.Top;
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(rWidth) });
            var gs1 = new GridSplitter
                          {
                              Width = 6,
                              VerticalAlignment = VerticalAlignment.Stretch,
                              HorizontalAlignment = HorizontalAlignment.Stretch,
                              ShowsPreview = true
                          };
            var gs2 = new GridSplitter
                          {
                              Width = 6,
                              VerticalAlignment = VerticalAlignment.Stretch,
                              HorizontalAlignment = HorizontalAlignment.Stretch,
                              ShowsPreview = true
                          };
            g.Children.Add(gs1);
            Grid.SetColumn(gs1, 1);
            g.Children.Add(gs2);
            Grid.SetColumn(gs2, 3);

            if (UserCancelled) return null;
            progress = 20;
            progressEnd = 40;
            var svL = new ScrollViewer
                          {
                              Tag = "L",
                              CanContentScroll = false,
                              HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                              VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                              Content = BuildGraphGUICanvas(rule.L, canvProp)
                          };
            g.Children.Add(svL);
            Grid.SetColumn(svL, 0);

            if (UserCancelled) return null;
            progress += 2;
            progressEnd = 60;
            var svK = new ScrollViewer
                          {
                              Tag = "K",
                              CanContentScroll = false,
                              HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                              VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                          };
            if (graphK != null)
                svK.Content = BuildGraphGUICanvas(graphK, canvProp);
            else
                svK.Content = new TextBlock
                                  {
                                      Text = "not enough information to display",
                                      Margin = new Thickness(3),
                                      TextWrapping = TextWrapping.Wrap,
                                      HorizontalAlignment = HorizontalAlignment.Stretch,
                                      VerticalAlignment = VerticalAlignment.Stretch
                                  };

            g.Children.Add(svK);
            Grid.SetColumn(svK, 2);

            if (UserCancelled) return null;
            progress += 2;
            progressEnd = 80;
            var svR = new ScrollViewer
                          {
                              Tag = "R",
                              CanContentScroll = false,
                              HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                              VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                              Content = BuildGraphGUICanvas(rule.R, canvProp)
                          };
            g.Children.Add(svR);
            Grid.SetColumn(svR, 4);

            return new Page
                       {
                           Background = Brushes.Black,
                           Content = new Border
                                         {
                                             BorderThickness = new Thickness(1),
                                             BorderBrush = Brushes.DarkGray,
                                             HorizontalAlignment = HorizontalAlignment.Center,
                                             VerticalAlignment = VerticalAlignment.Center,
                                             Child = g
                                         }
                       };
        }

        #endregion
        #region CheckRule
         
        private new Boolean checkRule(grammarRule gR)
        {
            if (progWindow == null) return true;

            gR.ReorderNodes();

            if ((gR.L.checkForRepeatNames()) && !suppressWarnings &&
                !progWindow.QueryUser("You are not allowed to have repeat names in L. I have changed these " +
                                      "names to be unique, which may have disrupted your context graph, K. Do you want to continue" +
                                      "saving?", 0, "Yes", "No", false))
                return false;

            if (UserCancelled) return false;
            progress += 2;

            if ((gR.R.checkForRepeatNames()) && !suppressWarnings &&
                !progWindow.QueryUser("You are not allowed to have repeat names in R. I have changed" +
                                      " these names to be unique, which may have disrupted your context graph, K. Do you" +
                                      " want to continue saving?", 0, "Yes", "No", false))
                return false;

            if (UserCancelled) return false;
            progress += 2;

            if ((NumKElements(gR) == 0) && !suppressWarnings &&
                !progWindow.QueryUser("No Context Graph: There appears to be no common elements between " +
                                      "the left and right hand sides of the rule. Is this intentional? If so, click yes to continue.",
                                      0, "Yes", "No", false))
                return false;

            if (UserCancelled) return false;
            progress += 2;

            if ((KarcsChangeDirection(gR) != "") && !suppressWarnings &&
                !progWindow.QueryUser("It appears that arc(s): " + KarcsChangeDirection(gR) +
                                      " change direction (to = from or vice-versa). Even though the arc(s) might be undirected," +
                                      " this can still lead to problems in the rule application, it is recommended that this is" +
                                      " fixed before saving. Save anyway?", 0, "Yes", "No", false))
                return false;

            if (UserCancelled) return false;
            progress += 2;

            if ((!ValidateFreeArcEmbeddingRules(gR)) && !suppressWarnings &&
                !progWindow.QueryUser("There appears to be invalid references in the free arc embedding rules." +
                                      " Node names used in free arc embedding rules do not exist. Save Anyway?",
                                      0, "Yes", "No", false))
                return false;

            if (UserCancelled) return false;
            progress += 2;

            if ((!ValidateFreeArcEmbeddingRules(gR)) && !suppressWarnings &&
                !progWindow.QueryUser("There appears to be invalid references in the free arc embedding rules." +
                                      " Node names used in free arc embedding rules do not exist. Save Anyway?",
                                      0, "Yes", "No", false))
                return false;

            if (UserCancelled) return false;
            progress += 2;
            return true;
        }
        #endregion
        
        #region Open

        public object[] OpenRuleAndCanvas(string filename)
        {
            XmlReader xR = null;
            try
            {
                xR = XmlReader.Create(filename);
                var xeRule = XElement.Load(xR);
                if (UserCancelled) return null;
                progress = 5;

                var shapes = xeRule.Element("{http://schemas.microsoft.com/winfx/2006/xaml/presentation}" + "Border");
                shapes = shapes.Elements().FirstOrDefault();
                if (UserCancelled) return null;
                progress = 7;

                var shapesL = (from s in shapes.Elements()
                               where ((s.Attribute("Tag") != null) && (s.Attribute("Tag").Value == "L"))
                               select s).FirstOrDefault();
                if (shapesL != null) shapesL = shapesL.Elements().First();
                else
                {
                    SearchIO.output("No Left Canvas of Shapes found.");
                    shapesL = new XElement("dummyL");
                }
                if (UserCancelled) return null;
                progress = 9;

                var shapesR = (from s in shapes.Elements()
                               where ((s.Attribute("Tag") != null) && (s.Attribute("Tag").Value == "R"))
                               select s).FirstOrDefault();
                if (shapesR != null) shapesR = shapesR.Elements().First();
                else
                {
                    SearchIO.output("No Left Canvas of Shapes found.");
                    shapesR = new XElement("dummyL");
                }
                if (UserCancelled) return null;
                progress = 11;

                CanvasProperty canvas = null;
                dispatch.Invoke((ThreadStart)delegate { canvas = LoadCanvasProperty(xeRule); });
                if (UserCancelled) return null;
                progress = 16;

                var temp = xeRule.Element("{ignorableUri}" + "grammarRule");
                var openRule = new grammarRule();
                if (temp != null)
                    openRule = DeSerializeRuleFromXML(RemoveXAMLns(RemoveIgnorablePrefix(temp.ToString())));
                if (UserCancelled) return null;
                progress = 40;
                progressEnd = 70;
                RestoreDisplayShapes(shapesL, openRule.L.nodes, openRule.L.arcs, openRule.L.hyperarcs);
                progressEnd = 100;
                RestoreDisplayShapes(shapesR, openRule.R.nodes, openRule.R.arcs, openRule.R.hyperarcs);

                removeNullWhiteSpaceEmptyLabels(openRule.L);
                removeNullWhiteSpaceEmptyLabels(openRule.R);
                xR.Close();
                return new object[] { openRule, canvas, filename };
            }
            catch
            {
                xR.Close();
                return OpenRuleAndCanvasesOLD(filename);
            }
        }

        #endregion
    }
}