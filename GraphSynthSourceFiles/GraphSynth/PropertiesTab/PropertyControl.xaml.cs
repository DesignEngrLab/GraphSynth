using System;
using System.Windows;
using System.Windows.Controls;
using GraphSynth.GraphDisplay;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for PropertyControl.xaml. This is the main function for the Properties Tab.
    /// </summary>
    public partial class PropertyControl : UserControl
    {
        private designGraph Graph;
        private GraphGUI GraphGUI;
        private grammarRule Rule;
        private ruleSetWindow rsW;
        private ruleSet RuleSet;
        private CanvasProperty _canvasProp;
        private graphWindow gW;
        private ruleWindow rW;
        private SelectionClass selection;

        # region Methods

        #region Hide And Show Expanders

        // this hides the graph property 
        private void HideAllSections()
        {
            HideSection(ComboBoxNodeArcSelector);
            HideSection(expNodeProperties);
            HideSection(expNodeDisplayProperties);
            HideSection(expArcProperties);
            HideSection(expArcDisplayProperties);
            HideSection(expHyperArcProperties);
            HideSection(expHyperArcDisplayProperties);
            HideSection(expGraphProperties);
            HideSection(expRuleProperties);
            HideSection(expFreeArcEmbedRules);
            HideSection(expRuleSetProperties);
            HideSection(expCanvasProperties);
        }

        private void HideSection(UIElement c)
        {
            stackAllProps.Children.Remove(c);
        }

        private void ShowAndCollapseSection(Control c)
        {
            if (!stackAllProps.Children.Contains(c))
                stackAllProps.Children.Add(c);
            if (c is Expander)
                ((Expander)c).IsExpanded = false;
        }

        private void ShowAndExpandSection(Control c)
        {
            if (!stackAllProps.Children.Contains(c))
                stackAllProps.Children.Add(c);
            if (c is Expander)
                ((Expander)c).IsExpanded = true;
        }

        #endregion

        /// <summary>
        ///   Updates the selected objects properties. This is called by the GraphGUI and SelectedObjects
        ///   properties shown above.
        /// </summary>
        public void Update()
        {
            HideAllSections();
            main.windowsMgr.ExpandOrCollapse();
            if ((typeof(graphWindow)).IsInstanceOfType(main.windowsMgr.activeWindow))
            {
                #region Update Graph Properties

                gW = (graphWindow)main.windowsMgr.activeWindow;
                rW = null;
                GraphGUI = ComboBoxNodeArcSelector.graphGUI = gW.graphGUI;
                CanvasProp = gW.canvasProps;
                Graph = gW.graph;
                Rule = null;
                RuleSet = null;
                selection = GraphGUI.Selection;
                ShowAndExpandSection(ComboBoxNodeArcSelector);
                ShowAndExpandGraphElementProperties(selection, Graph, GraphGUI);

                if ((selection.selectedNodes.Count == 0)
                    && (selection.selectedArcs.Count == 0)
                    && (selection.selectedHyperArcs.Count == 0))
                {
                    ShowAndExpandSection(expGraphProperties);
                    GraphPrpt.Update(Graph, gW);
                    ShowAndExpandSection(expCanvasProperties);
                }
                else
                {
                    ShowAndCollapseSection(expGraphProperties);
                    GraphPrpt.Update(Graph, gW);
                    ShowAndCollapseSection(expCanvasProperties);
                }

                #endregion
            }
            else if ((typeof(ruleWindow)).IsInstanceOfType(main.windowsMgr.activeWindow))
            {
                #region Update Rule Properties

                gW = null;
                rW = (ruleWindow)main.windowsMgr.activeWindow;
                if ((main.windowsMgr.activeGraphCanvas == rW.graphCanvasK)
                    || (main.windowsMgr.activeGraphCanvas == rW.graphCanvasL)
                    || (main.windowsMgr.activeGraphCanvas == rW.graphCanvasR))
                    GraphGUI = ComboBoxNodeArcSelector.graphGUI = main.windowsMgr.activeGraphCanvas;
                else GraphGUI = ComboBoxNodeArcSelector.graphGUI = rW.graphCanvasK;
                Graph = GraphGUI.graph;
                CanvasProp = rW.canvasProps;
                Rule = rW.rule;
                RuleSet = null;
                selection = GraphGUI.Selection;

                ShowAndExpandSection(ComboBoxNodeArcSelector);
                ShowAndExpandGraphElementProperties(selection, Graph, GraphGUI);
                if ((selection.selectedNodes.Count == 0)
                    && (selection.selectedArcs.Count == 0)
                    && (selection.selectedHyperArcs.Count == 0))
                {
                    ShowAndExpandSection(expRuleProperties);
                    RulePrpt.Update(Rule, rW);
                    ShowAndExpandSection(expFreeArcEmbedRules);
                    FreeArcEmbedRulePrpt.Update(Rule);
                    ShowAndExpandSection(expCanvasProperties);
                }
                else
                {
                    ShowAndCollapseSection(expRuleProperties);
                    RulePrpt.Update(Rule, rW);
                    ShowAndCollapseSection(expFreeArcEmbedRules);
                    FreeArcEmbedRulePrpt.Update(Rule);
                    ShowAndCollapseSection(expCanvasProperties);
                }

                #endregion
            }
            else if ((typeof(ruleSetWindow)).IsInstanceOfType(main.windowsMgr.activeWindow))
            {
                gW = null;
                rW = null;
                GraphGUI = null;
                Graph = null;
                Rule = null;
                rsW = ((ruleSetWindow)main.windowsMgr.activeWindow);
                RuleSet = rsW.ruleset;
                selection = null;

                ShowAndExpandSection(expRuleSetProperties);
                RuleSetPrpt.Update(RuleSet, rsW);
            }
            else
            {
                gW = null;
                rW = null;
                GraphGUI = null;
                Graph = null;
                Rule = null;
                RuleSet = null;
                selection = null;
            }
        }

        private void ShowAndExpandGraphElementProperties(SelectionClass selection, designGraph Graph, GraphGUI GraphGui)
        {
            if (selection.selectedArcs.Count > 0)
            {
                if (selection.selectedArcs.Count == 1)
                {
                    ShowAndExpandSection(expArcProperties);
                    ShowAndExpandSection(expArcDisplayProperties);
                }
                else
                {
                    ShowAndCollapseSection(expArcProperties);
                    ShowAndCollapseSection(expArcDisplayProperties);
                }
                ArcPrpt.Update(selection.selectedArcs, Graph, GraphGUI);
                ArcDispPrpt.Update(selection.selectedArcs, GraphGUI);
            }
            if (selection.selectedHyperArcs.Count > 0)
            {
                if (selection.selectedHyperArcs.Count == 1)
                {
                    ShowAndExpandSection(expHyperArcProperties);
                    ShowAndExpandSection(expHyperArcDisplayProperties);
                }
                else
                {
                    ShowAndCollapseSection(expHyperArcProperties);
                    ShowAndCollapseSection(expHyperArcDisplayProperties);
                }
                HyperArcPrpt.Update(selection.selectedHyperArcs, Graph, GraphGUI);
                HyperArcDispPrpt.Update(selection.selectedHyperArcs, GraphGUI);
            }
            if (selection.selectedNodes.Count > 0)
            {
                if ((selection.selectedNodes.Count == 1)
                    && (selection.selectedArcs.Count != 1))
                {
                    /* open up node properties if there's only one, but not if
                   * you've already opened for the single selected arc. Why should
                   * arc get preference here? it's just because it's harder to select
                   * an arc. */
                    ShowAndExpandSection(expNodeProperties);
                    ShowAndExpandSection(expNodeDisplayProperties);
                }
                else
                {
                    ShowAndCollapseSection(expNodeProperties);
                    ShowAndCollapseSection(expNodeDisplayProperties);
                }
                NodePrpt.Update(selection.selectedNodes, Graph, GraphGUI);
                NodeDispPrpt.Update(selection.selectedNodes, GraphGUI);
            }
        }

        #endregion

        public PropertyControl()
        {
            InitializeComponent();
        }

        public MainWindow main
        {
            get { return GSApp.main; }
        }

        public CanvasProperty CanvasProp
        {
            get { return _canvasProp; }
            set
            {
                _canvasProp = value;
                expCanvasProperties.Content = _canvasProp;
            }
        }
    }
}