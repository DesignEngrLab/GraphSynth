/*************************************************************************
 *     This BasicFiler file & interface is part of the GraphSynth.BaseClasses 
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
#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using GraphSynth.Representation;
#endregion

namespace GraphSynth
{
    /// <summary>
    ///   This method saves and opens basic graphs and rules (doesn't include WPF shapes)
    ///   as well as rulesets, which are the same as in earlier versions of GraphSynth.
    /// </summary>
    public class BasicFiler
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref = "BasicFiler" /> class.
        /// </summary>
        /// <param name = "iDir">The input directory.</param>
        /// <param name = "oDir">The output directory.</param>
        /// <param name = "rDir">The rules directory.</param>
        public BasicFiler(string iDir, string oDir, string rDir)
        {
            inputDirectory = iDir;
            outputDirectory = oDir;
            rulesDirectory = rDir;
        }

        #region Directory Properties

        /// <summary>
        ///   This constant is used to tell other XML parsers (namely XAML displayers)
        ///   to ignore elements that are prefaced with this.
        /// </summary>
        protected const string IgnorablePrefix = "GraphSynth:";

        /// <summary>
        ///   Gets or sets the input directory.
        /// </summary>
        /// <value>The input directory.</value>
        public string inputDirectory { get; set; }

        /// <summary>
        ///   Gets or sets the output directory.
        /// </summary>
        /// <value>The output directory.</value>
        public string outputDirectory { get; set; }

        /// <summary>
        ///   Gets or sets the rules directory.
        /// </summary>
        /// <value>The rules directory.</value>
        public string rulesDirectory { get; set; }

        #endregion


        /// <summary>
        ///   Saves the object, o, to the specified filename.
        /// </summary>
        /// <param name = "filename">The filename.</param>
        /// <param name = "o">The object to save.</param>
        /// <param name = "SuppressWarnings">if set to <c>true</c> [suppress warnings].</param>
        public virtual void Save(string filename, object o, Boolean SuppressWarnings = false)
        {
            if (o is designGraph)
                SaveGraph(filename, (designGraph)o);
            else if (o is grammarRule)
                SaveRule(filename, (grammarRule)o);
            else if (o is ruleSet)
                SaveRuleSet(filename, (ruleSet)o);
            else if (o is candidate)
                SaveCandidate(filename, (candidate)o);
            else if (o is List<candidate>)
                SaveCandidates(filename, (List<candidate>)o);
            else if (!SuppressWarnings)
                throw new Exception("Basic Filer (in GraphSynth.Representation) " +
                                    "received a different type than expected. Save was expecting " +
                                    "an object of type: designGraph, grammarRule, ruleSet, or candidate.");
        }


        /// <summary>
        ///   Opens the list of objects at the specified filename.
        /// </summary>
        /// <param name = "filename">The filename.</param>
        /// <param name = "SuppressWarnings">if set to <c>true</c> [suppresses warnings].</param>
        /// <returns>an array of opened objects</returns>
        public virtual object[] Open(string filename, Boolean SuppressWarnings = false)
        {
            /* Load the file. */
            var doc = new XmlDocument();
            doc.Load(filename);
            /* create prefix<->namespace mappings (if any)  */
            var nsMgr = new XmlNamespaceManager(doc.NameTable);
            /* Query the document */
            if ((doc.SelectNodes("/designGraph", nsMgr).Count > 0)  
                || (doc.DocumentElement.Attributes["Tag"] != null    
                && doc.DocumentElement.Attributes["Tag"].Value == "Graph"))
                return new object[] { OpenGraph(filename) };
            if ((doc.SelectNodes("/grammarRule", nsMgr).Count > 0)
                || (doc.DocumentElement.Attributes["Tag"] != null
                && doc.DocumentElement.Attributes["Tag"].Value == "Rule"))
                return new object[] { OpenRule(filename) };
            if (doc.SelectNodes("/ruleSet", nsMgr).Count > 0)
                return new object[] { OpenRuleSet(filename) };
            if (doc.SelectNodes("/candidate", nsMgr).Count > 0)
                return new object[] { OpenCandidate(filename) };

            if (!SuppressWarnings)
                throw new Exception("Basic Filer (in GraphSynth.Representation) " +
                                    "opened a different type than expected. Open was expecting " +
                                    "an object of type: designGraph, grammarRule, ruleSet, or candidate.");
            return new object[0];
        }

        #region Save & Open designGraph

        /// <summary>
        ///   Saves the graph.
        /// </summary>
        /// <param name = "filename">The filename.</param>
        /// <param name = "graph1">The graph1.</param>
        protected void SaveGraph(string filename, designGraph graph1)
        {
            StreamWriter graphWriter = null;
            graph1.name = Path.GetFileNameWithoutExtension(filename);
            graph1.checkForRepeatNames();
            removeNullWhiteSpaceEmptyLabels(graph1);
            try
            {
                graphWriter = new StreamWriter(filename);
                var s = SerializeGraphToXml(graph1);
                if (s != null) graphWriter.Write(s);
            }
            catch (FileNotFoundException fnfe)
            {
                SearchIO.output("***Error Writing to File***");
                SearchIO.output(fnfe.ToString());
            }
            finally
            {
                if (graphWriter != null) graphWriter.Close();
            }
        }

        /// <summary>
        ///   Serializes the graph to XML.
        /// </summary>
        /// <param name = "graph1">The graph1.</param>
        /// <returns></returns>
        protected string SerializeGraphToXml(designGraph graph1)
        {
            try
            {
                var settings = new XmlWriterSettings
                                   {
                                       Indent = true,
                                       NewLineOnAttributes = true,
                                       CloseOutput = true,
                                       OmitXmlDeclaration = true
                                   };
                var saveString = new StringBuilder();
                var saveXML = XmlWriter.Create(saveString, settings);
                var graphSerializer = new XmlSerializer(typeof(designGraph));
                graphSerializer.Serialize(saveXML, graph1);
                return (saveString.ToString());
            }
            catch (Exception ioe)
            {
                SearchIO.output("***XML Serialization Error***");
                SearchIO.output(ioe.ToString());
                return null;
            }
        }

        /// <summary>
        ///   Opens the graph.
        /// </summary>
        /// <param name = "filename">The filename.</param>
        /// <returns></returns>
        protected designGraph OpenGraph(string filename)
        {
            designGraph newDesignGraph = null;
            XElement xmlGraph = null;
            try
            {
                xmlGraph = XElement.Load(filename);
            }
            catch (FileLoadException fle)
            {
                SearchIO.output("File was not found or accessible: " + fle);
                filename = "";
            }
            if (!string.IsNullOrWhiteSpace(filename))
            {
                if (!xmlGraph.Name.LocalName.Contains("designGraph"))
                    xmlGraph = (from xe in xmlGraph.Elements()
                                where xe.Name.LocalName.Contains("designGraph")
                                select xe).FirstOrDefault();
                if (xmlGraph != null)
                {
                    newDesignGraph = DeSerializeGraphFromXML(xmlGraph.ToString());

                    if ((string.IsNullOrWhiteSpace(newDesignGraph.name)) || (newDesignGraph.name == "Untitled"))
                        newDesignGraph.name = Path.GetFileNameWithoutExtension(filename);
                    SearchIO.output(Path.GetFileName(filename) + " successfully loaded.");
                    RestoreDisplayShapes(newDesignGraph);
                }
                else
                    SearchIO.output(Path.GetFileName(filename) + " does not contain design graph data.");
            }
            return newDesignGraph;
        }

        /// <summary>
        ///   Deserialize graph from XML.
        /// </summary>
        /// <param name = "xmlString">The XML string.</param>
        /// <returns></returns>
        protected designGraph DeSerializeGraphFromXML(string xmlString)
        {
            try
            {
                var stringReader = new StringReader(xmlString);
                var graphDeserializer = new XmlSerializer(typeof(designGraph));
                var newDesignGraph = (designGraph)graphDeserializer.Deserialize(stringReader);
                newDesignGraph.RepairGraphConnections();
                removeNullWhiteSpaceEmptyLabels(newDesignGraph);
                return newDesignGraph;
            }
            catch (Exception ioe)
            {
                SearchIO.output("***Error Opening Graph:*** ");
                SearchIO.output(ioe.ToString());
                return null;
            }
        }

        /// <summary>
        /// Removes the null white space empty labels.
        /// </summary>
        /// <param name="g">The g.</param>
        protected static void removeNullWhiteSpaceEmptyLabels(designGraph g)
        {
            g.globalLabels.RemoveAll(string.IsNullOrWhiteSpace);
            foreach (var a in g.arcs)
                a.localLabels.RemoveAll(string.IsNullOrWhiteSpace);
            foreach (var a in g.nodes)
                a.localLabels.RemoveAll(string.IsNullOrWhiteSpace);
            foreach (var a in g.hyperarcs)
                a.localLabels.RemoveAll(string.IsNullOrWhiteSpace);
        }

        /// <summary>
        ///   Restores the display shapes.
        /// </summary>
        /// <param name = "graph">The graph.</param>
        private static void RestoreDisplayShapes(designGraph graph)
        {
            var oldX = 0.0;
            var oldY = 0.0;
            var oldZ = 0.0;
            var minY = double.PositiveInfinity;
            var shapeKey = "";

            #region Draw Nodes

            foreach (var n in graph.nodes)
            {
                if (n.extraData != null)
                {
                    for (var i = n.extraData.GetLength(0) - 1; i >= 0; i--)
                    {
                        var unkXmlElt = n.extraData[i];
                        if ((unkXmlElt.Name == "screenX") && (unkXmlElt.Value.Length > 0))
                            oldX = double.Parse(unkXmlElt.Value);
                        else if ((unkXmlElt.Name == "screenY") && (unkXmlElt.Value.Length > 0))
                            oldY = -double.Parse(unkXmlElt.Value);
                        if ((unkXmlElt.Name == "x") && (unkXmlElt.Value.Length > 0))
                            oldX = double.Parse(unkXmlElt.Value);
                        else if ((unkXmlElt.Name == "y") && (unkXmlElt.Value.Length > 0))
                            oldY = double.Parse(unkXmlElt.Value);
                        else if ((unkXmlElt.Name == "z") && (unkXmlElt.Value.Length > 0))
                            oldZ = double.Parse(unkXmlElt.Value);
                        else if ((unkXmlElt.Name == "shapekey") && (unkXmlElt.Value.Length > 0))
                            shapeKey = unkXmlElt.Value;
                        n.extraData[i] = null;
                    }
                }
                if ((n.X == 0.0f) && (n.Y == 0.0f) && (n.Z == 0.0f))
                {
                    n.X = oldX;
                    n.Y = oldY;
                    n.Z = oldZ;
                }
                if (n.Y < minY) minY = n.Y;
                n.DisplayShape = new ShapeData(shapeKey, n);
            }
            /* the whole point of minY is to translate the figure up so that the coordinates are
             * all non-negative. In the preceding parsing of xmlElements, you'll note that screenY
             * is parsed to a negative number. But since we are now using a proper right hand 
             * coordinate frame we need to move all of these to new positions. Hence, we now move 
             * all the y-coords up by the greatest negative number found. */
            if (minY < 0)
                foreach (var n in graph.nodes)
                    n.Y -= minY;

            #endregion

            shapeKey = "";

            #region Draw Arcs

            foreach (var a in graph.arcs)
            {
                if (a.extraData != null)
                {
                    for (var i = a.extraData.GetLength(0) - 1; i >= 0; i--)
                    {
                        var unkXmlElt = a.extraData[i];
                        if ((unkXmlElt.Name == "styleKey") && (unkXmlElt.Value.Length > 0))
                        {
                            shapeKey = unkXmlElt.Value;
                            a.extraData[i] = null;
                        }
                    }
                }
                a.DisplayShape = new ShapeData(shapeKey, a);
            }

            #endregion
            #region Draw Hyperarcs

            foreach (var h in graph.hyperarcs)
            {
                if (h.extraData != null)
                {
                    for (var i = h.extraData.GetLength(0) - 1; i >= 0; i--)
                    {
                        var unkXmlElt = h.extraData[i];
                        if ((unkXmlElt.Name == "styleKey") && (unkXmlElt.Value.Length > 0))
                        {
                            shapeKey = unkXmlElt.Value;
                            h.extraData[i] = null;
                        }
                    }
                }
                h.DisplayShape = new ShapeData(shapeKey, h);
            }

            #endregion
        }

        #endregion

        #region Save & Open grammarRule

        /// <summary>
        ///   Saves the rule.
        /// </summary>
        /// <param name = "filename">The filename.</param>
        /// <param name = "ruleToSave">The rule to save.</param>
        protected void SaveRule(string filename, grammarRule ruleToSave)
        {
            StreamWriter ruleWriter = null;
            try
            {
                ruleToSave.name = Path.GetFileNameWithoutExtension(filename);
                ruleToSave.L.checkForRepeatNames();
                removeNullWhiteSpaceEmptyLabels(ruleToSave.L);
                ruleToSave.R.checkForRepeatNames();
                removeNullWhiteSpaceEmptyLabels(ruleToSave.R);
                ruleToSave.ReorderNodes();
                ruleWriter = new StreamWriter(filename);
                var s = SerializeRuleToXml(ruleToSave);
                if (s != null) ruleWriter.Write(s);
            }
            catch (Exception ioe)
            {
                SearchIO.output("***XML Serialization Error***");
                SearchIO.output(ioe.ToString());
            }
            finally
            {
                if (ruleWriter != null) ruleWriter.Close();
            }
        }

        /// <summary>
        ///   Serializes the rule to XML.
        /// </summary>
        /// <param name = "ruleToSave">The rule to save.</param>
        /// <returns></returns>
        protected string SerializeRuleToXml(grammarRule ruleToSave)
        {
            try
            {
                var settings = new XmlWriterSettings
                                   {
                                       Indent = true,
                                       NewLineOnAttributes = true,
                                       CloseOutput = true,
                                       OmitXmlDeclaration = true
                                   };
                var saveString = new StringBuilder();
                var saveXML = XmlWriter.Create(saveString, settings);
                var ruleSerializer = new XmlSerializer(typeof(grammarRule));
                ruleSerializer.Serialize(saveXML, ruleToSave);
                return (saveString.ToString());
            }
            catch (Exception ioe)
            {
                SearchIO.output("***XML Serialization Error***");
                SearchIO.output(ioe.ToString());
                return null;
            }
        }

        /// <summary>
        ///   Opens the rule.
        /// </summary>
        /// <param name = "filename">The filename.</param>
        /// <returns></returns>
        public grammarRule OpenRule(string filename)
        {
            grammarRule newGrammarRule = null;
            XElement xmlRule = null;
            try
            {
                xmlRule = XElement.Load(filename);
            }
            catch (FileLoadException fle)
            {
                SearchIO.output("File was not found or accessible: " + fle);
                filename = "";
            }
            if (!string.IsNullOrWhiteSpace(filename))
            {
                if (!xmlRule.Name.LocalName.Contains("grammarRule"))
                    xmlRule = (from xe in xmlRule.Elements()
                               where xe.Name.LocalName.Contains("grammarRule")
                               select xe).FirstOrDefault();
                if (xmlRule != null)
                {
                    try
                    {
                        newGrammarRule = DeSerializeRuleFromXML(xmlRule.ToString());
                        RestoreDisplayShapes(newGrammarRule.L);
                        RestoreDisplayShapes(newGrammarRule.R);
                        removeNullWhiteSpaceEmptyLabels(newGrammarRule.L);
                        removeNullWhiteSpaceEmptyLabels(newGrammarRule.R);

                        if ((string.IsNullOrWhiteSpace(newGrammarRule.name)) || (newGrammarRule.name == "Untitled"))
                            newGrammarRule.name = Path.GetFileNameWithoutExtension(filename);
                    }
                    catch (Exception ioe)
                    {
                        SearchIO.output("***XML Serialization Error***");
                        SearchIO.output(ioe.ToString());
                    }
                }
            }
            return newGrammarRule;
        }

        /// <summary>
        ///   Deserialize rule from XML.
        /// </summary>
        /// <param name = "xmlString">The XML string.</param>
        /// <returns></returns>
        protected grammarRule DeSerializeRuleFromXML(string xmlString)
        {
            try
            {
                xmlString = xmlString.Replace("<Rotate>true</Rotate>", "<Rotate>OnlyZ</Rotate>");
                xmlString = xmlString.Replace("<Rotate>false</Rotate>", "<Rotate>Prohibited</Rotate>");
                xmlString = xmlString.Replace("<Rotate >true</Rotate>", "<Rotate>OnlyZ</Rotate>");
                xmlString = xmlString.Replace("<Rotate >false</Rotate>", "<Rotate>Prohibited</Rotate>");
                var stringReader = new StringReader(xmlString);
                var ruleDeserializer = new XmlSerializer(typeof(grammarRule));
                var newGrammarRule = (grammarRule)ruleDeserializer.Deserialize(stringReader);
                if (newGrammarRule.L == null) newGrammarRule.L = new designGraph();
                else newGrammarRule.L.RepairGraphConnections();

                if (newGrammarRule.R == null) newGrammarRule.R = new designGraph();
                else newGrammarRule.R.RepairGraphConnections();

                foreach (var er in newGrammarRule.embeddingRules.Where(er => er.oldLabels != null))
                {
                    foreach (var unkXmlElt in er.oldLabels)
                    {
                        /* this doesn't seem like the best place for this, but the double foreach
                           * loop is intended to help load old grammar rules that have the simpler
                           * version of embedding rules. */
                        if ((unkXmlElt.Name == "freeArcLabel") && (unkXmlElt.Value.Length > 0))
                            er.freeArcLabels.Add(unkXmlElt.Value);
                        if ((unkXmlElt.Name == "neighborNodeLabel") && (unkXmlElt.Value.Length > 0))
                            er.neighborNodeLabels.Add(unkXmlElt.Value);
                    }
                    er.oldLabels = null;
                }
                return newGrammarRule;
            }
            catch (Exception ioe)
            {
                SearchIO.output("***Error Opening Graph:*** ");
                SearchIO.output(ioe.ToString());
                return null;
            }
        }

        #region CheckRule

        /// <summary>
        /// Checks the rule with some issues that may have been overlooked.
        /// </summary>
        /// <param name="gR">The grammar rule.</param>
        /// <returns></returns>
        public static Boolean checkRule(grammarRule gR)
        {
            if ((gR.L.checkForRepeatNames()) &&
                !SearchIO.MessageBoxShow("You are not allowed to have repeat names in L. I have changed these " +
                                      "names to be unique, which may have disrupted your context graph, K. Do you want to continue?",
                                     "Repeat names in L", "Information", "YesNo", "Yes"))
                return false;

            if ((gR.R.checkForRepeatNames()) &&
                !SearchIO.MessageBoxShow("You are not allowed to have repeat names in R. I have changed" +
                                      " these names to be unique, which may have disrupted your context graph, K. Do you" +
                                      " want to continue?", "Repeat names in R", "Information", "YesNo", "Yes"))
                return false;

            if ((NotExistElementsinKR(gR)) &&
                !SearchIO.MessageBoxShow("There appears to be common elements between "
                + "the left and right hand sides of the rule that are indicated as \"Must NOT Exist\""
                + " within the left-hand side. This is not allowed. Continue Anyway?", "Improper use of negative elements",
                "Error", "YesNo", "No"))
                return false;

            if ((NumKElements(gR) == 0) &&
                !SearchIO.MessageBoxShow("There appears to be no common elements between " +
                                      "the left and right hand sides of the rule. Is this intentional? If so, click yes to continue.",
                                     "No Context Graph", "Information", "YesNo", "Yes"))
                return false;

            if ((KarcsChangeDirection(gR) != "") &&
                !SearchIO.MessageBoxShow("It appears that arc(s): " + KarcsChangeDirection(gR) +
                                      " change direction (to = from or vice-versa). Even though the arc(s) might be undirected," +
                                      " this can still lead to problems in the rule application, it is recommended that this is" +
                                      " fixed before saving. Save anyway?", "Change in Arc Direction", "Information", "YesNo", "Yes"))
                return false;

            if ((!ValidateFreeArcEmbeddingRules(gR)) &&
                !SearchIO.MessageBoxShow("There appears to be invalid references in the free arc embedding rules." +
                                      " Node names used in free arc embedding rules do not exist. Continue Anyway?",
                                    "Invalid Free-Arc References", "Error", "YesNo", "No"))
                return false;

            gR.ReorderNodes();
            return true;
        }

        /// <summary>
        /// Checks to see that the negative elements are not stored in K and R.
        /// </summary>
        /// <param name="gR">The grammar rule.</param>
        /// <returns></returns>
        protected static Boolean NotExistElementsinKR(grammarRule gR)
        {
            return (gR.L.nodes.Any(a => ((ruleNode)a).NotExist && gR.R.nodes.Exists(b => b.name.Equals(a.name)))
                || gR.L.arcs.Any(a => ((ruleArc)a).NotExist && gR.R.arcs.Exists(b => b.name.Equals(a.name)))
                || gR.L.hyperarcs.Any(a => ((ruleHyperarc)a).NotExist && gR.R.hyperarcs.Exists(b => b.name.Equals(a.name))));
        }

        /// <summary>
        /// Checks that the K arcs do not change direction.
        /// </summary>
        /// <param name="gR">The grammar rule.</param>
        /// <returns></returns>
        protected static string KarcsChangeDirection(grammarRule gR)
        {
            var badArcNames = "";
            foreach (var a in gR.L.arcs)
            {
                var b = (ruleArc)gR.R.arcs.FirstOrDefault(c => (c.name.Equals(a.name)));
                if (b != null)
                {
                    if (((a.To != null) && (b.From != null) && (a.To.name == b.From.name)) ||
                        ((a.From != null) && (b.To != null) && (a.From.name == b.To.name)))
                        badArcNames += a.name + ", ";
                }
            }
            return badArcNames;
        }

        /// <summary>
        /// Checks that the number of K elements is greater than 0.
        /// </summary>
        /// <param name="gR">The grammar rule.</param>
        /// <returns></returns>
        protected static int NumKElements(grammarRule gR)
        {
            return gR.L.nodes.Count(n => gR.R.nodes.Exists(a => a.name == n.name))
             + gR.L.arcs.Count(n => gR.R.arcs.Exists(a => a.name == n.name))
             + gR.L.hyperarcs.Count(n => gR.R.hyperarcs.Exists(a => a.name == n.name));

        }

        /// <summary>
        /// Validates the free arc embedding rules.
        /// </summary>
        /// <param name="gR">The grammar rule.</param>
        /// <returns></returns>
        protected static Boolean ValidateFreeArcEmbeddingRules(grammarRule gR)
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
        #endregion

        #region Save & Open ruleSet

        /// <summary>
        ///   Saves the rule set.
        /// </summary>
        /// <param name = "filename">The filename.</param>
        /// <param name = "ruleSetToSave">The rule set to save.</param>
        protected void SaveRuleSet(string filename, ruleSet ruleSetToSave)
        {
            StreamWriter ruleWriter = null;
            try
            {
                ruleSetToSave.name = Path.GetFileNameWithoutExtension(filename);
                ruleWriter = new StreamWriter(filename);
                var ruleSerializer = new XmlSerializer(typeof(ruleSet));
                ruleSerializer.Serialize(ruleWriter, ruleSetToSave);
            }
            catch (Exception ioe)
            {
                SearchIO.output("***XML Serialization Error***");
                SearchIO.output(ioe.ToString());
            }
            finally
            {
                if (ruleWriter != null) ruleWriter.Close();
            }
        }

        /// <summary>
        ///   Opens the rule set.
        /// </summary>
        /// <param name = "filename">The filename.</param>
        /// <returns></returns>
        public virtual ruleSet OpenRuleSet(string filename)
        {
            ruleSet newRuleSet = null;
            StreamReader ruleReader = null;
            try
            {
                ruleReader = new StreamReader(filename);
                var ruleDeserializer = new XmlSerializer(typeof(ruleSet));
                newRuleSet = (ruleSet)ruleDeserializer.Deserialize(ruleReader);
                newRuleSet.rulesDir = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar;
                newRuleSet.filer = this;
                var numRules = newRuleSet.ruleFileNames.Count;
                int numLoaded;
                newRuleSet.rules = LoadRulesFromFileNames(newRuleSet.rulesDir,
                                                          newRuleSet.ruleFileNames, out numLoaded);

                SearchIO.output(Path.GetFileName(filename) + " successfully loaded");
                if (numRules == numLoaded) SearchIO.output(" and all (" + numLoaded + ") rules loaded successfully.");
                else
                    SearchIO.output("     but "
                                    + (numRules - numLoaded) + " rules did not load.");

                if ((string.IsNullOrWhiteSpace(newRuleSet.name)) || (newRuleSet.name == "Untitled"))
                    newRuleSet.name = Path.GetFileNameWithoutExtension(filename);
            }
            catch (Exception ioe)
            {
                SearchIO.output("***XML Serialization Error***");
                SearchIO.output(ioe.ToString());
            }
            finally
            {
                if (ruleReader != null) ruleReader.Close();
            }

            return newRuleSet;
        }

        /// <summary>
        ///   Loads the rules from file names.
        /// </summary>
        /// <param name = "ruleDir">The rule dir.</param>
        /// <param name = "ruleFileNames">The rule file names.</param>
        /// <param name = "numLoaded">The num loaded.</param>
        /// <returns></returns>
        protected virtual List<grammarRule> LoadRulesFromFileNames(string ruleDir, List<string> ruleFileNames,
                                                                   out int numLoaded)
        {
            var rules = new List<grammarRule>();
            numLoaded = 0;
            while (numLoaded < ruleFileNames.Count)
            {
                var rulePath = ruleDir + ruleFileNames[numLoaded];
                if (File.Exists(rulePath))
                {
                    SearchIO.output("Loading " + ruleFileNames[numLoaded]);
                    object ruleObj = Open(rulePath);
                    if (ruleObj is grammarRule)
                        rules.Add((grammarRule)ruleObj);
                    else if (ruleObj is object[])
                        rules.AddRange(
                            ((object[])ruleObj).Where(o => o is grammarRule).Cast
                                <grammarRule>());
                    numLoaded++;
                }
                else
                {
                    SearchIO.output("Rule Not Found: " + ruleFileNames[numLoaded]);
                    ruleFileNames.RemoveAt(numLoaded);
                }
            }
            return rules;
        }

        /// <summary>
        ///   Reloads the specific rule.
        /// </summary>
        /// <param name = "rs">The rs.</param>
        /// <param name = "i">The i.</param>
        public virtual void ReloadSpecificRule(ruleSet rs, int i)
        {
            var rulePath = rs.rulesDir + rs.ruleFileNames[i];
            SearchIO.output("Loading " + rs.ruleFileNames[i]);
            object ruleObj = Open(rulePath);
            if (ruleObj is grammarRule)
                rs.rules[i] = (grammarRule)ruleObj;
            else if (ruleObj is object[] &&
                     ((object[])ruleObj)[0] is grammarRule)
                rs.rules[i] = ((grammarRule)((object[])ruleObj)[0]);
        }

        #endregion

        #region Save & Open candidate

        /// <summary>
        /// Saves the candidate.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="candidates">The candidates.</param>
        /// <param name="SaveToOutputDir">if set to <c>true</c> [save to output dir].</param>
        /// <param name="timeStamp">if set to <c>true</c> [time stamp].</param>
        public void SaveCandidates(string filename, IList candidates, Boolean SaveToOutputDir = true, Boolean timeStamp = false)
        {

            var outputDir = outputDirectory;
            if (!SaveToOutputDir) outputDir = Path.GetFullPath(filename);
            filename = Path.GetFileNameWithoutExtension(filename);
            for (var i = 0; i != candidates.Count; i++)
            {
                var counter = i.ToString(CultureInfo.InvariantCulture);
                counter = counter.PadLeft(3, '0');
                var tod = "";
                if (timeStamp)
                    tod = "." + DateTime.Now.Year + "." + DateTime.Now.Month + "." +
                          DateTime.Now.Day + "." + DateTime.Now.Hour + "." +
                          DateTime.Now.Minute + "." + DateTime.Now.Second + ".";
                SaveCandidate(outputDir + filename + counter + tod + ".xml", candidates[i] as candidate);
            }
        }

        /// <summary>
        ///   Saves the candidate.
        /// </summary>
        /// <param name = "filename">The filename.</param>
        /// <param name = "c1">The c1.</param>
        protected void SaveCandidate(string filename, candidate c1)
        {
            // c1.graph.checkForRepeatNames();
            StreamWriter candidateWriter = null;
            try
            {
                c1.graphFileName = Path.GetFileNameWithoutExtension(filename) + ".gxml";
                candidateWriter = new StreamWriter(filename);
                var candidateSerializer = new XmlSerializer(typeof(candidate));
                candidateSerializer.Serialize(candidateWriter, c1);
                Save(Path.GetDirectoryName(filename) + "/" + c1.graphFileName, c1.graph);
            }
            catch (Exception ioe)
            {
                SearchIO.output("***XML Serialization Error***");
                SearchIO.output(ioe.ToString());
            }
            finally
            {
                if (candidateWriter != null) candidateWriter.Close();
            }
        }

        /// <summary>
        ///   Opens the candidate.
        /// </summary>
        /// <param name = "filename">The filename.</param>
        /// <returns></returns>
        public candidate OpenCandidate(string filename)
        {
            candidate newCandidate = null;
            StreamReader candidateReader = null;
            try
            {
                candidateReader = new StreamReader(filename);
                var candidateDeserializer = new XmlSerializer(typeof(candidate));
                newCandidate = (candidate)candidateDeserializer.Deserialize(candidateReader);
                newCandidate.graph = (designGraph)Open(Path.GetDirectoryName(filename)
                                                        + "/" + newCandidate.graphFileName)[0];
            }
            catch (Exception ioe)
            {
                SearchIO.output("***XML Serialization Error***");
                SearchIO.output(ioe.ToString());
            }
            finally
            {
                if (candidateReader != null) candidateReader.Close();
            }
            return newCandidate;
        }

        #endregion
    }
}