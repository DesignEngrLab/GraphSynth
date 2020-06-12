using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace GraphSynth.GraphLayout
{
    public class SortedNameGrid : GraphLayoutBaseClass
    {
        #region Layout Declaration, Sliders
        public SortedNameGrid()
        {
            MakeSlider(SpacingProperty, "Spacing", "The spacing between the center of the nodes",
                       1.0, 3.0, 2, 50, true, 0);
            
        }

        public override string text
        {
            get { return "Sorted Name Grid"; }
        }
        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty SpacingProperty
            = DependencyProperty.Register("Spacing",
                                          typeof(double), typeof(SortedNameGrid),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public double Spacing
        {
            get
            {
                var val = 0.0;
                Dispatcher.Invoke((ThreadStart)delegate { val = (double)GetValue(SpacingProperty); });
                return val;
            }
            set { SetValue(SpacingProperty, value); }
        }

        #endregion

        #region Layout Methods / Algorithm
        protected override bool RunLayout()
        {
            var numRows = (int)Math.Ceiling(Math.Sqrt(numNodes));
            var numCols = (int)Math.Ceiling((double)numNodes / numRows);
            var sortedNames = new List<string>();

            for (var i = 0; i < numNodes; i++)
                sortedNames.Add(graph.nodes[i].name);
            sortedNames.Sort();
            backgroundWorker.ReportProgress(15);
            if (backgroundWorker.CancellationPending) return false;
            var left = Spacing;
            var top = Spacing * numRows;
            var step = 85.0 / numNodes;
            var k = 0;
            for (var i = 0; i < numRows; i++)
                for (var j = 0; j < numCols; j++)
                {
                    var index = graph.nodes.FindIndex(n => (sortedNames[k].Equals(n.name)));
                    graph.nodes[index].X = left + j * Spacing;
                    graph.nodes[index].Y = top - i * Spacing;
                    if (++k >= numNodes) break;
                    backgroundWorker.ReportProgress(15 + (int)(step * k));
                    if (backgroundWorker.CancellationPending) return false;
                }
            return true;
        }
        #endregion
    }
}