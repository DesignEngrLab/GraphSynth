using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using GraphSynth.GraphDisplay;
using GraphSynth.Representation;

namespace GraphSynth
{
    public partial class WPFFiler : BasicFiler
    {
        #region Open

        private object[] OpenRuleAndCanvasesOLD(string filename)
        {
            grammarRule openRule = null;
            var strRuleProperties = "";
            var xmlRule = new XmlDocument();
            UICanvas Lcanvas = null;
            UICanvas Rcanvas = null;
            xmlRule.Load(filename);
            try
            {
                var xmlLGraph = new XmlDocument();
                xmlLGraph.AppendChild(xmlLGraph.ImportNode(xmlRule.DocumentElement, false));
                //XmlDocument xmlKGraph = new XmlDocument();
                //xmlKGraph.AppendChild(xmlKGraph.ImportNode(xmlRule.DocumentElement, false));
                var xmlRGraph = new XmlDocument();
                xmlRGraph.AppendChild(xmlRGraph.ImportNode(xmlRule.DocumentElement, false));
                XmlNode tempNode;
                var baseGrid = xmlRule.DocumentElement.ChildNodes[0];
                var i = 0;
                while (baseGrid.ChildNodes.Count != 0)
                {
                    if ((baseGrid.ChildNodes[i].Name == IgnorablePrefix + "Canvas" &&
                         baseGrid.ChildNodes[i].Attributes[IgnorablePrefix + "Graph"].Value == "L")
                        ||
                        (baseGrid.ChildNodes[i].Name == IgnorablePrefix + "designGraph" &&
                         baseGrid.ChildNodes[i].Attributes[IgnorablePrefix + "Graph"].Value == "L")
                        ||
                        (baseGrid.ChildNodes[i].Name == "Canvas" &&
                         baseGrid.ChildNodes[i].Attributes[IgnorablePrefix + "Graph"].Value == "L"))
                    {
                        baseGrid.ChildNodes[i].Attributes.Remove(
                            baseGrid.ChildNodes[i].Attributes[IgnorablePrefix + "Graph"]);
                        tempNode = baseGrid.RemoveChild(baseGrid.ChildNodes[i]);
                        xmlLGraph.DocumentElement.AppendChild(xmlLGraph.ImportNode(tempNode, true));
                    }

                    else if ((baseGrid.ChildNodes[i].Name == IgnorablePrefix + "Canvas" &&
                              baseGrid.ChildNodes[i].Attributes[IgnorablePrefix + "Graph"].Value == "R")
                             ||
                             (baseGrid.ChildNodes[i].Name == IgnorablePrefix + "designGraph" &&
                              baseGrid.ChildNodes[i].Attributes[IgnorablePrefix + "Graph"].Value == "R")
                             ||
                             (baseGrid.ChildNodes[i].Name == "Canvas" &&
                              baseGrid.ChildNodes[i].Attributes[IgnorablePrefix + "Graph"].Value == "R"))
                    {
                        baseGrid.ChildNodes[i].Attributes.Remove(
                            baseGrid.ChildNodes[i].Attributes[IgnorablePrefix + "Graph"]);
                        tempNode = baseGrid.RemoveChild(baseGrid.ChildNodes[i]);
                        xmlRGraph.DocumentElement.AppendChild(xmlRGraph.ImportNode(tempNode, true));
                    }
                    else if (baseGrid.ChildNodes[i].Name == IgnorablePrefix + "RuleProperties")
                    {
                        tempNode = baseGrid.RemoveChild(baseGrid.ChildNodes[i]);
                        strRuleProperties = tempNode.OuterXml;
                    }
                    else if (baseGrid.ChildNodes[i].Name == "Grid.ColumnDefinitions" ||
                             baseGrid.ChildNodes[i].Name == "GridSplitter")
                    {
                        baseGrid.RemoveChild(baseGrid.ChildNodes[i]);
                        // not needed. Used to show rule properly in browser
                    }
                    else
                        i++;
                }

                if (strRuleProperties.IndexOf("xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"") !=
                    -1)
                    strRuleProperties =
                        strRuleProperties.Replace(
                            "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"", "");
                // get rid of the ignorable prefix
                if (strRuleProperties.IndexOf(IgnorablePrefix) != -1)
                    strRuleProperties = strRuleProperties.Remove(strRuleProperties.IndexOf(IgnorablePrefix),
                                                                 IgnorablePrefix.Length);
                if (strRuleProperties.IndexOf("xmlns:" + IgnorablePrefix + "=\"ignorableUri\"") != -1)
                    strRuleProperties =
                        strRuleProperties.Remove(
                            strRuleProperties.IndexOf("xmlns:" + IgnorablePrefix + "=\"ignorableUri\""),
                            "xmlns:".Length + IgnorablePrefix.Length + "=\"ignorableUri\"".Length);
                if (strRuleProperties.LastIndexOf(IgnorablePrefix) != -1)
                    strRuleProperties = strRuleProperties.Remove(strRuleProperties.LastIndexOf(IgnorablePrefix),
                                                                 IgnorablePrefix.Length);

                var rP = RuleProperties.DeSerializeFromXML(strRuleProperties);
                openRule = new grammarRule();
                CopyRulePropertiesToRule(rP, openRule);
                var graphAndCanvas = OpenGraphAndCanvasOLD(xmlLGraph);
                openRule.L = (designGraph)graphAndCanvas[0];
                Lcanvas = (UICanvas)graphAndCanvas[1];
                graphAndCanvas = OpenGraphAndCanvasOLD(xmlRGraph);
                openRule.R = (designGraph)graphAndCanvas[0];
                Rcanvas = (UICanvas)graphAndCanvas[1];
                if ((string.IsNullOrWhiteSpace(openRule.name)) || (openRule.name == "Untitled"))
                    openRule.name = Path.GetFileNameWithoutExtension(filename);
            }
            catch
            {
                openRule = OpenRule(filename);
            }
            if (openRule != null)
            {
                if (!suppressWarnings)
                    progWindow.QueryUser("Rule open in old format. Please re-save soon.", 1500, "OK",
                                         "", false);
                return new object[] { openRule, Lcanvas, Rcanvas };
            }
            else
            {
                if (!suppressWarnings)
                    progWindow.QueryUser("Failed to open rule.", 5000, "OK",
                                         "", false);
                return null;
            }
        }

        private void CopyRulePropertiesToRule(RuleProperties rP, grammarRule gR)
        {
            gR.name = rP.name;
            gR.spanning = rP.spanning;
            gR.induced = rP.induced;
            if (rP.negateLabels != null && rP.negateLabels.Count > 0)
            {
                gR.negateLabels = new List<string>();
                foreach (string s in rP.negateLabels)
                    gR.negateLabels.Add(s);
            }

            gR.containsAllGlobalLabels = rP.containsAllGlobalLabels;
            gR.OrderedGlobalLabels = rP.OrderedGlobalLabels;

            foreach (string s in rP.recognizeFunctions)
                gR.recognizeFunctions.Add(s);

            foreach (string s in rP.applyFunctions)
                gR.applyFunctions.Add(s);


            if (rP.embeddingRules != null && rP.embeddingRules.Count > 0)
            {
                gR.embeddingRules = new List<embeddingRule>();
                foreach (embeddingRule e in rP.embeddingRules)
                    gR.embeddingRules.Add(e);
            }

            gR.UseShapeRestrictions = rP.UseShapeRestrictions;
            gR.Translate = rP.Translate;
            gR.Skew = rP.Skew;
            gR.Scale = rP.Scale;
            gR.Flip = rP.Flip;
            gR.Projection = rP.Projection;
            gR.Rotate = rP.Rotate;
            gR.TransformNodeShapes = rP.TransformNodeShapes;
        }

        private object[] OpenGraphAndCanvasOLD(XmlDocument xmlGraphDisplay)
        {
            string strDesignGraph = null, strCanvasProperties = null;
            XmlElement xmlShapes = null;
            var newDesignGraph = new designGraph();
            var canvas = new UICanvas();


            for (var i = 0; i < xmlGraphDisplay.DocumentElement.ChildNodes.Count; i++)
            {
                if (xmlGraphDisplay.DocumentElement.ChildNodes[i].Name == IgnorablePrefix + "Canvas")
                    strCanvasProperties = xmlGraphDisplay.DocumentElement.ChildNodes[i].OuterXml;
                else if (xmlGraphDisplay.DocumentElement.ChildNodes[i].Name == IgnorablePrefix + "designGraph")
                    strDesignGraph = xmlGraphDisplay.DocumentElement.ChildNodes[i].OuterXml;
                else if (xmlGraphDisplay.DocumentElement.ChildNodes[i].Name == "Canvas")
                    xmlShapes = (XmlElement)xmlGraphDisplay.DocumentElement.ChildNodes[i];
            }

            //get rid of all the xaml related namespace stuff  // how to do this without hardcoding?
            strCanvasProperties =
                strCanvasProperties.Replace("xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"", "");
            // get rid of the ignorable prefix
            strCanvasProperties = strCanvasProperties.Remove(strCanvasProperties.IndexOf(IgnorablePrefix),
                                                             IgnorablePrefix.Length);
            // get rid of the schema info added by the .net classes for the ignorable prefix
            strCanvasProperties = strCanvasProperties.Replace("xmlns:GraphSynth=\"ignorableUri\"", "");

            //get rid of all the xaml related namespace stuff // how to do this without hardcoding?
            /// -- k spent n lot of time to know that it had to be removed for successful deserialization oofff!
            strDesignGraph =
                strDesignGraph.Replace("xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"", "");
            // get rid of the ignorable prefix
            strDesignGraph = strDesignGraph.Remove(strDesignGraph.IndexOf(IgnorablePrefix), IgnorablePrefix.Length);
            strDesignGraph = strDesignGraph.Remove(strDesignGraph.LastIndexOf(IgnorablePrefix), IgnorablePrefix.Length);
            // get rid of the schema info added by the .net classes for the ignorable prefix
            strDesignGraph = strDesignGraph.Replace("xmlns:GraphSynth=\"ignorableUri\"", "");
            //strDesignGraph = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" + strDesignGraph;

            newDesignGraph = DeSerializeGraphFromXML(strDesignGraph);
            canvas = UICanvas.DeSerializeFromXML(strCanvasProperties);
            RestoreDisplayShapesOLD(xmlShapes, newDesignGraph.nodes, newDesignGraph.arcs);
            return new object[] { newDesignGraph, canvas };
        }

        public void RestoreDisplayShapesOLD(XmlElement xmlShapes, List<node> nodes, List<arc> arcs)
        {
            foreach (node n in nodes)
            {
                foreach (XmlNode x in xmlShapes.ChildNodes)
                {
                    var s = x.OuterXml;
                    if (n.name.Equals(MyXamlHelpers.GetValue(s, "Tag")))
                    {
                        dispatch.Invoke((ThreadStart)delegate
                        { n.DisplayShape = new DisplayShape(s, ShapeRepresents.Node, n); });
                        break;
                    }
                }
            }
            //if (arcs != null)
            //{
            foreach (arc a in arcs)
            {
                foreach (XmlNode x in xmlShapes.ChildNodes)
                {
                    var s = x.OuterXml;
                    if (a.name.Equals(MyXamlHelpers.GetValue(s, "Tag")))
                    {
                        dispatch.Invoke((ThreadStart)delegate
                        { a.DisplayShape = new DisplayShape(s, ShapeRepresents.Arc, a); });
                        break;
                    }
                }
            }
        }

        #endregion
    }
}