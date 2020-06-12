using System;
using System.Threading;
using System.Windows;

namespace GraphSynth.GraphLayout
{
    public class CircularLayout : GraphLayoutBaseClass
    {
        #region Layout declaration, Sliders
        public CircularLayout()
        {
            MakeSlider(SpacingProperty, "Spacing", "The spacing between the center of the nodes",
                       1.0, 3, 2, 50, true, 0);
            MakeSlider(StartAngleProperty, "Angle Offset", "Rotates the circle by the angle", 0.0,
                       360.0, 45, 0.0, false, 1);
        }

        public override string text
        {
            get { return "Circular Layout"; }
        }
        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty SpacingProperty
            = DependencyProperty.Register("Spacing",
                                          typeof(double), typeof(CircularLayout),
                                          new FrameworkPropertyMetadata(20.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StartAngleProperty
            = DependencyProperty.Register("StartAngle",
                                          typeof(double), typeof(CircularLayout),
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

        public double StartAngle
        {
            get
            {
                var val = 0.0;
                Dispatcher.Invoke((ThreadStart)delegate { val = (double)GetValue(StartAngleProperty); });
                return val;
            }
            set { SetValue(StartAngleProperty, value); }
        }

        #endregion

        #region Layout Methods / Algorithm
        protected override bool RunLayout()
        {
            var arcLengths = new double[graph.nodes.Count];
            var circum = Spacing * graph.nodes.Count;
            for (var i = 0; i < graph.nodes.Count; i++)
            {
                arcLengths[i] = Math.Sqrt(graph.nodes[i].DisplayShape.Width * graph.nodes[i].DisplayShape.Width
                                          + graph.nodes[i].DisplayShape.Height * graph.nodes[i].DisplayShape.Height);
                circum += arcLengths[i];
            }
            var radius = circum / (2 * Math.PI);
            var angle = Math.PI * StartAngle / 180.0;
            for (var i = 0; i < graph.nodes.Count; i++)
            {
                angle += (arcLengths[i] + Spacing) / radius;
                graph.nodes[i].X = Math.Cos(angle) * radius;
                graph.nodes[i].Y = Math.Sin(angle) * radius;
                if (backgroundWorker.CancellationPending) return false;
            }
            return true;
        }
        #endregion
    }
}
