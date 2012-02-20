using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for FilerProgressWindow.xaml
    /// </summary>
    public partial class FilerProgressWindow : Window
    {
        private Boolean SuppressWarnings;
        public BackgroundWorker backgroundWorker;
        private Boolean completed;
        private string filename;
        private object[] storage;
        private WPFFiler wPFFiler;

        #region Static Constructor-like Methods

        private static FilerProgressWindow SetUpOpeningProgress(string filename, Boolean SuppressWarnings,
                                                                WPFFiler wPFFiler,
                                                                Boolean thisIsRuleSet)
        {
            var fpw = new FilerProgressWindow
                          {
                              filename = filename,
                              SuppressWarnings = SuppressWarnings,
                              wPFFiler = wPFFiler,
                              Title = "Progress: Opening..."
                          };
            if (thisIsRuleSet)
            {
                fpw.MinHeight = 182;
                fpw.lblopen2.Content = "Ruleset: " + Path.GetFileName(filename);
            }
            else
            {
                fpw.lblopen1.Content = Path.GetFileName(filename);
                fpw.stackContent.Children.Remove(fpw.lblopen2);
                fpw.stackContent.Children.Remove(fpw.progressBar2);
            }
            wPFFiler.progWindow = fpw;
            return fpw;
        }

        internal static object[] OpenGraph(string filename, Boolean SuppressWarnings,
                                           WPFFiler wPFFiler)
        {
            var fpw = SetUpOpeningProgress(filename, SuppressWarnings, wPFFiler, false);
            fpw.backgroundWorker.DoWork += fpw.Do_OpenGraph;
            fpw.backgroundWorker.RunWorkerCompleted += fpw.Completed_OpenGraphOrRule;

            fpw.backgroundWorker.RunWorkerAsync();
            fpw.ShowDialog();
            wPFFiler.progWindow = null;
            return fpw.storage;
        }

        internal static object[] OpenRule(string filename, Boolean SuppressWarnings,
                                          WPFFiler wPFFiler)
        {
            var fpw = SetUpOpeningProgress(filename, SuppressWarnings, wPFFiler, false);
            fpw.backgroundWorker.DoWork += fpw.Do_OpenRule;
            fpw.backgroundWorker.RunWorkerCompleted += fpw.Completed_OpenGraphOrRule;

            fpw.backgroundWorker.RunWorkerAsync();
            fpw.ShowDialog();
            wPFFiler.progWindow = null;
            return fpw.storage;
        }

        internal static object[] OpenRuleSet(string filename, Boolean SuppressWarnings,
                                             WPFFiler wPFFiler)
        {
            var fpw = SetUpOpeningProgress(filename, SuppressWarnings, wPFFiler, true);
            fpw.backgroundWorker.DoWork += fpw.Do_OpenRuleSet;
            fpw.backgroundWorker.RunWorkerCompleted += fpw.Completed_OpenRuleSet;

            fpw.backgroundWorker.RunWorkerAsync();
            fpw.ShowDialog();
            wPFFiler.progWindow = null;
            return fpw.storage;
        }


        private static FilerProgressWindow SetUpSavingProgress(string filename, Boolean SuppressWarnings,
                                                               WPFFiler wPFFiler,
                                                               object saveObjects)
        {
            var fpw = new FilerProgressWindow
                          {
                              filename = filename,
                              SuppressWarnings = false,
                              wPFFiler = wPFFiler,
                              Title = "Progress: Saving...",
                              lblopen1 = {Content = Path.GetFileName(filename)}
                          };

            fpw.stackContent.Children.Remove(fpw.lblopen2);
            fpw.stackContent.Children.Remove(fpw.progressBar2);
            wPFFiler.progWindow = fpw;
            fpw.backgroundWorker.RunWorkerCompleted += fpw.Completed_Save;
            if (!(saveObjects is object[])) fpw.storage = new[] { saveObjects };
            else fpw.storage = (object[])saveObjects;
            wPFFiler.progWindow = fpw;
            return fpw;
        }

        internal static void SaveGraph(string filename, Boolean SuppressWarnings,
                                       WPFFiler wPFFiler, object saveObjects)
        {
            var fpw = SetUpSavingProgress(filename, SuppressWarnings, wPFFiler, saveObjects);
            fpw.backgroundWorker.DoWork += fpw.Do_SaveGraph;
            fpw.backgroundWorker.RunWorkerAsync();
            fpw.ShowDialog();
            wPFFiler.progWindow = null;
        }

        internal static void SaveRule(string filename, Boolean SuppressWarnings,
                                      WPFFiler wPFFiler, object saveObjects)
        {
            var fpw = SetUpSavingProgress(filename, SuppressWarnings, wPFFiler, saveObjects);
            fpw.backgroundWorker.DoWork += fpw.Do_SaveRule;
            fpw.backgroundWorker.RunWorkerAsync();
            fpw.ShowDialog();
            wPFFiler.progWindow = null;
        }

        #endregion

        public FilerProgressWindow()
        {
            InitializeComponent();
            stackButtons.Children.Clear();
            backgroundWorker = new BackgroundWorker
                                   {
                                       WorkerReportsProgress = true,
                                       WorkerSupportsCancellation = true
                                   };
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
        }


        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (progressBar1.Dispatcher.CheckAccess())
            {
                if (e.ProgressPercentage > 0)
                    progressBar1.Value = e.ProgressPercentage;
                else
                    progressBar2.Value = -e.ProgressPercentage;
            }
        }

        #region Open Graph or Rule

        private void Do_OpenGraph(object sender, DoWorkEventArgs e)
        {
            e.Result = wPFFiler.OpenGraphAndCanvas(filename);
            if (backgroundWorker.CancellationPending) e.Cancel = true;
        }

        private void Completed_OpenGraphOrRule(object sender, RunWorkerCompletedEventArgs e)
        {
            completed = true;
            if (e.Error != null)
            {
                txtStatus.Text = "Error in opening" + Path.GetFileName(filename) + ": " + e.Error.Message;
                stackButtons.Children.Remove(btnYesOK);
                btnNoCancel.Content = "Cancel";
                storage = null;
                Thread.Sleep(1000);
            }
            else if (e.Cancelled)
                storage = null;
            else
                storage = (object[])e.Result;
            if (Dispatcher.CheckAccess()) Close();
            else Dispatcher.Invoke((ThreadStart)Close);
        }

        private void Do_OpenRule(object sender, DoWorkEventArgs e)
        {
            e.Result = wPFFiler.OpenRuleAndCanvas(filename);
            if (backgroundWorker.CancellationPending) e.Cancel = true;
        }

        #endregion

        #region Open RuleSet

        private void Do_OpenRuleSet(object sender, DoWorkEventArgs e)
        {
            e.Result = wPFFiler.OpenRuleSet(filename);
            if (backgroundWorker.CancellationPending) e.Cancel = true;
        }

        private void Completed_OpenRuleSet(object sender, RunWorkerCompletedEventArgs e)
        {
            completed = true;
            if (e.Error != null)
            {
                txtStatus.Text = "Error in opening" + Path.GetFileName(filename) + ": " + e.Error.Message;
                stackButtons.Children.Remove(btnYesOK);
                btnNoCancel.Content = "Cancel";
                storage = null;
                return;
            }
            else if (e.Cancelled)
                storage = null;
            else
                storage = new[] { e.Result };
            if (Dispatcher.CheckAccess()) Close();
            else Dispatcher.Invoke((ThreadStart)Close);
        }

        #endregion

        #region Save Graph or Rule

        private void Do_SaveGraph(object sender, DoWorkEventArgs e)
        {
            wPFFiler.SaveGraph(filename, storage);
            if (backgroundWorker.CancellationPending) e.Cancel = true;
        }

        private void Do_SaveRule(object sender, DoWorkEventArgs e)
        {
            wPFFiler.SaveRule(filename, storage);
            if (backgroundWorker.CancellationPending) e.Cancel = true;
        }

        private void Completed_Save(object sender, RunWorkerCompletedEventArgs e)
        {
            completed = true;
            if (e.Error != null)
            {
                txtStatus.Text = "Error in saving" + Path.GetFileName(filename) + ": " + e.Error.Message;
                stackButtons.Children.Remove(btnYesOK);
                btnNoCancel.Content = "Cancel";
                storage = null;
                return;
            }
            else if (e.Cancelled)
                storage = null;
            else
                storage = (object[])e.Result;
            if (Dispatcher.CheckAccess()) Close();
            else Dispatcher.BeginInvoke((ThreadStart)Close);
        }

        #endregion

        #region Query and Status

        private Boolean query;
        private EventWaitHandle wh;

        public Boolean QueryUser(string status, int timeToDefault, string strYesButton, string strNoButton,
                                 Boolean defaultResult)
        {
            Dispatcher.Invoke((ThreadStart)delegate
                                                {
                                                    stackButtons.Children.Clear();
                                                    txtStatus.Text = status;
                                                    if (strYesButton.Length > 0)
                                                    {
                                                        stackButtons.Children.Add(btnYesOK);
                                                        btnYesOK.Content = strYesButton;
                                                    }
                                                    else stackButtons.Children.Remove(btnYesOK);
                                                    if (strNoButton.Length > 0)
                                                    {
                                                        stackButtons.Children.Add(btnNoCancel);
                                                        btnNoCancel.Content = strNoButton;
                                                    }
                                                    else stackButtons.Children.Remove(btnNoCancel);
                                                    if (defaultResult) btnYesOK.Focus();
                                                    else btnNoCancel.Focus();
                                                });
            query = defaultResult;
            wh = new AutoResetEvent(false);
            if (timeToDefault > 0)
                wh.WaitOne(timeToDefault);
            else wh.WaitOne();
            return query;
        }


        private void btnNoCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!completed)
            {
                backgroundWorker.CancelAsync();
                storage = null;
                if (Dispatcher.CheckAccess()) Close();
                else Dispatcher.BeginInvoke((ThreadStart)Close);
            }
            else if (wh != null)
            {
                query = false;
                wh.Set();
            }
        }

        private void btnYesOK_Click(object sender, RoutedEventArgs e)
        {
            if (wh != null)
            {
                query = true;
                wh.Set();
            }
        }

        #endregion
    }
}