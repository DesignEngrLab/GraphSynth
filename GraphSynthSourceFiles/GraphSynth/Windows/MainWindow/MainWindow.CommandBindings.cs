using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMainWindow
    {
        /******* Set up of Command Bindings ********/

        private void setUpCommandBinding()
        {
            #region File

            /*************** New **************/
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, NewGraph_ClickOnExecuted,
                                                   NewGraph_ClickCanExecute));

            CommandBindings.Add(new CommandBinding(NewGrammarRuleCommand, NewGrammarRule_ClickOnExecuted,
                                                   NewGrammarRule_ClickCanExecute));
            InputBindings.Add(new KeyBinding(NewGrammarRuleCommand,
                                             new KeyGesture(Key.N, ModifierKeys.Control | ModifierKeys.Shift)));

            CommandBindings.Add(new CommandBinding(NewRuleSetCommand, NewRuleSet_ClickOnExecuted,
                                                   NewRuleSet_ClickCanExecute));
            InputBindings.Add(new KeyBinding(NewRuleSetCommand, new KeyGesture(Key.N, ModifierKeys.Alt)));
            /*************** Open *************/
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OpenOnExecuted, CanExecute_Open));
            /*************** Save *************/
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, SaveOnExecuted, CanExecute_Save));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, SaveAsOnExecuted, CanExecute_Save));
            InputBindings.Add(new KeyBinding(ApplicationCommands.SaveAs,
                                             new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift)));
            /*************** Export *************/
            CommandBindings.Add(new CommandBinding(ExportAsGS1XCommand, ExportAsGS1XOnExecuted, ExportAsGS1X_CanExecute));
            CommandBindings.Add(new CommandBinding(ExportAsPNGCommand, ExportAsPNGOnExecuted, ExportAsPNG_CanExecute));
            /*************** Close *************/
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, CloseActiveWindow_ClickOnExecuted,
                                                   CloseActiveWindow_ClickCanExecute));
            InputBindings.Add(new KeyBinding(ApplicationCommands.Close, new KeyGesture(Key.W, ModifierKeys.Control)));
            CommandBindings.Add(new CommandBinding(CloseAllOpenGraphsCommand, CloseAllOpenGraphs_ClickOnExecuted,
                                                   CloseAllOpenGraphs_ClickCanExecute));
            InputBindings.Add(new KeyBinding(CloseAllOpenGraphsCommand,
                                             new KeyGesture(Key.W, ModifierKeys.Control | ModifierKeys.Shift)));

            CommandBindings.Add(new CommandBinding(ExitCommand, ExitOnExecuted, ExitCanExecute));
            InputBindings.Add(new KeyBinding(ExitCommand, new KeyGesture(Key.X, ModifierKeys.Alt)));

            #endregion

            #region Edit

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Stop, StopOnExecuted, StopCanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, CutOnExecuted, CopyOrCutCanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, CopyOnExecuted, CopyOrCutCanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, PasteOnExecuted, PasteCanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, DeleteOnExecuted, CopyOrCutCanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, SelectAllOnExecuted,
                                                   SelectAllCanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Properties, PropertiesOnExecuted,
                                                   PropertiesCanExecute));
            CommandBindings.Add(new CommandBinding(SettingsCommand, SettingsOnExecuted, SettingsCanExecute));
            var SettingsCommandBinding = new CommandBinding(SettingsCommand
                                                            , SettingsOnExecuted, SettingsCanExecute);
            InputBindings.Add(new KeyBinding(SettingsCommand, Key.S, ModifierKeys.Alt));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, UndoOnExecuted, UndoCanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, RedoOnExecuted, RedoCanExecute));
            #endregion

            #region View

            /* Zoom In */
            CommandBindings.Add(new CommandBinding(ZoomInCommand, ZoomInOnExecuted, ZoomInCanExecute));
            InputBindings.Add(new KeyBinding(ZoomInCommand, Key.OemCloseBrackets, ModifierKeys.None));
            /* Zoom out */
            CommandBindings.Add(new CommandBinding(ZoomOutCommand, ZoomOutOnExecuted, ZoomOutCanExecute));
            InputBindings.Add(new KeyBinding(ZoomOutCommand, Key.OemOpenBrackets, ModifierKeys.None));

            #endregion #endregion

            #region Design

            #region SetActive

            CommandBindings.Add(new CommandBinding(SetActiveAsSeedCommand, SetActiveAsSeedOnExecuted,
                                                   SetActiveAsSeedCanExecute));
            InputBindings.Add(new KeyBinding(SetActiveAsSeedCommand, Key.D, ModifierKeys.Alt));
            CommandBindings.Add(new CommandBinding(SetActiveAsRuleSet0Command, SetActiveAsRuleSet0OnExecuted,
                                                   SetActiveAsRuleSet0CanExecute));
            InputBindings.Add(new KeyBinding(SetActiveAsRuleSet0Command, Key.D0, ModifierKeys.Control | ModifierKeys.Alt));
            CommandBindings.Add(new CommandBinding(SetActiveAsRuleSet1Command, SetActiveAsRuleSet1OnExecuted,
                                                   SetActiveAsRuleSet1CanExecute));
            InputBindings.Add(new KeyBinding(SetActiveAsRuleSet1Command, Key.D1, ModifierKeys.Control | ModifierKeys.Alt));
            CommandBindings.Add(new CommandBinding(SetActiveAsRuleSet2Command, SetActiveAsRuleSet2OnExecuted,
                                                   SetActiveAsRuleSet2CanExecute));
            InputBindings.Add(new KeyBinding(SetActiveAsRuleSet2Command, Key.D2, ModifierKeys.Control | ModifierKeys.Alt));
            CommandBindings.Add(new CommandBinding(SetActiveAsRuleSet3Command, SetActiveAsRuleSet3OnExecuted,
                                                   SetActiveAsRuleSet3CanExecute));
            InputBindings.Add(new KeyBinding(SetActiveAsRuleSet3Command, Key.D3, ModifierKeys.Control | ModifierKeys.Alt));
            CommandBindings.Add(new CommandBinding(SetActiveAsRuleSet4Command, SetActiveAsRuleSet4OnExecuted,
                                                   SetActiveAsRuleSet4CanExecute));
            InputBindings.Add(new KeyBinding(SetActiveAsRuleSet4Command, Key.D4, ModifierKeys.Control | ModifierKeys.Alt));
            CommandBindings.Add(new CommandBinding(SetActiveAsRuleSet5Command, SetActiveAsRuleSet5OnExecuted,
                                                   SetActiveAsRuleSet5CanExecute));
            InputBindings.Add(new KeyBinding(SetActiveAsRuleSet5Command, Key.D5, ModifierKeys.Control | ModifierKeys.Alt));
            CommandBindings.Add(new CommandBinding(SetActiveAsRuleSet6Command, SetActiveAsRuleSet6OnExecuted,
                                                   SetActiveAsRuleSet6CanExecute));
            InputBindings.Add(new KeyBinding(SetActiveAsRuleSet6Command, Key.D6, ModifierKeys.Control | ModifierKeys.Alt));
            CommandBindings.Add(new CommandBinding(SetActiveAsRuleSet7Command, SetActiveAsRuleSet7OnExecuted,
                                                   SetActiveAsRuleSet7CanExecute));
            InputBindings.Add(new KeyBinding(SetActiveAsRuleSet7Command, Key.D7, ModifierKeys.Control | ModifierKeys.Alt));
            CommandBindings.Add(new CommandBinding(SetActiveAsRuleSet8Command, SetActiveAsRuleSet8OnExecuted,
                                                   SetActiveAsRuleSet8CanExecute));
            InputBindings.Add(new KeyBinding(SetActiveAsRuleSet8Command, Key.D8, ModifierKeys.Control | ModifierKeys.Alt));
            CommandBindings.Add(new CommandBinding(SetActiveAsRuleSet9Command, SetActiveAsRuleSet9OnExecuted,
                                                   SetActiveAsRuleSet9CanExecute));
            InputBindings.Add(new KeyBinding(SetActiveAsRuleSet9Command, Key.D9, ModifierKeys.Control | ModifierKeys.Alt));
            CommandBindings.Add(new CommandBinding(ClearAllRuleSetsAndSeedCommand, ClearAllRuleSetsAndSeedOnExecuted,
                                                   ClearAllRuleSetsAndSeedCanExecute));
            InputBindings.Add(new KeyBinding(SetActiveAsRuleSet0Command, Key.OemMinus,
                                             ModifierKeys.Control | ModifierKeys.Alt));

            #endregion

            #endregion

            #region Help

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, HelpOnExecuted, HelpCanExecute));
            CommandBindings.Add(new CommandBinding(AboutGraphSynthCommand, AboutGraphSynthOnExecuted,
                                                   AboutGraphSynthCanExecute));

            #endregion

            #region WindowsManager

            CommandBindings.Add(new CommandBinding(FocusNextWindowCommand, FocusNextWindowOnExecuted,
                                                   FocusNextWindowCanExecute));
            InputBindings.Add(new KeyBinding(FocusNextWindowCommand, Key.Tab, ModifierKeys.Control));

            CommandBindings.Add(new CommandBinding(MinimizeAllCommand, MinimizeOnExecuted, MinimizeCanExecute));
            InputBindings.Add(new KeyBinding(MinimizeAllCommand, Key.M, ModifierKeys.Control));

            #endregion

            #region Tools Gallery

            //ToolsGallery
            CommandBindings.Add(new CommandBinding(DisconnectHeadCommand, DisconnectHeadCommandOnExecuted,
                                                   ArcConnectCommandCanExecute));
            InputBindings.Add(new KeyBinding(DisconnectHeadCommand, Key.D, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(DisconnectTailCommand, DisconnectTailCommandOnExecuted,
                                                   ArcConnectCommandCanExecute));
            InputBindings.Add(new KeyBinding(DisconnectTailCommand, Key.L, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(FlipArcCommand, FlipArcCommandOnExecuted, ArcConnectCommandCanExecute));
            InputBindings.Add(new KeyBinding(FlipArcCommand, Key.F, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(CaptureArcFormattingCommand, CaptureArcFormattingCommandOnExecuted,
                                                   CaptureArcFormattingCommandCanExecute));
            InputBindings.Add(new KeyBinding(CaptureArcFormattingCommand, Key.J, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(ApplyArcFormattingCommand, ApplyArcFormattingCommandOnExecuted,
                                                   ApplyArcFormattingCommandCanExecute));
            InputBindings.Add(new KeyBinding(ApplyArcFormattingCommand, Key.J, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(CaptureNodeFormattingCommand, CaptureNodeFormattingCommandOnExecuted,
                                                   CaptureNodeFormattingCommandCanExecute));
            InputBindings.Add(new KeyBinding(CaptureNodeFormattingCommand, Key.K, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(ApplyNodeFormattingCommand, ApplyNodeFormattingCommandOnExecuted,
                                                   ApplyNodeFormattingCommandCanExecute));
            InputBindings.Add(new KeyBinding(ApplyNodeFormattingCommand, Key.K, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(TestRuleCommand, TestRuleCommandTestRuleCommandOnExecuted));
            //, TestRuleCommandCanExecute));
            InputBindings.Add(new KeyBinding(TestRuleCommand, Key.T, ModifierKeys.Control));

            CommandBindings.Add(new CommandBinding(LoadCustomCommand, LoadCustomCommandOnExecuted,
                                                   LoadCustomCommandCanExecute));
            CommandBindings.Add(new CommandBinding(ReloadCustomCommand, ReloadCustomCommandOnExecuted,
                                                   ReloadCustomCommandCanExecute));
            CommandBindings.Add(new CommandBinding(ClearCustomCommand, ClearCustomCommandOnExecuted,
                                                   ClearCustomCommandCanExecute));

            #endregion
        }
        #region RoutedCommand Declarations

        // Custom Commands only. The others are common to ApplicationCommands
        //File
        public static RoutedCommand NewGrammarRuleCommand = new RoutedCommand();
        public static RoutedCommand NewRuleSetCommand = new RoutedCommand();
        public static RoutedCommand ExportAsGS1XCommand = new RoutedCommand();
        public static RoutedCommand ExportAsPNGCommand = new RoutedCommand();
        public static RoutedCommand CloseAllOpenGraphsCommand = new RoutedCommand();
        public static RoutedCommand ExitCommand = new RoutedCommand();
        //Edit
        public static RoutedCommand SettingsCommand = new RoutedCommand();
        //View
        public static List<RoutedUICommand> GraphLayoutCommands = new List<RoutedUICommand>();
        public static RoutedCommand ZoomInCommand = new RoutedCommand();
        public static RoutedCommand ZoomOutCommand = new RoutedCommand();
        // Design
        public static List<RoutedUICommand> SearchCommands = new List<RoutedUICommand>();
        public static RoutedCommand DesignCommand = new RoutedCommand();
        public static RoutedCommand SetActiveAsSeedCommand = new RoutedCommand();
        public static RoutedCommand SetActiveAsRuleSet0Command = new RoutedCommand();
        public static RoutedCommand SetActiveAsRuleSet1Command = new RoutedCommand();
        public static RoutedCommand SetActiveAsRuleSet2Command = new RoutedCommand();
        public static RoutedCommand SetActiveAsRuleSet3Command = new RoutedCommand();
        public static RoutedCommand SetActiveAsRuleSet4Command = new RoutedCommand();
        public static RoutedCommand SetActiveAsRuleSet5Command = new RoutedCommand();
        public static RoutedCommand SetActiveAsRuleSet6Command = new RoutedCommand();
        public static RoutedCommand SetActiveAsRuleSet7Command = new RoutedCommand();
        public static RoutedCommand SetActiveAsRuleSet8Command = new RoutedCommand();
        public static RoutedCommand SetActiveAsRuleSet9Command = new RoutedCommand();
        public static RoutedCommand ClearAllRuleSetsAndSeedCommand = new RoutedCommand();
        public static RoutedCommand RecognizeUserChooseApplyCommand = new RoutedCommand();
        public static RoutedCommand RecognizeRandomChooseApplyCommand = new RoutedCommand();
        // Help
        public static RoutedCommand AboutGraphSynthCommand = new RoutedCommand();

        // WindowsManager
        public static RoutedCommand FocusNextWindowCommand = new RoutedCommand();
        public static RoutedCommand MinimizeAllCommand = new RoutedCommand();

        //ToolsGallery
        public static RoutedCommand DisconnectHeadCommand = new RoutedCommand();
        public static RoutedCommand DisconnectTailCommand = new RoutedCommand();
        public static RoutedCommand FlipArcCommand = new RoutedCommand();
        public static RoutedCommand CaptureArcFormattingCommand = new RoutedCommand();
        public static RoutedCommand ApplyArcFormattingCommand = new RoutedCommand();
        public static RoutedCommand CaptureNodeFormattingCommand = new RoutedCommand();
        public static RoutedCommand ApplyNodeFormattingCommand = new RoutedCommand();
        public static RoutedCommand TestRuleCommand = new RoutedCommand();
        public static RoutedCommand LoadCustomCommand = new RoutedCommand();
        public static RoutedCommand ReloadCustomCommand = new RoutedCommand();
        public static RoutedCommand ClearCustomCommand = new RoutedCommand();

        #endregion
    }
}