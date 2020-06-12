using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using GraphSynth.Representation;

namespace GraphSynth.GraphLayout
{
    /// <summary>
    /// The Fruchterman-Reingold Algorithm is a force-directed layout algorithm. 
    /// Vertex layout is determined by the forces pulling vertices together and pushing them apart. 
    /// Attractive forces occur between adjacent vertices only, whereas repulsive forces occur between every pair of vertices. 
    /// Each iteration computes the sum of the forces on each vertex, then moves the vertices to their new positions. 
    /// The movement of vertices is mitigated by the temperature of the system for that iteration: 
    /// as the algorithm progresses through successive iterations, the temperature should decrease so that vertices settle in place.
    /// Adapted from the GraphSharp open-source project.
    /// </summary>
    public class FRLayout : GraphLayoutBaseClass
    {
        #region Global Declarations
        private double _temperature;
        private int Progress = 0;
        private double _maxWidth;
        private double _maxHeight;
        private double K, ConstantOfRepulsion, ConstantOfAttraction, InitialTemperature;
        private int VertexCount;
        private double AttractionMultiplier = 1.2;
        private double RepulsiveMultiplier = 0.6;
        internal int _iterationLimit = 200;
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
        public FRLayout()
        {
            MakeSlider(BoxWidthProperty, "Horizontal Spacing", "Scales the horizontal spacing between nodes",
                       1, 3.0, 2, 80, true, 0);
            MakeSlider(BoxHeightProperty, "Vertical Spacing", "Scales the vertical spacing between",
                       1, 3.0, 2, 80, true, 0);
        }
        public override string text
        {
            get { return "Fruchterman-Reingold Layout"; }
        }
        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty BoxWidthProperty
            = DependencyProperty.Register("BoxWidth",
                                          typeof(double), typeof(FRLayout),
                                          new FrameworkPropertyMetadata(20.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty BoxHeightProperty
            = DependencyProperty.Register("BoxHeight",
                                          typeof(double), typeof(FRLayout),
                                          new FrameworkPropertyMetadata(20.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public double BoxWidth
        {
            get
            {
                var val = 0.0;
                Dispatcher.Invoke((ThreadStart)delegate { val = (double)GetValue(BoxWidthProperty); });
                return val*4;
            }
            set { SetValue(BoxWidthProperty, value); }
        }

        public double BoxHeight
        {
            get
            {
                var val = 0.0;
                Dispatcher.Invoke((ThreadStart)delegate { val = (double)GetValue(BoxHeightProperty); });
                return val*4;
            }
            set { SetValue(BoxHeightProperty, value); }
        }


        #endregion

        #region Layout methods / Algorithm
        /// <summary>
        /// It computes the layout of the vertices.
        /// </summary>
        protected override bool RunLayout()
        {
            VertexPositions.Clear();
            populateVertexDictionary();
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
                if (updateEvery == 0)
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

            Progress = 95;
            backgroundWorker.ReportProgress(Progress);
            if (backgroundWorker.CancellationPending) return false;

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
            return true;
        }

        /// <summary>
        /// Creates a dictionary that returns a Point (X,Y value) for a given Representation.Node key
        /// Disregards nodes of degree zero (dangling nodes)
        /// </summary>
        private void populateVertexDictionary()
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
            var forces = new Dictionary<Point, Vector>();

            #region Repulsive forces
            var force = new Vector(0, 0);
            foreach(Point v in VertexPositions.Values)
            {
                force.X = 0; force.Y = 0;
                Point posV = v;
                foreach (Point u in VertexPositions.Values)
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
            //foreach (arc e in graph.arcs)
            foreach (node node in VertexPositions.Keys.ToList())
            {
                foreach (arc e in node.arcs.ToList())
                {
                    node source = e.From;
                    node target = e.To;

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
            //foreach (node v in graph.nodes)
            foreach(node v in VertexPositions.Keys.ToList())
            {
                Point pos = VertexPositions[v];

                //erõ limitálása a temperature-el
                Vector delta = forces[VertexPositions[v]];
                double length = Math.Max(delta.Length, double.Epsilon);
                delta = delta / length * Math.Min(delta.Length, _temperature);

                //erõhatás a pontra
                pos += delta;

                //falon ne menjünk ki
                //pos.X = Math.Min(_maxWidth, Math.Max(0, pos.X));
                //pos.Y = Math.Min(_maxHeight, Math.Max(0, pos.Y));
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