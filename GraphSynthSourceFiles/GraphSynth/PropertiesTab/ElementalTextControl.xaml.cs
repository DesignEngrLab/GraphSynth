using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GraphSynth.GraphDisplay;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    /// <summary>
    /// Interaction logic for ElementalTextControl.xaml
    /// </summary>
    public partial class ElementalTextControl : UserControl
    {
        public ElementalTextControl()
        {
            InitializeComponent();
        }

        public GraphGUI gui { get; set; }
        private List<IconShape> icons;
        private ShapeRepresents eltType;
        public List<graphElement> Elements
        {
            set
            {
                var eltTypeName = value[0].GetType().Name;
                eltTypeName = eltTypeName.Replace("rule", "");
                eltType = (ShapeRepresents)Enum.Parse(typeof(ShapeRepresents), eltTypeName, true);
                icons = new List<IconShape>();
                foreach (var elt in value)
                    if (typeof(node).IsInstanceOfType(elt))
                        icons.Add(gui.nodeIcons.FirstOrDefault(nI => nI.GraphElement == elt));
                    else if (typeof(arc).IsInstanceOfType(elt))
                        icons.Add(((ArcShape)elt.DisplayShape.Shape).icon);
                    else if (typeof(hyperarc).IsInstanceOfType(elt))
                        icons.Add(((HyperArcShape)elt.DisplayShape.Shape).icon);
                Update();
            }
        }

        private void chkIndependentProperties_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var icon in icons)
                switch (eltType)
                {
                    case ShapeRepresents.Node:
                        gui.nodeIcons.UnbindTextDisplayProperties(icon);
                        break;
                    case ShapeRepresents.Arc:
                        gui.arcIcons.UnbindTextDisplayProperties(icon);
                        break;
                    case ShapeRepresents.HyperArc:
                        gui.hyperarcIcons.UnbindTextDisplayProperties(icon);
                        break;
                }
            expTextProperties.IsExpanded = true;
            expTextProperties.IsExpanded = true;
            chkShowNodeName.IsEnabled = true;
            chkShowNodeLabels.IsEnabled = true;
            sldDistance.IsEnabled = true;
            sldFontSize.IsEnabled = true;
            sldPosition.IsEnabled = true;
        }

        private void chkIndependentProperties_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var icon in icons)
            {
                switch (eltType)
                {
                    case ShapeRepresents.Node:
                        gui.nodeIcons.BindTextDisplayProperties(icon);
                        break;
                    case ShapeRepresents.Arc:
                        gui.arcIcons.BindTextDisplayProperties(icon);
                        break;
                    case ShapeRepresents.HyperArc:
                        gui.hyperarcIcons.BindTextDisplayProperties(icon);
                        break;
                }
                MultiBindingExpression mbe = BindingOperations.GetMultiBindingExpression(icon,
                    IconShape.DisplayTextProperty);
                mbe.UpdateTarget();
            }
            expTextProperties.IsExpanded = false;
            expTextProperties.IsExpanded = false;
            chkShowNodeName.IsEnabled = false;
            chkShowNodeLabels.IsEnabled = false;
            sldDistance.IsEnabled = false;
            sldFontSize.IsEnabled = false;
            sldPosition.IsEnabled = false;
        }

        private void chkShowNodeName_Click(object sender, RoutedEventArgs e)
        {
            if ((chkShowNodeName.IsChecked == null) || (chkShowNodeName.IsChecked.Value == false))
                foreach (var icon in icons)
                    icon.ShowName = false;
            else
                foreach (var icon in icons)
                    icon.ShowName = true;
            if (icons.All(icon => icon.ShowName)) chkShowNodeName.IsChecked = true;
            else if (icons.All(icon => icon.ShowName == false)) chkShowNodeName.IsChecked = false;
            else chkShowNodeName.IsChecked = null;

        }
        private void chkShowNodeLabels_Click(object sender, RoutedEventArgs e)
        {
            if ((chkShowNodeLabels.IsChecked == null) || (chkShowNodeLabels.IsChecked.Value == false))
                foreach (var icon in icons)
                    icon.ShowLabels = false;
            else
                foreach (var icon in icons)
                    icon.ShowLabels = true;
            if (icons.All(icon => icon.ShowLabels)) chkShowNodeLabels.IsChecked = true;
            else if (icons.All(icon => icon.ShowLabels == false)) chkShowNodeLabels.IsChecked = false;
            else chkShowNodeLabels.IsChecked = null;
        }

        void Update()
        {
            if (icons.All(icon => icon.UniqueTextProperties))
                chkIndependentProperties.IsChecked = true;
            else if (icons.All(icon => icon.UniqueTextProperties == false))
                chkIndependentProperties.IsChecked = false;
            else chkIndependentProperties.IsChecked = null;

            if (icons.All(icon => icon.ShowName)) chkShowNodeName.IsChecked = true;
            else if (icons.All(icon => icon.ShowName == false)) chkShowNodeName.IsChecked = false;
            else chkShowNodeName.IsChecked = null;

            if (icons.All(icon => icon.ShowLabels)) chkShowNodeLabels.IsChecked = true;
            else if (icons.All(icon => icon.ShowLabels == false)) chkShowNodeLabels.IsChecked = false;
            else chkShowNodeLabels.IsChecked = null;

            if (icons.All(icon => icon.FontSize == icons[0].FontSize))
                sldFontSize.UpdateValue(icons[0].FontSize);
            else sldFontSize.UpdateValue(double.NaN);
            if (icons.All(icon => icon.DisplayTextDistance == icons[0].DisplayTextDistance))
                sldDistance.UpdateValue(icons[0].DisplayTextDistance);
            else sldDistance.UpdateValue(double.NaN);
            if (icons.All(icon => icon.DisplayTextPosition == icons[0].DisplayTextPosition))
                sldDistance.UpdateValue(icons[0].DisplayTextPosition);
            else sldDistance.UpdateValue(double.NaN);


        }

        private void sldFontSize_ValueChanged(object sender, RoutedEventArgs e)
        {
            foreach (var icon in icons)
                icon.FontSize = sldFontSize.Value;
        }

        private void sldPosition_ValueChanged(object sender, RoutedEventArgs e)
        {
            foreach (var icon in icons)
                icon.DisplayTextPosition = sldPosition.Value;
        }

        private void sldDistance_ValueChanged(object sender, RoutedEventArgs e)
        {
            foreach (var icon in icons)
                icon.DisplayTextDistance = sldDistance.Value;
        }

    }
}
