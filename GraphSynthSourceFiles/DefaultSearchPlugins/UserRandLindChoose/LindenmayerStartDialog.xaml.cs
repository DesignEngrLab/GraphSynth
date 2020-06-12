using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace GraphSynth.UserRandLindChoose
{
    /// <summary>
    ///   Interaction logic for LindenmayerStartDialog.xaml
    /// </summary>
    public partial class LindenmayerStartDialog : Window
    {
        private readonly int numRS;
        private readonly LindenmayerChooseProcess lindenmayerChooseProcess;


        public LindenmayerStartDialog(LindenmayerChooseProcess lCP)
        {
            InitializeComponent();
            lindenmayerChooseProcess = lCP;
            numRS = lindenmayerChooseProcess.settings.numOfRuleSets;
            checkBox1.IsChecked = lindenmayerChooseProcess.display;
            DisplayAndValidateNumCalls();
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            lindenmayerChooseProcess.display = true;
        }

        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            lindenmayerChooseProcess.display = false;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            if (DisplayAndValidateNumCalls())
            {
                lindenmayerChooseProcess.Cancel = false;
                Close();
            }
            else if (MessageBoxResult.Yes == MessageBox.Show("An invalid number of rules were provided in the " +
                                                             "dialog. Please correct. Would you like to reset to the default values?",
                                                             "Invalid number of rules provided.", MessageBoxButton.YesNo,
                                                             MessageBoxImage.Error, MessageBoxResult.Yes))
                buttonReset_Click(null, null);
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            lindenmayerChooseProcess.Cancel = true;
            Close();
        }


        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            var defValue = lindenmayerChooseProcess.settings.MaxRulesToApply;
            LindenmayerChooseProcess.numOfCalls = new int[numRS];
            for (var i = 0; i < numRS; i++)
                LindenmayerChooseProcess.numOfCalls[i] = defValue;
            DisplayAndValidateNumCalls();
        }

        private void tbNumCalls_LostFocus(object sender, RoutedEventArgs e)
        {
            var numbers = DoubleCollectionConverter.Convert(tbNumCalls.Text);
            LindenmayerChooseProcess.numOfCalls = new int[numbers.Count];
            for (var i = 0; i < numbers.Count; i++)
                LindenmayerChooseProcess.numOfCalls[i] = (int)numbers[i];
            DisplayAndValidateNumCalls();
        }

        private void tbNumCalls_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Tab) || (e.Key == Key.Enter) || (e.Key == Key.Return))
                tbNumCalls_LostFocus(sender, e);
        }


        private Boolean DisplayAndValidateNumCalls()
        {
            var result = true;
            if (LindenmayerChooseProcess.numOfCalls == null)
            {
                LindenmayerChooseProcess.numOfCalls = new int[numRS];
                buttonReset_Click(null, null);
                result = false;
            }
            else if (numRS == 0)
            {
                tbNumCalls.Text = "no rulesets";
                result = false;
            }
            else if (LindenmayerChooseProcess.numOfCalls.GetLength(0) == 0)
            {
                tbNumCalls.Text = "nan";
                result = false;
            }
            else
            {
                var numOfCallsStr = LindenmayerChooseProcess.numOfCalls[0].ToString(CultureInfo.InvariantCulture);

                for (var i = 1; i < numRS; i++)
                {
                    if (i >= LindenmayerChooseProcess.numOfCalls.GetLength(0))
                    {
                        numOfCallsStr += ", nan";
                        result = false;
                    }
                    else
                        numOfCallsStr += ", " + LindenmayerChooseProcess.numOfCalls[i];
                }
                tbNumCalls.Text = numOfCallsStr;
            }
            return result;
        }
    }
}