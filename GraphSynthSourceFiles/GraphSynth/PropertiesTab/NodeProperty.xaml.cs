using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphSynth.GraphDisplay;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for NodeProperty.xaml
    /// </summary>
    public partial class NodeProperty : UserControl
    {
        private designGraph graph;
        private GraphGUI gui;
        private IconShape nodeIcon;
        private List<node> nodes;

        public NodeProperty()
        {
            InitializeComponent();
        }

        #region Events

        #region Labels

        private void txtLabels_LostFocus(object sender, RoutedEventArgs e)
        {
            if (nodes.Count == 0) return;
            MultiBindingExpression mbe;
            var caretIndex = txtLabels.CaretIndex;
            var origLength = txtLabels.Text.Length;
            var oldLabels = firstNode.localLabels;
            var newLabels = StringCollectionConverter.convert(txtLabels.Text);

            if ((gui is RuleDisplay) &&
                (gui == ((RuleDisplay)gui).rW.graphGUIK))
            {
                var rW = ((RuleDisplay)gui).rW;
                var Lnode = rW.rule.L.nodes.Find(b => string.Equals(b.name, firstNode.name));
                var Rnode = rW.rule.R.nodes.Find(b => string.Equals(b.name, firstNode.name));

                var removedKLabels = oldLabels.Where(a => !newLabels.Contains(a)).ToList();
                foreach (string a in removedKLabels)
                {
                    Lnode.localLabels.Remove(a);
                    Rnode.localLabels.Remove(a);
                }
                var newLLabels = new List<string>(Lnode.localLabels.Union(newLabels));
                Lnode.localLabels.Clear();
                foreach (string a in newLLabels) Lnode.localLabels.Add(a);
                var nI = ((DisplayShape)Lnode.DisplayShape).icon;
                mbe = BindingOperations.GetMultiBindingExpression(nI, IconShape.DisplayTextProperty);
                mbe.UpdateTarget();

                var newRLabels = new List<string>(Rnode.localLabels.Union(newLabels));
                Rnode.localLabels.Clear();
                foreach (string a in newRLabels) Rnode.localLabels.Add(a);
                nI = ((DisplayShape)Rnode.DisplayShape).icon;
                mbe = BindingOperations.GetMultiBindingExpression(nI, IconShape.DisplayTextProperty);
                mbe.UpdateTarget();
            }
            else if (gui is RuleDisplay)
            {
                // this is a rule LHS or RHS
                var rW = ((RuleDisplay)gui).rW;
                node otherNode = null;
                if (gui == rW.graphGUIL)
                    otherNode = rW.rule.R.nodes.Find(b => string.Equals(b.name, firstNode.name));
                else
                    otherNode = rW.rule.L.nodes.Find(b => string.Equals(b.name, firstNode.name));
                if (otherNode != null)
                {
                    var Klabels = new List<string>(otherNode.localLabels.Intersect(newLabels));
                    otherNode =
                        rW.graphGUIK.graph.nodes.Find(b => string.Equals(b.name, firstNode.name));
                    otherNode.localLabels.Clear();
                    foreach (string a in Klabels) otherNode.localLabels.Add(a);
                    var nI = ((DisplayShape)otherNode.DisplayShape).icon;
                    mbe = BindingOperations.GetMultiBindingExpression(nI, IconShape.DisplayTextProperty);
                    mbe.UpdateTarget();
                }
            }
            firstNode.localLabels.Clear();
            foreach (string a in newLabels) firstNode.localLabels.Add(a);
            mbe = BindingOperations.GetMultiBindingExpression(nodeIcon, IconShape.DisplayTextProperty);
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
            if (nodes.Count == 0) return;
            var caretIndex = txtVariables.CaretIndex;
            var origLength = txtVariables.Text.Length;
            var oldVars = firstNode.localVariables;
            var newVars = DoubleCollectionConverter.convert(txtVariables.Text);

            if ((gui is RuleDisplay) &&
                (gui == ((RuleDisplay)gui).rW.graphGUIK))
            {
                var rW = ((RuleDisplay)gui).rW;
                var Lnode = rW.rule.L.nodes.Find(b => string.Equals(b.name, firstNode.name));
                var Rnode = rW.rule.R.nodes.Find(b => string.Equals(b.name, firstNode.name));

                var removedKVars = oldVars.Where(a => !newVars.Contains(a)).ToList();
                foreach (double a in removedKVars)
                {
                    Lnode.localVariables.Remove(a);
                    Rnode.localVariables.Remove(a);
                }
                var newLVars = new List<double>(Lnode.localVariables.Union(newVars));
                Lnode.localVariables.Clear();
                foreach (double a in newLVars) Lnode.localVariables.Add(a);

                var newRVars = new List<double>(Rnode.localVariables.Union(newVars));
                Rnode.localVariables.Clear();
                foreach (double a in newRVars) Rnode.localVariables.Add(a);
            }
            else if (gui is RuleDisplay)
            {
                // this is a rule LHS or RHS
                var rW = ((RuleDisplay)gui).rW;
                node otherNode = null;
                if (gui == rW.graphGUIL)
                    otherNode = rW.rule.R.nodes.Find(b => string.Equals(b.name, firstNode.name));
                else
                    otherNode = rW.rule.L.nodes.Find(b => string.Equals(b.name, firstNode.name));
                if (otherNode != null)
                {
                    var KVars = new List<double>(otherNode.localVariables.Intersect(newVars));
                    otherNode =
                        rW.graphGUIK.graph.nodes.Find(b => string.Equals(b.name, firstNode.name));
                    otherNode.localVariables.Clear();
                    foreach (double a in KVars) otherNode.localVariables.Add(a);
                }
            }
            firstNode.localVariables.Clear();
            foreach (double d in newVars)
                firstNode.localVariables.Add(d);
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
            if (!(typeof(ruleNode)).IsInstanceOfType(firstNode)
                || (e.Key == Key.Return) || (e.Key == Key.Enter))
                btnConfirm_Click(sender, null);
        }

        private void txtName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!(typeof(ruleNode)).IsInstanceOfType(firstNode))
            {
                btnConfirm_Click(sender, null);
                Update();
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            gui.NodeNameChanged(firstNode, txtName.Text);
            var binding
                = BindingOperations.GetMultiBindingExpression(nodeIcon, IconShape.DisplayTextProperty);
            binding.UpdateTarget();
            Update();
        }

        #endregion

        #region Node Type

        private void txtNodeType_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) txtNodeType_LostFocus(sender, e);
        }

        private void txtNodeType_LostFocus(object sender, RoutedEventArgs e)
        {
            foreach (node n in nodes)
            {
                n.XmlNodeType = txtNodeType.Text;
                /* make sure node is of right type - if not call the replacement function */
                if ((n is ruleNode)
                    && (n.nodeType != null) && (n.GetType() != n.nodeType))
                    graph.replaceNodeWithInheritedType(n);
            }
            Update();
        }

        #endregion

        #region Location

        private void txtBxPosX_LostFocus(object sender, RoutedEventArgs e)
        {
            double temp;
            if (Double.TryParse(txtBxPosX.Text, out temp))
            {
                foreach (node n in nodes)
                {
                    n.X = temp;
                    gui.MoveShapeToXYNodeCoordinates(n);
                }
                Update();
            }
        }

        private void txtBxPosX_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalNumber((TextBox)sender, e))
                txtBxPosX_LostFocus(sender, e);
        }

        private void txtBxPosY_LostFocus(object sender, RoutedEventArgs e)
        {
            double temp;
            if (Double.TryParse(txtBxPosY.Text, out temp))
            {
                foreach (node n in nodes)
                {
                    n.Y = temp;
                    gui.MoveShapeToXYNodeCoordinates(n);
                }
                Update();
            }
        }

        private void txtBxPosY_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalNumber((TextBox)sender, e))
                txtBxPosY_LostFocus(sender, e);
        }

        private void txtBxPosZ_LostFocus(object sender, RoutedEventArgs e)
        {
            int temp;
            if (int.TryParse(txtBxPosZ.Text, out temp))
            {
                foreach (node n in nodes)
                {
                    n.Z = temp;
                    Panel.SetZIndex((Shape)n.DisplayShape.Shape, temp);
                }
                Update();
            }
        }

        private void txtBxPosZ_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalNumber((TextBox)sender, e))
                txtBxPosZ_LostFocus(sender, e);
        }

        #endregion

        #region RuleNode Booleans

        private void chkContainsLocalLabels_Click(object sender, RoutedEventArgs e)
        {
            if ((chkContainsLocalLabels.IsChecked == null) || (chkContainsLocalLabels.IsChecked.Value == false))
                foreach (node a in nodes)
                    ((ruleNode)a).containsAllLocalLabels = false;
            else
                foreach (node a in nodes)
                    ((ruleNode)a).containsAllLocalLabels = true;
            Update();
        }

        private void chkStrictDegreeMatch_Click(object sender, RoutedEventArgs e)
        {
            if ((chkStrictDegreeMatch.IsChecked == null)
                || (chkStrictDegreeMatch.IsChecked.Value == false))
                foreach (node a in nodes)
                    ((ruleNode)a).strictDegreeMatch = false;
            else
                foreach (node a in nodes)
                    ((ruleNode)a).strictDegreeMatch = true;
            Update();
        }

        private void chkNotExist_Click(object sender, RoutedEventArgs e)
        {
            if ((chkNotExist.IsChecked == null)
                || (chkNotExist.IsChecked.Value == false))
                foreach (node a in nodes)
                    ((ruleNode)a).NotExist = false;
            else
                foreach (node a in nodes)
                    ((ruleNode)a).NotExist = true;
            Update();
        }

        #endregion

        #region Negate Labels

        private void txtNegLabels_LostFocus(object sender, RoutedEventArgs e)
        {
            var caretIndex = txtNegLabels.CaretIndex;
            var origLength = txtNegLabels.Text.Length;
            var lststr = StringCollectionConverter.convert(txtNegLabels.Text);
            ((ruleNode)firstNode).negateLabels.Clear();
            foreach (string str in lststr)
                ((ruleNode)firstNode).negateLabels.Add(str);
            Update();
            TextBoxHelper.SetCaret(txtNegLabels, caretIndex, origLength);
        }

        private void txtNegLabels_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxHelper.CanEvalString(e)) txtNegLabels_LostFocus(sender, null);
        }

        #endregion

        #endregion

        #region Update Methods

        internal void Update(List<node> _nodes, designGraph _graph, GraphGUI _gui)
        {
            nodes = _nodes;
            graph = _graph;
            gui = _gui;
            nodeIcon = ((DisplayShape)firstNode.DisplayShape).icon;
            Update();
        }

        private void Update()
        {
            if (nodes.Count == 1)
            {
                txtName.IsEnabled = txtLabels.IsEnabled = txtVariables.IsEnabled = true;
                txtName.Text = firstNode.name;
                btnConfirm.Visibility = firstNode is ruleNode
                    ? Visibility.Visible : Visibility.Hidden;
                txtLabels.Text = StringCollectionConverter.convert(firstNode.localLabels);
                txtVariables.Text = DoubleCollectionConverter.convert(firstNode.localVariables);

                txtNodeType.IsEnabled = true;
                if (firstNode.nodeType != null)
                    txtNodeType.Text = firstNode.nodeType.ToString();

                txtBxPosX.Foreground = txtBxPosY.Foreground = txtBxPosZ.Foreground = Brushes.Black;
                txtBxPosX.Text = firstNode.X.ToString();
                txtBxPosY.Text = firstNode.Y.ToString();
                txtBxPosZ.Text = firstNode.Z.ToString();

                if ((gui is RuleDisplay)
                    && (gui == ((RuleDisplay)gui).rW.graphGUIL))
                {
                    chkContainsLocalLabels.IsChecked = ((ruleNode)firstNode).containsAllLocalLabels;
                    chkStrictDegreeMatch.IsChecked = ((ruleNode)firstNode).strictDegreeMatch;
                    chkNotExist.IsChecked = ((ruleNode)firstNode).NotExist;

                    txtNegLabels.IsEnabled = true;
                    txtNegLabels.Text
                        = StringCollectionConverter.convert(((ruleNode)firstNode).negateLabels);
                    if (!stackNodeProps.Children.Contains(wrapRuleBools))
                        stackNodeProps.Children.Add(wrapRuleBools);
                    if (!stackNodeProps.Children.Contains(gridRuleNegLabels))
                        stackNodeProps.Children.Add(gridRuleNegLabels);
                }
                else
                {
                    stackNodeProps.Children.Remove(wrapRuleBools);
                    stackNodeProps.Children.Remove(gridRuleNegLabels);
                }
            }
            else if (nodes.Count > 1)
            {
                txtName.Text = "<multiple nodes>";
                txtLabels.Text = "<multiple nodes>";
                txtVariables.Text = "<multiple nodes>";
                txtName.IsEnabled = txtLabels.IsEnabled = txtVariables.IsEnabled = false;
                //                expLocation.IsEnabled = false;

                var allSame = true;
                /*****************  Node Type *************/
                var aType = firstNode.GetType();
                for (var i = 1; i < nodes.Count; i++)
                    if (!aType.Equals(nodes[i].GetType()))
                    {
                        allSame = false;
                        break;
                    }
                if (allSame)
                {
                    txtNodeType.IsEnabled = true;
                    txtNodeType.Text = aType.ToString();
                }
                else
                {
                    txtNodeType.IsEnabled = false;
                    txtNodeType.Text = "<multiple types>";
                }


                /*****************  X-Position *************/
                allSame = true;
                var pos = firstNode.X;
                for (var i = 1; i < nodes.Count; i++)
                    if (!pos.Equals(nodes[i].X))
                    {
                        allSame = false;
                        break;
                    }
                if (allSame)
                {
                    txtBxPosX.Foreground = Brushes.Black;
                    txtBxPosX.Text = pos.ToString();
                }
                else
                {
                    txtBxPosX.Foreground = Brushes.Gray;
                    txtBxPosX.Text = "diff";
                }
                /*****************  Y-Position *************/
                allSame = true;
                pos = firstNode.Y;
                for (var i = 1; i < nodes.Count; i++)
                    if (!pos.Equals(nodes[i].Y))
                    {
                        allSame = false;
                        break;
                    }
                if (allSame)
                {
                    txtBxPosY.Foreground = Brushes.Black;
                    txtBxPosY.Text = pos.ToString();
                }
                else
                {
                    txtBxPosY.Foreground = Brushes.Gray;
                    txtBxPosY.Text = "diff";
                }
                /*****************  Z-Position *************/
                allSame = true;
                pos = firstNode.Z;
                for (var i = 1; i < nodes.Count; i++)
                    if (!pos.Equals(nodes[i].Z))
                    {
                        allSame = false;
                        break;
                    }
                if (allSame)
                {
                    txtBxPosZ.Foreground = Brushes.Black;
                    txtBxPosZ.Text = pos.ToString();
                }
                else
                {
                    txtBxPosZ.Foreground = Brushes.Gray;
                    txtBxPosZ.Text = "diff";
                }

                if ((gui is RuleDisplay)
                    && (gui == ((RuleDisplay)gui).rW.graphGUIL))
                {
                    allSame = true;
                    var nBool = ((ruleNode)firstNode).containsAllLocalLabels;
                    for (var i = 1; i < nodes.Count; i++)
                        if (nBool != ((ruleNode)nodes[i]).containsAllLocalLabels)
                        {
                            allSame = false;
                            break;
                        }
                    if (allSame)
                        chkContainsLocalLabels.IsChecked = nBool;
                    else chkContainsLocalLabels.IsChecked = null;

                    allSame = true;
                    nBool = ((ruleNode)firstNode).strictDegreeMatch;
                    for (var i = 1; i < nodes.Count; i++)
                        if (nBool != ((ruleNode)nodes[i]).strictDegreeMatch)
                        {
                            allSame = false;
                            break;
                        }
                    if (allSame)
                        chkStrictDegreeMatch.IsChecked = nBool;
                    else chkStrictDegreeMatch.IsChecked = null;

                    allSame = true;
                    nBool = ((ruleNode)firstNode).NotExist;
                    for (var i = 1; i < nodes.Count; i++)
                        if (nBool != ((ruleNode)nodes[i]).NotExist)
                        {
                            allSame = false;
                            break;
                        }
                    if (allSame)
                        chkNotExist.IsChecked = nBool;
                    else chkNotExist.IsChecked = null;

                    txtNegLabels.IsEnabled = false;
                    txtNegLabels.Text = "<multiple nodes>";
                    if (!stackNodeProps.Children.Contains(wrapRuleBools))
                        stackNodeProps.Children.Add(wrapRuleBools);
                    if (!stackNodeProps.Children.Contains(gridRuleNegLabels))
                        stackNodeProps.Children.Add(gridRuleNegLabels);
                }
                else
                {
                    stackNodeProps.Children.Remove(wrapRuleBools);
                    stackNodeProps.Children.Remove(gridRuleNegLabels);
                }
            }
        }

        #endregion

        private node firstNode
        {
            get
            {
                if (nodes.Count > 0) return nodes[0];
                else return null;
            }
        }

    }
}