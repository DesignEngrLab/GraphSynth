using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using GraphSynth.Representation;
using System.Linq;


namespace GraphSynth.GraphLayout
{
    /// <summary>
    /// This layout is based on inverted self-organising maps. Self-organizing maps are similar with force-based layouts, 
    /// as linked nodes have the tendency to form clusters. The difference with the maps is that there is a uniform space filling 
    /// distrubtion of nodes. This makes the bounds within which the layout takes place important to be calculated correctly at the 
    /// beginning of the processing. ISOM layout algorithm is recommended to be used with well-connected graphs.
    /// 
    /// Self-Organizing Graphs are a new paradigm for graph layout. The method is based on a competitive learning strategy which 
    /// is an extension of Kohonen's self-organizing maps. To our knowledge this is the first connectionist method for graph layout. 
    /// To learn how self-organizing graphs and the ISOM learning algorithm works, there are two papers and simulations available at:
    /// "http://www.csse.monash.edu.au/~berndm/ISOM/index.html"
    /// 
    /// Description borrowed from "http://www.cs.ucy.ac.cy/~cs04pp2/WebHelp/index.htm#page=ISOM Graph Layout.htm"
    /// </summary>
    public class ISOMLayoutAgorithm : GraphLayoutBaseClass
    {
        #region Global Declarations
        private IDictionary<Representation.node, ISOMData> _isomDataDict = new Dictionary<Representation.node, ISOMData>();
        private int radius;
        List<node> nodeList = new List<node>();
        private Point _tempPos;
        private int Progress = 0;
        private Queue<Representation.node> _queue = new Queue<Representation.node>();
        private double adaptation;
        private readonly Random _rnd = new Random(DateTime.Now.Millisecond);
        private IDictionary<Representation.node, Point> VertexPositions = new Dictionary<Representation.node,Point>();
        #endregion

        #region Layout Declaration, Sliders
        public ISOMLayoutAgorithm()
        {
            MakeSlider(VerticalSpacingProperty, "Height",
                       "The height to be used by the algorithm",
                       1.5, 3, 1, 100, true, 0);
            MakeSlider(HorizontalSpacingProperty, "Width",
                       "The width to be used by the algorithm",
                       1.5, 3, 1, 100, true, 0);
            MakeSlider(MaxEpochProperty, "Maximum Epoch",
                       "The maximum epoch to be used by the algorithm",
                       1.0, 3.5, 1, 2000, true, 0);

        }

