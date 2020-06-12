using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GraphSynth.Representation;
using Timer = System.Timers.Timer;

namespace GraphSynth.UserRandLindChoose
{
    /// <summary>
    ///   Interaction logic for userChooseWindow.xaml
    /// </summary>
    public partial class UserChooseWindow : Window
    {
        private readonly Timer checkForStopTimer = new Timer();
        private readonly List<int> optionNumbers;
        private readonly List<option> options;
        public int[] choice = new[] { -2 };
        private List<int> confluentChoices;

        public UserChooseWindow()
        {
            /* the following is common to all GS window types. */
            InitializeComponent();
            ShowInTaskbar = true;
        }

        private UserChooseWindow(List<option> opts, GlobalSettings settings, Boolean hideUndo)
            : this()
        {
            checkForStopTimer.Elapsed += processTimer_Tick;
            checkForStopTimer.Interval = 500;
            checkForStopTimer.Start();

            options = opts;
            Title = "Choices from RuleSet #" + opts[0].ruleSetIndex;
            optionNumbers = new List<int>();
            for (var i = 0; i != options.Count; i++)
            {
                recognizedRulesList.Items.Add(new UserChooseWindowItem(options[i], settings));
                optionNumbers.Add(i);
            }
            if (hideUndo) btnUndo.IsEnabled = false;
        }

        public static int[] PromptUser(List<option> opts, GlobalSettings settings, Boolean hideUndo)
        {
            var sCW = new UserChooseWindow(opts, settings, hideUndo);
            sCW.ShowDialog();
            return sCW.choice;
        }


        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            var numChecked = recognizedRulesList.SelectedItems.Count;
            checkForStopTimer.Stop();

            switch (numChecked)
            {
                case 0:
                    MessageBox.Show("No Options Checked.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case 1:
                    choice = new[] { optionNumbers[recognizedRulesList.SelectedIndex] };
                    Close();
                    break;
                default:
                    {
                        var itemInds = FindSelectedIndices();
                        choice = new int[itemInds.GetLength(0)];
                        for (var i = 0; i < itemInds.GetLength(0); i++)
                            choice[i] = optionNumbers[itemInds[i]];
                        Close();
                    }
                    break;
            }
            checkForStopTimer.Start();
        }

        private void btnStopGeneration_Click(object sender, RoutedEventArgs e)
        {
            choice = new[] { -2 };
            Close();
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            choice = new[] { -1 };
            Close();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            var numToRemove = recognizedRulesList.SelectedItems.Count;
            if (numToRemove == recognizedRulesList.Items.Count)
            {
                MessageBox.Show("You cannot remove all possible options.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (numToRemove == recognizedRulesList.Items.Count - 1)
            {
                var toRemove = new int[numToRemove];
                FindSelectedIndices().CopyTo(toRemove, 0);
                for (var i = numToRemove; i != 0; i--)
                {
                    if (toRemove[i - 1] != optionNumbers.Count)
                    {
                        recognizedRulesList.Items.RemoveAt(toRemove[i - 1]);
                        optionNumbers.RemoveAt(toRemove[i - 1]);
                    }
                }
                if (MessageBoxResult.Yes == MessageBox.Show(
                    "You are removing all but one option [" +
                    (((ListBoxItem)recognizedRulesList.SelectedItems[0]).Content) +
                    "]. Would you like to apply this option?",
                    "Apply Remaining Option?", MessageBoxButton.YesNo, MessageBoxImage.Question))
                {
                    choice = new[] { optionNumbers[0] };
                    Close();
                }
            }
            else
            {
                var toRemove = new int[numToRemove];

                for (var i = numToRemove; i != 0; i--)
                {
                    if (toRemove[i - 1] != optionNumbers.Count)
                    {
                        recognizedRulesList.Items.RemoveAt(toRemove[i - 1]);
                        optionNumbers.RemoveAt(toRemove[i - 1]);
                    }
                }
            }
        }

        private void processTimer_Tick(object sender, ElapsedEventArgs e)
        {
            if (!SearchIO.GetTerminateRequest(Thread.CurrentThread.ManagedThreadId)) return;
            choice = new[] { -2 };
            Close();
        }

        private int[] FindSelectedIndices()
        {
            var SelectedIndices = new int[recognizedRulesList.SelectedItems.Count];

            for (var i = 0; i < recognizedRulesList.SelectedItems.Count; i++)
                SelectedIndices[i] = recognizedRulesList.Items.IndexOf(recognizedRulesList.SelectedItems[i]);

            return SelectedIndices;
        }

        private void recolorOptions()
        {
            if (recognizedRulesList.SelectedIndex == -1)
                foreach (var item in recognizedRulesList.Items)
                    ((ListBoxItem)item).Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            else
            {
                for (var i = 0; i < optionNumbers.Count; i++)
                    if (confluentChoices.Contains(optionNumbers[i]))
                        ((ListBoxItem)recognizedRulesList.Items[i]).Foreground =
                            new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                    else if (!((ListBoxItem)recognizedRulesList.Items[i]).IsSelected)
                        ((ListBoxItem)recognizedRulesList.Items[i]).Foreground =
                            new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    else
                        ((ListBoxItem)recognizedRulesList.Items[i]).Foreground =
                            new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            }
        }

        private void recognizedRulesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var latestCheck = optionNumbers[recognizedRulesList.Items.IndexOf(e.AddedItems[0])];
                if (recognizedRulesList.SelectedItems.Count == 1)
                    confluentChoices = new List<int>(options[latestCheck].confluence);
                else if (confluentChoices.Contains(latestCheck))
                    confluentChoices = new List<int>(confluentChoices.Intersect(options[latestCheck].confluence));
                else
                {
                    recognizedRulesList.UnselectAll();
                    recognizedRulesList.SelectedIndex = latestCheck;
                    confluentChoices = new List<int>(options[latestCheck].confluence);
                }
            }
            else if (e.RemovedItems.Count > 0)
            {
                if (recognizedRulesList.SelectedItems.Count == 0)
                    confluentChoices.Clear();
                else buildConfluentList();
            }
            recolorOptions();
        }

        private void buildConfluentList()
        {
            confluentChoices = new List<int>(options[recognizedRulesList.SelectedIndex].confluence);
            foreach (var si in recognizedRulesList.SelectedItems)
                confluentChoices =
                    new List<int>(
                        confluentChoices.Intersect(
                            options[optionNumbers[recognizedRulesList.Items.IndexOf(si)]].confluence));
        }
    }
}