using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for ShapeProperty.xaml
    /// </summary>
    public partial class ShapeProperty : UserControl
    {
        private static Dictionary<string, transfromType> transformDict;
        private grammarRule _rule;

        public ShapeProperty()
        {
            InitializeComponent();
            foreach (object s in TransformDict.Keys)
            {
                cmdTranslate.Items.Add(s.ToString());
                cmdScale.Items.Add(s.ToString());
                cmdSkew.Items.Add(s.ToString());
                cmdFlip.Items.Add(s.ToString());
                cmdProjection.Items.Add(s.ToString());
                cmdRotate.Items.Add(s.ToString());
            }
        }

        public grammarRule rule
        {
            get { return _rule; }
            set
            {
                _rule = value;
                cmdTranslate.SelectedIndex = (int)_rule.Translate;
                cmdScale.SelectedIndex = (int)_rule.Scale;
                cmdSkew.SelectedIndex = (int)_rule.Skew;
                cmdFlip.SelectedIndex = (int)_rule.Flip;
                cmdProjection.SelectedIndex = (int)_rule.Projection;
                cmdRotate.SelectedIndex = (int)_rule.Rotate;
                chkMatchRShapes.IsChecked = _rule.RestrictToNodeShapeMatch;
                chkTransformNodeShapes.IsChecked = _rule.TransformNodeShapes;
                chkTransformNodePositions.IsChecked = _rule.TransformNodePositions;
                chkShapeRuleProperties.IsChecked = _rule.UseShapeRestrictions;
                EnableOrDisableComboBoxes();
            }
        }

        public static Dictionary<string, transfromType> TransformDict
        {
            get
            {
                if (transformDict == null) defineTransformDict();
                return transformDict;
            }
        }

        private static void defineTransformDict()
        {
            transformDict = new Dictionary<string, transfromType>();
            transformDict.Add("Not Allowed", transfromType.Prohibited);
            transformDict.Add("Allowed only in X", transfromType.OnlyX);
            transformDict.Add("Allowed only in Y", transfromType.OnlyY);
            transformDict.Add("Allowed only in Z", transfromType.OnlyZ);
            transformDict.Add("Allowed Uniformly in X,Y,& Z", transfromType.XYZUniform);
            transformDict.Add("Allowed in X,Y,& Z Independently", transfromType.XYZIndependent);
        }

        private void cmdTranslate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rule.Translate = TransformDict[cmdTranslate.SelectedItem.ToString()];
        }

        private void cmdScale_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rule.Scale = TransformDict[cmdScale.SelectedItem.ToString()];
        }

        private void cmdSkew_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rule.Skew = TransformDict[cmdSkew.SelectedItem.ToString()];
        }

        private void cmdFlip_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rule.Flip = TransformDict[cmdFlip.SelectedItem.ToString()];
        }

        private void cmdProjection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rule.Projection = TransformDict[cmdProjection.SelectedItem.ToString()];
        }

        private void cmdRotation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rule.Rotate = TransformDict[cmdRotate.SelectedItem.ToString()];
        }     

        private void chkMatchRShapes_Checked(object sender, RoutedEventArgs e)
        {
            rule.RestrictToNodeShapeMatch = true;
        }

        private void chkMatchRShapes_Unchecked(object sender, RoutedEventArgs e)
        {
            rule.RestrictToNodeShapeMatch = false;
        }

        private void chkTransformNodeShapes_Checked(object sender, RoutedEventArgs e)
        {
            rule.TransformNodeShapes = true;
        }

        private void chkTransformNodeShapes_Unchecked(object sender, RoutedEventArgs e)
        {
            rule.TransformNodeShapes = false;
        }

        private void chkTransformNodePositions_Checked(object sender, RoutedEventArgs e)
        {
            rule.TransformNodePositions = true;
        }

        private void chkTransformNodePositions_Unchecked(object sender, RoutedEventArgs e)
        {
            rule.TransformNodePositions = false;
        }


        private void chkShapeRuleProperties_Checked(object sender, RoutedEventArgs e)
        {
            rule.UseShapeRestrictions = true;
            EnableOrDisableComboBoxes();
        }

        private void chkShapeRuleProperties_Unchecked(object sender, RoutedEventArgs e)
        {
            rule.UseShapeRestrictions = false;
            EnableOrDisableComboBoxes();
        }

        void EnableOrDisableComboBoxes()
        {
            cmdTranslate.IsEnabled =
                cmdScale.IsEnabled =
                    cmdSkew.IsEnabled =
                        cmdFlip.IsEnabled =
                            cmdProjection.IsEnabled =
                                cmdRotate.IsEnabled = rule.UseShapeRestrictions;

        }
    }
}