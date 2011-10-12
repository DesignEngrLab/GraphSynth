using System.Windows;

namespace GraphSynth.GraphDisplay
{
    /// <summary>
    ///   Interaction logic for KDeleteDialog.xaml
    /// </summary>
    public partial class KDeleteDialog : Window
    {
        public int result = -1;

        public KDeleteDialog()
        {
            InitializeComponent();
        }

        private void OKbutton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool) radioButtonLKR.IsChecked) result = 0;
            else if ((bool) radioButtonLK.IsChecked) result = 1;
            else if ((bool) radioButtonKR.IsChecked) result = 2;
            else return;
            Close();
        }

        private void Cancelbutton_Click(object sender, RoutedEventArgs e)
        {
            result = -1;
            Close();
        }
    }
}