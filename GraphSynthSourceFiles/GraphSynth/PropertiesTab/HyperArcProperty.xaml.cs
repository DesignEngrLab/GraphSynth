using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GraphSynth.GraphDisplay;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for HyperArcProperty.xaml
    /// </summary>
    public partial class HyperArcProperty : UserControl
    {
        private HyperArcIconShape hyperArcIcon;
        private List<hyperarc> hyperarcs;

        private designGraph graph;
        private GraphGUI gui;

        public HyperArcProperty()
        {
            InitializeComponent();
            cmbNodeList.ItemsSource = choices
             = new ObservableCollection<KeyValuePair<int, string>>
                          {
                              new KeyValuePair<int, string>( -1, "<none>")
                          };
        }

        #region Events

        #region Labels

        private void txtLabels_LostFocus(object sender, RoutedEventArgs e)
        {
            if (hyperarcs.Count == 0) return;
            MultiBindingExpression mbe;
            var caretIndex = txtLabels.CaretIndex;
            var origLength = txtLabels.Text.Length;
            var oldLabels = firstHyperArc.localLabels;
            var newLabels = StringCollectionConverter.convert(txtLabels.Text);

            if ((gui is RuleDisplay) &&
                (gui == ((RuleDisplay)gui).rW.graphGUIK))
            {
                var rW = ((RuleDisplay)gui).rW;
                var Larc = rW.rule.L.hyperarcs.Find(b => string.Equals(b.name, firstHyperArc.name));
                var Rarc = rW.rule.R.hyperarcs.Find(b => string.Equals(b.name, firstHyperArc.name));

                var removedKLabels = oldLabels.Where(a => !newLabels.Contains(a)).ToList();
                foreach (string a in removedKLabels)
                {
                    Larc.localLabels.Remove(a);
                    Rarc.localLabels.Remove(a);
                }
                var newLLabels = new List<string>(Larc.localLabels.Union(newLabels));
                Larc.localLabels.Clear();
                foreach (string a in newLLabels) Larc.localLabels.Add(a);
                var nI = ((HyperArcShape)Larc.DisplayShape.Shape).icon;
                mbe = BindingOperations.GetMultiBindingExpression(nI, IconShape.DisplayTextProperty);
                mbe.UpdateTarget();

                var newRLabels = new List<string>(Rarc.localLabels.Union(newLabels));
                Rarc.localLabels.Clear();
                foreach (string a in newRLabels) Rarc.localLabels.Add(a);
                nI = ((HyperArcShape)Rarc.DisplayShape.Shape).icon;
                mbe = BindingOperations.GetMultiBindingExpression(nI, IconShape.DisplayTextProperty);
                mbe.UpdateTarget();
            }
            else if (gui is RuleDisplay)
            {
                // this is a rule LHS or RHS
                var rW = ((RuleDisplay)gui).rW;
                hyperarc otherHyperArc = null;
                otherHyperArc = gui == rW.graphGUIL
                    ? rW.rule.R.hyperarcs.Find(b => string.Equals(b.name, firstHyperArc.name))
                    : rW.rule.L.hyperarcs.Find(b => string.Equals(b.name, firstHyperArc.name));
                if (otherHyperArc != null)
                {
                    var Klabels = new List<string>(otherHyperArc.localLabels.Intersect(newLabels));
                    otherHyperArc =
                        rW.graphGUIK.graph.hyperarcs.Find(b => string.Equals(b.name, firstHyperArc.name));
                    otherHyperArc.localLabels.Clear();
                    foreach (string a in Klabels) otherHyperArc.localLabels.Add(a);
                    var nI = ((HyperArcShape)otherHyperArc.DisplayShape.Shape).icon;
                    mbe = BindingOperations.GetMultiBindingExpression(nI, IconShape.DisplayTextProperty);
                    mbe.UpdateTarget();
                }
            }
            firstHyperArc.localLabels.Clear();
            foreach (string a in newLabels) firstHyperArc.localLabels.Add(a);
            mbe = BindingOperations.GetMultiBindingExpression(hyperArcIcon, IconShape.DisplayTextProperty);
            mbe.UpdateTarget();
            Update();
            TextBoxHelper.SetCaret(txtLabels, caretIndex, origLength);
        }

        private void txtLabels_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalString(e))
                txtLabels_LostFocus(sender, null);
        }

        #endregion

        #region Variables

        private void txtVariables_LostFocus(object sender, RoutedEventArgs e)
        {
            if (hyperarcs.Count == 0) return;
            var caretIndex = txtVariables.CaretIndex;
            var origLength = txtVariables.Text.Length;
            var oldVars = firstHyperArc.localVariables;
            var newVars = DoubleCollectionConverter.convert(txtVariables.Text);

            if ((gui is RuleDisplay) &&
                (gui == ((RuleDisplay)gui).rW.graphGUIK))
            {
                var rW = ((RuleDisplay)gui).rW;
                var Larc = rW.rule.L.hyperarcs.Find(b => string.Equals(b.name, firstHyperArc.name));
                var Rarc = rW.rule.R.hyperarcs.Find(b => string.Equals(b.name, firstHyperArc.name));

                var removedKVars = oldVars.Where(a => !newVars.Contains(a)).ToList();
                foreach (double a in removedKVars)
                {
                    Larc.localVariables.Remove(a);
                    Rarc.localVariables.Remove(a);
                }
                var newLVars = new List<double>(Larc.localVariables.Union(newVars));
                Larc.localVariables.Clear();
                foreach (double a in newLVars) Larc.localVariables.Add(a);

                var newRVars = new List<double>(Rarc.localVariables.Union(newVars));
                Rarc.localVariables.Clear();
                foreach (double a in newRVars) Rarc.localVariables.Add(a);
            }
            else if (gui is RuleDisplay)
            {
                // this is a rule LHS or RHS
                var rW = ((RuleDisplay)gui).rW;
                hyperarc otherArc = null;
                otherArc = gui == rW.graphGUIL
                    ? rW.rule.R.hyperarcs.Find(b => string.Equals(b.name, firstHyperArc.name))
                    : rW.rule.L.hyperarcs.Find(b => string.Equals(b.name, firstHyperArc.name));
                if (otherArc != null)
                {
                    var KVars = new List<double>(otherArc.localVariables.Intersect(newVars));
                    otherArc =
                        rW.graphGUIK.graph.hyperarcs.Find(b => string.Equals(b.name, firstHyperArc.name));
                    otherArc.localVariables.Clear();
                    foreach (double a in KVars) otherArc.localVariables.Add(a);
                }
            }
            firstHyperArc.localVariables.Clear();
            foreach (double d in newVars)
                firstHyperArc.localVariables.Add(d);
            Update();
            TextBoxHelper.SetCaret(txtVariables, caretIndex, origLength);
        }

        private void txtVariables_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalNumber(txtVariables, e)) txtVariables_LostFocus(sender, null);
        }

        #endregion

        #region Name

        private void txtName_KeyUp(object sender, KeyEventArgs e)
        {
            if (!(typeof(ruleHyperarc)).IsInstanceOfType(firstHyperArc)
                || (e.Key == Key.Return) || (e.Key == Key.Enter))
                btnConfirm_Click(sender, null);
        }

        private void txtName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!(typeof(ruleHyperarc)).IsInstanceOfType(firstHyperArc))
            {
                btnConfirm_Click(sender, null);
                Update();
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            gui.HyperArcNameChanged(firstHyperArc, txtName.Text);
            var binding
                = BindingOperations.GetMultiBindingExpression(hyperArcIcon, IconShape.DisplayTextProperty);
            binding.UpdateTarget();
            Update();
        }

        #endregion

        #region Negate Labels

        private void txtNegLabels_LostFocus(object sender, RoutedEventArgs e)
        {
            var lststr = StringCollectionConverter.convert(txtNegLabels.Text);
            ((ruleHyperarc)firstHyperArc).negateLabels.Clear();
            foreach (string str in lststr)
                ((ruleHyperarc)firstHyperArc).negateLabels.Add(str);
            Update();
        }

        private void txtNegLabels_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalString(e))
                txtNegLabels_LostFocus(sender, null);
        }

        #endregion

        #region Arc Type

        private void txtArcType_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) txtArcType_LostFocus(sender, e);
        }

        private void txtArcType_LostFocus(object sender, RoutedEventArgs e)
        {
            foreach (hyperarc a in hyperarcs)
            {
                a.XmlHyperarcType = txtHyperArcType.Text;
                /* make sure node is of right type - if not call the replacement function */
                if ((a.hyperarcType != null) && (a.GetType() != typeof(ruleHyperarc))
                    && (a.GetType() != a.hyperarcType))
                    graph.replaceHyperArcWithInheritedType(a);
            }
            Update();
        }

        #endregion


        #region RuleHyperArc Booleans

        private void chkContainsLocalLabels_Click(object sender, RoutedEventArgs e)
        {
            if ((chkContainsLocalLabels.IsChecked == null) || (chkContainsLocalLabels.IsChecked.Value == false))
                foreach (hyperarc a in hyperarcs)
                    ((ruleHyperarc)a).containsAllLocalLabels = false;
            else
                foreach (hyperarc a in hyperarcs)
                    ((ruleHyperarc)a).containsAllLocalLabels = true;
            Update();
        }

        private void chkStrictNodeCount_Click(object sender, RoutedEventArgs e)
        {
            if ((chkStrictNodeCount.IsChecked == null) || (chkStrictNodeCount.IsChecked.Value == false))
                foreach (hyperarc a in hyperarcs)
                    ((ruleHyperarc)a).strictNodeCountMatch = false;
            else
                foreach (hyperarc a in hyperarcs)
                    ((ruleHyperarc)a).strictNodeCountMatch = true;
            Update();
        }


        private void chkNotExist_Click(object sender, RoutedEventArgs e)
        {
            if ((chkNotExist.IsChecked == null) || (chkNotExist.IsChecked.Value == false))
                foreach (hyperarc a in hyperarcs)
                    ((ruleHyperarc)a).NotExist = false;
            else
                foreach (hyperarc a in hyperarcs)
                    ((ruleHyperarc)a).NotExist = true;
            Update();
        }

        #endregion

        #endregion

        #region Update Methods

        internal void Update(List<hyperarc> _hyperarcs, designGraph _graph, GraphGUI _gui)
        {
            hyperarcs = _hyperarcs;
            graph = _graph;
            gui = _gui;
            hyperArcIcon = ((HyperArcShape)firstHyperArc.DisplayShape.Shape).icon;
            Update();
        }

        private void Update()
        {
            if (hyperarcs.Count == 1)
            {
                txtName.IsEnabled = txtLabels.IsEnabled = txtVariables.IsEnabled = true;
                txtName.Text = firstHyperArc.name;
                if (firstHyperArc is ruleHyperarc)
                    btnConfirm.Visibility = Visibility.Visible;
                else btnConfirm.Visibility = Visibility.Hidden;
                txtLabels.Text = StringCollectionConverter.convert(firstHyperArc.localLabels);
                txtVariables.Text = DoubleCollectionConverter.convert(firstHyperArc.localVariables);

                txtHyperArcType.IsEnabled = true;
                txtHyperArcType.Text = firstHyperArc.GetType().ToString();

                if ((gui is RuleDisplay)
                    && (gui == ((RuleDisplay)gui).rW.graphGUIL))
                {
                    chkContainsLocalLabels.IsChecked = ((ruleHyperarc)firstHyperArc).containsAllLocalLabels;
                    chkStrictNodeCount.IsChecked = ((ruleHyperarc)firstHyperArc).strictNodeCountMatch;
                    chkNotExist.IsChecked = ((ruleHyperarc)firstHyperArc).NotExist;
                    txtNegLabels.IsEnabled = true;
                    txtNegLabels.Text
                        = StringCollectionConverter.convert(((ruleHyperarc)firstHyperArc).negateLabels);
                    if (!stackHyperArcProps.Children.Contains(wrapRuleBools))
                        stackHyperArcProps.Children.Add(wrapRuleBools);
                    if (!stackHyperArcProps.Children.Contains(gridRuleNegLabels))
                        stackHyperArcProps.Children.Add(gridRuleNegLabels);
                }
                else
                {
                    stackHyperArcProps.Children.Remove(wrapRuleBools);
                    stackHyperArcProps.Children.Remove(gridRuleNegLabels);
                }
            }
            else
            {
                txtName.Text = "<multiple hyperarcs>";
                txtLabels.Text = "<multiple hyperarcs>";
                txtVariables.Text = "<multiple hyperarcs>";
                txtName.IsEnabled = txtLabels.IsEnabled = txtVariables.IsEnabled = false;

                var allSame = true;
                var aType = firstHyperArc.GetType();
                for (var i = 1; i < hyperarcs.Count; i++)
                    if (!aType.Equals(hyperarcs[i].GetType()))
                    {
                        allSame = false;
                        break;
                    }
                if (allSame)
                {
                    txtHyperArcType.IsEnabled = true;
                    txtHyperArcType.Text = aType.ToString();
                }
                else
                {
                    txtHyperArcType.IsEnabled = false;
                    txtHyperArcType.Text = "<multiple types>";
                }



                if ((gui is RuleDisplay)
                    && (gui == ((RuleDisplay)gui).rW.graphGUIL))
                {
                    allSame = true;
                    var aBool = ((ruleHyperarc)firstHyperArc).containsAllLocalLabels;
                    for (var i = 1; i < hyperarcs.Count; i++)
                        if (aBool != ((ruleHyperarc)hyperarcs[i]).containsAllLocalLabels)
                        {
                            allSame = false;
                            break;
                        }
                    if (allSame)
                        chkContainsLocalLabels.IsChecked = aBool;
                    else chkContainsLocalLabels.IsChecked = null;

                    allSame = true;
                    aBool = ((ruleHyperarc)firstHyperArc).strictNodeCountMatch;
                    for (var i = 1; i < hyperarcs.Count; i++)
                        if (aBool != ((ruleHyperarc)hyperarcs[i]).strictNodeCountMatch)
                        {
                            allSame = false;
                            break;
                        }
                    if (allSame)
                        chkStrictNodeCount.IsChecked = aBool;
                    else chkStrictNodeCount.IsChecked = null;

                    allSame = true;
                    aBool = ((ruleHyperarc)firstHyperArc).NotExist;
                    for (var i = 1; i < hyperarcs.Count; i++)
                        if (aBool != ((ruleHyperarc)hyperarcs[i]).NotExist)
                        {
                            allSame = false;
                            break;
                        }
                    if (allSame)
                        chkNotExist.IsChecked = aBool;
                    else chkNotExist.IsChecked = null;


                    txtNegLabels.IsEnabled = false;
                    txtNegLabels.Text = "<multiple hyperarcs>";
                    if (!stackHyperArcProps.Children.Contains(wrapRuleBools))
                        stackHyperArcProps.Children.Add(wrapRuleBools);
                    if (!stackHyperArcProps.Children.Contains(gridRuleNegLabels))
                        stackHyperArcProps.Children.Add(gridRuleNegLabels);
                }
                else
                {
                    stackHyperArcProps.Children.Remove(wrapRuleBools);
                    stackHyperArcProps.Children.Remove(gridRuleNegLabels);
                }
            }
        }

        #endregion

        private hyperarc firstHyperArc
        {
            get { return hyperarcs[0]; }
        }

        ObservableCollection<KeyValuePair<int, string>> choices;
        private void UpdateDisconnectComboBox(object sender, MouseEventArgs e)
        {
            var length = 1;
            if (hyperarcs.Count == 1) length += firstHyperArc.nodes.Count;
            while (choices.Count > length) choices.RemoveAt(choices.Count - 1);
            if (hyperarcs.Count > 1) return;
            for (int i = 0; i < firstHyperArc.nodes.Count; i++)
                if (choices.Count <= i + 1)
                    choices.Add(new KeyValuePair<int, string>(i, firstHyperArc.nodes[i].name));
                else if (!choices[i + 1].Value.Equals(firstHyperArc.nodes[i].name))
                    choices[i + 1] = new KeyValuePair<int, string>(i, firstHyperArc.nodes[i].name);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((int)cmbNodeList.SelectedValue < 0) return;
            gui.DisconnectHyperArcConnection(firstHyperArc, firstHyperArc.nodes[(int)cmbNodeList.SelectedValue]);
            UpdateDisconnectComboBox(sender, null);
            cmbNodeList.SelectedIndex = 0;
        }
    }
}