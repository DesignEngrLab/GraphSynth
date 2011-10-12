using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GraphSynth.Representation;
using Microsoft.Win32;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for ruleSetWindow.xaml
    /// </summary>
    public partial class ruleSetWindow : Window
    {
        private readonly List<int> Deselected = new List<int>();
        private bool Select;
        public string filename;
        public ruleSet ruleset;
        private Boolean saveRulesToo;

        private MainWindow main
        {
            get { return GSApp.main; }
        }

        #region Constructor
        public ruleSetWindow(ruleSet rs, string filename, string title)
        {
            try
            {
                /* the following is common to all GS window types. */
                InitializeComponent();
                Owner = main;
                ShowInTaskbar = false;
                foreach (CommandBinding cb in main.CommandBindings)
                    CommandBindings.Add(cb);
                foreach (InputBinding ib in main.InputBindings)
                    InputBindings.Add(ib);
                /***************************************************/
                ruleset = rs;
                this.filename = !string.IsNullOrEmpty(filename) ? filename : "Untitled";
                Title = !string.IsNullOrEmpty(title) ? title : Path.GetFileNameWithoutExtension(this.filename);

                listBoxOfRules.Items.Clear();
                for (var i = 1; i <= ruleset.rules.Count; i++)
                {
                    var li = new ListBoxItem
                                 {
                                     Content = i + ". " + ruleset.ruleFileNames[(i - 1)]
                                 };
                    listBoxOfRules.Items.Add(li);
                }
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        #endregion

        private void BecomeActiveSubWindow(object sender, EventArgs e)
        {
            main.windowsMgr.SetAsActive(this);
            main.propertyUpdate();
        }

        private void btnCheckAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                listBoxOfRules.SelectAll();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                listBoxOfRules.UnselectAll();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private void btnAddRule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] filenames;

                var fileChooser = new OpenFileDialog();
                fileChooser.Title = "Add rules";
                fileChooser.InitialDirectory = GSApp.settings.RulesDirAbs;
                fileChooser.Filter = "grammar rule file (*.grxml)|*.grxml|xml file (*.xml)|*.xml";
                fileChooser.Multiselect = true;
                if ((Boolean)fileChooser.ShowDialog(this))
                {
                    filenames = fileChooser.FileNames;

                    if (filenames.GetLength(0) == 0)
                        MessageBox.Show("Error in rule loading. RuleSet unchanged",
                                        "Error in rule loading.", MessageBoxButton.OK, MessageBoxImage.Information);
                    else
                    {
                        foreach (string filename in filenames)
                        {
                            try
                            {
                                var tempRuleObj = GSApp.settings.filer.Open(filename);
                                ruleset.Add((grammarRule)tempRuleObj[0]);
                                var ruleFileName = MyIOPath.GetRelativePath(filename,
                                                                            GSApp.settings.RulesDirAbs);
                                var ruleNumber = listBoxOfRules.Items.Count + 1;
                                var li = new ListBoxItem();
                                li.Content = ruleNumber + ". " + ruleFileName;
                                listBoxOfRules.Items.Add(li);
                                ruleset.ruleFileNames.Add(ruleFileName);
                            }
                            catch
                            {
                                MessageBox.Show("Error in loading rule: " + filename,
                                                "Error in rule loading.", MessageBoxButton.OK,
                                                MessageBoxImage.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var numToRemove = listBoxOfRules.SelectedItems.Count;
                var toRemove = GetSelectedIndex();
                if (toRemove != null && numToRemove > 0)
                {
                    for (var i = numToRemove; i != 0; i--)
                    {
                        listBoxOfRules.Items.RemoveAt(toRemove[i - 1]);
                        ruleset.rules.RemoveAt(toRemove[i - 1]);
                        ruleset.ruleFileNames.RemoveAt(toRemove[i - 1]);
                    }
                    /* now re-number the list. */
                    string itemString;
                    char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

                    for (var i = 1; i <= listBoxOfRules.Items.Count; i++)
                    {
                        itemString = (string)(listBoxOfRules.Items[(i - 1)] as ListBoxItem).Content;
                        itemString = itemString.TrimStart(digits);
                        (listBoxOfRules.Items[(i - 1)] as ListBoxItem).Content = i + itemString;
                    }
                }
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tempString = null;
                grammarRule tempRule = null;
                var numToMoveUp = listBoxOfRules.SelectedItems.Count;
                var toMoveUp = GetSelectedIndex();

                for (var i = 0; i != numToMoveUp; i++)
                {
                    if ((toMoveUp[i] != 0) &&
                        (!listBoxOfRules.SelectedItems.Contains(listBoxOfRules.Items[toMoveUp[i] - 1])))
                    {
                        tempString = (string)(listBoxOfRules.Items[toMoveUp[i] - 1] as ListBoxItem).Content;
                        (listBoxOfRules.Items[toMoveUp[i] - 1] as ListBoxItem).Content =
                            (listBoxOfRules.Items[toMoveUp[i]] as ListBoxItem).Content;
                        (listBoxOfRules.Items[toMoveUp[i]] as ListBoxItem).Content = tempString;
                        (listBoxOfRules.Items[toMoveUp[i] - 1] as ListBoxItem).IsSelected = true;
                        (listBoxOfRules.Items[toMoveUp[i]] as ListBoxItem).IsSelected = false;

                        tempRule = ruleset.rules[toMoveUp[i] - 1];
                        ruleset.rules[toMoveUp[i] - 1] = ruleset.rules[toMoveUp[i]];
                        ruleset.rules[toMoveUp[i]] = tempRule;
                        tempString = ruleset.ruleFileNames[toMoveUp[i] - 1];
                        ruleset.ruleFileNames[toMoveUp[i] - 1] = ruleset.ruleFileNames[toMoveUp[i]];
                        ruleset.ruleFileNames[toMoveUp[i]] = tempString;
                    }
                }
                /* now re-number the list. */
                string itemString;
                char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

                for (var i = 1; i <= listBoxOfRules.Items.Count; i++)
                {
                    itemString = (string)(listBoxOfRules.Items[(i - 1)] as ListBoxItem).Content;
                    itemString = itemString.TrimStart(digits);
                    (listBoxOfRules.Items[(i - 1)] as ListBoxItem).Content = i + itemString;
                }
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tempString = null;
                grammarRule tempRule = null;
                var numToMoveDown = listBoxOfRules.SelectedItems.Count;
                var toMoveDown = GetSelectedIndex();

                for (var i = numToMoveDown - 1; i >= 0; i--)
                {
                    if ((toMoveDown[i] != listBoxOfRules.Items.Count - 1) &&
                        (!listBoxOfRules.SelectedItems.Contains(listBoxOfRules.Items[toMoveDown[i] + 1])))
                    {
                        tempString = (string)(listBoxOfRules.Items[toMoveDown[i] + 1] as ListBoxItem).Content;
                        (listBoxOfRules.Items[toMoveDown[i] + 1] as ListBoxItem).Content =
                            (listBoxOfRules.Items[toMoveDown[i]] as ListBoxItem).Content;
                        (listBoxOfRules.Items[toMoveDown[i]] as ListBoxItem).Content = tempString;
                        (listBoxOfRules.Items[toMoveDown[i] + 1] as ListBoxItem).IsSelected = true;
                        (listBoxOfRules.Items[toMoveDown[i]] as ListBoxItem).IsSelected = false;

                        tempRule = ruleset.rules[toMoveDown[i] + 1];
                        ruleset.rules[toMoveDown[i] + 1] = ruleset.rules[toMoveDown[i]];
                        ruleset.rules[toMoveDown[i]] = tempRule;
                        tempString = ruleset.ruleFileNames[toMoveDown[i] + 1];
                        ruleset.ruleFileNames[toMoveDown[i] + 1] = ruleset.ruleFileNames[toMoveDown[i]];
                        ruleset.ruleFileNames[toMoveDown[i]] = tempString;
                    }
                }
                /* now re-number the list. */
                string itemString;
                char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

                for (var i = 1; i <= listBoxOfRules.Items.Count; i++)
                {
                    itemString = (string)(listBoxOfRules.Items[(i - 1)] as ListBoxItem).Content;
                    itemString = itemString.TrimStart(digits);
                    (listBoxOfRules.Items[(i - 1)] as ListBoxItem).Content = i + itemString;
                }
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (saveRulesToo)
            {
                for (var i = 0; i < ruleset.ruleFileNames.Count; i++)
                {
                    var filename = ruleset.rulesDir + ruleset.ruleFileNames[i];
                    try
                    {
                        if (!main.windowsMgr.FindAndFocusFileInCollection(filename, WindowType.Rule))
                        {
                            var tempRuleObj = new[] { GSApp.settings.filer.Open(filename)[0], filename };
                            main.addAndShowRuleWindow(tempRuleObj);
                        }
                        main.SaveOnExecuted(null, null);
                        main.CloseActiveWindow_ClickOnExecuted(null, null);
                    }
                    catch (Exception exc)
                    {
                        ErrorLogger.Catch(exc);
                    }
                }
                saveRulesToo = false;
            }
            GSApp.main.SaveOnExecuted(this, null);
        }

        private void btnSaveRules_Click(object sender, RoutedEventArgs e)
        {
            saveRulesToo = true;
        }

        private int[] GetSelectedIndex()
        {
            try
            {
                var intsSelectedIndex = new int[listBoxOfRules.SelectedItems.Count];
                var j = 0;
                for (var i = 0; i < listBoxOfRules.Items.Count; i++)
                {
                    if (listBoxOfRules.SelectedItems.Contains(listBoxOfRules.Items[i]))
                    {
                        intsSelectedIndex[j] = i;
                        j++;
                    }
                }
                return intsSelectedIndex;
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
                return null;
            }
        }

        private void listBoxOfRules_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var index = -1;
            if (listBoxOfRules.SelectedItems.Count > 0 && Select)
                index =
                    listBoxOfRules.Items.IndexOf(listBoxOfRules.SelectedItems[listBoxOfRules.SelectedItems.Count - 1]);
            else if (Deselected.Count > 0 && !Select)
                index = Deselected.Last();

            if (index >= 0)
            {
                var tempRuleObj = GSApp.settings.filer.Open(ruleset.rulesDir + ruleset.ruleFileNames[index], true);
                if (tempRuleObj != null)
                {
                    tempRuleObj = new[]
                                      {
                                          tempRuleObj[0],
                                          ruleset.rulesDir + ruleset.ruleFileNames[index]
                                      };
                    main.addAndShowRuleWindow(tempRuleObj);
                }
            }
        }

        private void listBoxOfRules_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.RemovedItems.Count > 0)
                    Select = false;
                if (e.AddedItems.Count > 0)
                    Select = true;
                var RemovedIndexes = new List<int>();
                foreach (ListBoxItem Li in e.RemovedItems)
                    RemovedIndexes.Add(listBoxOfRules.Items.IndexOf(Li));

                foreach (int index in RemovedIndexes)
                    if (!Deselected.Contains(index)) Deselected.Add(index);

                foreach (int index in new List<int>(Deselected))
                    if (!RemovedIndexes.Contains(index)) Deselected.Remove(index);
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }
    }
}