using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for RuleSetProperty.xaml
    /// </summary>
    public partial class RuleSetProperty : UserControl
    {
        private ruleSet SelectedRuleSet;

        public RuleSetProperty()
        {
            InitializeComponent();

            PopulateChoiceMethodComboboxes(cmdChoiceMethod);

            PopulateCandidatesComboboxes(cmdInterimCandidates);
            PopulateCandidatesComboboxes(cmdFinalCandidates);

            PopulateNextGenerationComboboxes(cmdAfterNormalCycle);
            PopulateNextGenerationComboboxes(cmdChoiceSendsStop);
            PopulateNextGenerationComboboxes(cmdNoOfCallsReached);
            PopulateNextGenerationComboboxes(cmdNoRulesRecognized);
            PopulateNextGenerationComboboxes(cmdTriggerRuleInvoked);
        }

        private void PopulateNextGenerationComboboxes(ComboBox cmbBox)
        {
            for (var i = -5; i <= 10; i++)
                cmbBox.Items.Add((nextGenerationSteps)i);
        }

        private void PopulateCandidatesComboboxes(ComboBox cmbBox)
        {
            for (var i = 0; i < 3; i++)
                cmbBox.Items.Add((feasibilityState)i);
        }

        private void PopulateChoiceMethodComboboxes(ComboBox cmbBox)
        {
            cmbBox.Items.Add(choiceMethods.Design);
            cmbBox.Items.Add(choiceMethods.Automatic);
        }

        private void cmdChoiceMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedRuleSet.choiceMethod = (choiceMethods)cmdChoiceMethod.SelectedItem;
        }

        private void cmdInterimCandidates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedRuleSet.interimCandidates = (feasibilityState)cmdInterimCandidates.SelectedItem;
        }

        private void cmdFinalCandidates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedRuleSet.finalCandidates = (feasibilityState)cmdFinalCandidates.SelectedItem;
        }

        private void cmdAfterNormalCycle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedRuleSet.generationAfterNormal = (nextGenerationSteps)cmdAfterNormalCycle.SelectedItem;
        }

        private void cmdChoiceSendsStop_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedRuleSet.generationAfterChoice = (nextGenerationSteps)cmdChoiceSendsStop.SelectedItem;
        }

        private void cmdNoOfCallsReached_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedRuleSet.generationAfterCycleLimit = (nextGenerationSteps)cmdNoOfCallsReached.SelectedItem;
        }

        private void cmdNoRulesRecognized_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedRuleSet.generationAfterNoRules = (nextGenerationSteps)cmdNoRulesRecognized.SelectedItem;
        }

        private void cmdTriggerRuleInvoked_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedRuleSet.generationAfterTriggerRule = (nextGenerationSteps)cmdTriggerRuleInvoked.SelectedItem;
        }

        private void txtTriggerRuleNo_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalNumber((TextBox)sender, e))
                txtTriggerRuleNo_LostFocus(sender, e);
        }

        private void txtTriggerRuleNo_LostFocus(object sender, RoutedEventArgs e)
        {
            int TriggRuleNo;
            if (int.TryParse(txtTriggerRuleNo.Text, out TriggRuleNo))
                SelectedRuleSet.TriggerRuleNum = TriggRuleNo;
            else SelectedRuleSet.TriggerRuleNum = -1;
            txtTriggerRuleNo.Text = TriggRuleNo.ToString();
        }



        private void txtApplySourceFiles_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Space) || (e.Key == Key.OemComma) || (e.Key == Key.Enter) || (e.Key == Key.Return)
                || (e.Key == Key.OemComma))
                txtApplySourceFiles_LostFocus(sender, e);
        }

        private void txtApplySourceFiles_LostFocus(object sender, RoutedEventArgs e)
        {
            var lststr = StringCollectionConverter.convert(txtApplySourceFiles.Text);
            SelectedRuleSet.applySourceFiles.Clear();
            foreach (string s in lststr)
                RuleParamCodeFiler.checkForRuleFile(SelectedRuleSet,
                                                    SelectedRuleSet.applySourceFiles, s);
            txtApplySourceFiles.Text = StringCollectionConverter.convert(SelectedRuleSet.applySourceFiles);
        }

        private void txtRecognizeSourceFiles_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Space) || (e.Key == Key.OemComma) || (e.Key == Key.Enter) || (e.Key == Key.Return)
                || (e.Key == Key.OemComma))
                txtRecognizeSourceFiles_LostFocus(sender, e);
        }

        private void txtRecognizeSourceFiles_LostFocus(object sender, RoutedEventArgs e)
        {
            var lststr = StringCollectionConverter.convert(txtRecognizeSourceFiles.Text);
            SelectedRuleSet.recognizeSourceFiles.Clear();
            foreach (string s in lststr)
                RuleParamCodeFiler.checkForRuleFile(SelectedRuleSet,
                                                    SelectedRuleSet.recognizeSourceFiles, s);
            txtRecognizeSourceFiles.Text = StringCollectionConverter.convert(SelectedRuleSet.recognizeSourceFiles);
        }

        internal void Update(ruleSet RuleSet, ruleSetWindow rsW)
        {
            SelectedRuleSet = RuleSet;
            txtName.Text = rsW.Title;
            for (int i = 0; i < GSApp.settings.rulesets.GetLength(0); i++)
                if (GSApp.settings.rulesets[i].Equals(SelectedRuleSet))
                {
                    txtName.Text += " (set to RuleSet #" + i+")";
                    break;
                }
            
            txtTriggerRuleNo.Text = SelectedRuleSet.TriggerRuleNum.ToString();

            cmdChoiceMethod.SelectedIndex = cmdChoiceMethod.Items.IndexOf(SelectedRuleSet.choiceMethod);

            cmdInterimCandidates.SelectedIndex = cmdInterimCandidates.Items.IndexOf(SelectedRuleSet.interimCandidates);
            cmdFinalCandidates.SelectedIndex = cmdFinalCandidates.Items.IndexOf(SelectedRuleSet.finalCandidates);

            cmdAfterNormalCycle.SelectedIndex = cmdAfterNormalCycle.Items.IndexOf(SelectedRuleSet.generationAfterNormal);
            cmdChoiceSendsStop.SelectedIndex = cmdChoiceSendsStop.Items.IndexOf(SelectedRuleSet.generationAfterChoice);
            cmdNoOfCallsReached.SelectedIndex =
                cmdNoOfCallsReached.Items.IndexOf(SelectedRuleSet.generationAfterCycleLimit);
            cmdNoRulesRecognized.SelectedIndex =
                cmdNoRulesRecognized.Items.IndexOf(SelectedRuleSet.generationAfterNoRules);
            cmdTriggerRuleInvoked.SelectedIndex =
                cmdTriggerRuleInvoked.Items.IndexOf(SelectedRuleSet.generationAfterTriggerRule);
            txtRecognizeSourceFiles.Text = StringCollectionConverter.convert(SelectedRuleSet.recognizeSourceFiles);
            txtApplySourceFiles.Text = StringCollectionConverter.convert(SelectedRuleSet.applySourceFiles);
        }

    }
}