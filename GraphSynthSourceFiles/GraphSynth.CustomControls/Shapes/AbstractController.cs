using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphSynth.GraphDisplay
{
    public abstract class AbstractController : UserControl
    {
        public abstract double[] parameters { get; set; }
        protected Shape displayShape;
        protected AbstractController(Shape displayShape)
        {
            this.displayShape = displayShape;
            DefineSliders();
        }
        protected AbstractController(Shape displayShape, double[] parameters)
            : this(displayShape)
        {
            this.parameters = parameters;
        }

        internal abstract Point DetermineTextPoint(FormattedText text, double location, double distance);
        protected abstract void SlidersValuesChanged(object sender, RoutedEventArgs e);
        protected abstract void DefineSliders();

        internal void Redraw()
        {
            displayShape.InvalidateMeasure();
        }
        public AbstractController copy(Shape _displayArc)
        {
            var ctr = this.GetType().GetConstructor(new[] { typeof(Shape), typeof(double[]) });
            return (AbstractController)ctr.Invoke(new object[] { _displayArc, parameters });
        }
        public void copyValueTo(AbstractController victim)
        {
            victim.parameters = this.parameters;
        }
        public sealed override string ToString()
        {
            return ":" + GetType().Name + "," + DoubleCollectionConverter.convert(parameters);
        }


        internal static Boolean ConstructFromString(string p, Shape displayShape, out AbstractController Controller)
        {
            try
            {
                var ctrlData = p.Split(new[] { ',' }, 2);
                var ctrlName = ctrlData[0];
                var ctrlParams = DoubleCollectionConverter.convert(ctrlData[1]).ToArray();
                var ctrlType = Type.GetType("GraphSynth.GraphDisplay." + ctrlName, true);
                var constructor = ctrlType.GetConstructor(new[] { typeof(Shape), typeof(double[]) });
                Controller = (AbstractController)constructor.Invoke(new object[] { displayShape, ctrlParams });
                return true;
            }
            catch { Controller = null; return false; }
        }
    }
}