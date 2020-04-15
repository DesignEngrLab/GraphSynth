using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GraphSynth.GraphDisplay;
using GraphSynth.GraphLayout;
using GraphSynth.Representation;
using Microsoft.Win32;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMainWindow
    {
        #region File

        #region New

        public void NewGraph_ClickOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var canvas = TemplatePickerWindow.ShowWindowDialog();
            if (canvas == null) canvas = new CanvasProperty();
            var gW = new graphWindow(canvas);
            windowsMgr.AddandShowWindow(gW);
        }

        public void NewGrammarRule_ClickOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                /// 
                /// a temporary fix until we phase out UI.UICanvas
                var canvas = TemplatePickerWindow.ShowWindowDialog();
                if (canvas == null) canvas = new CanvasProperty();
                var rW = new ruleWindow(canvas);
                windowsMgr.AddandShowWindow(rW);
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public void NewRuleSet_ClickOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var rSW = new ruleSetWindow(
                    new ruleSet(GSApp.settings.RulesDirAbs), null, null);
                windowsMgr.AddandShowWindow(rSW);
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public void NewGraph_ClickCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        public void NewGrammarRule_ClickCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        public void NewRuleSet_ClickCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        #endregion

        #region Open

        public void OpenOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var filename = "";
            try
            {
                filename = getOpenFilename(GSApp.settings.WorkingDirAbsolute);
            }
            catch (FileNotFoundException fnfe)
            {
                SearchIO.output("File Not Found: " + fnfe);
            }
            if (!string.IsNullOrWhiteSpace(filename)) OpenAndShow(filename);
        }

        public void OpenAndShow(string filename)
        {
            var openedItems = GSApp.settings.filer.Open(filename);
            if (openedItems == null) return;
            else if (openedItems[0] is ruleSet)
            {
                var rs = (ruleSet)openedItems[0];
                addAndShowRuleSetWindow(rs, filename);
            }
            else if (openedItems[0] is candidate)
            {
                var tempString = "";
                var c = (candidate)openedItems[0];
                SearchIO.output("The candidate found in " + filename, 0);
                if (c.performanceParams.Count > 0)
                {
                    tempString = "has the following performance parameters";
                    foreach (double a in c.performanceParams)
                        tempString += ": " + a;
                    SearchIO.output(tempString, 0);
                }
                if (c.age > 0)
                    SearchIO.output("The candidate has existed for " + c.age + " iterations.", 0);
                SearchIO.output("Its generation ended in RuleSet #" + c.activeRuleSetIndex, 0);
                tempString = "Generation terminated with";
                foreach (GenerationStatuses a in c.GenerationStatus)
                    tempString += ": " + a;
                SearchIO.output(tempString, 0);
                c.graph.RepairGraphConnections();
                ((WPFFiler)GSApp.settings.filer).RestoreDisplayShapes(null, c.graph.nodes, c.graph.arcs, c.graph.hyperarcs);
                //System.Windows.MessageBox.Show("Code for Candidate");
                addAndShowGraphWindow(c.graph, filename);
                // graphWindow gW = new graphWindow(c.graph, filename);
                // windowsMgr.AddandShowWindow(gW);
            }
            else if (openedItems[0] is designGraph)
                addAndShowGraphWindow(openedItems);
            else if (openedItems[0] is grammarRule)
                addAndShowRuleWindow(openedItems);
            else SearchIO.output("Nothing opened. No GraphSynth object found in " + filename);
        }

        public string getOpenFilename(string dir)
        {
            try
            {
                var fileChooser = new OpenFileDialog();
                fileChooser.Title = "Open a graph, rule, or rule set from ...";
                fileChooser.InitialDirectory = dir;
                fileChooser.Filter = "GraphSynth files|*.gxml;*.grxml;*.rsxml|All xml files|*.xml";
                if ((Boolean)fileChooser.ShowDialog(this))
                    return fileChooser.FileName;
                else return "";
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
                return "";
            }
        }

        #endregion

        #region Save

        /// <summary>
        ///   Determines what's in the window, activeWindow, and saves it.
        /// </summary>
        /// <param name = "activeWindow">active window</param>
        public void SaveOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                SaveActiveWindow(false);
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public void SaveAsOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                SaveActiveWindow(true);
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public void ExportAsGS1X_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (windowsMgr.activeWindow is graphWindow);
        }

        public void ExportAsPNG_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (windowsMgr.activeWindow is graphWindow);
        }

        public void CanExecute_Open(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        public void CanExecute_Save(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (windowsMgr.activeWindow is graphWindow ||
                             windowsMgr.activeWindow is ruleWindow ||
                             windowsMgr.activeWindow is ruleSetWindow);
        }

        public void SaveActiveWindow(Boolean QueryForFile)
        {
            string filename;
            if (windowsMgr.activeWindow == null)
                MessageBox.Show("Please select an window that contains a graph, rule, or rule set.", "Error Saving",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            else if (windowsMgr.activeWindow is graphWindow)
            {
                var gW = (graphWindow)windowsMgr.activeWindow;
                if (!QueryForFile && (Path.IsPathRooted(gW.filename)))
                    filename = gW.filename;
                else
                    filename = GetSaveFilename("graph file (*.gxml)|*.gxml|xml file (*.xml)|*.xml",
                                               windowsMgr.activeGraphCanvas.graph.name,
                                               GSApp.settings.WorkingDirAbsolute);
                if (filename != "")
                {
                    GSApp.settings.filer.Save(filename, gW);
                    gW.UserChanged = false;
                    gW.Title = gW.graph.name = Path.GetFileNameWithoutExtension(filename);
                }
            }
            else if (windowsMgr.activeWindow is ruleWindow)
            {
                var rW = (ruleWindow)windowsMgr.activeWindow;
                if (!QueryForFile && (Path.IsPathRooted(rW.filename)))
                    filename = rW.filename;
                else
                    filename = GetSaveFilename("grammar rule file (*.grxml)|*.grxml|xml file (*.xml)|*.xml",
                                              rW.rule.name, GSApp.settings.RulesDirAbs);
                if (filename != "")
                {
                    GSApp.settings.filer.Save(filename, rW);
                    rW.UserChanged = false;
                    rW.Title = rW.rule.name = Path.GetFileNameWithoutExtension(filename);
                }
            }
            else if (windowsMgr.activeWindow is ruleSetWindow)
            {
                var rSW = (ruleSetWindow)windowsMgr.activeWindow;

                if (!QueryForFile && (Path.IsPathRooted(rSW.Filename)))
                    filename = rSW.Filename;
                else
                    filename = GetSaveFilename("rule set file (*.rsxml)|*.rsxml|xml file (*.xml)|*.xml",
                                               rSW.Ruleset.name, GSApp.settings.RulesDirAbs);
                if (filename != "")
                {
                    GSApp.settings.filer.Save(filename, rSW.Ruleset);
                    if (string.IsNullOrWhiteSpace(rSW.Ruleset.name))
                        rSW.Title = rSW.Ruleset.name = Path.GetFileNameWithoutExtension(filename);
                }
            }
            else
                MessageBox.Show("Please select an window that contains a graph, rule, or rule set.",
                                "Error Saving", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ExportAsGS1XOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var tempFiler = GSApp.settings.filer;
                GSApp.settings.filer = new BasicFiler(GSApp.settings.InputDirAbs,
                                                      GSApp.settings.OutputDirAbs, GSApp.settings.RulesDirAbs);
                SaveActiveWindow(true);
                GSApp.settings.filer = tempFiler;
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public void ExportAsPNGOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (windowsMgr.activeWindow == null)
                    MessageBox.Show("Please select an window that contains a graph, rule, or rule set.", "Error Saving",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                else if (windowsMgr.activeWindow is graphWindow)
                {
                    var filename = GetSaveFilename("Portable Network Graphics file (*.png)|*.png",
                                                   windowsMgr.activeGraphCanvas.graph.name,
                                                   GSApp.settings.WorkingDirAbsolute);
                    if (filename != "")
                        GraphicsExporter.ExportAsPNG(filename, windowsMgr.activeGraphCanvas);
                }
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        /// <summary>
        ///   Gets the save filename.
        /// </summary>
        /// <param name = "filter">The filter of file types.</param>
        /// <param name = "name">a starting name.</param>
        /// <param name = "dir">The directory</param>
        /// <returns></returns>
        public string GetSaveFilename(string filter, string name, string dir)
        {
            var fileChooser = new SaveFileDialog();
            fileChooser.Title = "Save Active " + StringCollectionConverter.Convert(filter)[0] + " as ...";
            fileChooser.InitialDirectory = dir;
            fileChooser.Filter = filter;
            fileChooser.FileName = name;
            fileChooser.CheckFileExists = false;
            string filename;

            try
            {
                if ((Boolean)fileChooser.ShowDialog(this))
                {
                    filename = fileChooser.FileName;
                    if (string.IsNullOrWhiteSpace(filename))
                        MessageBox.Show("Invalid file name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return filename;
                }
                return "";
            }
            catch (Exception exc)
            {
                try
                {
                    /* this is wonky and strange, but the reason is that the SaveFileDialog
                     * will crash if the FileName does not pass the filter. There is a bunch 
                     * of code that could be written to avoid this, such as parsing the filter,
                     * but it seemed fine to just reset the FileName and try once more. */
                    fileChooser.FileName = "";
                    if ((Boolean)fileChooser.ShowDialog(this))
                    {
                        filename = fileChooser.FileName;
                        if (string.IsNullOrWhiteSpace(filename))
                            MessageBox.Show("Invalid file name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return filename;
                    }
                    return "";
                }

                catch
                {
                    ErrorLogger.Catch(exc);
                    return "";
                }
            }
        }

        #endregion

        #region Close

        public void CloseActiveWindow_ClickOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (windowsMgr.activeWindow != null)
                    windowsMgr.activeWindow.Close();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public void CloseAllOpenGraphs_ClickOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            windowsMgr.CloseAllGraphWindows();
        }

        public void CloseActiveWindow_ClickCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (windowsMgr.activeWindow != this);
        }

        public void CloseAllOpenGraphs_ClickCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (windowsMgr.GraphWindows.Count > 0);
        }

        public void ExitOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public void ExitCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (windowsMgr.NumActiveSPC > 0)
            {
                MessageBox.Show("Please close all active search processes.", "Active Processes still running",
                                MessageBoxButton.OK);
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        #endregion

        #endregion

        #region Edit

        public void StopOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (windowsMgr.activeGraphCanvas != null)
                windowsMgr.activeGraphCanvas.EditingMode = InkCanvasEditingMode.Select;
            SetSelectedAddItem(-1);
            if (windowsMgr.activeGraphCanvas != null
                && windowsMgr.activeGraphCanvas.Selection != null)
                windowsMgr.activeGraphCanvas.Selection
                    = new SelectionClass(windowsMgr.activeGraphCanvas);
        }

        public void StopCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        public void CopyOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                windowsMgr.activeGraphCanvas.Copy();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public void PasteOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                windowsMgr.activeGraphCanvas.Paste();
                propertyUpdate();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public void CutOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                windowsMgr.activeGraphCanvas.Cut();
                windowsMgr.activeGraphCanvas.Selection.UpdateSelection();
                propertyUpdate();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public void CopyOrCutCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((windowsMgr.activeWindow is graphWindow
                             || windowsMgr.activeWindow is ruleWindow)
                            && windowsMgr.activeGraphCanvas.Selection != null
                            && windowsMgr.activeGraphCanvas.Selection.SelectedShapes != null
                            && (windowsMgr.activeGraphCanvas.Selection.selectedArcs.Count +
                                windowsMgr.activeGraphCanvas.Selection.selectedNodes.Count +
                                windowsMgr.activeGraphCanvas.Selection.selectedHyperArcs.Count) > 0);
        }

        public void PasteCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (Clipboard.ContainsData(DataFormats.Text)
                            && (windowsMgr.activeWindow is graphWindow
                                || windowsMgr.activeWindow is ruleWindow));
        }

        private void SelectAllCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (windowsMgr.activeWindow is graphWindow
                            || windowsMgr.activeWindow is ruleWindow);
        }

        public void DeleteOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                windowsMgr.activeGraphCanvas.Delete();
                windowsMgr.activeGraphCanvas.Selection.UpdateSelection();
                propertyUpdate();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public void SelectAllOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                windowsMgr.activeGraphCanvas.SelectAll();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public void PropertiesOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            propertyUpdate();
        }

        public void PropertiesCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (windowsMgr.activeWindow is graphWindow
                            || windowsMgr.activeWindow is ruleWindow
                            || windowsMgr.activeWindow is ruleSetWindow);
        }

        public void SettingsOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            windowsMgr.OpenAndShowSettings();
        }

        public void SettingsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }


        public void UndoOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                windowsMgr.activeGraphCanvas.Undo();
                propertyUpdate();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }
        public void UndoCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            if (windowsMgr.activeGraphCanvas == null) e.CanExecute = false;
            else e.CanExecute = windowsMgr.activeGraphCanvas.UndoCanExecute();
        }
        public void RedoOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                windowsMgr.activeGraphCanvas.Redo();
                propertyUpdate();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }
        public void RedoCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (windowsMgr.activeGraphCanvas == null) e.CanExecute = false;
            else e.CanExecute = windowsMgr.activeGraphCanvas.RedoCanExecute();
        }

        #endregion

        #region Help

        public void HelpOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var process = new Process();
            process.StartInfo.FileName = "http://www.graphsynth.com/help";
            process.StartInfo.Verb = "open";
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }

        public void HelpCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        public void AboutGraphSynthOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var about = new AboutGraphSynth(true);
                about.ShowDialog();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public void AboutGraphSynthCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        #endregion

        #region View

        public void GraphLayoutOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Run_ithGraphLayout(GraphLayoutCommands.IndexOf(e.Command as RoutedUICommand));
        }

        public void Run_ithGraphLayout(int i)
        {
            if (i >= GraphLayoutAlgorithms.Count)
                MessageBox.Show("There are only " + GraphLayoutAlgorithms.Count +
                                "graph layout algorithms found; none at position " + i + ".");
            else if (i >= 0)
            {
                var newLayAlgo = GraphLayoutController.Make(GraphLayoutAlgorithms[i]);
                newLayAlgo.Run(windowsMgr.activeGraphCanvas);
            }
        }

        public void GraphLayoutCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (windowsMgr.activeGraphCanvas != null);
        }

        public void ZoomInOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            windowsMgr.activeGraphCanvas.zoomIn();
        }

        public void ZoomInCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (windowsMgr.activeGraphCanvas != null);
        }

        public void ZoomOutOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            windowsMgr.activeGraphCanvas.zoomOut();
        }

        public void ZoomOutCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (windowsMgr.activeGraphCanvas != null);
        }

        #endregion

        #region Output Box

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                outputTextBox.Clear(); // Document.Blocks.Clear();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                outputTextBox.Copy();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private void btnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filename = getOutputSaveFilename();
                if (filename != "")
                {
                    var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
                    try
                    {
                        var sw = new StreamWriter(fileStream, Encoding.Default);
                        sw.Write(outputTextBox.Text);
                        sw.Flush();
                        sw.Close();
                    }
                    catch
                    {
                        MessageBox.Show("No output data was saved.", "Incorrect Filename",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        fileStream.Close();
                    }
                }
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private string getOutputSaveFilename()
        {
            try
            {
                var fileChooser = new SaveFileDialog();
                fileChooser.Title = "Save Output Text as ...";
                fileChooser.InitialDirectory = GSApp.settings.OutputDirAbs;
                fileChooser.Filter = "Text file (*.txt)|*.txt";
                string filename;
                if ((Boolean)fileChooser.ShowDialog(this))
                {
                    filename = fileChooser.FileName;
                    if (string.IsNullOrWhiteSpace(filename))
                        MessageBox.Show("Invalid file name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return filename;
                }
                return "";
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
                return null;
            }
        }

        #endregion

        #region Windows Manager

        public void MinimizeOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (windowsMgr.AllWindowsMinimized())
                windowsMgr.RestoreWindows();
            else windowsMgr.MinimizeWindows();
        }

        public void MinimizeCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        public void FocusNextWindowOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            windowsMgr.FocusNextWindow();
        }

        public void FocusNextWindowCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (windowsMgr.NumberOfWindows > 1)
                e.CanExecute = true;
        }

        #endregion
    }
}