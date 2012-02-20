using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using GraphSynth.Representation;

namespace GraphSynth
{
    public partial class ConsoleFiler : BasicFiler
    {
        #region Save

        public void SaveRule(string filename, object[] saveObjects)
        {
            lock (fileTransfer)
            {
                if (saveObjects.GetLength(0) == 1)
                {
                    if (saveObjects[0] is grammarRule)
                        SaveRule(filename, (grammarRule)saveObjects[0], null);
                }
                else
                    SaveRule(filename, saveObjects[0] as grammarRule, null);
            }
        }


        private void SaveRule(string filename, grammarRule rule, designGraph graphK)
        {
            rule.name = Path.GetFileNameWithoutExtension(filename);

            if (checkRule(rule))
            {
                // progress is now at 13 
                var SaveString = BuildXAMLRulePage(rule, graphK);

                /* A little manipulation is needed to stick the GraphSynth objects within the page. *
                 * The IgnorableSetup string ensures that XAML viewers ignore the following      *
                 * GraphSynth specific elements: the canvas data, and the graph data. This eff-  *
                 * ectively separates the topology and data of the graph from the graphic elements.       */
                SaveString = SaveString.Insert(SaveString.IndexOf(">"),
                                               IgnorableSetup + "Tag=\"Rule\" ");
                /* remove the ending Page tag but put it back at the end. */
                SaveString = SaveString.Replace("</Page>", "");

                ///* add the graph data. */
                SaveString += "\n\n" + AddIgnorablePrefix(SerializeRuleToXml(rule)) + "\n\n";
                /* put the closing tag back on. */
                SaveString += "</Page>";
                try
                {
                    File.WriteAllText(filename, SaveString);
                }
                catch (Exception E)
                {
                        SearchIO.output("File Access Exception" + E.Message, 10000, "", "Cancel", false);
                }
                SearchIO.output("**** Rule successfully saved. ****", 2);
            }
        }

        private string BuildXAMLRulePage(grammarRule rule, designGraph graphK)
        {
            return "";
            //if (canvProp == null) canvProp = new CanvasProperty();
            //var g = new Grid();
            //g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(canvProp.CanvasWidth.Left) });
            //g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
            //g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(canvProp.CanvasWidth.Top) });
            //g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
            //var rWidth = canvProp.CanvasWidth.Bottom - 12 - canvProp.CanvasWidth.Left - canvProp.CanvasWidth.Top;
            //g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(rWidth) });
            //var gs1 = new GridSplitter
            //              {
            //                  Width = 6,
            //                  VerticalAlignment = VerticalAlignment.Stretch,
            //                  HorizontalAlignment = HorizontalAlignment.Stretch,
            //                  ShowsPreview = true
            //              };
            //var gs2 = new GridSplitter
            //              {
            //                  Width = 6,
            //                  VerticalAlignment = VerticalAlignment.Stretch,
            //                  HorizontalAlignment = HorizontalAlignment.Stretch,
            //                  ShowsPreview = true
            //              };
            //g.Children.Add(gs1);
            //Grid.SetColumn(gs1, 1);
            //g.Children.Add(gs2);
            //Grid.SetColumn(gs2, 3);

            //if (UserCancelled) return null;
            //progress = 20;
            //progressEnd = 40;
            //var svL = new ScrollViewer
            //              {
            //                  Tag = "L",
            //                  CanContentScroll = false,
            //                  HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            //                  VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            //                  Content = BuildGraphGUICanvas(rule.L, canvProp)
            //              };
            //g.Children.Add(svL);
            //Grid.SetColumn(svL, 0);

            //if (UserCancelled) return null;
            //progress += 2;
            //progressEnd = 60;
            //var svK = new ScrollViewer
            //              {
            //                  Tag = "K",
            //                  CanContentScroll = false,
            //                  HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            //                  VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            //              };
            //if (graphK != null)
            //    svK.Content = BuildGraphGUICanvas(graphK, canvProp);
            //else
            //    svK.Content = new TextBlock
            //                      {
            //                          Text = "not enough information to display",
            //                          Margin = new Thickness(3),
            //                          TextWrapping = TextWrapping.Wrap,
            //                          HorizontalAlignment = HorizontalAlignment.Stretch,
            //                          VerticalAlignment = VerticalAlignment.Stretch
            //                      };

            //g.Children.Add(svK);
            //Grid.SetColumn(svK, 2);

            //if (UserCancelled) return null;
            //progress += 2;
            //progressEnd = 80;
            //var svR = new ScrollViewer
            //              {
            //                  Tag = "R",
            //                  CanContentScroll = false,
            //                  HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            //                  VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            //                  Content = BuildGraphGUICanvas(rule.R, canvProp)
            //              };
            //g.Children.Add(svR);
            //Grid.SetColumn(svR, 4);

            //return new Page
            //           {
            //               Background = Brushes.Black,
            //               Content = new Border
            //                             {
            //                                 BorderThickness = new Thickness(1),
            //                                 BorderBrush = Brushes.DarkGray,
            //                                 HorizontalAlignment = HorizontalAlignment.Center,
            //                                 VerticalAlignment = VerticalAlignment.Center,
            //                                 Child = g
            //                             }
            //           };
        }

        #endregion

        #region CheckRule

