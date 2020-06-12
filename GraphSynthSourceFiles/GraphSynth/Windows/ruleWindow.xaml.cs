using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using GraphSynth.GraphDisplay;
using GraphSynth.Representation;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace GraphSynth.UI
{
    public partial class ruleWindow : Window, IRuleWindow
    {
        #region Properties

        private List<string> KLabels = new List<string>();
        private List<double> KVariables = new List<double>();
        private Boolean userChanged;

        private static MainWindow main
        {
            get { return GSApp.main; }
        }

        public string filename { get; set; }
        public CanvasProperty canvasProps { get; private set; }

        public Boolean UserChanged
        {
            get { return (userChanged || graphGUIL.userChanged || graphGUIK.userChanged || graphGUIR.userChanged); }
            set { userChanged = graphGUIL.userChanged = graphGUIK.userChanged = graphGUIR.userChanged = value; }
        }

        public RuleDisplay graphGUIL
        {
            get { return graphCanvasL; }
        }

        public RuleDisplay graphGUIK
        {
            get { return graphCanvasK; }
        }

        public RuleDisplay graphGUIR
        {
            get { return graphCanvasR; }
        }

        public grammarRule rule { get; private set; }

        #endregion

        #region Constructor

        public ruleWindow()
            : this(new grammarRule())
        {
        }

        public ruleWindow(grammarRule gr)
            : this(gr, new CanvasProperty())
        {
        }

        public ruleWindow(CanvasProperty canvasProperties)
            : this(new grammarRule(), canvasProperties)
        {
        }

        public ruleWindow(grammarRule gr, CanvasProperty canvasProperties)
            : this(gr, canvasProperties, "Untitled")
        {
        }

        public ruleWindow(grammarRule gr, string filename)
            : this(gr, new CanvasProperty(), filename)
        {
        }

        public ruleWindow(grammarRule gr, CanvasProperty canvasProperties,
                          string filename)
            : this(gr, canvasProperties, filename, Path.GetFileNameWithoutExtension(filename))
        {
        }

        public ruleWindow(grammarRule gr, CanvasProperty canvasProperties,
                          string filename, string title)
        {
            /* the following is common to all GS window types. */
            InitializeComponent();
            Owner = main;
            graphCanvasK.ScrollOwner = scrollViewerK;
            graphCanvasL.ScrollOwner = scrollViewerL;
            graphCanvasR.ScrollOwner = scrollViewerR;
            ShowInTaskbar = false;
            foreach (CommandBinding cb in main.CommandBindings)
            {
                CommandBindings.Add(cb);
                graphCanvasK.CommandBindings.Add(cb);
                graphCanvasL.CommandBindings.Add(cb);
                graphCanvasR.CommandBindings.Add(cb);
            }
            foreach (InputBinding ib in main.InputBindings)
            {
                InputBindings.Add(ib);
                graphCanvasK.InputBindings.Add(ib);
                graphCanvasL.InputBindings.Add(ib);
                graphCanvasR.InputBindings.Add(ib);
            }
            /***************************************************/

            rule = gr;
            this.filename = !string.IsNullOrEmpty(filename) ? filename : "Untitled";
            Title = !string.IsNullOrEmpty(title) ? title : Path.GetFileNameWithoutExtension(this.filename);

            canvasProps = canvasProperties;
            canvasProps.AddGUIToControl(graphCanvasK);
            canvasProps.AddGUIToControl(graphCanvasL);
            canvasProps.AddGUIToControl(graphCanvasR);
            AdoptWindowWideCanvasProperties();

            InitDrawRule();
        }

        public void AdoptWindowWideCanvasProperties()
        {
            var border = 0.2;
            lblLLabels.FontSize = lblLVariables.FontSize
                                  = txtLGlobalLabels.FontSize = txtLGlobalVariables.FontSize
                                                                = canvasProps.GlobalTextSize;
            lblKLabels.FontSize = lblKVariables.FontSize
                                  = txtKGlobalLabels.FontSize = txtKGlobalVariables.FontSize
                                                                = canvasProps.GlobalTextSize;
            lblRLabels.FontSize = lblRVariables.FontSize
                                  = txtRGlobalLabels.FontSize = txtRGlobalVariables.FontSize
                                                                = canvasProps.GlobalTextSize;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = Math.Max(canvasProps.WindowLeft, SystemParameters.VirtualScreenLeft);
            Top = Math.Max(canvasProps.WindowTop, SystemParameters.VirtualScreenTop);

            Height = graphCanvasL.Height = graphCanvasK.Height =
                                           graphCanvasR.Height = canvasProps.CanvasHeight;
            graphCanvasL.Width = canvasProps.CanvasWidth.Left;
            mainGrid.ColumnDefinitions[0].Width = new GridLength(canvasProps.CanvasWidth.Left);
            graphCanvasK.Width = canvasProps.CanvasWidth.Top;
            mainGrid.ColumnDefinitions[2].Width = new GridLength(canvasProps.CanvasWidth.Top);
            graphCanvasR.Width = canvasProps.CanvasWidth.Right;
            // this is commented out s.t. the R graph will just fill the remaining area.
            //mainGrid.ColumnDefinitions[4].Width = new GridLength(canvasProps.CanvasWidth.Right);
            Width = canvasProps.CanvasWidth.Bottom;
        }

        #endregion

        #region Window-wide Events

        /// <summary>
        ///   This method handles adding the nodes to graph when shortcut keys 1,2,3.... etc are pressed.
        /// </summary>
        /// <param name = "e">has key pressed info</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            try
            {
                // this is to add a new node if the keys pressed are 1,2,3....
                if (e.Source == this)
                    if (graphCanvasL.IsMouseOver)
                    {
                        var p = Mouse.GetPosition(graphCanvasL);
                        graphCanvasL.HandleKeyboardShortcuts(e.Key, p, e.Source);
                    }
                    else if (graphCanvasK.IsMouseOver)
                    {
                        var p = Mouse.GetPosition(graphCanvasK);
                        graphCanvasK.HandleKeyboardShortcuts(e.Key, p, e.Source);
                    }
                    else if (graphCanvasR.IsMouseOver)
                    {
                        var p = Mouse.GetPosition(graphCanvasR);
                        graphCanvasR.HandleKeyboardShortcuts(e.Key, p, e.Source);
                    }
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (UserChanged)
            {
                var save =
                    MessageBox.Show("Do you want to save changes to " + Title + " before closing?",
                                    "GraphSynth: Save Changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question,
                                    MessageBoxResult.Cancel);
                if (save == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (save == MessageBoxResult.Yes)
                    main.SaveOnExecuted(null, null);
            }
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            // graphCanvasL.SynchronizeShapeAndStringDescription();
            //  graphCanvasR.SynchronizeShapeAndStringDescription();
            base.OnClosed(e);
        }

        /// <summary>
        ///   This is a crucial event. This method sets the rule, canvas and other properties that need to be 
        ///   shown in property tab. If this method is not called for some reason. The properties shown may not
        ///   coresspond to the open rule.
        /// </summary>
        /// <param name = "sender"></param>
        /// <param name = "e"></param>
        private void BecomeActiveSubWindow(object sender, EventArgs e)
        {
            main.windowsMgr.SetAsActive(this);
            if (graphCanvasL.IsMouseOver)
                main.windowsMgr.SetActiveGraphCanvas(graphCanvasL);
            else if (graphCanvasR.IsMouseOver)
                main.windowsMgr.SetActiveGraphCanvas(graphCanvasR);
            else main.windowsMgr.SetActiveGraphCanvas(graphCanvasK);
        }


        private void GridSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Window_SizeChanged(sender, null);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            graphCanvasL.RedrawResizeAndReposition();
            graphCanvasK.RedrawResizeAndReposition();
            graphCanvasR.RedrawResizeAndReposition();
            canvasProps.CanvasHeight = Height;
            canvasProps.CanvasWidth = new Thickness(mainGrid.ColumnDefinitions[0].Width.Value,
                                                    mainGrid.ColumnDefinitions[2].Width.Value,
                                                    Width - mainGrid.ColumnDefinitions[0].Width.Value
                                                    - mainGrid.ColumnDefinitions[1].Width.Value -
                                                    mainGrid.ColumnDefinitions[2].Width.Value
                                                    - mainGrid.ColumnDefinitions[3].Width.Value,
                                                    Width);
        }


        private void LLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var totalW = Width;
            mainGrid.ColumnDefinitions[0].Width = new GridLength(0.8 * totalW);
            mainGrid.ColumnDefinitions[2].Width = new GridLength(0.1 * totalW);
            Window_SizeChanged(sender, null);
        }

        private void KLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var totalW = Width;
            mainGrid.ColumnDefinitions[0].Width = new GridLength(0.1 * totalW);
            mainGrid.ColumnDefinitions[2].Width = new GridLength(0.8 * totalW);
            Window_SizeChanged(sender, null);
        }

        private void RLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var totalW = Width;
            mainGrid.ColumnDefinitions[0].Width = new GridLength(0.1 * totalW);
            mainGrid.ColumnDefinitions[2].Width = new GridLength(0.1 * totalW);
            Window_SizeChanged(sender, null);
        }

        private void StackPanel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var fontSize = lblLLabels.FontSize;
            if (e.Delta > 0) fontSize *= 1.05;
            else if (e.Delta < 0) fontSize /= 1.05;
            lblLLabels.FontSize = lblLVariables.FontSize
                                  = txtLGlobalLabels.FontSize = txtLGlobalVariables.FontSize = fontSize;
            lblKLabels.FontSize = lblKVariables.FontSize
                                  = txtKGlobalLabels.FontSize = txtKGlobalVariables.FontSize = fontSize;
            lblRLabels.FontSize = lblRVariables.FontSize
                                  = txtRGlobalLabels.FontSize = txtRGlobalVariables.FontSize = fontSize;
        }
        #endregion

        #region Labels and Variables Textbox Events

        /* L Changes to Labels and Variables */

        private void txtLGlobalLabels_LostFocus(object sender, RoutedEventArgs e)
        {
            /* there is an added difficulty here since the negating labels occupies
             * the same textbox */
            var setOfLabels = txtLGlobalLabels.Text.Split(new[] { "~(" },
                                                          2, StringSplitOptions.RemoveEmptyEntries);
            if (setOfLabels.Any())
                main.property.RulePrpt.txtLGlobalLabels_LostFocus(new TextBox { Text = setOfLabels[0] }, e);
            if (setOfLabels.Count() > 1)
                main.property.RulePrpt.txtLNegatingLabels_LostFocus(new TextBox { Text = setOfLabels[1] }, e);
        }

        private void txtLGlobalLabels_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalString(e))
            {
                var caretIndex = txtLGlobalLabels.CaretIndex;
                var origLength = txtLGlobalLabels.Text.Length;
                txtLGlobalLabels_LostFocus(sender, e);
                TextBoxHelper.SetCaret(txtLGlobalLabels, caretIndex, origLength);
            }
        }

        private void txtLGlobalVariables_LostFocus(object sender, RoutedEventArgs e)
        {
            main.property.RulePrpt.txtLVariables_LostFocus(sender, e);
        }

        private void txtLGlobalVariables_KeyUp(object sender, KeyEventArgs e)
        {
            main.property.RulePrpt.txtLVariables_KeyUp(sender, e);
        }


        /* K Changes to Labels and Variables */

        private void txtKGlobalLabels_LostFocus(object sender, RoutedEventArgs e)
        {
            var labels = StringCollectionConverter.Convert(txtKGlobalLabels.Text);
            var removedKLabels = KLabels.Where(a => !labels.Contains(a)).ToList();
            foreach (string a in removedKLabels)
            {
                rule.L.globalLabels.Remove(a);
                rule.R.globalLabels.Remove(a);
            }
            KLabels = labels;
            var union = KLabels.Union(rule.L.globalLabels);
            main.property.RulePrpt.txtLGlobalLabels_LostFocus(new TextBox
                                                                  {
                                                                      Text =
                                                                          StringCollectionConverter.Convert(
                                                                              new List<string>(union))
                                                                  }, null);

            union = KLabels.Union(rule.R.globalLabels);
            main.property.RulePrpt.txtRGlobalLabels_LostFocus(new TextBox
                                                                  {
                                                                      Text =
                                                                          StringCollectionConverter.Convert(
                                                                              new List<string>(union))
                                                                  }, null);
        }

        private void txtKGlobalLabels_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalString(e))
            {
                var caretIndex = txtKGlobalLabels.CaretIndex;
                var origLength = txtKGlobalLabels.Text.Length;
                //List<string> labels = StringCollectionConverter.convert(txtKGlobalLabels.Text);
                //txtKGlobalLabels.Text = StringCollectionConverter.convert(labels);
                txtKGlobalLabels_LostFocus(sender, e);
                TextBoxHelper.SetCaret(txtKGlobalLabels, caretIndex, origLength);
            }
        }

        private void txtKGlobalVariables_LostFocus(object sender, RoutedEventArgs e)
        {
            var vars = DoubleCollectionConverter.Convert(txtKGlobalVariables.Text);
            var removedKVars = KVariables.Where(a => !vars.Contains(a)).ToList();
            foreach (double a in removedKVars)
            {
                rule.L.globalVariables.Remove(a);
                rule.R.globalVariables.Remove(a);
            }
            KVariables = vars;

            var union = KVariables.Union(rule.L.globalVariables);
            txtLGlobalVariables_LostFocus(new TextBox
                                              {
                                                  Text = DoubleCollectionConverter.Convert(new List<double>(union))
                                              }, null);

            union = KVariables.Union(rule.R.globalVariables);
            txtRGlobalVariables_LostFocus(new TextBox
                                              {
                                                  Text = DoubleCollectionConverter.Convert(new List<double>(union))
                                              }, null);
        }

        private void txtKGlobalVariables_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalNumber(txtKGlobalVariables, e))
            {
                var caretIndex = txtKGlobalVariables.CaretIndex;
                var origLength = txtKGlobalVariables.Text.Length;
                //List<double> vals = DoubleCollectionConverter.convert(txtKGlobalVariables.Text);
                //txtKGlobalVariables.Text = DoubleCollectionConverter.convert(vals);
                txtKGlobalVariables_LostFocus(sender, e);
                TextBoxHelper.SetCaret(txtKGlobalVariables, caretIndex, origLength);
            }
        }


        /* R Changes to Labels and Variables */

        private void txtRGlobalLabels_LostFocus(object sender, RoutedEventArgs e)
        {
            main.property.RulePrpt.txtRGlobalLabels_LostFocus(sender, e);
        }

        private void txtRGlobalLabels_KeyUp(object sender, KeyEventArgs e)
        {
            main.property.RulePrpt.txtRGlobalLabels_KeyUp(sender, e);
        }

        private void txtRGlobalVariables_LostFocus(object sender, RoutedEventArgs e)
        {
            main.property.RulePrpt.txtRVariables_LostFocus(sender, e);
        }

        private void txtRGlobalVariables_KeyUp(object sender, KeyEventArgs e)
        {
            main.property.RulePrpt.txtRVariables_KeyUp(sender, e);
        }

        #endregion

        #region Methods

        private void InitDrawRule()
        {
            try
            {
                //  graphCanvasK.rW = graphCanvasL.rW = graphCanvasR.rW = this;

                if (rule.L == null) rule.L = new designGraph();
                graphCanvasL.graph = rule.L;
                graphCanvasL.InitDrawGraph();
                graphCanvasL.RedrawResizeAndReposition();

                if (rule.R == null) rule.R = new designGraph();
                graphCanvasR.graph = rule.R;
                graphCanvasR.InitDrawGraph();
                graphCanvasR.RedrawResizeAndReposition();

                graphCanvasK.graph = new designGraph();
                initDrawKGraph();
                // this method extracts the common node to the K graph from L and R graph
                graphCanvasK.RedrawResizeAndReposition();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private void initDrawKGraph()
        {
            try
            {
                #region nodes

                // loop through all the nodes in L graph
                for (var i = 0; i < rule.L.nodes.Count; i++)
                {
                    if (rule.R.nodes.Exists(b => (b.name == rule.L.nodes[i].name)))
                    {
                        // matched node will be copied to K
                        var tempNode = (ruleNode) (rule.L.nodes[i]).copy();
                        graphCanvasK.graph.addNode(tempNode);
                        graphCanvasK.addNodeShape(tempNode);
                    }
                }

                #endregion

                #region arcs

                // loop through all the arcs in L graph
                for (var i = 0; i < rule.L.arcs.Count; i++)
                {
                    if (rule.R.arcs.Exists(b => (b.name == rule.L.arcs[i].name)))
                    {
                        var Karc = (ruleArc) (rule.L.arcs[i]).copy();

                        //add it to K graph
                        if (rule.L.arcs[i].From != null)
                            Karc.From = graphCanvasK.graph.nodes.FirstOrDefault
                                (n => n.name == rule.L.arcs[i].From.name);
                        else Karc.From = null;
                        if (rule.L.arcs[i].To != null)
                            Karc.To = graphCanvasK.graph.nodes.FirstOrDefault
                                (n => n.name == rule.L.arcs[i].To.name);
                        else Karc.To = null;

                        graphCanvasK.graph.addArc(Karc, Karc.From, Karc.To);
                        graphCanvasK.AddArcShape(Karc);
                        graphCanvasK.SetUpNewArcShape(Karc);
                    }
                }

                #endregion

                #region hyperarcs

                foreach (var Lha in rule.L.hyperarcs)
                {
                    var Rha = rule.R.hyperarcs.FirstOrDefault(b => (b.name == Lha.name));
                    if (Rha != null)
                    {
                        var Kha = (ruleHyperarc) (Lha).copy();
                        var attachedNodeNames = Lha.nodes.Select(a => a.name);
                        attachedNodeNames = attachedNodeNames.Intersect(Rha.nodes.Select(a => a.name));
                        var attachedNodes = attachedNodeNames.Select(name => (node) graphCanvasK.graph[name]).ToList();

                        // add it to K graph
                        graphCanvasK.graph.addHyperArc(Kha, attachedNodes);
                        graphCanvasK.AddHyperArcShape(Kha);
                    }
                }
                #endregion
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        #endregion

        /* this is used by both the global label and variable textboxes */

        private void txtGlobal_TextChanged(object sender, TextChangedEventArgs e)
        {
            userChanged = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            canvasProps.ViewValueChanged(null, null);
          }

        private void GraphCanvasL_OnLostFocus(object sender, RoutedEventArgs e)
        {
            Console.Write("lostFocus ");
            graphGUIL.OnLostFocusPublic(e);
        }

    }
}