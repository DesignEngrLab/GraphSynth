using System;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Xml.Serialization;
using GraphSynth.GraphDisplay;
using GraphSynth.UI;

namespace GraphSynth
{
    public class UICanvas
    {
        [XmlAttribute]
        public double ArcDisplayTextDistance;
        [XmlAttribute]
        public double ArcDisplayTextPosition;
        [XmlAttribute]
        public double ArcLabelFontSize;

        [XmlAttribute] //[System.Xml.Serialization.XmlElementAttribute("AxesColor",DataType = "SolidColorBrush")]  
        public string AxesColor;

        [XmlAttribute]
        public double AxesOpacity;
        [XmlAttribute]
        public double AxesThick;
        [XmlAttribute]
        public string BackgroundColor;

        //[System.Xml.Serialization.XmlElementAttribute("GridColor",DataType = "SolidColorBrush")]
        [XmlAttribute]
        public string GridColor;
        [XmlAttribute]
        public double GridOpacity;
        [XmlAttribute]
        public double GridSpacing;
        [XmlAttribute]
        public double GridThick;
        
        [XmlAttribute]
        public double NodeDisplayTextDistance;
        [XmlAttribute]
        public double NodeDisplayTextPosition;
        [XmlAttribute]
        public double NodeLabelFontSize;

        [XmlAttribute]
        public double ScaleFactor;
        [XmlAttribute]
        public double ShapeOpacity;
        [XmlAttribute]
        public Boolean ShowArcLabel;
        [XmlAttribute]
        public Boolean ShowArcName;
        [XmlAttribute]
        public Boolean ShowNodeLabel;
        [XmlAttribute]
        public Boolean ShowNodeName;
        [XmlAttribute]
        public Boolean SnapToGrid;
        [XmlAttribute]
        public Boolean ZoomToFit;

        public UICanvas(GraphGUI gd)
        {
            AxesColor = ((SolidColorBrush)gd.gridAndAxes.AxesColor).Color.ToString();
            AxesOpacity = gd.gridAndAxes.AxesOpacity;
            AxesThick = gd.gridAndAxes.AxesThick;

            GridColor = ((SolidColorBrush)gd.gridAndAxes.GridColor).Color.ToString();
            GridOpacity = gd.gridAndAxes.GridOpacity;
            GridSpacing = gd.gridAndAxes.GridSpacing;
            GridThick = gd.gridAndAxes.GridThick;

            ShapeOpacity = gd.nodeShapes.Opacity;


            ShowNodeName = gd.nodeIcons.ShowName;
            ShowNodeLabel = gd.nodeIcons.ShowLabels;
            ShowArcName = gd.arcIcons.ShowName;
            ShowArcLabel = gd.arcIcons.ShowLabels;

            NodeLabelFontSize = gd.nodeIcons.FontSize;
            ArcLabelFontSize = gd.arcIcons.FontSize;
            NodeDisplayTextDistance = gd.nodeIcons.DisplayTextDistance;
            NodeDisplayTextPosition = gd.nodeIcons.DisplayTextPosition;
            ArcDisplayTextDistance = gd.arcIcons.DisplayTextDistance;
            ArcDisplayTextPosition = gd.arcIcons.DisplayTextPosition;

            SnapToGrid = gd.SnapToGrid;

            ScaleFactor = gd.ScaleFactor;
            ZoomToFit = gd.ZoomToFit;
            if (gd.Background is SolidColorBrush)
                BackgroundColor = gd.Background.ToString();
            else
                BackgroundColor = "#FFFFFFFF";
        }

        public UICanvas()
        {
            AxesColor = new SolidColorBrush(Colors.Black).ToString();
            AxesOpacity = 1;
            AxesThick = 0.5;

            GridColor = new SolidColorBrush(Colors.Black).ToString();
            GridOpacity = 0.3;
            GridSpacing = 24;
            GridThick = 0.25;

            ShapeOpacity = 1;

            ShowNodeName = true;
            ShowNodeLabel = true;
            ShowArcName = true;
            ShowArcLabel = true;

            NodeLabelFontSize = 12;
            ArcLabelFontSize = 12;
            NodeDisplayTextDistance = 1.0;
            NodeDisplayTextPosition = 0.0;
            ArcDisplayTextDistance = 0.5;
            ArcDisplayTextPosition = 0.5;

            SnapToGrid = false;
            ScaleFactor = 1;
            ZoomToFit = false;

            BackgroundColor = new SolidColorBrush(Colors.White).ToString();
        }

        public static string SerializeCanvasToXml(UICanvas canvas)
        {
            try
            {
                var sb = new StringBuilder();
                TextWriter tw = new StringWriter(sb);
                var canvasSerializer = new XmlSerializer(typeof(UICanvas));
                canvasSerializer.Serialize(tw, canvas);
                return (sb.ToString());
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
                return null;
            }
        }

        public static UICanvas DeSerializeFromXML(string xmlString)
        {
            try
            {
                xmlString = xmlString.Replace("Canvas", "UICanvas");
                UICanvas newCanvas = null;
                var stringReader = new StringReader(xmlString);
                var canvasDeserializer = new XmlSerializer(typeof(UICanvas));
                newCanvas = (UICanvas)canvasDeserializer.Deserialize(stringReader);
                return newCanvas;
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
                return null;
            }
        }


        public void UpdateCanvasProperties(GraphGUI gd)
        {
            gd.gridAndAxes.AxesColor = BrushSelector.GetBrushFromString(AxesColor);
            gd.gridAndAxes.AxesOpacity = AxesOpacity;
            gd.gridAndAxes.AxesThick = AxesThick;

            gd.gridAndAxes.GridColor = BrushSelector.GetBrushFromString(GridColor);
            gd.gridAndAxes.GridOpacity = GridOpacity;
            gd.gridAndAxes.GridSpacing = GridSpacing;
            gd.gridAndAxes.GridThick = GridThick;

            gd.nodeShapes.Opacity = ShapeOpacity;

            gd.nodeIcons.ShowName = ShowNodeName;
            gd.nodeIcons.ShowLabels = ShowNodeLabel;
            gd.arcIcons.ShowName = ShowArcName;
            gd.arcIcons.ShowLabels = ShowArcLabel;

            gd.nodeIcons.FontSize = NodeLabelFontSize;
            gd.nodeIcons.DisplayTextDistance = NodeDisplayTextDistance;
            gd.nodeIcons.DisplayTextPosition = NodeDisplayTextPosition;
            gd.arcIcons.FontSize = ArcLabelFontSize;
            gd.arcIcons.DisplayTextDistance = ArcDisplayTextDistance;
            gd.arcIcons.DisplayTextPosition = ArcDisplayTextPosition;


            gd.SnapToGrid = SnapToGrid;

            gd.ScaleFactor = ScaleFactor;
            gd.ZoomToFit = ZoomToFit;
            //if (BackgroundColor.Equals("#FFFFFFFF"))
            //    gd.Background =
            //else
            //    gd.Background = new SolidColorBrush(BrushSelector.GetColorFromString(BackgroundColor));
        }
    }
}