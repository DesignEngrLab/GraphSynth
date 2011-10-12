using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for graphWindow.xaml
    /// </summary>
    public partial class graphWindow : Window
    {
        #region Properties

        private Boolean userChanged;

        private static MainWindow main
        {
            get { return GSApp.main; }
        }

        public designGraph graph { get; private set; }
        public string filename { get;  set; }
        public CanvasProperty canvasProps { get; private set; }

        public Boolean UserChanged
        {
            get { return (userChanged || graphGUI.userChanged); }
            set { userChanged = graphGUI.userChanged = value; }
        }

        #endregion

        #region Constructor

        /* this first one is still used for opening candidates */

        public graphWindow()
            : this(new designGraph())
        {
        }

        public graphWindow(designGraph dg)
            : this(dg, new CanvasProperty())
        {
        }

        public graphWindow(designGraph dg, string filename)
            : this(dg, new CanvasProperty(), filename)
        {
        }

        public graphWindow(CanvasProperty canvasProperties)
            : this(new designGraph(), canvasProperties)
        {
        }

        public graphWindow(designGraph dg, CanvasProperty canvasProperties)
            : this(dg, canvasProperties, "Untitled")
        {
        }

        public graphWindow(designGraph dg, CanvasProperty canvasProperties,
                           string filename)
            : this(dg, canvasProperties, filename, Path.GetFileNameWithoutExtension(filename))
        {
        }


        public graphWindow(designGraph dg, CanvasProperty canvasProperties,
                           string filename, string title)
        {
            /* the following is common to all GS window types. */
            InitializeComponent();
            Owner = main;
            ShowInTaskbar = false;
            foreach (CommandBinding cb in main.CommandBindings)
            {
                CommandBindings.Add(cb);
                graphGUI.CommandBindings.Add(cb);
            }
            foreach (InputBinding ib in main.InputBindings)
            {
                InputBindings.Add(ib);
                graphGUI.InputBindings.Add(ib);
            }
            /***************************************************/

            graph = graphGUI.graph = dg;
            graphGUI.ScrollOwner = scrollViewer1;

            canvasProps = canvasProperties ?? new CanvasProperty();
            canvasProps.AddGUIToControl(graphGUI);
            AdoptWindowWideCanvasProperties();

            this.filename = !string.IsNullOrEmpty(filename) ? filename : "Untitled";
            Title = !string.IsNullOrEmpty(title) ? title : Path.GetFileNameWithoutExtension(this.filename);

            graphGUI.InitDrawGraph();

            txtGlobalVariables.Text = DoubleCollectionConverter.convert(graph.globalVariables);
            txtGlobalLabels.Text = StringCollectionConverter.convert(graph.globalLabels);
        }

        public void AdoptWindowWideCanvasProperties()
        {
            Height = canvasProps.CanvasHeight;
            Width = canvasProps.CanvasWidth.Left;
            lblLabels.FontSize = lblVariables.FontSize
                                 = txtGlobalLabels.FontSize = txtGlobalVariables.FontSize
                                                              = canvasProps.GlobalTextSize;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = canvasProps.WindowLeft;
            Top = canvasProps.WindowTop;
        }

        #endregion

        #region Window-wide Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            canvasProps.ViewValueChanged(null, null);
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (UserChanged)
            {
                var save =
                    MessageBox.Show("Do you want to save changes to " + Title + " before closing?",
                                    "GraphSynth: Save Changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question,
                                    MessageBoxResult.No);
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
            // graphGUI.SynchronizeShapeAndStringDescription();
            graphGUI.ClearShapeBanks();
            base.OnClosed(e);
        }

        private void BecomeActiveSubWindow(object sender, EventArgs e)
        {
            main.windowsMgr.SetAsActive(this);
            main.windowsMgr.SetActiveGraphCanvas(graphGUI);
        }

        protected void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            graphGUI.RedrawResizeAndReposition();
            canvasProps.CanvasHeight = Height;
            canvasProps.CanvasWidth = new Thickness(Width);
        }

        private void GlobalText_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var fontSize = lblLabels.FontSize;
            if (e.Delta > 0) fontSize *= 1.05;
            else if (e.Delta < 0) fontSize /= 1.05;
            lblLabels.FontSize = lblVariables.FontSize
                                 = txtGlobalLabels.FontSize = txtGlobalVariables.FontSize
                                                              = canvasProps.GlobalTextSize = fontSize;
        }

        #endregion

        #region Labels and Variables Textbox Events

        /* this is used by both the global label and variable textboxes */

        private void txtGlobal_TextChanged(object sender, TextChangedEventArgs e)
        {
            userChanged = true;
        }

        private void txtGlobalLabels_LostFocus(object sender, RoutedEventArgs e)
        {
            main.property.GraphPrpt.txtGlobalLabels_LostFocus(sender, e);
        }

        private void txtGlobalLabels_KeyUp(object sender, KeyEventArgs e)
        {
            main.property.GraphPrpt.txtGlobalLabels_KeyUp(sender, e);
        }

        private void txtGlobalVariables_LostFocus(object sender, RoutedEventArgs e)
        {
            main.property.GraphPrpt.txtVariables_LostFocus(sender, e);
        }

        private void txtGlobalVariables_KeyUp(object sender, KeyEventArgs e)
        {
            main.property.GraphPrpt.txtVariables_KeyUp(sender, e);
        }

        #endregion

    }
}