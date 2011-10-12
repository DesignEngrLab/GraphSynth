using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphSynth.GraphDisplay;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMainWindow
    {
        private string arcShape;
        private Boolean comboBoxGraphSelected, comboBoxRuleSelected;
        private string nodeShape;

        private void MouseEnter_ToolRefresh(object sender, MouseEventArgs e)
        {
            try
            {
                TestRuleCommandCanExecute(sender, e);
            }
            catch
            {
            }
        }

        public void DisconnectHeadCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (arc a in windowsMgr.activeGraphCanvas.Selection.selectedArcs)
                windowsMgr.activeGraphCanvas.DisconnectArcHead(a);
        }

        public void DisconnectTailCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (arc a in windowsMgr.activeGraphCanvas.Selection.selectedArcs)
                windowsMgr.activeGraphCanvas.DisconnectArcTail(a);
        }

        public void FlipArcCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (arc a in windowsMgr.activeGraphCanvas.Selection.selectedArcs)
                windowsMgr.activeGraphCanvas.FlipArc(a);
        }

        public void ArcConnectCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (windowsMgr.activeGraphCanvas != null
                            && windowsMgr.activeGraphCanvas.Selection != null
                            && windowsMgr.activeGraphCanvas.Selection.selectedArcs.Count > 0);
        }

        public void CaptureArcFormattingCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (arcShape == null)
            {
                if (SelectedAddItem.Contains("Arc"))
                    arcShape = (string)Application.Current.Resources[SelectedAddItem];
                else if (windowsMgr.activeGraphCanvas.Selection.SelectedArc != null)
                    arcShape = ((DisplayShape)windowsMgr.activeGraphCanvas.Selection.SelectedArc.
                        DisplayShape).String;
                if (arcShape != null)
                {
                    var s = (Path)MyXamlHelpers.Parse(arcShape);
                    s.RenderTransform = new MatrixTransform();
                    ApplyArcFormatVB.Child = (Shape)MyXamlHelpers.Parse(arcShape);
                }
                txtblkCaptureArcFormat.Text = "Release Arc Format";
                txtblkCaptureArcFormat.Foreground = Brushes.Red;
                SetSelectedAddItem(0);
            }
            else
            {
                arcShape = null;
                ApplyArcFormatVB.Child = null;
                txtblkCaptureArcFormat.Text = "Capture Arc Format";
                txtblkCaptureArcFormat.Foreground = Brushes.Black;
            }
        }

        public void ApplyArcFormattingCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                var datum = new arc();
                datum.DisplayShape = new DisplayShape(arcShape, ShapeRepresents.Arc, datum);
                if ((windowsMgr.activeGraphCanvas != null) &&
                    (windowsMgr.activeGraphCanvas.Selection.SelectedArc != null))
                    foreach (arc a in windowsMgr.activeGraphCanvas.Selection.selectedArcs)
                    {
                        windowsMgr.activeGraphCanvas.ApplyArcFormatting(a, datum,
                                                                        (Boolean)checkBoxApplyArcShape.IsChecked,
                                                                        (Boolean)checkBoxApplyArcDir.IsChecked);
                        windowsMgr.activeGraphCanvas.ArcPropertyChanged(a);
                    }
            }
        }

        public void CaptureArcFormattingCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (arcShape != null
                            || (windowsMgr.activeGraphCanvas != null
                                && windowsMgr.activeGraphCanvas.Selection != null
                                && windowsMgr.activeGraphCanvas.Selection.selectedArcs.Count >= 1)
                            || (SelectedAddItem.Contains("Arc")));
        }

        public void ApplyArcFormattingCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (arcShape != null
                            && windowsMgr.activeGraphCanvas != null
                            && windowsMgr.activeGraphCanvas.Selection != null
                            && windowsMgr.activeGraphCanvas.Selection.selectedArcs.Count >= 1);
        }

        public void CaptureNodeFormattingCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (nodeShape == null)
            {
                if (SelectedAddItem.Contains("Node"))
                    nodeShape = (string)Application.Current.Resources[SelectedAddItem];
                else if (windowsMgr.activeGraphCanvas.Selection.SelectedNode != null)
                    nodeShape = ((DisplayShape)windowsMgr.activeGraphCanvas.Selection.SelectedNode.
                        DisplayShape).String;
                if (nodeShape != null)
                {
                    var s = (Shape)MyXamlHelpers.Parse(nodeShape);
                    s.RenderTransform = new MatrixTransform();
                    ApplyNodeFormatVB.Child = s;
                }
                txtblkCaptureNodeFormat.Text = "Release Node Format";
                txtblkCaptureNodeFormat.Foreground = Brushes.Red;
                SetSelectedAddItem(0);
            }
            else
            {
                nodeShape = null;
                ApplyNodeFormatVB.Child = null;
                txtblkCaptureNodeFormat.Text = "Capture Node Format";
                txtblkCaptureNodeFormat.Foreground = Brushes.Black;
            }
        }

        public void ApplyNodeFormattingCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                var datum = new node();
                datum.DisplayShape = new DisplayShape(nodeShape,ShapeRepresents.Node, datum);
                if ((windowsMgr.activeGraphCanvas != null) &&
                    (windowsMgr.activeGraphCanvas.Selection.SelectedNode != null))
                    foreach (node n in windowsMgr.activeGraphCanvas.Selection.selectedNodes)
                        windowsMgr.activeGraphCanvas.ApplyNodeFormatting(n, datum,
                                                                         (Boolean)checkBoxApplyNodeDims.IsChecked,
                                                                         (Boolean)checkBoxApplyNodeShape.IsChecked
                                                                         , false);
            }
        }

        public void CaptureNodeFormattingCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (nodeShape != null
                            || (windowsMgr.activeGraphCanvas != null
                                && windowsMgr.activeGraphCanvas.Selection != null
                                && windowsMgr.activeGraphCanvas.Selection.selectedNodes.Count >= 1)
                            || (SelectedAddItem.Contains("Node")));
        }

        public void ApplyNodeFormattingCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((nodeShape != null)
                            && windowsMgr.activeGraphCanvas != null
                            && windowsMgr.activeGraphCanvas.Selection != null
                            && windowsMgr.activeGraphCanvas.Selection.selectedNodes.Count >= 1);
        }

        public void TestRuleCommandTestRuleCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var tempSeedWin = (graphWindow)((WinData)ComboBoxopenedGraphs.SelectedItem).Win;
            var tempRuleWin = (ruleWindow)((WinData)ComboBoxopenedRules.SelectedItem).Win;

            TestRuleChooser.Run(tempSeedWin.graph, tempRuleWin.rule);
        }

        public void TestRuleCommandCanExecute(object sender, RoutedEventArgs e)
        {
            ComboBoxopenedGraphs.ItemsSource = windowsMgr.GraphWindows;
            ComboBoxopenedRules.ItemsSource = windowsMgr.RuleWindows;

            TestRuleButton.IsEnabled = ((windowsMgr.GraphWindows.Count > 0) && (windowsMgr.RuleWindows.Count > 0));
            if ((windowsMgr.GraphWindows.Count > 0) &&
                (!comboBoxGraphSelected || ((ComboBoxopenedGraphs.SelectedIndex < 0)
                                            && (ComboBoxopenedGraphs.SelectedIndex >= ComboBoxopenedGraphs.Items.Count))))
            {
                ComboBoxopenedGraphs.SelectedIndex = 0;
                comboBoxGraphSelected = false;
            }
            else ComboBoxopenedGraphs.SelectedIndex = -1;
            if ((windowsMgr.RuleWindows.Count > 0) &&
                (!comboBoxRuleSelected || ((ComboBoxopenedRules.SelectedIndex < 0)
                                           && (ComboBoxopenedRules.SelectedIndex >= ComboBoxopenedRules.Items.Count))))
            {
                ComboBoxopenedRules.SelectedIndex = 0;
                comboBoxRuleSelected = false;
            }
            else ComboBoxopenedRules.SelectedIndex = 1;
        }

        private void ComboBoxopenedRules_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            comboBoxRuleSelected = ((ComboBoxopenedRules.SelectedIndex >= 0)
                                    && (ComboBoxopenedRules.SelectedIndex < ComboBoxopenedRules.Items.Count));
        }

        private void ComboBoxopenedGraphs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            comboBoxGraphSelected = ((ComboBoxopenedGraphs.SelectedIndex >= 0)
                                     && (ComboBoxopenedGraphs.SelectedIndex < ComboBoxopenedGraphs.Items.Count));
        }


        private void AddNodeArcButton_Click(object sender, RoutedEventArgs e)
        {
            if (stayOn) SetSelectedAddItem(-1);
            else SetSelectedAddItem(((Button)sender).Name);
            stayOn = false;
        }

        private void AddNodeArcButton_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            SetSelectedAddItem(((Button)sender).Name);
            stayOn = true;
        }

        //public void AddNodeArcCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        //{ // this wasn't need because the click events prove more useful since they have a 
        //    // proper sender, and they can show when double-click occurs.
        //}
        //public void AddNodeArcCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = (windowsMgr.activeGraphCanvas != null);
        //}
        private void checkBoxApplyFormat_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        public void LoadCustomCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        public void LoadCustomCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
        }

        public void ReloadCustomCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        public void ReloadCustomCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
        }

        public void ClearCustomCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        public void ClearCustomCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
        }
    }
}