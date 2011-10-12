using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GraphSynth.GraphDisplay;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for NodeDisplayProperty.xaml
    /// </summary>
    public partial class NodeDisplayProperty : UserControl
    {
        private GraphGUI gui;
        private List<node> nodes;

        public NodeDisplayProperty()
        {
            InitializeComponent();
        }

        private node firstNode
        {
            get { return nodes[0]; }
        }

        #region Size Events

        private void txtBxWidth_LostFocus(object sender, RoutedEventArgs e)
        {
            double temp;
            if (Double.TryParse(txtBxWidth.Text, out temp))
            {
                foreach (node n in nodes)
                {
                    n.DisplayShape.Width = temp;
                    gui.NodePropertyChanged(n);
                }
                //Update();
            }
        }

        private void txtBxWidth_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalNumber((TextBox)sender, e))
                txtBxWidth_LostFocus(sender, e);
        }

        private void txtBxHeight_LostFocus(object sender, RoutedEventArgs e)
        {
            double temp;
            if (Double.TryParse(txtBxHeight.Text, out temp))
            {
                foreach (node n in nodes)
                {
                    n.DisplayShape.Height = temp;
                    gui.NodePropertyChanged(n);
                }
                //  Update();
            }
        }

        private void txtBxHeight_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalNumber((TextBox)sender, e))
                txtBxHeight_LostFocus(sender, e);
        }

        #endregion

        #region Stroke Thickness

        private void sldStrokeThickness_ValueChanged(object sender, RoutedEventArgs e)
        {
            foreach (node n in nodes)
            {
                n.DisplayShape.StrokeThickness
                    = sldStrokeThickness.Value; //*(Math.Min(dispShape.Height, dispShape.Width) / 2);                
                gui.NodePropertyChanged(n);
            }
            //   Update();
        }

        #endregion

        #region Rotation

        private void sldRotation_ValueChanged(object sender, RoutedEventArgs e)
        {
            foreach (node n in nodes)
                gui.Rotate(n, sldRotation.Value);
            // Update();
        }

        #endregion

        #region Color

        private void FillColorSelector_ValueChanged(object sender, RoutedEventArgs e)
        {
            foreach (node n in nodes)
            {
                n.DisplayShape.Fill = FillColorSelector.Value;
                gui.NodePropertyChanged(n);
            }
            //  Update();
        }

        private void StrokeColorSelector_ValueChanged(object sender, RoutedEventArgs e)
        {
            foreach (node n in nodes)
            {
                n.DisplayShape.Stroke = StrokeColorSelector.Value;
                gui.NodePropertyChanged(n);
            }
            //   Update();
        }

        #endregion

        internal void Update(List<node> _nodes, GraphGUI _gui)
        {
            textPrpt.gui = gui = _gui;
            nodes = _nodes;
            textPrpt.Elements = nodes.Cast<graphElement>().ToList();
            Update();
        }

        private void Update()
        {
            /*************Fill Color*************/
            var allSame = true;
            var fill = (Brush)nodes[0].DisplayShape.Fill;
            for (var i = 1; i < nodes.Count; i++)
                if (!BrushSelector.EqualBrushes(fill, (Brush)nodes[i].DisplayShape.Fill))
                {
                    allSame = false;
                    break;
                }
            if (allSame) FillColorSelector.ReadInBrushValue(fill);
            else FillColorSelector.ReadInBrushValue(null);

            /*************Stroke Color*************/
            allSame = true;
            var stroke = (Brush)nodes[0].DisplayShape.Stroke;
            for (var i = 1; i < nodes.Count; i++)
                if (!BrushSelector.EqualBrushes(stroke, (Brush)nodes[i].DisplayShape.Stroke))
                {
                    allSame = false;
                    break;
                }
            if (allSame)
            {
                if (stroke != null) StrokeColorSelector.ReadInBrushValue(stroke);
                else StrokeColorSelector.ReadInBrushValue(Brushes.Transparent);
            }
            else StrokeColorSelector.ReadInBrushValue(null);

            /*************Stroke Thickness*************/
            allSame = true;
            var thick = nodes[0].DisplayShape.StrokeThickness;
            for (var i = 1; i < nodes.Count; i++)
                if (!thick.Equals(nodes[i].DisplayShape.StrokeThickness))
                {
                    allSame = false;
                    break;
                }
            if (allSame)
                sldStrokeThickness.UpdateValue(nodes[0].DisplayShape.StrokeThickness);
            else sldStrokeThickness.UpdateValue(double.NaN);

            /*************Rotataion*************/
            allSame = true;
            var rotat = Math.Atan2(nodes[0].DisplayShape.TransformMatrix[0, 1],
                                   nodes[0].DisplayShape.TransformMatrix[0, 0]);
            for (var i = 1; i < nodes.Count; i++)
            {
                if (rotat !=
                    Math.Atan2(nodes[i].DisplayShape.TransformMatrix[0, 1], nodes[i].DisplayShape.TransformMatrix[0, 0]))
                {
                    allSame = false;
                    break;
                }
            }
            if (allSame) sldRotation.UpdateValue(180 * rotat / Math.PI);
            else sldRotation.UpdateValue(double.NaN);

            /*************Width*************/
            allSame = true;
            var w = nodes[0].DisplayShape.Width;
            for (var i = 1; i < nodes.Count; i++)
                if (!w.Equals(nodes[i].DisplayShape.Width))
                {
                    allSame = false;
                    break;
                }
            if (allSame)
            {
                txtBxWidth.Foreground = Brushes.Black;
                txtBxWidth.Text = w.ToString();
            }
            else
            {
                txtBxWidth.Foreground = Brushes.Gray;
                txtBxWidth.Text = "diff";
            }

            /*************Height*************/
            allSame = true;
            var h = nodes[0].DisplayShape.Height;
            for (var i = 1; i < nodes.Count; i++)
                if (!h.Equals(nodes[i].DisplayShape.Height))
                {
                    allSame = false;
                    break;
                }
            if (allSame)
            {
                txtBxHeight.Foreground = Brushes.Black;
                txtBxHeight.Text = h.ToString();
            }
            else
            {
                txtBxHeight.Foreground = Brushes.Gray;
                txtBxHeight.Text = "diff";
            }
        }
    }
}