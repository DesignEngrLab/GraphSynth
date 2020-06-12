using GraphSynth.Representation;
using System;
using System.IO;
using System.Windows;

namespace GraphSynth.UserRandLindChoose
{
    /// <summary>
    ///   Interaction logic for SaveResultDialog.xaml
    /// </summary>
    public partial class SaveResultDialog : Window
    {
        private readonly BasicFiler _filer;
        private readonly candidate _c;
        public static void Show(BasicFiler filer, candidate c)
        {
            var diag = new SaveResultDialog(filer, c);
            diag.ShowDialog();
        }
        public SaveResultDialog(BasicFiler filer, candidate c)
        {
            _filer = filer;
            _c = c;
            InitializeComponent();
            var now = DateTime.Now;
            var timeString = "." + now.Year + "." + now.Month + "." + now.Day + "." + now.Hour + "." + now.Minute + "." + now.Second + "." + now.Millisecond;
            filenameTextBox.Text = "ResultFrom." + Path.GetFileNameWithoutExtension(c.graphFileName) + timeString;
            directoryText.Text = filer.outputDirectory;
        }

        private void buttonSaveAsGraph_Click_1(object sender, RoutedEventArgs e)
        {
            _filer.Save(_filer.outputDirectory + filenameTextBox.Text + ".gxml", _c.graph);
            Close();
        }

        private void buttonSaveAsCandidate_Click_1(object sender, RoutedEventArgs e)
        {
            _filer.Save(_filer.outputDirectory + filenameTextBox.Text + ".xml", _c);
            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}