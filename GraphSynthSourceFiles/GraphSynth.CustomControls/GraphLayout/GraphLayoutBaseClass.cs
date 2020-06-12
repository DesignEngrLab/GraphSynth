using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Shapes;
using GraphSynth.GraphDisplay;
using GraphSynth.Representation;
using GraphSynth.UI;

namespace GraphSynth.GraphLayout
{
    /// <summary>
    ///   Graph Layout Base Class
    /// </summary>
    public abstract class GraphLayoutBaseClass : Window
    {
        private readonly Path PauseIcon;
        private readonly Path PlayIcon;
        private readonly ToggleButton PlayStopButton;
        private readonly Button keepButton;
        private readonly ProgressBar progressBar1;
        private readonly Button revertButton;
        private readonly StackPanel stackContent;
        private readonly TextBlock txtStatus;
        public BackgroundWorker backgroundWorker;
        private Boolean completed;
        private string eMessage;
        protected int numNodes;
        private double[,] origNodeXYZs;
        private EventWaitHandle progressWait;
        private bool success;

        protected GraphLayoutBaseClass()
        {
            PlayIcon =
                (Path)MyXamlHelpers.Parse("<Path Name=\"PlayIcon\" Stroke=\"Black\" Fill=\"Green\" Data=\"M 0 0 V 20 L 20 10 Z\" />");
            PauseIcon =
                (Path)MyXamlHelpers.Parse("<Path Name=\"PauseIcon\" Stroke=\"Black\" Fill=\"DarkRed\" Data=\"M3,0L17,0 20,3 20,17 17,20 3,20 0,17 0,3z\"/>");

            PlayStopButton = new ToggleButton();
            progressBar1 = new ProgressBar();
            stackContent = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Vertical
            };
            txtStatus = new TextBlock
            {
                Margin = new Thickness(3),
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Text = "Graph Layout operation in progress."
            };
            keepButton = new Button
            {
                Content = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Text = "Keep & Close"
                }
            };
            revertButton = new Button
            {
                Content = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Text = "Revert & Close"
                }
            };

            var midGrid = new Grid();
            midGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            midGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50.0, GridUnitType.Star) });
            midGrid.Children.Add(PlayStopButton);
            midGrid.Children.Add(progressBar1);
            Grid.SetColumn(PlayStopButton, 0);
            Grid.SetColumn(progressBar1, 1);

            var btmGrid = new Grid();
            btmGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(500.0, GridUnitType.Star) });
            btmGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            btmGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            btmGrid.Children.Add(txtStatus);
            btmGrid.Children.Add(keepButton);
            btmGrid.Children.Add(revertButton);
            Grid.SetColumn(txtStatus, 0);
            Grid.SetColumn(keepButton, 1);
            Grid.SetColumn(revertButton, 2);

            WindowStartupLocation = WindowStartupLocation.Manual;
            SizeToContent = SizeToContent.Height;
            Width = 400;
            Content = new StackPanel { Orientation = Orientation.Vertical };
            ((StackPanel)Content).Children.Add(stackContent);
            ((StackPanel)Content).Children.Add(midGrid);
            ((StackPanel)Content).Children.Add(btmGrid);
            PlayStopButton.Checked += PlayStopButton_Checked;
            PlayStopButton.Unchecked += PlayStopButton_Unchecked;
            revertButton.Click += revertButton_Click;
            keepButton.Click += keepButton_Click;
        }

        public abstract string text { get; }
        public double[] Origin { get; set; }

        public designGraph graph
        {
            get
            {
                if (SelectedGraphGUI == null) return null;
                return SelectedGraphGUI.graph;
            }
        }

        public GraphGUI SelectedGraphGUI { get; set; }

        protected virtual bool RunLayout()
        {
            throw new NotImplementedException();
        }

        private void InitializeBackgroundWorker()
        {
            backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted
                += backgroundWorker_RunWorkerCompleted;
            backgroundWorker.DoWork += backgroundWorker_DoWork;
        }


        protected void MakeSlider(DependencyProperty depProperty, string label, string toolTip,
                                  double min, double max, double tickFreq, double initValue, Boolean logarithmic,
                                  int sigDigs)
        {
            var newSATB = new SldAndTextbox
            {
                Label = label,
                Minimum = min,
                Maximum = max,
                TickFrequency = tickFreq,
                ToolTip = toolTip
            };
            if (logarithmic)
                newSATB.Converter = new SliderToTextBoxTextLogarithmicConverter { SigDigs = 1 };
            else newSATB.Converter = new SliderToTextBoxTextLinearConverter { SigDigs = 1 };
            newSATB.UpdateValue(initValue);
            newSATB.ValueChanged += SliderValueChanged;
            var binding = new Binding
            {
                Source = newSATB,
                Mode = BindingMode.TwoWay,
                Path = new PropertyPath(SldAndTextbox.ValueProperty)
            };
            SetBinding(depProperty, binding);
            stackContent.Children.Add(newSATB);
        }

        private void SliderValueChanged(object sender, RoutedEventArgs e)
        {
            //progressWait.WaitOne(400);

            PlayStopButton_Checked(sender, e);
        }


        public void Run(GraphGUI selectedGraphGUI)
        {
            SelectedGraphGUI = selectedGraphGUI;
            if (graph.checkForRepeatNames())
                MessageBox.Show("There were repeat names in the graph. A number has been added to "
                                + "the end of the name to make them all unique", "Names Changed due to repeats.");
            numNodes = graph.nodes.Count;
            origNodeXYZs = new double[3, numNodes];
            for (var i = 0; i < numNodes; i++)
            {
                origNodeXYZs[0, i] = graph.nodes[i].X;
                origNodeXYZs[1, i] = graph.nodes[i].Y;
                origNodeXYZs[2, i] = graph.nodes[i].Z;
            }
            Owner = selectedGraphGUI.OwnerWindow;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = Owner.Left;
            Top = Owner.Top;
            PlayStopButton.IsChecked = true;
            Show();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            backgroundWorker.ReportProgress(5);
            if (backgroundWorker.CancellationPending) return;

            try
            {
                success = RunLayout();
                if (success)
                {
                    backgroundWorker.ReportProgress(100);
                    SelectedGraphGUI.RedrawResizeAndReposition(true);
                }
                progressWait.Set();
            }
            catch (Exception exc)
            {
                eMessage = exc.Message;
            }
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            completed = true;
            if (e.Cancelled) return;
            if (success)
            {
                SelectedGraphGUI.MoveShapesToXYNodeCoordinates();
                txtStatus.Text = "Layout was successful.";
                keepButton.IsEnabled = true;
            }
            else
            {
                txtStatus.Text = "Layout was not successful. Reverting back to previous layout. Error :" + eMessage;
                keepButton.IsEnabled = false;
            }
            PlayStopButton.IsChecked = false;
        }


        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void PlayStopButton_Checked(object sender, RoutedEventArgs e)
        {
            PlayStopButton.Content = PauseIcon;
            PlayStopButton.ToolTip = "Press to stop (currently playing).";
            if ((backgroundWorker != null) && (backgroundWorker.IsBusy))
            {
                backgroundWorker.CancelAsync();
                //progressWait.WaitOne(100);
                backgroundWorker.Dispose();
            }
            InitializeBackgroundWorker();
            progressWait = new AutoResetEvent(false);
            completed = success = false;
            backgroundWorker.RunWorkerAsync();
        }

        private void PlayStopButton_Unchecked(object sender, RoutedEventArgs e)
        {
            PlayStopButton.Content = PlayIcon;
            PlayStopButton.ToolTip = "Press to play (currently stopped).";

            if ((backgroundWorker != null) && (backgroundWorker.IsBusy))
            {
                backgroundWorker.CancelAsync();
                progressWait.WaitOne(100);
                backgroundWorker.Dispose();
            }
        }


        private void keepButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void revertButton_Click(object sender, RoutedEventArgs e)
        {
            if (success)
            {
                for (var i = 0; i < numNodes; i++)
                {
                    graph.nodes[i].X = origNodeXYZs[0, i];
                    graph.nodes[i].Y = origNodeXYZs[1, i];
                    graph.nodes[i].Z = origNodeXYZs[2, i];
                }
                SelectedGraphGUI.MoveShapesToXYNodeCoordinates();
            }
            Close();
        }

        /// <summary>
        /// Determines whether [the specified type] is inherited from GraphLayoutBaseClass.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static Boolean IsInheritedType(Type t)
        {
            while (t != typeof(object))
            {
                if (t == typeof(GraphLayoutBaseClass)) return true;
                t = t.BaseType;
            }
            return false;
        }

        public static GraphLayoutBaseClass Make(Type lt)
        {
            try
            {
                var constructor = lt.GetConstructor(new Type[] { });
                return (GraphLayoutBaseClass)constructor.Invoke(new object[] { });
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
                return null;
            }
        }
    }
}