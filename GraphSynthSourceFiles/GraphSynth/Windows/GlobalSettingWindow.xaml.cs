using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using GraphSynth.Representation;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using TextBox = System.Windows.Controls.TextBox;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for GlobalSettingWindow.xaml
    /// </summary>
    public partial class GlobalSettingWindow : Window
    {
        private Button[] RSButton;
        private TextBox[] RSText;
        private string c2b = "<--- click to browse.";

        public GlobalSettings newSettings;

        #region Constructor

        public GlobalSettingWindow()
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

            AddControlsInSeedsAndRulesTab();
            
            PopulateVerbosityComboBox();

            newSettings = GSApp.settings.Duplicate();
            refreshTextBoxes();
            DisplaySettings();
            GenerationSettings();
            UserInterfaceSettings();
        }

        private void AddControlsInSeedsAndRulesTab()
        {
            RSButton = new Button[10];
            RSText = new TextBox[10];

            for (var i = 0; i != 10; i++)
            {
                RSButton[i] = new Button
                                  {
                                      TabIndex = i + 1,
                                      Content = "RuleSet #" + i
                                  };
                RSButton[i].Click += RSbutton_Click;
                grdSeedTab.Children.Add(RSButton[i]);
                Grid.SetColumn(RSButton[i], 0);
                Grid.SetRow(RSButton[i], i + 3);

                RSText[i] = new TextBox
                                {
                                    TextWrapping = TextWrapping.Wrap,
                                    IsReadOnly = false
                                };
                RSText[i].LostFocus += RuleSetTxtbox_LostFocus;
                RSText[i].KeyUp += RuleSetTxtbox_KeyUp;
                grdSeedTab.Children.Add(RSText[i]);
                Grid.SetColumn(RSText[i], 1);
                Grid.SetRow(RSText[i], i + 3);
            }
        }

        private void PopulateNextGenerationComboboxes(ComboBox cmbBox)
        {
            cmbBox.Items.Add(nextGenerationSteps.Unspecified);
            cmbBox.Items.Add(nextGenerationSteps.Stop);
            cmbBox.Items.Add(nextGenerationSteps.Loop);
            cmbBox.Items.Add(nextGenerationSteps.GoToPrevious);
            cmbBox.Items.Add(nextGenerationSteps.GoToNext);
            cmbBox.Items.Add(nextGenerationSteps.GoToRuleSet0);
            cmbBox.Items.Add(nextGenerationSteps.GoToRuleSet1);
            cmbBox.Items.Add(nextGenerationSteps.GoToRuleSet2);
            cmbBox.Items.Add(nextGenerationSteps.GoToRuleSet3);
            cmbBox.Items.Add(nextGenerationSteps.GoToRuleSet4);
            cmbBox.Items.Add(nextGenerationSteps.GoToRuleSet5);
            cmbBox.Items.Add(nextGenerationSteps.GoToRuleSet6);
            cmbBox.Items.Add(nextGenerationSteps.GoToRuleSet7);
            cmbBox.Items.Add(nextGenerationSteps.GoToRuleSet8);
            cmbBox.Items.Add(nextGenerationSteps.GoToRuleSet9);
            cmbBox.Items.Add(nextGenerationSteps.GoToRuleSet10);
        }

        private void PopulateVerbosityComboBox()
        {
            cmbDefaultVerbosity.Items.Add(ThreadPriority.Lowest);
            cmbDefaultVerbosity.Items.Add(ThreadPriority.BelowNormal);
            cmbDefaultVerbosity.Items.Add(ThreadPriority.Normal);
            cmbDefaultVerbosity.Items.Add(ThreadPriority.AboveNormal);
            cmbDefaultVerbosity.Items.Add(ThreadPriority.Highest);
        }

        private void DisplaySettings()
        {
            chkSearchControllerAutoPlay.IsChecked = newSettings.SearchControllerPlayOnStart;
            chkGetHelpFromOnline.IsChecked = newSettings.GetHelpFromOnline;
        }

        private void GenerationSettings()
        {
            txtNoOfRuleSets.Text = newSettings.numOfRuleSets.ToString();
            txtMaxNoOfRulesToApply.Text = newSettings.MaxRulesToApply.ToString();
            chkRecompileRules.IsChecked = newSettings.RecompileRuleConditions;
        }

        private void UserInterfaceSettings()
        {
            txtMaxNoOfRulesToDisplay.Text = newSettings.MaxRulesToDisplay.ToString();
            cmbDefaultVerbosity.SelectedIndex =
                cmbDefaultVerbosity.Items.IndexOf((ThreadPriority)newSettings.DefaultVerbosity);
        }

        #endregion

        #region Refresh

        private void refreshTextBoxes()
        {
            if (newSettings.CustomShapesFile.Length == 0) txtCustomShapes.Text = c2b;
            else txtCustomShapes.Text = newSettings.CustomShapesFile;
            if (newSettings.WorkingDirAbsolute.Length == 0) txtWorkingDirectory.Text = c2b;
            else
            {
                txtWorkingDirectory.Text = newSettings.WorkingDirAbsolute;
                txtWorkingRelativeDirectory.Text = newSettings.WorkingDirRelative;
            }
            if (newSettings.InputDir.Length == 0) txtInputDirectory.Text = c2b;
            else txtInputDirectory.Text = newSettings.InputDir;
            if (newSettings.OutputDir.Length == 0) txtOutputDirectory.Text = c2b;
            else txtOutputDirectory.Text = newSettings.OutputDir;
            if (newSettings.RulesDir.Length == 0) txtRulesDirectory.Text = c2b;
            else txtRulesDirectory.Text = newSettings.RulesDir;
            if ((bool)chkGetHelpFromOnline.IsChecked)
                txtHelpDirectory.IsEnabled = btnHelpDirectory.IsEnabled = false;
            else
            {
                txtHelpDirectory.IsEnabled = btnHelpDirectory.IsEnabled = true;
                if (newSettings.LocalHelpDir.Length == 0) txtHelpDirectory.Text = c2b;
                else txtHelpDirectory.Text = newSettings.LocalHelpDir;
            }
            if (newSettings.SearchDir.Length == 0) txtSearchDirectory.Text = c2b;
            else txtSearchDirectory.Text = newSettings.SearchDir;
            if (newSettings.GraphLayoutDir.Length == 0) txtGraphLayoutDirectory.Text = c2b;
            else txtGraphLayoutDirectory.Text = newSettings.GraphLayoutDir;
            if (newSettings.DefaultSeedFileName.Length == 0) txtDefaultSeedGraph.Text = c2b;
            else txtDefaultSeedGraph.Text = newSettings.DefaultSeedFileName;
            if (newSettings.CompiledRuleFunctions.Length == 0) txtCompiledRulesDLL.Text = c2b;
            else txtCompiledRulesDLL.Text = newSettings.CompiledRuleFunctions;
            for (var i = 0; i != 10; i++)
            {
                if (i < newSettings.numOfRuleSets)
                {
                    if (i >= newSettings.defaultRSFileNames.Count)
                        newSettings.defaultRSFileNames.Add("");
                    if (newSettings.defaultRSFileNames[i].Length == 0) RSText[i].Text = c2b;
                    else
                        RSText[i].Text = MyIOPath.GetRelativePath(newSettings.defaultRSFileNames[i], newSettings.RulesDirAbs);
                    RSText[i].Visibility = Visibility.Visible;
                    RSButton[i].Visibility = Visibility.Visible;
                }
                else
                {
                    RSButton[i].Visibility = Visibility.Hidden;
                    RSText[i].Visibility = Visibility.Hidden;
                }
            }
        }

        #endregion

        #region Bottom Buttons

        private void btnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            if (newSettings.numOfRuleSets < newSettings.defaultRSFileNames.Count)
                newSettings.defaultRSFileNames.RemoveRange(newSettings.numOfRuleSets, newSettings.defaultRSFileNames.Count
                    - newSettings.numOfRuleSets);
            newSettings.DefaultRuleSets
                = StringCollectionConverter.convert(newSettings.defaultRSFileNames);
            newSettings.saveNewSettings();
            btnApplyInThisProcess.IsDefault = true;
        }

        private void btnSaveAS_Click(object sender, RoutedEventArgs e)
        {
            if (newSettings.numOfRuleSets < newSettings.defaultRSFileNames.Count)
                newSettings.defaultRSFileNames.RemoveRange(newSettings.numOfRuleSets, newSettings.defaultRSFileNames.Count
                    - newSettings.numOfRuleSets);
            newSettings.DefaultRuleSets
                = StringCollectionConverter.convert(newSettings.defaultRSFileNames);
            newSettings.saveNewSettings(main.GetSaveFilename("GraphSynth config file (*.gsconfig)|*.gsconfig",
                                                             "", GSApp.settings.WorkingDirAbsolute));
            btnApplyInThisProcess.IsDefault = true;
        }


        private void btnApplyInThisProcess_Click(object sender, RoutedEventArgs e)
        {
            if (newSettings.numOfRuleSets<newSettings.defaultRSFileNames.Count)
                newSettings.defaultRSFileNames.RemoveRange(newSettings.numOfRuleSets,newSettings.defaultRSFileNames.Count
                    -newSettings.numOfRuleSets);
            newSettings.DefaultRuleSets
                = StringCollectionConverter.convert(newSettings.defaultRSFileNames);
            var tempSettings = GSApp.settings;
            GSApp.settings = newSettings;
            SearchIO.defaultVerbosity = newSettings.DefaultVerbosity;
            try
            {
                newSettings.filer = new WPFFiler(newSettings.InputDirAbs, newSettings.OutputDirAbs, newSettings.RulesDirAbs);
                newSettings.LoadDefaultSeedAndRuleSets();
                for (int i = 0; i < newSettings.numOfRuleSets; i++)
                    if (newSettings.rulesets[i] != null)
                        ((ruleSet)newSettings.rulesets[i]).RuleSetIndex = i;
                /* The following three command are a repeat from MainWindow.Startup.cs. */
                main.setUpGraphElementAddButtons();
                main.setUpGraphLayoutMenu();
                main.setUpSearchProcessMenu();
            }
            catch (Exception ee)
            {
                var msgResult = MessageBox.Show("Settings did not work because of the following error: " + ee +
                                                " Revert to Previous Settings?", "Error in Settings. Revert back?",
                                                MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (msgResult == MessageBoxResult.Yes || msgResult == MessageBoxResult.OK)
                    GSApp.settings = tempSettings;
                return;
            }
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Directories

        private void btnWorkingDirectory_Click(object sender, RoutedEventArgs e)
        {
            var newDir = "";
            if (getWorkingDirectory(newSettings.WorkingDirAbsolute, out newDir))
            {
                newSettings.AttemptSubDirMigration(newDir);
                newSettings.WorkingDirAbsolute = newDir;
            }
            refreshTextBoxes();
        }

        public static Boolean getWorkingDirectory(string startDir, out string newDir)
        {
            var folderBrowserDialog = new FolderBrowserDialog
                                          {
                                              SelectedPath = startDir,
                                              Description = "Set a working directory for GraphSynth (input, output, " +
                                                            "rules, and help directories will be set relative to this)."
                                          };

            if ((folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                || (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.Yes))
            {
                var dir_path = Path.GetDirectoryName(folderBrowserDialog.SelectedPath + "/");
                if (!string.IsNullOrWhiteSpace(dir_path))
                {
                    newDir = dir_path;
                    return true;
                }
            }
            newDir = "";
            return false;
        }


        private void btnInputDirectory_Click(object sender, RoutedEventArgs e)
        {
            var newDir = "";
            if (getDirectory("input", out newDir))
                newSettings.InputDir = newDir;
            refreshTextBoxes();
        }

        private void btnOutputDirectory_Click(object sender, RoutedEventArgs e)
        {
            var newDir = "";
            if (getDirectory("output", out newDir))
                newSettings.OutputDir = newDir;
            refreshTextBoxes();
        }

        private void btnRulesDirectory_Click(object sender, RoutedEventArgs e)
        {
            var newDir = "";
            if (getDirectory("rules", out newDir))
                newSettings.RulesDir = newDir;
            refreshTextBoxes();
        }

        private void btnHelpDirectory_Click(object sender, RoutedEventArgs e)
        {
            var newDir = "";
            if (getDirectory("local help", out newDir))
                newSettings.LocalHelpDir = newDir;
            refreshTextBoxes();
        }

        private void btnSearchDirectory_Click(object sender, RoutedEventArgs e)
        {
            var newDir = "";
            if (getDirectory("search plugins", out newDir))
                newSettings.SearchDir = newDir;
            refreshTextBoxes();
        }

        private void btnGraphLayoutDirectory_Click(object sender, RoutedEventArgs e)
        {
            var newDir = "";
            if (getDirectory("graph layout plugins", out newDir))
                newSettings.GraphLayoutDir = newDir;
            refreshTextBoxes();
        }

        private void chkGetHelpFromOnline_Checked(object sender, RoutedEventArgs e)
        {
            newSettings.GetHelpFromOnline = true;
            refreshTextBoxes();
        }

        private void chkGetHelpFromOnline_Unchecked(object sender, RoutedEventArgs e)
        {
            newSettings.GetHelpFromOnline = false;
            refreshTextBoxes();
        }

        private Boolean getDirectory(string titletypeString, out string newDir)
        {
            var folderBrowserDialog = new FolderBrowserDialog
                                          {
                                              SelectedPath = newSettings.WorkingDirAbsolute,
                                              Description = "Set the " + titletypeString + " directory for GraphSynth."
                                          };

            if ((folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                || (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.Yes))
            {

                var dir_path = Path.GetDirectoryName(folderBrowserDialog.SelectedPath + "/");
                if (!string.IsNullOrWhiteSpace(dir_path))
                {
                    newDir = dir_path;
                    return true;
                }
            }
            newDir = "";
            return false;
        }

        #endregion

        #region Seed and Rulesets

        private void txtDefaultSeedGraph_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter) || (e.Key == Key.Tab) || (e.Key == Key.Return))
                txtDefaultSeedGraph_LostFocus(sender, e);
        }

        private void txtDefaultSeedGraph_LostFocus(object sender, RoutedEventArgs e)
        {
            var text = ((TextBox)sender).Text.Trim();
            if (text.Length > 0)
            {
                if (c2b.Equals(text)) text = "";
                else if (!File.Exists(newSettings.InputDirAbs + text))
                {
                    if (File.Exists(newSettings.InputDirAbs + text + ".gxml"))
                        text += ".gxml";
                    else
                    {
                        text = "";
                        MessageBox.Show("You must choose a file that currently exists.", "File not found.",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            newSettings.DefaultSeedFileName = text;
            refreshTextBoxes();
        }

        private void btnDefaultSeedGraph_Click(object sender, RoutedEventArgs e)
        {
            var filename = "";
            if (getOpenFilename(newSettings.InputDirAbs,
                                "Open a graph to set as the seed", "graph files|*.gxml|All xml files|*.xml",
                                out filename, true))
                newSettings.DefaultSeedFileName = filename;
            refreshTextBoxes();
        }

        private void btnCompiledRulesDLL_Click(object sender, RoutedEventArgs e)
        {
            var filename = "";
            if (getOpenFilename(newSettings.RulesDirAbs,
                                "Open an existing (or future) compiled library file.",
                                "dynamic-link library (*.dll)|*.dll", out filename, false))
                newSettings.CompiledRuleFunctions = filename;
            refreshTextBoxes();
        }

        private void RSbutton_Click(object sender, RoutedEventArgs e)
        {
            var i = 0;
            while (!sender.Equals(RSButton[i]))
                i++;
            var filename = "";
            if (getOpenFilename(newSettings.RulesDirAbs,
                                "Open a ruleset file for ruleset #" + i,
                                "ruleset files|*.rsxml|All xml files|*.xml", out filename, true))
                newSettings.defaultRSFileNames[i] = filename;
            refreshTextBoxes();
        }

        private void RuleSetTxtbox_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter) || (e.Key == Key.Tab) || (e.Key == Key.Return))
                RuleSetTxtbox_LostFocus(sender, e);
        }

        private void RuleSetTxtbox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textbox = (TextBox)sender;
            var text = textbox.Text.Trim();
            var index = Array.FindIndex(RSText, x => (textbox == x));
            if (text.Length > 0)
            {
                if (c2b.Equals(text)) text = "";
                else if (!File.Exists(newSettings.RulesDirAbs + text))
                {
                    if (File.Exists(newSettings.RulesDirAbs + text + ".rsxml"))
                        text += ".rsxml";
                    else
                    {
                        text = "";
                        MessageBox.Show("You must choose a file that currently exists.", "File not found.",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            newSettings.defaultRSFileNames[index] = text;
            refreshTextBoxes();
        }

        private void txtNoOfRuleSets_TextChanged(object sender, TextChangedEventArgs e)
        {
            int NoOfRuleSets;
            if (int.TryParse(txtNoOfRuleSets.Text, out NoOfRuleSets))
            {
                newSettings.numOfRuleSets = NoOfRuleSets;
            }
            else
            {
                newSettings.numOfRuleSets = 0;
            }
            refreshTextBoxes();
        }

        private void txtNoOfRuleSets_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtNoOfRuleSets.Text = newSettings.numOfRuleSets.ToString();
                refreshTextBoxes();
            }
        }

        private Boolean getOpenFilename(string initDir, string title, string filter, out string newfilename,
                                        Boolean mustExist)
        {
            var fileChooser = new OpenFileDialog
                                  {
                                      Title = title,
                                      InitialDirectory = initDir,
                                      Filter = filter,
                                      CheckFileExists = mustExist
                                  };
            if ((Boolean)fileChooser.ShowDialog(this))
            {
                newfilename = fileChooser.FileName;
                return true;
            }
            else
            {
                newfilename = "";
                return false;
            }
        }

        #endregion

        #region Limits etc.

        #region Display

        private void chkRecompileRules_Checked(object sender, RoutedEventArgs e)
        {
            newSettings.RecompileRuleConditions = true;
        }

        private void chkRecompileRules_Unchecked(object sender, RoutedEventArgs e)
        {
            newSettings.RecompileRuleConditions = false;
        }


        private void chkSearchControllerAutoPlay_Checked(object sender, RoutedEventArgs e)
        {
            newSettings.SearchControllerPlayOnStart = true;
        }

        private void chkSearchControllerAutoPlay_Unchecked(object sender, RoutedEventArgs e)
        {
            newSettings.SearchControllerPlayOnStart = false;
        }

        private void txtMaxNoOfRulesToDisplay_TextChanged(object sender, TextChangedEventArgs e)
        {
            int MaxRulesToDisplay;
            if (int.TryParse(txtMaxNoOfRulesToDisplay.Text, out MaxRulesToDisplay))
                newSettings.MaxRulesToDisplay = MaxRulesToDisplay;
        }

        private void txtMaxNoOfRulesToDisplay_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                txtMaxNoOfRulesToDisplay.Text = newSettings.MaxRulesToDisplay.ToString();
        }

        private void cmbDefaultVerbosity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            newSettings.DefaultVerbosity = (int)(ThreadPriority)cmbDefaultVerbosity.SelectedItem;
        }

        private void btnCustomShapes_Click(object sender, RoutedEventArgs e)
        {
            var filename = "";
            if (getOpenFilename(newSettings.WorkingDirAbsolute,
                                "Open a file of custom node and arc shapes", "XAML files|*.xaml", out filename, true))
                newSettings.CustomShapesFile = filename;
            refreshTextBoxes();
        }

        #endregion

        #region Generation

        private void txtMaxNoOfRulesToApply_TextChanged(object sender, TextChangedEventArgs e)
        {
            int MaxRulesToApply;
            if (int.TryParse(txtMaxNoOfRulesToApply.Text, out MaxRulesToApply))
                newSettings.MaxRulesToApply = MaxRulesToApply;
        }

        private void txtMaxNoOfRulesToApply_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                txtMaxNoOfRulesToApply.Text = newSettings.MaxRulesToApply.ToString();
        }

        #endregion

        #endregion

        private MainWindow main
        {
            get { return GSApp.main; }
        }

        private void BecomeActiveSubWindow(object sender, EventArgs e)
        {
            main.windowsMgr.SetAsActive(this);
        }
    }
}