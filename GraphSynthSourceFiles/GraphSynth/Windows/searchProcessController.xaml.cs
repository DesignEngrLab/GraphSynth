using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GraphSynth.Search;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for searchProcessController.xaml
    /// </summary>
    public partial class searchProcessController : Window
    {
        #region Fields

        private readonly Thread searchThread;
        private long abortedTime;
        public DispatcherTimer processTimer = new DispatcherTimer();
        private long startTime;

        #endregion

        public searchProcessController(SearchProcess sp, int processNum)
        {
            /* the following is common to all GS window types. */
            InitializeComponent();
            Owner = GSApp.main;
            //ShowInTaskbar = false;
            foreach (CommandBinding cb in GSApp.main.CommandBindings)
                CommandBindings.Add(cb);
            foreach (InputBinding ib in GSApp.main.InputBindings)
                InputBindings.Add(ib);
            /***************************************************/

            try
            {
                InitializePriorityAndVerbosityComboBoxes();
                searchThread = new Thread(sp.RunSearchProcess);
                searchThread.SetApartmentState(ApartmentState.STA);
                searchThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
                //this.Text = "Search Process #" + processNum.ToString();
                searchThread.Name = "S" + processNum + "> ";
                cmbPriority.SelectedIndex = 0;
                SearchIO.setVerbosity(searchThread.Name, GSApp.settings.DefaultVerbosity);
                cmbVerbosity.SelectedIndex = GSApp.settings.DefaultVerbosity;
                processTimer.Tick += updateSPCDisplay;
                processTimer.Interval = getIntervalFromVerbosity();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private void InitializePriorityAndVerbosityComboBoxes()
        {
            try
            {
                cmbPriority.Items.Add("lowest");
                cmbPriority.Items.Add("below normal");
                cmbPriority.Items.Add("normal");
                cmbPriority.Items.Add("above normal");
                cmbPriority.Items.Add("highest");

                cmbVerbosity.Items.Add("lowest");
                cmbVerbosity.Items.Add("below normal");
                cmbVerbosity.Items.Add("normal");
                cmbVerbosity.Items.Add("above normal");
                cmbVerbosity.Items.Add("highest");
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public void updateSPCDisplay(object sender, EventArgs e)
        {
            try
            {
                var currentState = searchThread.ThreadState;
                /* the ugly truth is that the threadState can change during the following
                 * conditions. To avoid problems we set up a local variable first. */
                lblIterationBox.Content = SearchIO.getIteration(searchThread.Name).ToString();
                lblMiscBox.Content = SearchIO.getMiscObject(searchThread.Name);
                cmbVerbosity.SelectedIndex = SearchIO.getVerbosity(searchThread.Name);
                processTimer.Interval = getIntervalFromVerbosity();
                try
                {
                    cmbPriority.SelectedIndex = (int)searchThread.Priority;
                }
                catch
                {
                }
                switch (currentState)
                {
                    case ThreadState.SuspendRequested:
                    case ThreadState.StopRequested:
                    case ThreadState.AbortRequested:
                        btnPlay.IsEnabled = false;
                        btnPause.IsEnabled = false;
                        btnStop.IsEnabled = false;
                        btnAbort.IsEnabled = true;
                        cmbPriority.IsEnabled = true;
                        cmbVerbosity.IsEnabled = true;
                        updateTimeDisplay();
                        break;
                    case ThreadState.Suspended:
                        btnPlay.IsEnabled = true;
                        btnPause.IsEnabled = false;
                        btnStop.IsEnabled = false;
                        btnAbort.IsEnabled = true;
                        cmbPriority.IsEnabled = true;
                        cmbVerbosity.IsEnabled = true;
                        break;
                    case ThreadState.WaitSleepJoin:
                    case ThreadState.Running:
                        btnPlay.IsEnabled = false;
                        btnPause.IsEnabled = true;
                        btnStop.IsEnabled = true;
                        btnAbort.IsEnabled = true;
                        cmbPriority.IsEnabled = true;
                        cmbVerbosity.IsEnabled = true;
                        updateTimeDisplay();
                        break;
                    case ThreadState.Aborted:
                    case ThreadState.Stopped:
                        if (abortedTime != 0)
                        {
                            var test = DateTime.Now.Ticks - abortedTime;
                            if (test > 50000000)
                            /* after 5 seconds(?) close the search process window */
                            {
                                processTimer.Stop();
                                Close();
                            }
                        }
                        else
                        {
                            btnPlay.IsEnabled = false;
                            btnPause.IsEnabled = false;
                            btnStop.IsEnabled = false;
                            btnAbort.IsEnabled = false;
                            //  this.ControlBox = true;
                            cmbPriority.IsEnabled = false;
                            cmbVerbosity.IsEnabled = false;
                            abortedTime = DateTime.Now.Ticks;
                        }
                        break;
                }
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (searchThread.ThreadState)
                {
                    case ThreadState.Unstarted:
                        searchThread.Start();
                        processTimer.Start();
                        startTime = DateTime.Now.Ticks;
                        break;
                    case ThreadState.WaitSleepJoin:
                    case ThreadState.Suspended:
                    case ThreadState.Running:
                        searchThread.Resume();
                        processTimer.IsEnabled = true;
                        startTime = DateTime.Now.Ticks -
                                    SearchIO.getTimeInterval(searchThread.Name).Ticks;
                        break;
                    default:
                        SearchIO.output("Cannot (re-)start thread because it is " +
                                        searchThread.ThreadState, 2);
                        break;
                }
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((searchThread.ThreadState == ThreadState.Running) ||
                    (searchThread.ThreadState == ThreadState.WaitSleepJoin))
                {
                    searchThread.Suspend();
                }
                else
                    SearchIO.output("Cannot pause thread because it is " +
                                    searchThread.ThreadState, 2);
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private void btnAbort_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (searchThread.ThreadState == ThreadState.Suspended)
                    searchThread.Resume();
                if ((searchThread.ThreadState == ThreadState.Running) ||
                    (searchThread.ThreadState == ThreadState.WaitSleepJoin) ||
                    (searchThread.ThreadState == ThreadState.AbortRequested) ||
                    (searchThread.ThreadState == ThreadState.StopRequested) ||
                    (searchThread.ThreadState == ThreadState.SuspendRequested))
                {
                    searchThread.Abort();
                    searchThread.Join();
                }
                else
                    SearchIO.output("Cannot hard-stop thread because it is " +
                                    searchThread.ThreadState, 2);
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (searchThread.ThreadState != ThreadState.Suspended)
                {
                    SearchIO.setTerminationRequest(searchThread.Name);
                    SearchIO.output("A stop request has been sent to your search process.");
                }
                else SearchIO.output("Cannot stop thread because it is currently paused.", 2);
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private void cmbPriority_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                searchThread.Priority = (ThreadPriority)cmbPriority.SelectedIndex;
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private void cmbVerbosity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SearchIO.setVerbosity(searchThread.Name, cmbVerbosity.SelectedIndex);
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public TimeSpan getIntervalFromVerbosity()
        {
            /* this is a very subjective little function. Basically, we want to 
             * update the searchProcessController more often for high verbosity
             * systems. The time it returns is in tenths-of-a-microsecond, and 
             * so 10,000 corresponds to an update every millisecond and 10,000,000
             * is a second. */
            switch (SearchIO.getVerbosity(searchThread.Name))
            {
                case 0:
                    return new TimeSpan(25000000);
                case 1:
                    return new TimeSpan(10000000);
                case 2:
                    return new TimeSpan(5000000);
                case 3:
                    return new TimeSpan(1000000);
                case 4:
                    return new TimeSpan(500000);
            }
            return new TimeSpan(500000);
        }

        private void updateTimeDisplay()
        {
            var dispStr = "";
            var dispTime = new TimeSpan(DateTime.Now.Ticks - startTime);
            SearchIO.setTimeInterval(searchThread.Name, dispTime);
            if (dispTime.Days > 0) dispStr += dispTime.Days + ",";
            if (dispTime.Hours > 0) dispStr += dispTime.Hours.ToString().PadLeft(2, '0') + ":";
            if (dispTime.Minutes > 0) dispStr += dispTime.Minutes.ToString().PadLeft(2, '0') + ":";
            if (dispTime.TotalMilliseconds > 1)
                lblTimeDisplay.Content = dispStr
                                         + dispTime.Seconds.ToString().PadLeft(2, '0') + "."
                                         + dispTime.Milliseconds.ToString().PadRight(3, '0');
            else
                lblTimeDisplay.Content = "DD,hh:mm:ss.sss";
        }
    }
}