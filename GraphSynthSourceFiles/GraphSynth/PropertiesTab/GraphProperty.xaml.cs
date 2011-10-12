using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for GraphProperty.xaml
    /// </summary>
    public partial class GraphProperty : UserControl
    {
        private graphWindow graphWin;
        private designGraph selectedGraph;

        #region Events

        #region Global Labels

        public void txtGlobalLabels_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalString(e))
                txtGlobalLabels_LostFocus(sender, e);
        }

        public void txtGlobalLabels_LostFocus(object sender, RoutedEventArgs e)
        {
            var senderTextBox = (TextBox)sender;
            var caretIndex = senderTextBox.CaretIndex;
            var origLength = senderTextBox.Text.Length;
            var lststr = StringCollectionConverter.convert(senderTextBox.Text);
            selectedGraph.globalLabels.Clear();
            foreach (string str in lststr)
            {
                selectedGraph.globalLabels.Add(str);
            }
            Update();
            TextBoxHelper.SetCaret(senderTextBox, caretIndex, origLength);
        }

        #endregion

        #region Global Variables

        public void txtVariables_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalNumber((TextBox)sender, e))
                txtVariables_LostFocus(sender, e);
        }

        public void txtVariables_LostFocus(object sender, RoutedEventArgs e)
        {
            var senderTextBox = (TextBox)sender;
            var caretIndex = senderTextBox.CaretIndex;
            var origLength = senderTextBox.Text.Length;
            var lst = DoubleCollectionConverter.convert(senderTextBox.Text);
            selectedGraph.globalVariables.Clear();
            foreach (double d in lst)
            {
                selectedGraph.globalVariables.Add(d);
            }
            Update();
            TextBoxHelper.SetCaret(senderTextBox, caretIndex, origLength);
        }

        #endregion

        #endregion

        public GraphProperty()
        {
            InitializeComponent();
        }

        internal void Update(designGraph graph, graphWindow gW)
        {
            selectedGraph = graph;
            graphWin = gW;
            Update();
        }

        private void Update()
        {
            txtFilename.Text =  (string.IsNullOrWhiteSpace(graphWin.filename))
                ?  graphWin.Title: graphWin.filename;
            txtFilename.PageRight();

            if (GSApp.settings.seed == selectedGraph)
            {
                txtSeed.Text = "This is the current seed.";
                txtSeed.Height = double.NaN;
            }
            else
            {
                txtSeed.Text = "";
                txtSeed.Height = 0.0;
            }
            graphWin.txtGlobalVariables.Text = txtVariables.Text
                                               = DoubleCollectionConverter.convert(selectedGraph.globalVariables);
            graphWin.txtGlobalLabels.Text = txtGlobalLabels.Text
                                            = StringCollectionConverter.convert(selectedGraph.globalLabels);
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            var oldFilename = graphWin.filename;
            try
            {
                var ext = Path.GetExtension(txtFilename.Text);
                if (!ext.Equals(".gxml")) txtFilename.Text += ".gxml";
                var newFilename =graphWin.filename= txtFilename.Text;
                GSApp.main.SaveActiveWindow(false);
                graphWin.Title = Path.GetFileNameWithoutExtension(newFilename);
            }
            catch
            {
                txtFilename.Text = graphWin.filename = oldFilename;
                graphWin.Title = Path.GetFileNameWithoutExtension(oldFilename);
                
            }
        }

        private void txtFilename_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter) || (e.Key == Key.Return))
                saveBtn_Click(sender, e);
        }
    }
}