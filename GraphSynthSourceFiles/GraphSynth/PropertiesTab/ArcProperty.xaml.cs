using System.Collections.Generic;
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
    ///   Interaction logic for ArcProperty.xaml
    /// </summary>
    public partial class ArcProperty : UserControl
    {
        private ArcIconShape arcIcon;
        private List<arc> arcs;

        private designGraph graph;
        private GraphGUI gui;

        public ArcProperty()
        {
            InitializeComponent();
        }

        #region Events

        #region Labels

        private void txtLabels_LostFocus(object sender, RoutedEventArgs e)
        {
            if (arcs.Count == 0) return;
            MultiBindingExpression mbe;
            var caretIndex = txtLabels.CaretIndex;
            var origLength = txtLabels.Text.Length;
            var oldLabels = firstArc.localLabels;
            var newLabels = StringCollectionConverter.convert(txtLabels.Text);

            if ((typeof(RuleDisplay).IsInstanceOfType(gui)) &&
                (gui == ((RuleDisplay)gui).rW.graphGUIK))
            {
                var rW = ((RuleDisplay)gui).rW;
                var Larc = rW.rule.L.arcs.Find(b => string.Equals(b.name, firstArc.name));
                var Rarc = rW.rule.R.arcs.Find(b => string.Equals(b.name, firstArc.name));

                var removedKLabels = oldLabels.Where(a => !newLabels.Contains(a)).ToList();
                foreach (string a in removedKLabels)
                {
                    Larc.localLabels.Remove(a);
                    Rarc.localLabels.Remove(a);
                }
                var newLLabels = new List<string>(Larc.localLabels.Union(newLabels));
                Larc.localLabels.Clear();
                foreach (string a in newLLabels) Larc.localLabels.Add(a);
                var nI = ((ArcShape)Larc.DisplayShape.Shape).icon;
                mbe = BindingOperations.GetMultiBindingExpression(nI, IconShape.DisplayTextProperty);
                mbe.UpdateTarget();

                var newRLabels = new List<string>(Rarc.localLabels.Union(newLabels));
                Rarc.localLabels.Clear();
                foreach (string a in newRLabels) Rarc.localLabels.Add(a);
                nI = ((ArcShape)Rarc.DisplayShape.Shape).icon;
                mbe = BindingOperations.GetMultiBindingExpression(nI, IconShape.DisplayTextProperty);
                mbe.UpdateTarget();
            }
            else if (typeof(RuleDisplay).IsInstanceOfType(gui))
            {
                // this is a rule LHS or RHS
                var rW = ((RuleDisplay)gui).rW;
                arc otherArc = null;
                if (gui == rW.graphGUIL)
                    otherArc = rW.rule.R.arcs.Find(delegate(arc b) { return string.Equals(b.name, firstArc.name); });
                else
                    otherArc = rW.rule.L.arcs.Find(delegate(arc b) { return string.Equals(b.name, firstArc.name); });
                if (otherArc != null)
                {
                    var Klabels = new List<string>(otherArc.localLabels.Intersect(newLabels));
                    otherArc =
                        rW.graphGUIK.graph.arcs.Find(delegate(arc b) { return string.Equals(b.name, firstArc.name); });
                    otherArc.localLabels.Clear();
                    foreach (string a in Klabels) otherArc.localLabels.Add(a);
                    var nI = ((ArcShape)otherArc.DisplayShape.Shape).icon;
                    mbe = BindingOperations.GetMultiBindingExpression(nI, IconShape.DisplayTextProperty);
                    mbe.UpdateTarget();
                }
            }
            firstArc.localLabels.Clear();
            foreach (string a in newLabels) firstArc.localLabels.Add(a);
            mbe = BindingOperations.GetMultiBindingExpression(arcIcon, IconShape.DisplayTextProperty);
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
            if (arcs.Count == 0) return;
            var caretIndex = txtVariables.CaretIndex;
            var origLength = txtVariables.Text.Length;
            var oldVars = firstArc.localVariables;
            var newVars = DoubleCollectionConverter.convert(txtVariables.Text);

            if ((typeof(RuleDisplay).IsInstanceOfType(gui)) &&
                (gui == ((RuleDisplay)gui).rW.graphGUIK))
            {
                var rW = ((RuleDisplay)gui).rW;
                var Larc = rW.rule.L.arcs.Find(b => string.Equals(b.name, firstArc.name));
                var Rarc = rW.rule.R.arcs.Find(b => string.Equals(b.name, firstArc.name));

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
            else if (typeof(RuleDisplay).IsInstanceOfType(gui))
            {
                // this is a rule LHS or RHS
                var rW = ((RuleDisplay)gui).rW;
                arc otherArc = null;
                if (gui == rW.graphGUIL)
                    otherArc = rW.rule.R.arcs.Find(b => string.Equals(b.name, firstArc.name));
                else
                    otherArc = rW.rule.L.arcs.Find(b => string.Equals(b.name, firstArc.name));
                if (otherArc != null)
                {
                    var KVars = new List<double>(otherArc.localVariables.Intersect(newVars));
                    otherArc =
                        rW.graphGUIK.graph.arcs.Find(b => string.Equals(b.name, firstArc.name));
                    otherArc.localVariables.Clear();
                    foreach (double a in KVars) otherArc.localVariables.Add(a);
                }
            }
            firstArc.localVariables.Clear();
            foreach (double d in newVars)
                firstArc.localVariables.Add(d);
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
            if (!(typeof(ruleArc)).IsInstanceOfType(firstArc)
                || (e.Key == Key.Return) || (e.Key == Key.Enter))
                btnConfirm_Click(sender, null);
        }

        private void txtName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!(typeof(ruleArc)).IsInstanceOfType(firstArc))
            {
                btnConfirm_Click(sender, null);
                Update();
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            gui.ArcNameChanged(firstArc, txtName.Text);
            var binding
                = BindingOperations.GetMultiBindingExpression(arcIcon, IconShape.DisplayTextProperty);
            binding.UpdateTarget();
            Update();
        }

        #endregion

        #region Negate Labels

        private void txtNegLabels_LostFocus(object sender, RoutedEventArgs e)
        {
            var lststr = StringCollectionConverter.convert(txtNegLabels.Text);
            ((ruleArc)firstArc).negateLabels.Clear();
            foreach (string str in lststr)
                ((ruleArc)firstArc).negateLabels.Add(str);
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
            foreach (arc a in arcs)
            {
                a.XmlArcType = txtArcType.Text;
                /* make sure node is of right type - if not call the replacement function */
                if ((a.arcType != null) && (a.GetType() != typeof(ruleArc))
                    && (a.GetType() != a.arcType))
                    graph.replaceArcWithInheritedType(a);

            }
            Update();
        }

        #endregion

        #region Arc Booleans

        private void chkDirected_Click(object sender, RoutedEventArgs e)
        {
            if ((chkDirected.IsChecked == null) || (chkDirected.IsChecked.Value == false))
                foreach (arc a in arcs)
                {
                    a.directed = false;
                    ((ArcShape)a.DisplayShape.Shape).directed = false;
                    gui.ArcPropertyChanged(a);
                }
            else
                foreach (arc a in arcs)
                {
                    /* this next line is a bit strange. but I want to make sure that if an
                     * arc is switching from undirected to directed, it shows up this way on 
                     * the screen. The user can always click to hide the arrow heads - I feel
                     * this is the better UI decision. The reason it is done first is that we want
                     * to only do this for arcs that are switching from undirected to directed. */
                    if (!a.directed) ((ArcShape)a.DisplayShape.Shape).ShowArrowHeads = true;
                    a.directed = true;
                    ((ArcShape)a.DisplayShape.Shape).directed = true;
                    gui.ArcPropertyChanged(a);
                }
            Update();
        }

        private void chkDoublyDirected_Click(object sender, RoutedEventArgs e)
        {
            if ((chkDoublyDirected.IsChecked == null) || (chkDoublyDirected.IsChecked.Value == false))
                foreach (arc a in arcs)
                {
                    a.doublyDirected = false;
                    ((ArcShape)a.DisplayShape.Shape).doublyDirected = false;
                    gui.ArcPropertyChanged(a);
                }
            else
                foreach (arc a in arcs)
                {
                    a.doublyDirected = true;
                    ((ArcShape)a.DisplayShape.Shape).doublyDirected = true;
                    gui.ArcPropertyChanged(a);
                }
            Update();
        }

        #endregion

        #region RuleArc Booleans

        private void chkContainsLocalLabels_Click(object sender, RoutedEventArgs e)
        {
            if ((chkContainsLocalLabels.IsChecked == null) || (chkContainsLocalLabels.IsChecked.Value == false))
                foreach (arc a in arcs)
                    ((ruleArc)a).containsAllLocalLabels = false;
            else
                foreach (arc a in arcs)
                    ((ruleArc)a).containsAllLocalLabels = true;
            Update();
        }

        private void chkDirectionIsEqual_Click(object sender, RoutedEventArgs e)
        {
            if ((chkDirectionIsEqual.IsChecked == null) || (chkDirectionIsEqual.IsChecked.Value == false))
                foreach (arc a in arcs)
                    ((ruleArc)a).directionIsEqual = false;
            else
                foreach (arc a in arcs)
                    ((ruleArc)a).directionIsEqual = true;
            Update();
        }

        private void chkNullMeansNull_Click(object sender, RoutedEventArgs e)
        {
            if ((chkNullMeansNull.IsChecked == null) || (chkNullMeansNull.IsChecked.Value == false))
                foreach (arc a in arcs)
                    ((ruleArc)a).nullMeansNull = true;
            else
                foreach (arc a in arcs)
                    ((ruleArc)a).nullMeansNull = false;
            Update();
        }

        private void chkNotExist_Click(object sender, RoutedEventArgs e)
        {
            if ((chkNotExist.IsChecked == null) || (chkNotExist.IsChecked.Value == false))
                foreach (arc a in arcs)
                    ((ruleArc)a).NotExist = false;
            else
                foreach (arc a in arcs)
                    ((ruleArc)a).NotExist = true;
            Update();
        }

        #endregion

        #endregion

        #region Update Methods

        internal void Update(List<arc> _arcs, designGraph _graph, GraphGUI _gui)
        {
            arcs = _arcs;
            graph = _graph;
            gui = _gui;
            arcIcon = ((ArcShape)firstArc.DisplayShape.Shape).icon;
            Update();
        }

        private void Update()
        {
            if (arcs.Count == 1)
            {
                txtName.IsEnabled = txtLabels.IsEnabled = txtVariables.IsEnabled = true;
                txtName.Text = firstArc.name;
                if (typeof(ruleArc).IsInstanceOfType(firstArc))
                    btnConfirm.Visibility = Visibility.Visible;
                else btnConfirm.Visibility = Visibility.Hidden;
                txtLabels.Text = StringCollectionConverter.convert(firstArc.localLabels);
                txtVariables.Text = DoubleCollectionConverter.convert(firstArc.localVariables);

                txtArcType.IsEnabled = true;
                txtArcType.Text = firstArc.GetType().ToString();
                chkDirected.IsChecked = firstArc.directed;
                chkDoublyDirected.IsChecked = firstArc.doublyDirected;

                if ((typeof(RuleDisplay).IsInstanceOfType(gui))
                    && (gui == ((RuleDisplay)gui).rW.graphGUIL))
                {
                    chkContainsLocalLabels.IsChecked = ((ruleArc)firstArc).containsAllLocalLabels;
                    chkDirectionIsEqual.IsChecked = ((ruleArc)firstArc).directionIsEqual;
                    chkNullMeansNull.IsChecked = ((ruleArc)firstArc).nullMeansNull;
                    chkNotExist.IsChecked = ((ruleArc)firstArc).NotExist;
                    txtNegLabels.IsEnabled = true;
                    txtNegLabels.Text
                        = StringCollectionConverter.convert(((ruleArc)firstArc).negateLabels);
                    if (!stackArcProps.Children.Contains(wrapRuleBools))
                        stackArcProps.Children.Add(wrapRuleBools);
                    if (!stackArcProps.Children.Contains(gridRuleNegLabels))
                        stackArcProps.Children.Add(gridRuleNegLabels);
                }
                else
                {
                    stackArcProps.Children.Remove(wrapRuleBools);
                    stackArcProps.Children.Remove(gridRuleNegLabels);
                }
            }
            else
            {
                txtName.Text = "<multiple arcs>";
                txtLabels.Text = "<multiple arcs>";
                txtVariables.Text = "<multiple arcs>";
                txtName.IsEnabled = txtLabels.IsEnabled = txtVariables.IsEnabled = false;

                var allSame = true;
                var aType = firstArc.GetType();
                for (var i = 1; i < arcs.Count; i++)
                    if (!aType.Equals(arcs[i].GetType()))
                    {
                        allSame = false;
                        break;
                    }
                if (allSame)
                {
                    txtArcType.IsEnabled = true;
                    txtArcType.Text = aType.ToString();
                }
                else
                {
                    txtArcType.IsEnabled = false;
                    txtArcType.Text = "<multiple types>";
                }


                allSame = true;
                var aBool = firstArc.directed;
                for (var i = 1; i < arcs.Count; i++)
                    if (aBool != arcs[i].directed)
                    {
                        allSame = false;
                        break;
                    }
                if (allSame)
                    chkDirected.IsChecked = aBool;
                else chkDirected.IsChecked = null;

                allSame = true;
                aBool = firstArc.doublyDirected;
                for (var i = 1; i < arcs.Count; i++)
                    if (aBool != arcs[i].doublyDirected)
                    {
                        allSame = false;
                        break;
                    }
                if (allSame)
                    chkDoublyDirected.IsChecked = aBool;
                else chkDoublyDirected.IsChecked = null;

                if ((typeof(RuleDisplay).IsInstanceOfType(gui))
                    && (gui == ((RuleDisplay)gui).rW.graphGUIL))
                {
                    allSame = true;
                    aBool = ((ruleArc)firstArc).containsAllLocalLabels;
                    for (var i = 1; i < arcs.Count; i++)
                        if (aBool != ((ruleArc)arcs[i]).containsAllLocalLabels)
                        {
                            allSame = false;
                            break;
                        }
                    if (allSame)
                        chkContainsLocalLabels.IsChecked = aBool;
                    else chkContainsLocalLabels.IsChecked = null;

                    allSame = true;
                    aBool = ((ruleArc)firstArc).directionIsEqual;
                    for (var i = 1; i < arcs.Count; i++)
                        if (aBool != ((ruleArc)arcs[i]).directionIsEqual)
                        {
                            allSame = false;
                            break;
                        }
                    if (allSame)
                        chkDirectionIsEqual.IsChecked = aBool;
                    else chkDirectionIsEqual.IsChecked = null;

                    allSame = true;
                    aBool = ((ruleArc)firstArc).nullMeansNull;
                    for (var i = 1; i < arcs.Count; i++)
                        if (aBool != ((ruleArc)arcs[i]).nullMeansNull)
                        {
                            allSame = false;
                            break;
                        }
                    if (allSame)
                        chkNullMeansNull.IsChecked = aBool;
                    else chkNullMeansNull.IsChecked = null;

                    allSame = true;
                    aBool = ((ruleArc)firstArc).NotExist;
                    for (var i = 1; i < arcs.Count; i++)
                        if (aBool != ((ruleArc)arcs[i]).NotExist)
                        {
                            allSame = false;
                            break;
                        }
                    if (allSame)
                        chkNotExist.IsChecked = aBool;
                    else chkNotExist.IsChecked = null;

                    txtNegLabels.IsEnabled = false;
                    txtNegLabels.Text = "<multiple arcs>";
                    if (!stackArcProps.Children.Contains(wrapRuleBools))
                        stackArcProps.Children.Add(wrapRuleBools);
                    if (!stackArcProps.Children.Contains(gridRuleNegLabels))
                        stackArcProps.Children.Add(gridRuleNegLabels);
                }
                else
                {
                    stackArcProps.Children.Remove(wrapRuleBools);
                    stackArcProps.Children.Remove(gridRuleNegLabels);
                }
            }
        }

        #endregion

        private arc firstArc
        {
            get { return arcs[0]; }
        }
    }
}