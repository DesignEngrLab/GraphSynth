using System;
using System.IO;
using System.Linq;
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
                SaveString = SaveString.Insert(SaveString.IndexOf(">", StringComparison.Ordinal),
                                               IgnorableSetup + "Tag=\"Rule\" ");
                /* remove the ending Page tag but put it back at the end. */
                SaveString = SaveString.Replace("</Page>", "");

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
                        SearchIO.output("File Access Exception" + E.Message, 10000, "", "Cancel", false);
                }
                SearchIO.output("**** Rule successfully saved. ****", 2);
            }
        }

        private string BuildXAMLRulePage(grammarRule rule, designGraph graphK)
        {
            return "";
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