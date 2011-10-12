using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using GraphSynth.Representation;

namespace GraphSynth
{
    public class RuleProperties
    {
        public transfromType Flip;
        [XmlAttribute]
        public bool OrderedGlobalLabels;
        public transfromType Projection;
        public Boolean Rotate;

        public transfromType Scale;

        public transfromType Skew;

        public Boolean TransformNodeShapes;
        public transfromType Translate;
        public bool UseShapeRestrictions;
        public List<string> applyFunctions;
        [XmlAttribute]
        public bool containsAllGlobalLabels;
        public List<embeddingRule> embeddingRules;
        [XmlAttribute]
        public bool induced;

        [XmlAttribute] //[System.Xml.Serialization.XmlElementAttribute("AxesColor",DataType = "SolidColorBrush")]  
        public string name;

        public List<string> negateLabels;
        public List<string> recognizeFunctions;
        [XmlAttribute]
        public bool spanning;

        public RuleProperties()
        {
        }

        public RuleProperties(grammarRule gR)
        {
            name = gR.name;
            spanning = gR.spanning;
            induced = gR.induced;

            if (gR.negateLabels.Count > 0)
            {
                negateLabels = new List<string>();
                foreach (string s in gR.negateLabels)
                    negateLabels.Add(s);
            }

            containsAllGlobalLabels = gR.containsAllGlobalLabels;
            OrderedGlobalLabels = gR.OrderedGlobalLabels;

            if (gR.recognizeFunctions.Count > 0)
            {
                recognizeFunctions = new List<string>();
                foreach (string s in gR.recognizeFunctions)
                    recognizeFunctions.Add(s);
            }

            if (gR.applyFunctions.Count > 0)
            {
                applyFunctions = new List<string>();
                foreach (string s in gR.applyFunctions)
                    applyFunctions.Add(s);
            }

            if (gR.embeddingRules.Count > 0)
            {
                embeddingRules = new List<embeddingRule>();
                foreach (embeddingRule e in gR.embeddingRules)
                    embeddingRules.Add(e);
            }

            UseShapeRestrictions = gR.UseShapeRestrictions;

            Translate = gR.Translate;
            Skew = gR.Skew;
            Scale = gR.Scale;
            Flip = gR.Flip;
            Projection = gR.Projection;
            Rotate = gR.Rotate;
            TransformNodeShapes = gR.TransformNodeShapes;
        }

        public string SerializeRulePropertiesToXml()
        {
            try
            {
                var sb = new StringBuilder();
                TextWriter tw = new StringWriter(sb);
                var Serializer = new XmlSerializer(typeof(RuleProperties));
                Serializer.Serialize(tw, this);
                return (sb.ToString());
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
                return null;
            }
        }

        public static RuleProperties DeSerializeFromXML(string xmlString)
        {
            try
            {
                RuleProperties newrP = null;
                var stringReader = new StringReader(xmlString);
                var Deserializer = new XmlSerializer(typeof(RuleProperties));
                newrP = (RuleProperties)Deserializer.Deserialize(stringReader);
                return newrP;
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
                return null;
            }
        }
    }
}