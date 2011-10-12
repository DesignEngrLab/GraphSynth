using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Serialization;
using GraphSynth.GraphDisplay;
using System.Xml;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for CanvasProperty.xaml
    /// </summary>
    public partial class CanvasProperty : UserControl
    {
        #region Constructor

        public CanvasProperty()
        {
            InitializeComponent();
            ArcFontSize = 12;
            ArcTextDistance = 0.0;
            ArcTextPosition = 0.5;
            AxesColor = Brushes.Black;
            AxesOpacity = 0.5;
            AxesThick = 0.5;
            BackgroundColor = Brushes.White;
            GridColor = Brushes.Black;
            GridOpacity = 0.5;
            GridSpacing = 96;
            GridThick = 0.25;
            HyperArcFontSize = 12;
            HyperArcTextDistance = 0.0;
            HyperArcTextPosition = 0.5;
            NodeFontSize = 12;
            NodeTextDistance = 0.0;
            NodeTextPosition = 0.0;
            ScaleFactor = 1.0;
            ShapeOpacity = 1.0;
            ShowArcLabel = true;
            ShowArcName = true;
            ShowNodeLabel = true;
            ShowNodeName = true;
            ShowHyperArcLabel = true;
            ShowHyperArcName = true;
            SnapToGrid = true;
            ZoomToFit = true;
            CanvasHeight = 300;
            CanvasWidth = new Thickness(300, 300, 300, 912);
            GlobalTextSize = 12;
            WindowLeft = 500;
            WindowTop = 300;
        }

        #endregion

        public List<GraphGUI> controlledGUIs = new List<GraphGUI>();

        #region Properties for XML Serialization

        #region Grid and Axes

        public Brush BackgroundColor
        {
            get { return BackgroundColorSelector.Value; }
            set { BackgroundColorSelector.ReadInBrushValue(value); }
        }

        public Brush AxesColor
        {
            get { return AxesColorSelector.Value; }
            set { AxesColorSelector.ReadInBrushValue(value); }
        }

        public double AxesOpacity
        {
            get { return sldAxesOpacity.Value; }
            set { sldAxesOpacity.UpdateValue(value); }
        }

        public double AxesThick
        {
            get { return sldAxesThickness.Value; }
            set { sldAxesThickness.UpdateValue(value); }
        }

        public Brush GridColor
        {
            get { return GridColorSelector.Value; }
            set { GridColorSelector.ReadInBrushValue(value); }
        }

        public double GridOpacity
        {
            get { return sldGridOpacity.Value; }
            set { sldGridOpacity.UpdateValue(value); }
        }

        public double GridSpacing
        {
            get { return sldGridSpacing.Value; }
            set { sldGridSpacing.UpdateValue(value); }
        }

        public double GridThick
        {
            get { return sldGridThickness.Value; }
            set { sldGridThickness.UpdateValue(value); }
        }

        public Boolean SnapToGrid
        {
            get { return (Boolean)chkSnapToGrid.IsChecked; }
            set { chkSnapToGrid.IsChecked = value; }
        }

        #endregion

        #region Shape Viewing

        public double ScaleFactor
        {
            get { return sldZoom.Value / 100.0; }
            set { sldZoom.UpdateValue(100.0 * value); }
        }

        public double ShapeOpacity
        {
            get { return sldShapeOpacity.Value; }
            set { sldShapeOpacity.UpdateValue(value); }
        }

        public Boolean ZoomToFit
        {
            get { return (Boolean)chkZoomToFit.IsChecked; }
            set { chkZoomToFit.IsChecked = value; }
        }

        #endregion

        #region Text Viewing

        public Boolean ShowNodeName
        {
            get { return (Boolean)chkShowNodeName.IsChecked; }
            set { chkShowNodeName.IsChecked = value; }
        }

        public Boolean ShowNodeLabel
        {
            get { return (Boolean)chkShowNodeLabels.IsChecked; }
            set { chkShowNodeLabels.IsChecked = value; }
        }

        public Boolean ShowArcName
        {
            get { return (Boolean)chkShowArcName.IsChecked; }
            set { chkShowArcName.IsChecked = value; }
        }

        public Boolean ShowArcLabel
        {
            get { return (Boolean)chkShowArcLabels.IsChecked; }
            set { chkShowArcLabels.IsChecked = value; }
        }

        public Boolean ShowHyperArcName
        {
            get { return (Boolean)chkShowHyperArcName.IsChecked; }
            set { chkShowHyperArcName.IsChecked = value; }
        }

        public Boolean ShowHyperArcLabel
        {
            get { return (Boolean)chkShowHyperArcLabels.IsChecked; }
            set { chkShowHyperArcLabels.IsChecked = value; }
        }

        public double NodeFontSize
        {
            get { return sldNodeFontSize.Value; }
            set { sldNodeFontSize.UpdateValue(value); }
        }

        public double ArcFontSize
        {
            get { return sldArcFontSize.Value; }
            set { sldArcFontSize.UpdateValue(value); }
        }

        public double HyperArcFontSize
        {
            get { return sldHyperArcFontSize.Value; }
            set { sldHyperArcFontSize.UpdateValue(value); }
        }

        public double NodeTextDistance
        {
            get { return sldNodeDistance.Value; }
            set { sldNodeDistance.UpdateValue(value); }
        }

        public double NodeTextPosition
        {
            get { return sldNodePosition.Value; }
            set { sldNodePosition.UpdateValue(value); }
        }

        public double ArcTextDistance
        {
            get { return sldArcDistance.Value; }
            set { sldArcDistance.UpdateValue(value); }
        }

        public double ArcTextPosition
        {
            get { return sldArcPosition.Value; }
            set { sldArcPosition.UpdateValue(value); }
        }
        public double HyperArcTextDistance
        {
            get { return sldHyperArcDistance.Value; }
            set { sldHyperArcDistance.UpdateValue(value); }
        }

        public double HyperArcTextPosition
        {
            get { return sldHyperArcPosition.Value; }
            set { sldHyperArcPosition.UpdateValue(value); }
        }

        #endregion

        #region Additional Global Properties

        public double GlobalTextSize { get; set; }
        public double CanvasHeight { get; set; }
        /* the CanvasWidth is not a double?! The reason I have used Thickness is
         * so extra values can be captured for rule windows. This may seem a bit
         * wonky but Thickness is nice since it can accept either a single value
         * for the width of a single graph, or four values for describing the L (left),
         * R (right), K (top), slider (bottom) widths of a rule. */
        public Thickness CanvasWidth { get; set; }
        public double WindowLeft { get; set; }
        public double WindowTop { get; set; }

        #endregion

        /// <summary>
        ///   Gets or sets the old data. In order to be compatible with previous versions,
        ///   this oldData object will be used to catch old files that use screenX and 
        ///   screenY instead of the new format.
        /// </summary>
        /// <value>The old data.</value>
        [XmlAnyElement]
        public XmlElement[] extraData { get; set; }


        [XmlAttribute("AutoLayout")]
        public XmlAttribute extraAttributes { get; set; }
        #endregion

        #region XML Serialization

        public static string SerializeCanvasToXml(CanvasProperty canvas)
        {
            var xmlString = XamlWriter.Save(canvas);
            var startOfResources = xmlString.IndexOf("<CanvasProperty.Resources>");
            var endOfResources = xmlString.LastIndexOf("</CanvasProperty>");
            xmlString = xmlString.Remove(startOfResources, (endOfResources - startOfResources));

            return xmlString;
        }

        public static CanvasProperty DeSerializeFromXML(string xmlString)
        {
            try
            {
                /* to make it load old versions of the CanvasProperties from
                 * 1/2009 to 9/2009, we replace Canvas with CanvasProperty */
                xmlString = xmlString.Replace("<Canvas ", "<CanvasProperty ");
                xmlString = xmlString.Replace("</Canvas>", "</CanvasProperty>");
                var context = new ParserContext();
                context.XmlnsDictionary.Add("GraphSynth", "clr-namespace:GraphSynth.UI;assembly=GraphSynth");
                return (CanvasProperty)MyXamlHelpers.Parse(xmlString, context);

                /***** Notice!: If you have crashed GS2.0 here, then
                 * the try-catch failed. This happens due to a setting
                 * in your Visual Studio environment. To fix this:
                 * 1) Go to Debug->Exceptions.
                 * 2) expand Common Language Runtime Exceptions
                 * 3) Scroll Down to System.Windows.Markup.XamlParseException
                 * 4) uncheck the box in the "Thrown" Column. */
            }
            catch
            {
                SearchIO.output("Failed to open Canvas data from file. "
                                + "It is likely non-existent, or in an old format.");
                return new CanvasProperty();
            }
        }

        #endregion

        #region Updating to and from GraphGUI object

        private Boolean DontRecurseOnZoomChange;

        public void AddGUIToControl(GraphGUI graphGUI)
        {
            controlledGUIs.Add(graphGUI);
            Update();
        }

        public void Update()
        {
            foreach (GraphGUI gd in controlledGUIs)
            {
                gd.Background = BackgroundColor;
                gd.gridAndAxes.AxesColor = AxesColor;
                gd.gridAndAxes.AxesOpacity = AxesOpacity;
                gd.gridAndAxes.AxesThick = AxesThick;
                gd.gridAndAxes.GridColor = GridColor;
                gd.gridAndAxes.GridOpacity = GridOpacity;
                gd.gridAndAxes.GridSpacing = GridSpacing;
                gd.gridAndAxes.GridThick = GridThick;
                gd.SnapToGrid = SnapToGrid;

                gd.nodeShapes.Opacity = ShapeOpacity;

                gd.nodeIcons.ShowName = ShowNodeName;
                gd.nodeIcons.ShowLabels = ShowNodeLabel;
                gd.arcIcons.ShowName = ShowArcName;
                gd.arcIcons.ShowLabels = ShowArcLabel;
                gd.hyperarcIcons.ShowName = ShowHyperArcName;
                gd.hyperarcIcons.ShowLabels = ShowHyperArcLabel;

                gd.nodeIcons.FontSize = NodeFontSize;
                gd.nodeIcons.DisplayTextDistance = NodeTextDistance;
                gd.nodeIcons.DisplayTextPosition = NodeTextPosition;
                gd.arcIcons.FontSize = ArcFontSize;
                gd.arcIcons.DisplayTextDistance = ArcTextDistance;
                gd.arcIcons.DisplayTextPosition = ArcTextPosition;
                gd.hyperarcIcons.FontSize = HyperArcFontSize;
                gd.hyperarcIcons.DisplayTextDistance = HyperArcTextDistance;
                gd.hyperarcIcons.DisplayTextPosition = HyperArcTextPosition;
            }
        }

        public void ViewValueChanged(object sender, RoutedEventArgs e)
        {
            if (!DontRecurseOnZoomChange)
            {
                DontRecurseOnZoomChange = true;
                foreach (GraphGUI gd in controlledGUIs)
                {
                    if (ZoomToFit && !gd.ZoomToFit) gd.ZoomToFit = true;
                    else if (gd.ScaleFactor != ScaleFactor)
                    {
                        gd.ScaleFactor = ScaleFactor;
                        gd.ZoomToFit = false;
                    }
                    else if (!ZoomToFit && gd.ZoomToFit) gd.ZoomToFit = false;
                }
                DontRecurseOnZoomChange = false;
            }
        }

        #endregion

        private void ValueChanged(object sender, RoutedEventArgs e)
        {
            Update();
        }


        private void TemplateButton_Click(object sender, RoutedEventArgs e)
        {
            var canvas = TemplatePickerWindow.ShowWindowDialog();
            if (canvas == null) return;
            var type = typeof(CanvasProperty);
            var propInfo = type.GetProperties();
            var numProps = propInfo.GetLength(0) - (typeof(UserControl)).GetProperties().GetLength(0);
            for (var j = 0; j < numProps; j++)
                propInfo[j].SetValue(this, propInfo[j].GetValue(canvas, null), null);
            var w = GSApp.main.windowsMgr.activeWindow;
            if (typeof(graphWindow).IsInstanceOfType(w))
                ((graphWindow)w).AdoptWindowWideCanvasProperties();
            else if (typeof(ruleWindow).IsInstanceOfType(w))
                ((ruleWindow)w).AdoptWindowWideCanvasProperties();
            Update();
        }

    }
}