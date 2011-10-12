using System.Windows;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for CommentEditWindow.xaml
    /// </summary>
    public partial class CommentEditWindow : Window
    {
        public string comment;

        public CommentEditWindow()
        {
            InitializeComponent();
        }

        public static string ShowWindowDialog(string origComment)
        {
            var cew = new CommentEditWindow();
            cew.txtComment.Text = cew.comment = origComment;
            cew.ShowDialog();
            return cew.comment;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Enter_Button_Click(object sender, RoutedEventArgs e)
        {
            comment = txtComment.Text;
            Close();
        }
    }
}