        private Boolean checkRule(grammarRule gR)
        {
            if ((gR.L.checkForRepeatNames()) && 
                SearchIO.output("You are not allowed to have repeat names in L. I have changed these " +
                                      "names to be unique, which may have disrupted your context graph, K. Do you want to continue" +
                                      "saving?", 0, "Yes", "No", false))
                return false;

            if ((gR.R.checkForRepeatNames()) && 
                SearchIO.output("You are not allowed to have repeat names in R. I have changed" +
                                      " these names to be unique, which may have disrupted your context graph, K. Do you" +
                                      " want to continue saving?", 0, "Yes", "No", false))
                return false;


            if ((NumKElements(gR) == 0) && 
                SearchIO.output("No Context Graph: There appears to be no common elements between " +
                                      "the left and right hand sides of the rule. Is this intentional? If so, click yes to continue.",
                                      0, "Yes", "No", false))
                return false;


            if ((KarcsChangeDirection(gR) != "") && 
                SearchIO.output("It appears that arc(s): " + KarcsChangeDirection(gR) +
                                      " change direction (to = from or vice-versa). Even though the arc(s) might be undirected," +
                                      " this can still lead to problems in the rule application, it is recommended that this is" +
                                      " fixed before saving. Save anyway?", 0, "Yes", "No", false))
                return false;


            if ((!ValidateFreeArcEmbeddingRules(gR)) && 
                SearchIO.output("There appears to be invalid references in the free arc embedding rules." +
                                     " Node names used in free arc embedding rules do not exist. Save Anyway?",
                                      0, "Yes", "No", false))
                return false;

            if ((!ValidateFreeArcEmbeddingRules(gR)) && 
                SearchIO.output("There appears to be invalid references in the free arc embedding rules." +
                                      " Node names used in free arc embedding rules do not exist. Save Anyway?",
                                      0, "Yes", "No", false))
                return false;

            gR.ReorderNodes();
            return true;
        }

        private string KarcsChangeDirection(grammarRule gR)
        {
            var badArcNames = "";
            foreach (arc a in gR.L.arcs)
            {
                var aR = a as ruleArc;
                var b = (ruleArc)gR.R.arcs.Find(delegate(arc c) { return (c.name == a.name); });
                if (b != null)
                {
                    if (((a.To != null) && (b.From != null) && (a.To.name == b.From.name)) ||
                        ((a.From != null) && (b.To != null) && (a.From.name == b.To.name)))
                        badArcNames += a.name + ", ";
                }
            }
            return badArcNames;
        }

        private int NumKElements(grammarRule gR)
        {
            return gR.L.nodes.Count(n => gR.R.nodes.Exists(a => a.name == n.name))
             + gR.L.arcs.Count(n => gR.R.arcs.Exists(a => a.name == n.name))
             + gR.L.hyperarcs.Count(n => gR.R.hyperarcs.Exists(a => a.name == n.name));

        }

        private Boolean ValidateFreeArcEmbeddingRules(grammarRule gR)
        {
            if (gR.embeddingRules == null) return true;
            var result = true;
            for (var i = 0; i < gR.embeddingRules.Count; i++)
            {
                var eR = gR.embeddingRules[i];
                if ((string.IsNullOrWhiteSpace(eR.LNodeName)) || (eR.LNodeName.Equals("<any>")))
                    eR.LNodeName = null;
                else if (!gR.L.nodes.Any(nL => nL.name.Equals(eR.LNodeName)))
                {
                    SearchIO.output("Error in the embedding rules #" + i +
                                    ": No L-node named " + eR.LNodeName);
                    result = false;
                }
                if ((string.IsNullOrWhiteSpace(eR.RNodeName)) || (eR.RNodeName.Equals("<any>")))
                    eR.RNodeName = null;
                else if (!gR.R.nodes.Any(nR => nR.name.Equals(eR.RNodeName)))
                {
                    SearchIO.output("Error in the embedding rules #" + i +
                                    ": No R-node named " + eR.RNodeName);
                    result = false;
                }
                if (!result) break;
            }
            return result;
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
         

                var shapes = xeRule.Element("{http://schemas.microsoft.com/winfx/2006/xaml/presentation}" + "Border");
                shapes = shapes.Elements().FirstOrDefault();


                var shapesL = (from s in shapes.Elements()
                               where ((s.Attribute("Tag") != null) && (s.Attribute("Tag").Value == "L"))
                               select s).FirstOrDefault();
                if (shapesL != null) shapesL = shapesL.Elements().First();
                else
                {
                    SearchIO.output("No Left Canvas of Shapes found.");
                    shapesL = new XElement("dummyL");
                }

                var shapesR = (from s in shapes.Elements()
                               where ((s.Attribute("Tag") != null) && (s.Attribute("Tag").Value == "R"))
                               select s).FirstOrDefault();
                if (shapesR != null) shapesR = shapesR.Elements().First();
                else
                {
                    SearchIO.output("No Left Canvas of Shapes found.");
                    shapesR = new XElement("dummyL");
                }


                var temp = xeRule.Element("{ignorableUri}" + "grammarRule");
                var openRule = new grammarRule();
                if (temp != null)
                    openRule = DeSerializeRuleFromXML(RemoveXAMLns(RemoveIgnorablePrefix(temp.ToString())));
            
                RestoreDisplayShapes(shapesL, openRule.L.nodes, openRule.L.arcs, openRule.L.hyperarcs);
                RestoreDisplayShapes(shapesR, openRule.R.nodes, openRule.R.arcs, openRule.R.hyperarcs);

                return new object[] { openRule,  filename };
            }
            catch
            {
                return null;
            }
            finally
            {
                xR.Close();
            }
        }

        #endregion
    }
}