using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using GraphSynth.Representation;

namespace GraphSynth.GraphLayout
{
    public class BoundedFRLayout : GraphLayoutBaseClass
    {
        #region Global Declarations
        private double _temperature;
        private double _maxWidth;
        private double _maxHeight;
        private double K, ConstantOfRepulsion, ConstantOfAttraction, InitialTemperature;
        private int Progress = 0;
        private int VertexCount;
        internal int _iterationLimit = 250;
        internal double _lambda = 0.95;
        private IDictionary<Representation.node, Point> VertexPositions = new Dictionary<Representation.node, Point>();
        internal FRCoolingFunction _coolingFunction = FRCoolingFunction.Exponential;
        public enum FRCoolingFunction
        {
            Linear,
            Exponential
        }
        #endregion

        #region Layout Declaration, Sliders
        public BoundedFRLayout()
        {
            MakeSlider(BoxWidthProperty, "Width", "The width of the desired bounding box",
                       3, 3.75, 2, 1500, true, 0);
            MakeSlider(BoxHeightProperty, "Height", "The height of the desired bounding box",
                       3, 3.75, 2, 1500, true, 0);
            MakeSlider(AttractionMultiplierProperty, "Attraction Multiplier", "The attraction multiplier to be used by the algorithm (Default = 1.2)",
                       0.1, 5, 2, 1, false, 0);
            MakeSlider(RepulsiveMultiplierProperty, "Repulsive Multiplier", "The repulsive multiplier to be used by the algorithm (Default = 0.6)",
                      0.1, 5, 2, 0.4, false, 0);
        }
        public override string text
        {
            get { return "Bounded Fruchterman-Reingold Layout"; }
        }
        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty BoxWidthProperty
            = DependencyProperty.Register("BoxWidth",
                                          typeof(double), typeof(BoundedFRLayout),
                                          new FrameworkPropertyMetadata(20.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty BoxHeightProperty
            = DependencyProperty.Register("BoxHeight",
                                          typeof(double), typeof(BoundedFRLayout),
                                          new FrameworkPropertyMetadata(20.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty AttractionMultiplierProperty
            = DependencyProperty.Register("AttractionMultiplier",
                                          typeof(double), typeof(BoundedFRLayout),
                                          new FrameworkPropertyMetadata(20.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty RepulsiveMultiplierProperty
            = DependencyProperty.Register("RepulsiveMultiplier",
                                          typeof(double), typeof(BoundedFRLayout),
                                          new FrameworkPropertyMetadata(20.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));
        public double BoxWidth
        {
            get
            {
                var val = 0.0;
                Dispatcher.Invoke((ThreadStart)delegate { val = (double)GetValue(BoxWidthProperty); });
                return val;
            }
            set { SetValue(BoxWidthProperty, value); }
        }

        public double BoxHeight
        {
            get
            {
                var val = 0.0;
                Dispatcher.Invoke((ThreadStart)delegate { val = (double)GetValue(BoxHeightProperty); });
                return val;
            }
            set { SetValue(BoxHeightProperty, value); }
        }

        public double AttractionMultiplier
        {
            get
            {
                var val = 0.0;
                Dispatcher.Invoke((ThreadStart)delegate { val = (double)GetValue(AttractionMultiplierProperty); });
                return val;
            }
            set { SetValue(AttractionMultiplierProperty, value); }
        }

        public double RepulsiveMultiplier
        {
            get
            {
                var val = 0.0;
                Dispatcher.Invoke((ThreadStart)delegate { val = (double)GetValue(RepulsiveMultiplierProperty); });
                return val;
            }
            set { SetValue(RepulsiveMultiplierProperty, value); }
        }
        #endregion

        #region Layout methods / Algorithm
        /// <summary>
        /// It computes the layout of the vertices.
        /// </summary>
        protected override bool RunLayout()
        {
            VertexPositions.Clear();
            PopulateVertexDictionary();
            VertexCount = VertexPositions.Count;
            _maxWidth = BoxWidth;
            _maxHeight = BoxHeight;
            K = Math.Sqrt(BoxWidth * BoxHeight / VertexCount);
            ConstantOfRepulsion = Math.Pow(K * RepulsiveMultiplier, 2);
            ConstantOfAttraction = K * AttractionMultiplier;
            InitialTemperature = Math.Min(BoxWidth, BoxHeight) / 10;
      
            // Actual temperature of the 'mass'. Used for cooling.
            var minimalTemperature = InitialTemperature * 0.01;
   
            _temperature = InitialTemperature;

            Progress = 20;
            backgroundWorker.ReportProgress(Progress);
            if (backgroundWorker.CancellationPending) return false;


            int updateEvery = 10;

            for (int i = 0; i < _iterationLimit && _temperature > minimalTemperature; i++)
            {
                if(updateEvery == 0)
                {
                    Progress = Progress + 3;
                    if (Progress > 85) 
                    {
                        Progress = 85;
                    }
                    backgroundWorker.ReportProgress(Progress);
                    if (backgroundWorker.CancellationPending) return false;
                    updateEvery = 10;
                }

                IterateOne();

                //make some cooling
                switch (_coolingFunction)
                {
                    case FRCoolingFunction.Linear:
                        _temperature *= (1.0 - (double)i / (double)_iterationLimit);
                        break;
                    case FRCoolingFunction.Exponential:
                        _temperature *= _lambda;
                        break;
                }
                updateEvery--;
            }

            //PORT VERTEXPOSITIONS TO graph.node (UPDATE LOCATIONS OF NODES)
            int index = 0;
            foreach (Point point in VertexPositions.Values)
            {
                if (graph.nodes[index].name != null)
                {
                    if (graph.nodes[index].arcs != null)
                    {
                        if (point.X == double.NaN || point.Y == double.NaN) return false;
                        graph.nodes[index].X = point.X;
                        graph.nodes[index].Y = point.Y;
                        index++;
                    }
                }
            }
            Progress = 95;
            backgroundWorker.ReportProgress(Progress);
            if (backgroundWorker.CancellationPending) return false;
            return true;
        }

        /// <summary>
        /// Creates a dictionary that returns a Point (X,Y value) for a given Representation.Node key
        /// Disregards nodes of degree zero (dangling nodes)
        /// </summary>
        private void PopulateVertexDictionary()
        {
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                if (graph.nodes[i].name != null)
                {
                    if (graph.nodes[i].arcs != null)
                    {
                        Point newPoint = new Point();
                        newPoint.X = graph.nodes[i].X;
                        newPoint.Y = graph.nodes[i].Y;
                        VertexPositions.Add(graph.nodes[i], newPoint);
                    }
                }
            }
        }

        protected void IterateOne()
        {
            //create the forces (zero forces)
            //var forces = new Dictionary<Vertex, Vector>();
            var forces = new Dictionary<Point, Vector>();

            #region Repulsive forces
            var force = new Vector(0, 0);
            foreach (Point v in VertexPositions.Values.ToList())
            {
                force.X = 0; force.Y = 0;
                Point posV = v;
                foreach (Point u in VertexPositions.Values.ToList())
                {
                    //doesn't repulse itself
                    if (u.Equals(v))
                        continue;

                    //calculating repulsive force
                    Vector delta = posV - u;
                    double length = Math.Max(delta.Length, double.Epsilon);
                    delta = delta / length * ConstantOfRepulsion / length;

                    force += delta;
                }
                forces[v] = force;
            }
            #endregion

            #region Attractive forces

            foreach (node node in VertexPositions.Keys.ToList())
            {
                foreach (arc e in node.arcs.ToList())
                {
                    node source = e.From;
                    node target = e.To;

                    //vonzóerõ számítása a két pont közt
                    if (source != null && target != null)
                    {
                        Vector delta = VertexPositions[source] - VertexPositions[target];
                        double length = Math.Max(delta.Length, double.Epsilon);
                        delta = delta / length * Math.Pow(length, 2) / ConstantOfAttraction;

                        forces[VertexPositions[source]] -= delta;
                        forces[VertexPositions[target]] += delta;
                    }
                }
            }
            
            #endregion

            #region Limit displacement
            foreach (node v in VertexPositions.Keys.ToList())
            {
                Point pos = VertexPositions[v];

                //erõ limitálása a temperature-el
                Vector delta = forces[VertexPositions[v]];
                double length = Math.Max(delta.Length, double.Epsilon);
                delta = delta / length * Math.Min(delta.Length, _temperature);

                //erõhatás a pontra
                pos += delta;

                //falon ne menjünk ki
                pos.X = Math.Min(_maxWidth, Math.Max(0, pos.X));
                pos.Y = Math.Min(_maxHeight, Math.Max(0, pos.Y));
                VertexPositions[v] = pos;
            }
            #endregion
        }

        public FRCoolingFunction CoolingFunction
        {
            get { return _coolingFunction; }
            set
            {
                _coolingFunction = value;
            }
        }

        protected void UpdateParameters()
        {
            K = Math.Sqrt(BoxWidth * BoxHeight / VertexCount);
        }
        #endregion
    }
}

//////////////////////////////////////////////
//  This Graph Layout is a modification of  //
//  a graph layout within the GraphSharp    //
//  application. Portions of code within    //
//  this file were taken directly from      //
//  GraphSharp.                             //
//                                          //
//  Go to http://graphsharp.codeplex.com/   //
//  to find out more about GraphSharp.      //
//                                          //
//////////////////////////////////////////////