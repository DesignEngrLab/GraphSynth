using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMainWindow
    {
        public CanvasProperty SeedCanvas { get; set; }


        #region SetActive

        public void SetActiveAsSeedOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var activeWin = windowsMgr.activeWindow;
            var activeGD = ((graphWindow)activeWin).graphGUI;
            if ((GSApp.settings.seed == null) || (GSApp.settings.seed == activeGD.graph) ||
                (MessageBoxResult.Yes == MessageBox.Show("The graph "
                                                         + GSApp.settings.seed.name
                                                         + " is already loaded as the seed."
                                                         + " Replace it with the active graph?", "Seed already defined.",
                                                         MessageBoxButton.YesNo, MessageBoxImage.Information)))
            {
                GSApp.settings.seed = activeGD.graph;
                GSApp.settings.DefaultSeedFileName = ((graphWindow) activeWin).filename;
                SeedCanvas = ((graphWindow)activeWin).canvasProps;
            }
        }

        public void SetActiveAsSeedCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (windowsMgr.activeWindow is graphWindow);
        }

        private void DesignDropDown_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (GSApp.settings.seed != null)
            {
                var tb = new TextBlock
                    {
                        Text = "Set Active as Seed: " + GSApp.settings.seed.name,
                        FontWeight = FontWeights.Bold
                //tb.FontSize = 9f;
                    };
                SetActiveAsSeedMenuItem.Header = tb;
            }
            else
            {
                var tb = new TextBlock();
                tb.Text = "Set Active as Seed";
                tb.FontWeight = FontWeights.Regular;
                SetActiveAsSeedMenuItem.Header = tb;
            }
            for (var i = 0; i != GSApp.settings.rulesets.GetLength(0); i++)
            {
                if (GSApp.settings.rulesets[i] != null)
                {
                    var tb = new TextBlock();
                    tb.Text = "Set Active as Rule Set #" + i + ": " +
                              ((ruleSet)GSApp.settings.rulesets[i]).name;
                    tb.FontWeight = FontWeights.Bold;
                    ((MenuItem)DesignDropDown.Items[1 + i]).Header = tb;
                }
                else
                {
                    var tb = new TextBlock();
                    tb.Text = "Set Active as Rule Set #" + i;
                    tb.FontWeight = FontWeights.Regular;
                    ((MenuItem)DesignDropDown.Items[1 + i]).Header = tb;
                }
            }
        }

        public void SetActiveAsRuleSet0OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (GSApp.settings.numOfRuleSets < 1)
            {
                GSApp.settings.numOfRuleSets = 1;
                MessageBox.Show("There were no rulesets allocated in Settings. This has now been changed to 1"
                                +
                                " (Edit-->Settings->Seed & Rules->Number of Rulesets). You may want to save settings to avoid"
                                + " this happening in the future.", "May need to save settings.", MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            GSApp.settings.rulesets = new ruleSet[GSApp.settings.numOfRuleSets];
            defineRuleSet(0);
        }

        public void SetActiveAsRuleSet0CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (windowsMgr.activeWindow is ruleSetWindow);
        }

        public void SetActiveAsRuleSet1OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            defineRuleSet(1);
        }

        public void SetActiveAsRuleSet1CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((windowsMgr.activeWindow != null)
                            && (windowsMgr.activeWindow is ruleSetWindow)
                            && (GSApp.settings.numOfRuleSets > 1));
            if (GSApp.settings.numOfRuleSets > 1)
            {
                setRuleSet1toolStripMenuItem.Visibility = Visibility.Visible;
                setRuleSet1toolStripMenuItem.Height = 25;
            }
            else
            {
                setRuleSet1toolStripMenuItem.Visibility = Visibility.Hidden;
                setRuleSet1toolStripMenuItem.Height = 0;
            }
        }

        public void SetActiveAsRuleSet2OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            defineRuleSet(2);
        }

        public void SetActiveAsRuleSet2CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((windowsMgr.activeWindow != null)
                            && (windowsMgr.activeWindow is ruleSetWindow)
                            && (GSApp.settings.numOfRuleSets > 2));
            if (GSApp.settings.numOfRuleSets > 2)
            {
                setRuleSet2toolStripMenuItem.Visibility = Visibility.Visible;
                setRuleSet2toolStripMenuItem.Height = 25;
            }
            else
            {
                setRuleSet2toolStripMenuItem.Visibility = Visibility.Hidden;
                setRuleSet2toolStripMenuItem.Height = 0;
            }
        }

        public void SetActiveAsRuleSet3OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            defineRuleSet(3);
        }

        public void SetActiveAsRuleSet3CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((windowsMgr.activeWindow != null)
                            && (windowsMgr.activeWindow is ruleSetWindow)
                            && (GSApp.settings.numOfRuleSets > 3));
            if (GSApp.settings.numOfRuleSets > 3)
            {
                setRuleSet3toolStripMenuItem.Visibility = Visibility.Visible;
                setRuleSet3toolStripMenuItem.Height = 25;
            }
            else
            {
                setRuleSet3toolStripMenuItem.Visibility = Visibility.Hidden;
                setRuleSet3toolStripMenuItem.Height = 0;
            }
        }

        public void SetActiveAsRuleSet4OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            defineRuleSet(4);
        }

        public void SetActiveAsRuleSet4CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((windowsMgr.activeWindow != null)
                            && (windowsMgr.activeWindow is ruleSetWindow)
                            && (GSApp.settings.numOfRuleSets > 4));
            if (GSApp.settings.numOfRuleSets > 4)
            {
                setRuleSet4toolStripMenuItem.Visibility = Visibility.Visible;
                setRuleSet4toolStripMenuItem.Height = 25;
            }
            else
            {
                setRuleSet4toolStripMenuItem.Visibility = Visibility.Hidden;
                setRuleSet4toolStripMenuItem.Height = 0;
            }
        }

        public void SetActiveAsRuleSet5OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            defineRuleSet(5);
        }

        public void SetActiveAsRuleSet5CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((windowsMgr.activeWindow != null)
                            && (windowsMgr.activeWindow is ruleSetWindow)
                            && (GSApp.settings.numOfRuleSets > 5));
            if (GSApp.settings.numOfRuleSets > 5)
            {
                setRuleSet5toolStripMenuItem.Visibility = Visibility.Visible;
                setRuleSet5toolStripMenuItem.Height = 25;
            }
            else
            {
                setRuleSet5toolStripMenuItem.Visibility = Visibility.Hidden;
                setRuleSet5toolStripMenuItem.Height = 0;
            }
        }

        public void SetActiveAsRuleSet6OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            defineRuleSet(6);
        }

        public void SetActiveAsRuleSet6CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((windowsMgr.activeWindow != null)
                            && (windowsMgr.activeWindow is ruleSetWindow)
                            && (GSApp.settings.numOfRuleSets > 6));
            if (GSApp.settings.numOfRuleSets > 6)
            {
                setRuleSet6toolStripMenuItem.Visibility = Visibility.Visible;
                setRuleSet6toolStripMenuItem.Height = 25;
            }
            else
            {
                setRuleSet6toolStripMenuItem.Visibility = Visibility.Hidden;
                setRuleSet6toolStripMenuItem.Height = 0;
            }
        }

        public void SetActiveAsRuleSet7OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            defineRuleSet(7);
        }

        public void SetActiveAsRuleSet7CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((windowsMgr.activeWindow != null)
                            && (windowsMgr.activeWindow is ruleSetWindow)
                            && (GSApp.settings.numOfRuleSets > 7));
            if (GSApp.settings.numOfRuleSets > 7)
            {
                setRuleSet7toolStripMenuItem.Visibility = Visibility.Visible;
                setRuleSet7toolStripMenuItem.Height = 25;
            }
            else
            {
                setRuleSet7toolStripMenuItem.Visibility = Visibility.Hidden;
                setRuleSet7toolStripMenuItem.Height = 0;
            }
        }

        public void SetActiveAsRuleSet8OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            defineRuleSet(8);
        }

        public void SetActiveAsRuleSet8CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((windowsMgr.activeWindow != null)
                            && (windowsMgr.activeWindow is ruleSetWindow)
                            && (GSApp.settings.numOfRuleSets > 8));
            if (GSApp.settings.numOfRuleSets > 8)
            {
                setRuleSet8toolStripMenuItem.Visibility = Visibility.Visible;
                setRuleSet8toolStripMenuItem.Height = 25;
            }
            else
            {
                setRuleSet8toolStripMenuItem.Visibility = Visibility.Hidden;
                setRuleSet8toolStripMenuItem.Height = 0;
            }
        }

        public void SetActiveAsRuleSet9OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            defineRuleSet(9);
        }

        public void SetActiveAsRuleSet9CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((windowsMgr.activeWindow != null)
                            && (windowsMgr.activeWindow is ruleSetWindow)
                            && (GSApp.settings.numOfRuleSets > 9));
            if (GSApp.settings.numOfRuleSets > 9)
            {
                setRuleSet9toolStripMenuItem.Visibility = Visibility.Visible;
                setRuleSet9toolStripMenuItem.Height = 25;
            }
            else
            {
                setRuleSet9toolStripMenuItem.Visibility = Visibility.Hidden;
                setRuleSet9toolStripMenuItem.Height = 0;
            }
        }

        private void defineRuleSet(int index)
        {
            var activeRSC = (ruleSetWindow)windowsMgr.activeWindow;
            if ((GSApp.settings.rulesets[index] == null) || (GSApp.settings.rulesets[index] == activeRSC.Ruleset) ||
                (MessageBoxResult.Yes == MessageBox.Show("The ruleset " +
                                                         ((ruleSet)GSApp.settings.rulesets[index]).name +
                                                         " is already loaded into rule set #"
                                                         + index + ". Replace with active ruleset?",
                                                         "RuleSet already defined.",
                                                         MessageBoxButton.YesNo, MessageBoxImage.Information)))
            {
                GSApp.settings.rulesets[index] = activeRSC.Ruleset;
                activeRSC.Ruleset.RuleSetIndex = index;
            }
        }


        public void ClearAllRuleSetsAndSeedOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            GSApp.settings.seed = null;
            GSApp.settings.DefaultSeedFileName = "";
            for (var i = 0; i != GSApp.settings.numOfRuleSets; i++)
                GSApp.settings.rulesets[i] = null;
        }

        public void ClearAllRuleSetsAndSeedCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((GSApp.settings.seed != null)
                            || (GSApp.settings.rulesets.Any(s => s != null)));
        }

        #endregion

        #region Run Search Process

        public void RunSearchProcessCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var i = SearchCommands.IndexOf(e.Command as RoutedUICommand);
            if (i < 0 || i > SearchAlgorithms.Count) e.CanExecute = false;
            else
            {
                var s = SearchAlgorithms[i];
                var numRuleSets = 0;
                while ((numRuleSets < GSApp.settings.rulesets.GetLength(0)) &&
                       (GSApp.settings.rulesets[numRuleSets] != null))
                {
                    numRuleSets++;
                }
                e.CanExecute = ((!s.RequireSeed || (s.RequireSeed && GSApp.settings.seed != null))
                                && (s.RequiredNumRuleSets <= numRuleSets));
            }
        }

        public void RunSearchProcessOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var i = SearchCommands.IndexOf(e.Command as RoutedUICommand);
            if (i < 0 || i > SearchAlgorithms.Count)
                MessageBox.Show("No Search Process found for " + e.Command);
            else
                addAndShowSearchController(new searchProcessController(SearchAlgorithms[i],
                                                                       windowsMgr.SearchProcessID),
                                           (GSApp.settings.SearchControllerPlayOnStart || SearchAlgorithms[i].AutoPlay));
        }

        #endregion

        /***** This file includes all the functions (event handlers) related to the
         * Design Drop down menu.  **********/

    }
}