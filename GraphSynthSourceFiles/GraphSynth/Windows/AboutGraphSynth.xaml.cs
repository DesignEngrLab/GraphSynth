using System;
using System.Windows;
using System.Windows.Input;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for AboutGraphSynth.xaml
    /// </summary>
    public partial class AboutGraphSynth : Window
    {
        public AboutGraphSynth(Boolean fromHelp)
        {
            InitializeComponent();
            if (fromHelp)
            {
                /* the following is common to all GS window types. */
                Owner = GSApp.main;
                ShowInTaskbar = false;
                foreach (CommandBinding cb in GSApp.main.CommandBindings)
                    CommandBindings.Add(cb);
                foreach (InputBinding ib in GSApp.main.InputBindings)
                    InputBindings.Add(ib);
                /***************************************************/
            }
            else
            {
                GSApp.console.outputBox = outputTextBox;
                outputTextBox.Visibility = Visibility.Visible;
                closeButton.Visibility = Visibility.Hidden;
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}