using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphSynth.Representation;

namespace GraphSynth.GraphDisplay
{
    public abstract class HyperArcController : AbstractController
    {
        public HyperArcShape displayArc
        {
            get { return (HyperArcShape)displayShape; }
        }

        protected HyperArcController(Shape displayShape)
            : base(displayShape)
        {
        }

        protected HyperArcController(Shape displayShape, double[] parameters)
            : base(displayShape, parameters)
        {
        }

        protected override void SlidersValuesChanged(object sender, RoutedEventArgs e)
        {
            var gui = (GraphGUI)displayArc.Parent;
            if (gui != null) gui.HyperArcPropertyChanged((hyperarc)displayArc.icon.GraphElement);
            Redraw();
        }


        internal override Point DetermineTextPoint(FormattedText text, double location, double distance)
        {
            var angle = 2 * Math.PI * System.Convert.ToDouble(location);
            var radius = Math.Min(Math.Abs(text.Width / (2 * Math.Cos(angle))),
                                  Math.Abs(text.Height / (2 * Math.Sin(angle))));

            return new Point(displayArc.Center.X + distance * radius * Math.Cos(-angle) - text.Width / 2,
                             displayArc.Center.Y + distance * radius * Math.Sin(-angle) + text.Height / 2);
        }


        internal abstract Geometry DefineSegment();

    }
}