using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for FreeArcEmbeddingRules.xaml
    /// </summary>
    public partial class FreeArcEmbeddingRules
    {
        public FreeArcEmbeddingRules()
        {
            InitializeComponent();
        }

        #region Fields and Properties

        // below are the fields used by proerties
        private BindingExpression listBoxBE;

        private grammarRule selectedRule;

        private embeddingRule selectedEmbeddingRule
        {
            get
            {
                var sItem = eRulesListBox.SelectedItem as DataRowView;
                if (sItem == null) return null;
                else return sItem["rule"] as embeddingRule;
            }
        }

        private int SelectedIndex
        {
            get { return eRulesListBox.SelectedIndex; }
        }

        #endregion

        #region EventHandlers

        private void eRulesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectedEmbeddingRule != null)
                Update();
            if (eRulesListBox.SelectedIndex == -1) DisableProperties();
            else EnableProperties();
            if (eRulesListBox.SelectedIndex > 0) btnUp.IsEnabled = true;
            else btnUp.IsEnabled = false;
            if ((eRulesListBox.SelectedIndex < eRulesListBox.Items.Count - 1) &&
                (eRulesListBox.SelectedIndex >= 0)) btnDown.IsEnabled = true;
            else btnDown.IsEnabled = false;
        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e)
        {
            var newRule = new embeddingRule();
            // adds a new rule at the end
            selectedRule.embeddingRules.Add(newRule);
            listBoxBE.UpdateTarget();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var sI = eRulesListBox.SelectedIndex;
            selectedRule.embeddingRules.RemoveAt(sI);
            listBoxBE.UpdateTarget();
            eRulesListBox.SelectedIndex = sI - 1;
        }

        private void btnDuplicate_Click(object sender, RoutedEventArgs e)
        {
            var newCopy = new embeddingRule();

            newCopy.allowArcDuplication = selectedEmbeddingRule.allowArcDuplication;
            foreach (string a in selectedEmbeddingRule.freeArcLabels)
                newCopy.freeArcLabels.Add(a);
            foreach (string a in selectedEmbeddingRule.freeArcNegabels)
                newCopy.freeArcNegabels.Add(a);
            foreach (string a in selectedEmbeddingRule.neighborNodeLabels)
                newCopy.neighborNodeLabels.Add(a);
            foreach (string a in selectedEmbeddingRule.neighborNodeNegabels)
                newCopy.neighborNodeNegabels.Add(a);

            newCopy.newDirection = selectedEmbeddingRule.newDirection;
            newCopy.originalDirection = selectedEmbeddingRule.originalDirection;
            newCopy.LNodeName = selectedEmbeddingRule.LNodeName;
            newCopy.RNodeName = selectedEmbeddingRule.RNodeName;

            // code above just copies each member in embeddedrule 
            selectedRule.embeddingRules.Add(newCopy); // this adds the copied rule to the end of the list
            listBoxBE.UpdateTarget();
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            var erule = selectedRule.embeddingRules[SelectedIndex];
            selectedRule.embeddingRules.Remove(erule); // remove from current positions
            selectedRule.embeddingRules.Insert(SelectedIndex - 1, erule); // insert into previous position
            listBoxBE.UpdateTarget();
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            var erule = selectedRule.embeddingRules[SelectedIndex];
            selectedRule.embeddingRules.Remove(erule); //remove from current position
            selectedRule.embeddingRules.Insert(SelectedIndex + 1, erule); // insert into next position
            listBoxBE.UpdateTarget();
        }


        private void txtFreeArcLabels_LostFocus(object sender, RoutedEventArgs e)
        {
            var lststr = StringCollectionConverter.convert(txtFreeArcLabels.Text.Replace("<any>", ""));
            selectedEmbeddingRule.freeArcLabels.Clear();
            foreach (string str in lststr)
                selectedEmbeddingRule.freeArcLabels.Add(str);
            Update();
        }

        private void txtFreeArcLabels_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalString(e)) txtFreeArcLabels_LostFocus(sender, null);
        }

        private void txtFreeArcNegabels_LostFocus(object sender, RoutedEventArgs e)
        {
            var lststr = StringCollectionConverter.convert(txtFreeArcNegabels.Text.Replace("<none>", ""));
            selectedEmbeddingRule.freeArcNegabels.Clear();
            foreach (string str in lststr)
                selectedEmbeddingRule.freeArcNegabels.Add(str);
            Update();
        }

        private void txtFreeArcNegabels_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalString(e)) txtFreeArcNegabels_LostFocus(sender, null);
        }

        private void cmdLNodeName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((cmdLNodeName.SelectedItem == null)
                || (string.IsNullOrWhiteSpace(cmdLNodeName.SelectedItem.ToString()))
                || (cmdLNodeName.SelectedItem.ToString() == "<any>"))
                selectedEmbeddingRule.LNodeName = null;
            else selectedEmbeddingRule.LNodeName = cmdLNodeName.SelectedItem.ToString();
            Update();
        }

        private void txtNeighborLabels_LostFocus(object sender, RoutedEventArgs e)
        {
            var lststr = StringCollectionConverter.convert(txtNeighborLabels.Text.Replace("<any>", ""));
            selectedEmbeddingRule.neighborNodeLabels.Clear();
            foreach (string str in lststr)
                selectedEmbeddingRule.neighborNodeLabels.Add(str);
            Update();
        }

        private void txtNeighborLabels_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalString(e)) txtNeighborLabels_LostFocus(sender, null);
        }

        private void txtNeighborNegabels_LostFocus(object sender, RoutedEventArgs e)
        {
            var lststr = StringCollectionConverter.convert(txtNeighborNegabels.Text.Replace("<none>", ""));
            selectedEmbeddingRule.neighborNodeNegabels.Clear();
            foreach (string str in lststr)
                selectedEmbeddingRule.neighborNodeNegabels.Add(str);
            Update();
        }

        private void txtNeighborNegabels_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalString(e)) txtNeighborNegabels_LostFocus(sender, null);
        }

        private void cmdOriginalDirection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedEmbeddingRule.originalDirection = (sbyte)(cmdOriginalDirection.SelectedIndex - 1);
            Update();
        }

        private void cmdRNodeName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((cmdRNodeName.SelectedItem == null)
                || (string.IsNullOrWhiteSpace(cmdRNodeName.SelectedItem.ToString()))
                || (cmdRNodeName.SelectedItem.ToString() == "<null> (leave dangling)"))
                selectedEmbeddingRule.RNodeName = null;
            else selectedEmbeddingRule.RNodeName = cmdRNodeName.SelectedItem.ToString();
            Update();
        }

        private void chkAllowDup_Checked(object sender, RoutedEventArgs e)
        {
            selectedEmbeddingRule.allowArcDuplication = true;
            Update();
        }

        private void chkAllowDup_Unchecked(object sender, RoutedEventArgs e)
        {
            selectedEmbeddingRule.allowArcDuplication = false;
            Update();
        }

        private void cmdNewDirection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedEmbeddingRule.newDirection = (sbyte)(cmdNewDirection.SelectedIndex - 1);
            Update();
        }

        #endregion

        #region Methods

        private Boolean noRecursion;

        internal void Update(grammarRule Rule)
        {
            PopulateNodeNames(Rule.L.nodes, cmdLNodeName, "<any>");
            PopulateNodeNames(Rule.R.nodes, cmdRNodeName, "<null> (leave dangling)");

            if (selectedRule == Rule)
            {
                if (Rule.embeddingRules.Count == 0) DisableProperties();
                else if (Rule.embeddingRules.Contains(selectedEmbeddingRule))
                    Update();
                else eRulesListBox.SelectedIndex = 0;
            }
            else
            {
                selectedRule = Rule;
                BindingOperations.ClearBinding(this, ItemsControl.ItemsSourceProperty);
                var ItemsBinding = new Binding();
                ItemsBinding.Source = Rule.embeddingRules;
                ItemsBinding.Mode = BindingMode.OneWay;
                ItemsBinding.Converter = new EmbeddingRuleListConverter();
                eRulesListBox.SetBinding(ItemsControl.ItemsSourceProperty, ItemsBinding);
                listBoxBE = BindingOperations.GetBindingExpression(eRulesListBox, ItemsControl.ItemsSourceProperty);
                eRulesListBox.DisplayMemberPath = "name";
                if (Rule.embeddingRules.Count == 0) DisableProperties();
                else eRulesListBox.SelectedIndex = 0;
            }
        }

        // this method is used to disable the controls
        // when there are no existing freearcEmbedding rules defined for the Rule and enable otherwise
        private void DisableProperties()
        {
            txtFreeArcLabels.IsEnabled = txtFreeArcNegabels.IsEnabled = txtNeighborLabels.IsEnabled
                                                                        =
                                                                        txtNeighborNegabels.IsEnabled =
                                                                        cmdLNodeName.IsEnabled =
                                                                        cmdNewDirection.IsEnabled
                                                                        =
                                                                        cmdOriginalDirection.IsEnabled =
                                                                        cmdRNodeName.IsEnabled = chkAllowDup.IsEnabled
                                                                                                 =
                                                                                                 btnDown.IsEnabled =
                                                                                                 btnUp.IsEnabled =
                                                                                                 btnDuplicate.IsEnabled
                                                                                                 =
                                                                                                 btnDelete.IsEnabled =
                                                                                                 false;
        }

        private void EnableProperties()
        {
            txtFreeArcLabels.IsEnabled = txtFreeArcNegabels.IsEnabled = txtNeighborLabels.IsEnabled
                                                                        =
                                                                        txtNeighborNegabels.IsEnabled =
                                                                        cmdLNodeName.IsEnabled =
                                                                        cmdNewDirection.IsEnabled
                                                                        =
                                                                        cmdOriginalDirection.IsEnabled =
                                                                        cmdRNodeName.IsEnabled = chkAllowDup.IsEnabled
                                                                                                 =
                                                                                                 btnDown.IsEnabled =
                                                                                                 btnUp.IsEnabled =
                                                                                                 btnDuplicate.IsEnabled
                                                                                                 =
                                                                                                 btnDelete.IsEnabled =
                                                                                                 true;
        }

        private void Update()
        {
            if (noRecursion) return;
            noRecursion = true;
            EnableProperties();
            //free arc labels text 
            if (selectedEmbeddingRule.freeArcLabels.Count == 0)
                txtFreeArcLabels.Text = "<any>";
            else
                txtFreeArcLabels.Text = StringCollectionConverter.convert(selectedEmbeddingRule.freeArcLabels);

            //free arc neg labels
            if (selectedEmbeddingRule.freeArcNegabels.Count == 0)
                txtFreeArcNegabels.Text = "<none>";
            else
                txtFreeArcNegabels.Text = StringCollectionConverter.convert(selectedEmbeddingRule.freeArcNegabels);

            //L node name
            if ((string.IsNullOrWhiteSpace(selectedEmbeddingRule.LNodeName))
                || (!cmdLNodeName.Items.Contains(selectedEmbeddingRule.LNodeName)))
                cmdLNodeName.SelectedItem = "<any>";
            else if (!selectedEmbeddingRule.LNodeName.Equals(cmdLNodeName.SelectedItem))
                cmdLNodeName.SelectedItem = selectedEmbeddingRule.LNodeName;

            // neighbor node labels
            if (selectedEmbeddingRule.neighborNodeLabels.Count == 0)
                txtNeighborLabels.Text = "<any>";
            else
                txtNeighborLabels.Text = StringCollectionConverter.convert(selectedEmbeddingRule.neighborNodeLabels);

            // neighbor node negabels
            if (selectedEmbeddingRule.neighborNodeNegabels.Count == 0)
                txtNeighborNegabels.Text = "<none>";
            else
                txtNeighborNegabels.Text = StringCollectionConverter.convert(selectedEmbeddingRule.neighborNodeNegabels);

            // orginal direction
            if (cmdOriginalDirection.SelectedIndex != selectedEmbeddingRule.originalDirection + 1)
                cmdOriginalDirection.SelectedIndex = selectedEmbeddingRule.originalDirection + 1;

            //R node Name
            if ((string.IsNullOrWhiteSpace(selectedEmbeddingRule.RNodeName))
                || (!cmdRNodeName.Items.Contains(selectedEmbeddingRule.RNodeName)))
                cmdRNodeName.SelectedItem = "<null> (leave dangling)";
            else if (!selectedEmbeddingRule.RNodeName.Equals(cmdRNodeName.SelectedItem))
                cmdRNodeName.SelectedItem = selectedEmbeddingRule.RNodeName;


            // allow duplication
            if (chkAllowDup.IsChecked != selectedEmbeddingRule.allowArcDuplication)
                chkAllowDup.IsChecked = selectedEmbeddingRule.allowArcDuplication;

            //new direction
            if (cmdNewDirection.SelectedIndex != selectedEmbeddingRule.newDirection + 1)
                cmdNewDirection.SelectedIndex = selectedEmbeddingRule.newDirection + 1;
            var s = eRulesListBox.SelectedIndex;
            listBoxBE.UpdateTarget();
            eRulesListBox.SelectedIndex = s;
            noRecursion = false;
        }

        private void PopulateNodeNames(List<node> list, ComboBox comboBox, string defString)
        {
            var selectStr = (string)comboBox.SelectedItem;
            comboBox.Items.Clear();
            comboBox.Items.Add(defString);
            foreach (node n in list)
            {
                comboBox.Items.Add(n.name);
            }
            comboBox.SelectedItem = selectStr;
        }

        #endregion
    }
}