        public override string text
        {
            get { return "ISOM Layout"; }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty VerticalSpacingProperty
            = DependencyProperty.Register("Veritcal Spacing",
                                          typeof(double), typeof(ISOMLayoutAgorithm),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty HorizontalSpacingProperty
            = DependencyProperty.Register("Horizontal Spacing",
                                          typeof(double), typeof(ISOMLayoutAgorithm),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty MaxEpochProperty
          = DependencyProperty.Register("Maximum Epoch",
                                        typeof(double), typeof(ISOMLayoutAgorithm),
                                        new FrameworkPropertyMetadata(0.0,
                                                                      FrameworkPropertyMetadataOptions.AffectsRender));
        public double VerticalSpacing
        {
            get
            {
                var val = 0.0;
                Dispatcher.Invoke((ThreadStart)delegate { val = (double)GetValue(VerticalSpacingProperty); });
                return val;
            }
            set { SetValue(VerticalSpacingProperty, value); }
        }

        public double HorizontalSpacing
        {
            get
            {
                var val = 0.0;
                Dispatcher.Invoke((ThreadStart)delegate { val = (double)GetValue(HorizontalSpacingProperty); });
                return val;
            }
            set { SetValue(HorizontalSpacingProperty, value); }
        }

        public double MaxEpoch
        {
            get
            {
                var val = 0.0;
                Dispatcher.Invoke((ThreadStart)delegate { val = (double)GetValue(MaxEpochProperty); });
                return val;
            }
            set { SetValue(MaxEpochProperty, value); }
        }
        #endregion

        #region Layout methods / Algorithm
        protected override bool RunLayout()
        {
            Thread.Sleep(1000);
            _isomDataDict.Clear();      //Clearing storage elements in case user chooses to "Relayout"
            _queue.Clear();
            VertexPositions.Clear();
            populateVertexDictionary();
            nodeList.Clear();
            nodeList = VertexPositions.Keys.ToList();

            for (var i = 0; i < VertexPositions.Keys.Count; i++)
            {
                ISOMData isomData;
                if (!_isomDataDict.TryGetValue(nodeList[i], out isomData))
				{
					isomData = new ISOMData();
                    _isomDataDict[nodeList[i]] = isomData;
				}
            }
            backgroundWorker.ReportProgress(25);
            if (backgroundWorker.CancellationPending) return false;
            radius = InitialRadius; //Default value is 5

            int updateEvery = (int) MaxEpoch / 200;

            for (int epoch = 0; epoch < MaxEpoch; epoch++)      //MaxEpoch is adjustable by user (using slider)
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
                    updateEvery = (int)MaxEpoch / 200;
                }

                Adjust();

                //Update Parameters
                double factor = Math.Exp(-1 * CoolingFactor * (1.0 * epoch / MaxEpoch));
                adaptation = Math.Max(MinAdaption, factor * InitialAdaption);
                if (radius > MinRadius && epoch % RadiusConstantTime == 0)
                {
                    radius--;
                }
                updateEvery--;
            }


            backgroundWorker.ReportProgress(90);
            if (backgroundWorker.CancellationPending) return false;
            return true;
 
        }

        protected void  Adjust()
		{
			_tempPos = new Point();
			//get a random point in the container
            _tempPos.X = 0.1 * (HorizontalSpacing * 9) + (_rnd.NextDouble() * 0.8 * (HorizontalSpacing * 9));
			_tempPos.Y = 0.1 * (VerticalSpacing * 9 ) + ( _rnd.NextDouble() * 0.8 * (VerticalSpacing * 9));

			//find the closest vertex to this random point
			Representation.node closest = GetClosest( _tempPos );

			//adjust the vertices to the selected vertex
			for (var i = 0; i < nodeList.Count; i++)
			{
                if (_isomDataDict.Keys.Contains(nodeList[i]))
                {
                    ISOMData vid = _isomDataDict[nodeList[i]];
                    vid.Distance = 0;
                    vid.Visited = false;
                }
				
			}
			AdjustVertex(closest);
		}

		private void AdjustVertex(Representation.node closest)
		{
            if (_isomDataDict.Keys.Contains(closest))
            {
                _queue.Clear();
                ISOMData vid = _isomDataDict[closest];
                vid.Distance = 0;
                vid.Visited = true;
                _queue.Enqueue(closest);
                while (_queue.Count > 0)
                {
                    if (_queue.Count != 0)
                    {
                        Representation.node current = _queue.Dequeue();
                        if (current != null)
                        {
                            ISOMData currentVid = _isomDataDict[current];

                            Point pos = VertexPositions[current];

                            Vector force = _tempPos - pos;
                            double factor = adaptation / Math.Pow(2, currentVid.Distance);

                            pos += factor * force;
                            VertexPositions[current] = pos;

                            List<node> neighbors = new List<node>(GetNeighbors(current));

                            if (currentVid.Distance < radius)
                            {
                                for (int i = 0; i < neighbors.Count; i++)
                                {
                                    ISOMData nvid = _isomDataDict[neighbors[i]];
                                    if (!nvid.Visited)
                                    {
                                        nvid.Visited = true;
                                        nvid.Distance = currentVid.Distance + 1;
                                        _queue.Enqueue(neighbors[i]);
                                    }
                                }
                            }

                            int index = 0;
                            /*
                            for (int i = 0; graph.nodes[i] != current; i++)
                            {
                                index++;
                            }

                            graph.nodes[index].X = VertexPositions[current].X;
                            graph.nodes[index].Y = VertexPositions[current].Y;
                             */

                            //PORT VERTEXPOSITIONS TO graph.node (UPDATE LOCATIONS OF NODES)

                            foreach (Point point in VertexPositions.Values.ToList())
                            {
                                if (graph.nodes[index].name != null)
                                {
                                    if (graph.nodes[index].arcs != null)
                                    {
                                        if (point.X == double.NaN || point.Y == double.NaN) return;
                                        graph.nodes[index].X = point.X;
                                        graph.nodes[index].Y = point.Y;
                                        index++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
		}

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

        /// <summary>
        /// Finds all the nodes that are adjacent to a specified node
        /// </summary>
        /// <param name="current">Node to be checked for neighbors </param>
        /// <returns>Returns a list of nodes neighboring the specified node</returns>
        private List<node> GetNeighbors(node current)
        {
            List<arc> arcsOut = new List<arc>(current.arcsFrom);
            List<arc> arcsIn = new List<arc>(current.arcsTo);
            
            List<node> neighbors = new List<node>();

            //var allArcs = arcsOut.Union(arcsIn).ToList(); Combine both into one list?>

            for (int i = 0; i < arcsOut.Count; i++)
            {
                node neighbour = arcsOut[i].To;
                neighbors.Add(neighbour);
            }
            for (int i = 0; i < arcsIn.Count; i++)
            {
                node neighbour = arcsIn[i].From;
                neighbors.Add(neighbour);
            }
            return neighbors;

        }

        /// <summary>
        /// Finds the the closest vertex to the given position.
        /// </summary>
        /// <param name="tempPos">The position.</param>
        /// <returns>Returns with the reference of the closest vertex.</returns>
        private Representation.node GetClosest(Point tempPos)
		{
			Representation.node vertex = default(Representation.node);
			double distance = double.MaxValue;

			//find the closest vertex

            for (var i = 0; i < nodeList.Count; i++)
			{
                Point nodeLocation = new Point(nodeList[i].X, nodeList[i].Y);
				double d = ( tempPos - nodeLocation ).Length;
				if ( d < distance )
				{
					vertex = nodeList[i];
					distance = d;
				}
			}
			return vertex;
		}
        #endregion

        #region Helper Class
        private class ISOMData
        {
            public Vector Force = new Vector();
            public bool Visited = false;
            public double Distance = 0.0;
        }
        #endregion

        #region Parameters
        private int _radiusConstantTime = 100;
        /// <summary>
        /// Radius constant time. Default value is 100.
        /// </summary>
        public int RadiusConstantTime
        {
            get { return _radiusConstantTime; }
            set
            {
                _radiusConstantTime = value;
                //NotifyPropertyChanged("RadiusConstantTime");
            }
        }

        private int _initialRadius = 5;
        /// <summary>
        /// Default value is 5.
        /// </summary>
        public int InitialRadius
        {
            get { return _initialRadius; }
            set
            {
                _initialRadius = value;
                //NotifyPropertyChanged("InitialRadius");
            }
        }

        private int _minRadius = 1;
        /// <summary>
        /// Minimal radius. Default value is 1.
        /// </summary>
        public int MinRadius
        {
            get { return _minRadius; }
            set
            {
                _minRadius = value;
                //NotifyPropertyChanged("MinRadius");
            }
        }

        private double _initialAdaption = 0.9;
        /// <summary>
        /// Default value is 0.9.
        /// </summary>
        public double InitialAdaption
        {
            get { return _initialAdaption; }
            set
            {
                _initialAdaption = value;
                //NotifyPropertyChanged("InitialAdaption");
            }
        }

        private double _minAdaption;
        /// <summary>
        /// Default value is 0.
        /// </summary>
        public double MinAdaption
        {
            get { return _minAdaption; }
            set
            {
                _minAdaption = value;
                //NotifyPropertyChanged("MinAdaption");
            }
        }

        private double _coolingFactor = 2;
        /// <summary>
        /// Default value is 2.
        /// </summary>
        public double CoolingFactor
        {
            get { return _coolingFactor; }
            set
            {
                _coolingFactor = value;
                //NotifyPropertyChanged("CoolingFactor");
            }
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