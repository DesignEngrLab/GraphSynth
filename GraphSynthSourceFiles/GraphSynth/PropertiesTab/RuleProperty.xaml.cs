using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for GraphProperty.xaml
    /// </summary>
    public partial class RuleProperty : UserControl
    {
        private grammarRule rule;
        private ruleWindow ruleWin;
        
        #region Events

        #region Comment

        private void txtComment_KeyUp(object sender, KeyEventArgs e)
        {
            rule.comment = txtComment.Text;
        }

        private void txtComment_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((txtComment.Text.Length > 0) && (rule.comment != txtComment.Text))
                rule.comment = txtComment.Text;
            Update();
            txtComment.SelectionStart = 0;
        }

        private void btnComment_Click(object sender, RoutedEventArgs e)
        {
            txtComment.Text = rule.comment = CommentEditWindow.ShowWindowDialog(rule.comment);
        }

        #endregion

        #region Global Labels

        /* L Global Labels */

        public void txtLGlobalLabels_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalString(e))
                txtLGlobalLabels_LostFocus(sender, e);
        }

        public void txtLGlobalLabels_LostFocus(object sender, RoutedEventArgs e)
        {
            var senderTextBox = (TextBox)sender;
            var origLength = senderTextBox.Text.Length;
            var caretIndex = senderTextBox.CaretIndex;
            var lststr = StringCollectionConverter.convert(senderTextBox.Text);
            rule.L.globalLabels.Clear();
            foreach (string str in lststr)
            {
                rule.L.globalLabels.Add(str);
            }
            Update();
            TextBoxHelper.SetCaret(senderTextBox, caretIndex, origLength);
        }

        /* L Negating Global Labels */

        public void txtLNegatingLabels_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalString(e))
                txtLNegatingLabels_LostFocus(sender, e);
        }

        public void txtLNegatingLabels_LostFocus(object sender, RoutedEventArgs e)
        {
            var senderTextBox = (TextBox)sender;
            var caretIndex = senderTextBox.CaretIndex;
            var origLength = senderTextBox.Text.Length;
            var lststr = StringCollectionConverter.convert(senderTextBox.Text);
            rule.negateLabels.Clear();
            foreach (string str in lststr)
            {
                rule.negateLabels.Add(str);
            }
            Update();
            TextBoxHelper.SetCaret(senderTextBox, caretIndex, origLength);
        }

        /* R Global Labels */

        public void txtRGlobalLabels_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalString(e))
                txtRGlobalLabels_LostFocus(sender, e);
        }

        public void txtRGlobalLabels_LostFocus(object sender, RoutedEventArgs e)
        {
            var senderTextBox = (TextBox)sender;
            var caretIndex = senderTextBox.CaretIndex;
            var origLength = senderTextBox.Text.Length;
            var lststr = StringCollectionConverter.convert(senderTextBox.Text);
            rule.R.globalLabels.Clear();
            foreach (string str in lststr)
            {
                rule.R.globalLabels.Add(str);
            }
            Update();
            TextBoxHelper.SetCaret(senderTextBox, caretIndex, origLength);
        }

        #endregion

        #region Global Variables

        /* L Global Variables */

        public void txtLVariables_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalNumber((TextBox)sender, e))
                txtLVariables_LostFocus(sender, e);
        }

        public void txtLVariables_LostFocus(object sender, RoutedEventArgs e)
        {
            var senderTextBox = (TextBox)sender;
            var caretIndex = senderTextBox.CaretIndex;
            var origLength = senderTextBox.Text.Length;
            var lst = DoubleCollectionConverter.convert(senderTextBox.Text);
            rule.L.globalVariables.Clear();
            foreach (double d in lst)
            {
                rule.L.globalVariables.Add(d);
            }
            Update();
            TextBoxHelper.SetCaret(senderTextBox, caretIndex, origLength);
        }

        /* R Global Variables */

        public void txtRVariables_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalNumber((TextBox)sender, e))
                txtRVariables_LostFocus(sender, e);
        }

        public void txtRVariables_LostFocus(object sender, RoutedEventArgs e)
        {
            var senderTextBox = (TextBox)sender;
            var caretIndex = senderTextBox.CaretIndex;
            var origLength = senderTextBox.Text.Length;
            var lst = DoubleCollectionConverter.convert(senderTextBox.Text);
            rule.R.globalVariables.Clear();
            foreach (double d in lst)
            {
                rule.R.globalVariables.Add(d);
            }
            Update();
            TextBoxHelper.SetCaret(senderTextBox, caretIndex, origLength);
        }

        #endregion

        #region Extra Function Methods

        private void txtRecognizeFunctions_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Space) || (e.Key == Key.OemComma) || (e.Key == Key.Enter) || (e.Key == Key.Return)
                || (e.Key == Key.OemComma))
                txtRecognizeFunctions_LostFocus(sender, e);
        }

        private void txtRecognizeFunctions_LostFocus(object sender, RoutedEventArgs e)
        {
            var senderTextBox = (TextBox)sender;
            var caretIndex = senderTextBox.CaretIndex;
            var origLength = senderTextBox.Text.Length;
            var lststr = StringCollectionConverter.convert(senderTextBox.Text);
            rule.recognizeFunctions.Clear();
            foreach (string str in lststr)
            {
                rule.recognizeFunctions.Add(str);
            }
            RuleParamCodeFiler.checkForFunctions(true, rule, rule.recognizeFunctions);
            Update();
            senderTextBox.Text += " ";
            TextBoxHelper.SetCaret(senderTextBox, caretIndex, origLength);
        }

        private void txtApplyFunctions_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Space) || (e.Key == Key.OemComma) || (e.Key == Key.Enter) || (e.Key == Key.Return)
                || (e.Key == Key.OemComma))
                txtApplyFunctions_LostFocus(sender, e);
        }

        private void txtApplyFunctions_LostFocus(object sender, RoutedEventArgs e)
        {
            var senderTextBox = (TextBox)sender;
            var caretIndex = senderTextBox.CaretIndex;
            var origLength = senderTextBox.Text.Length;
            var lststr = StringCollectionConverter.convert(senderTextBox.Text);
            rule.applyFunctions.Clear();
            foreach (string str in lststr)
            {
                rule.applyFunctions.Add(str);
            }
            RuleParamCodeFiler.checkForFunctions(false, rule, rule.applyFunctions);
            Update();
            senderTextBox.Text += " ";
            TextBoxHelper.SetCaret(senderTextBox, caretIndex, origLength);
        }

        #endregion

        #region CheckBoxes

        private void chkInduced_Checked(object sender, RoutedEventArgs e)
        {
            rule.induced = true;
        }

        private void chkInduced_Unchecked(object sender, RoutedEventArgs e)
        {
            rule.induced = false;
        }

        private void chkSpanning_Checked(object sender, RoutedEventArgs e)
        {
            rule.spanning = true;
        }

        private void chkSpanning_Unchecked(object sender, RoutedEventArgs e)
        {
            rule.spanning = false;
        }

        private void chkContainsAllGlobalLabels_Checked(object sender, RoutedEventArgs e)
        {
            rule.containsAllGlobalLabels = true;
        }

        private void chkContainsAllGlobalLabels_Unchecked(object sender, RoutedEventArgs e)
        {
            rule.containsAllGlobalLabels = false;
        }

        private void chkOrderedGlobalLabels_Checked(object sender, RoutedEventArgs e)
        {
            rule.OrderedGlobalLabels = true;
            Update();
        }

        private void chkOrderedGlobalLabels_Unchecked(object sender, RoutedEventArgs e)
        {
            rule.OrderedGlobalLabels = false;
            Update();
        }

        private void chkTerminationRule_Checked(object sender, RoutedEventArgs e)
        {
            rule.termination = true;
            Update();
        }

        private void chkTerminationRule_Unchecked(object sender, RoutedEventArgs e)
        {
            rule.termination = false;
            Update();
        }

        #endregion

        #endregion

        public RuleProperty()
        {
            InitializeComponent();
        }

        internal void Update(grammarRule rule, ruleWindow rW)
        {
            this.rule = rule;
            ruleWin = rW;
            ShapeRulePrpt.rule = rule;
            Update();
        }

        private void Update()
        {
            txtFilename.Text = (string.IsNullOrWhiteSpace(ruleWin.filename))
                ? ruleWin.Title : ruleWin.filename;
            txtFilename.PageRight();
            /* updating L, K, and R global variables. */
            ruleWin.txtLGlobalVariables.Text = txtLVariables.Text
                                               = DoubleCollectionConverter.convert(rule.L.globalVariables);
            ruleWin.txtRGlobalVariables.Text = txtRVariables.Text
                                               = DoubleCollectionConverter.convert(rule.R.globalVariables);
            var KVars = rule.R.globalVariables.Intersect(rule.L.globalVariables);
            var listKVars = new List<double>();
            foreach (double x in KVars) listKVars.Add(x);
            ruleWin.txtKGlobalVariables.Text = DoubleCollectionConverter.convert(listKVars);

            /* updating L, K, and R global labels. This involves an additional need to consider both order labels and
             * negating labels. */
            ruleWin.txtLGlobalLabels.Text = txtLGlobalLabels.Text
                                            = StringCollectionConverter.convert(rule.L.globalLabels);
            ruleWin.txtRGlobalLabels.Text = txtRGlobalLabels.Text
                                            = StringCollectionConverter.convert(rule.R.globalLabels);
            /* the idea below was to change the way orderLabels appear but it lead to problems when switching
             * back and forth between ordered and unordered. It is repairable but truthfully, most rules do not have
             * a lot of labels. The host may build up a substantial amount, so this concept may be more appropriate
             * to graphDisplay and graph properties than rules. */
            //if (rule.OrderedGlobalLabels)
            //{
            //    ruleWin.txtLGlobalLabels.Text = txtLGlobalLabels.Text
            //        = txtLGlobalLabels.Text.Replace(", ", "-");
            //    ruleWin.txtRGlobalLabels.Text = txtRGlobalLabels.Text
            //        = txtRGlobalLabels.Text.Replace(", ", "-");
            //    ruleWin.txtKGlobalLabels.Text = "";
            //}
            //else
            //{
            var listKLabels = new List<string>(rule.R.globalLabels.Intersect(rule.L.globalLabels));
            ruleWin.txtKGlobalLabels.Text = StringCollectionConverter.convert(listKLabels);
            //}
            txtLNegatingLabels.Text = StringCollectionConverter.convert(rule.negateLabels);
            if (txtLNegatingLabels.Text.Length > 0)
                ruleWin.txtLGlobalLabels.Text += " ~(" + txtLNegatingLabels.Text + ")";
            txtApplyFunctions.Text = StringCollectionConverter.convert(rule.applyFunctions);
            txtRecognizeFunctions.Text = StringCollectionConverter.convert(rule.recognizeFunctions);
            chkContainsAllGlobalLabels.IsChecked = rule.containsAllGlobalLabels;
            chkInduced.IsChecked = rule.induced;
            chkOrderedGlobalLabels.IsChecked = rule.OrderedGlobalLabels;
            chkSpanning.IsChecked = rule.spanning;
            chkTerminationRule.IsChecked = rule.termination;
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            var oldFilename = ruleWin.filename;
            try
            {
                var ext = Path.GetExtension(txtFilename.Text);
                if (!ext.Equals(".grxml")) txtFilename.Text += ".grxml";
                var newFilename = ruleWin.filename = txtFilename.Text;
                GSApp.main.SaveActiveWindow(false);
                ruleWin.Title = Path.GetFileNameWithoutExtension(newFilename);
            }
            catch
            {
                txtFilename.Text = ruleWin.filename = oldFilename;
                ruleWin.Title = Path.GetFileNameWithoutExtension(oldFilename);
            }
        }

        private void txtFilename_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter) || (e.Key == Key.Return))
                saveBtn_Click(sender, e);
        }
    }
}