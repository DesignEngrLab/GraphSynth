using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace GraphSynth.GraphLayout
{
    public class BalloonTreeLayout : GraphLayoutBaseClass
    {
        #region Global Declarations
        private readonly IDictionary<Representation.node, BalloonData> datas = new Dictionary<Representation.node, BalloonData>();
        private int root;
        private HashSet<Representation.node> visitedVertices = new HashSet<Representation.node>();
        internal int minRadius = 2;

        
        public enum Orientation
        {
            down,
            left,
            up,
            right
        } ;

        private class BalloonData
        {
            public int d;
            public int r;
            public float a;
            public float c;
            public float f;
        }

        #endregion

        #region Layout declaration, Sliders
        public BalloonTreeLayout()
        {
            MakeSlider(VerticalSpacingProperty, "Vertical Spacing",
                       "The vertical spacing between the center of the nodes",
                       1.0, 3.0, 2, 40, true, 0);
            MakeSlider(HorizontalSpacingProperty, "Horizontal Spacing",
                       "The horizontal spacing between the center of the nodes",
                       1.0, 3.0,  2, 40, true, 0);
        }

        public override string text
        {
            get { return "Balloon Tree Layout"; }
        }
        #endregion

        #region Dependency Properties


        public static readonly DependencyProperty VerticalSpacingProperty
            = DependencyProperty.Register("Vertical Spacing",
                                          typeof(double), typeof(BalloonTreeLayout),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty HorizontalSpacingProperty
            = DependencyProperty.Register("Horizontal Spacing",
                                          typeof(double), typeof(BalloonTreeLayout),
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

        #endregion

        #region Layout Methods / Algorithm
        protected override bool RunLayout()
        {
            var istree = true;

            InitializeData();
            backgroundWorker.ReportProgress(20);
            if (backgroundWorker.CancellationPending) return false;

            istree = FindRoot();
            backgroundWorker.ReportProgress(40);
            if (backgroundWorker.CancellationPending) return false;

            if (istree == false)
            {
                backgroundWorker.ReportProgress(0);
                throw new Exception("The input graph is not a tree. This layout only works on tree structures.");
            }

            FirstWalk(graph.nodes[root]);
            backgroundWorker.ReportProgress(60);
            if (backgroundWorker.CancellationPending) return false;

            visitedVertices.Clear();

            SecondWalk( graph.nodes[root], null, 0, 0, 1, 0 );
            backgroundWorker.ReportProgress(80);
            if (backgroundWorker.CancellationPending) return false; ;

            NormalizePositions(graph);
            backgroundWorker.ReportProgress(100);
            if (backgroundWorker.CancellationPending) return false; 
            
            return true;
        }

        private void InitializeData()

        {
            for (var i = 0; i < graph.nodes.Count; i++)
                datas[graph.nodes[i]] = new BalloonData();

            visitedVertices.Clear();

        }

        private bool FindRoot()
        {
            HashSet<Representation.node> visitedLeaves = new HashSet<Representation.node>();
            Representation.designGraph copiedgraph = graph.copy();
            bool istree = true;

            while (istree == true && copiedgraph.nodes.Count>1) 
            {
                istree = false;
                for (var i = 0; i < copiedgraph.nodes.Count; i++)
                {
                    if (copiedgraph.nodes[i].arcsFrom.Count == 0)
                    {
                        for (var j = 0; (i < copiedgraph.nodes.Count) && (j < copiedgraph.nodes[i].arcsTo.Count); j++)
                        {
                            string removedname = copiedgraph.nodes[i].arcsTo[j].To.name;
                            copiedgraph.removeNode(copiedgraph.nodes[i].arcsTo[j].To, false);

                            for (var l = 0; l < copiedgraph.arcs.Count; l++)
                            {
                                if (copiedgraph.arcs[l].To != null)
                                {
                                    if (copiedgraph.arcs[l].To.name == removedname)
                                    {
                                        copiedgraph.removeArc(copiedgraph.arcs[l]);
                                    }
                                }
                            }
                            istree = true;
                        }
                        break;
                    }   
                }
            }

            if (istree == false)
            {
                throw new Exception("This graph layout can only be run on trees.");
                //return false;
            }
            var k = 0;
            for (k = 0; copiedgraph.nodes[0].name != graph.nodes[k].name; k++)
            { }
            root = k;

            return true;
        }

       private void FirstWalk(Representation.node v)
        {
            var data = datas[v];
            visitedVertices.Add(v);
            data.d = 0;
            float s = 0;

            foreach (var edge in v.arcsFrom)
             {
                var otherVertex = edge.To;
                var otherData = datas[otherVertex];

                if (!visitedVertices.Contains(otherVertex))
                {
                    FirstWalk(otherVertex);
                    data.d = Math.Max(data.d, otherData.r);
                    otherData.a = (float)Math.Atan(((float)otherData.r) / (data.d + otherData.r));
                    s += otherData.a;
                }
            }
            AdjustChildren(v, data, s);
            SetRadius(v, data);
        }

       private void SecondWalk(Representation.node v, Representation.node r, double x, double y, float l, float t)
        {
            visitedVertices.Add( v );
            BalloonData data = datas[v];

            for (var i = 0; i < graph.nodes.Count; i++)
            {
                if (graph.nodes[i].name == v.name)
                {
                    graph.nodes[i].X = x * HorizontalSpacing;
                    graph.nodes[i].Y = y * VerticalSpacing;
                }
            }

            float dd = l * data.d;
            float p = (float)( t + Math.PI );
            int degree = v.degree;
            float fs = ( degree == 0 ? 0 : data.f / degree );
            float pr = 0;

            foreach ( var edge in v.arcsFrom)
            {
                var otherVertex = edge.To;
                if ( visitedVertices.Contains( otherVertex ) )
                    continue;

                var otherData = datas[otherVertex];
                float aa = data.c * otherData.a;
                float rr = (float)( data.d * Math.Tan( aa ) / ( 1 - Math.Tan( aa ) ) );
                p += pr + aa + fs;

                float xx = (float)( ( l * rr + dd ) * Math.Cos( p ) );
                float yy = (float)( ( l * rr + dd ) * Math.Sign( p ) );
                pr = aa; ;
                SecondWalk( otherVertex, v, x + xx, y + yy, l * data.c, p );
            }
        }

        private void SetRadius( Representation.node v, BalloonData data )
        {
            data.r = (int)Math.Max( data.d / 2, minRadius );
        }

        private void AdjustChildren( Representation.node v, BalloonData data, float s )
        {
            if ( s > Math.PI )
            {
                data.c = (float)Math.PI / s;
                data.f = 0;
            }
            else
            {
                data.c = 1;
                data.f = (float)Math.PI - s;
            }
        }

        protected static void NormalizePositions( Representation.designGraph graph)
        {
            if ( graph.arcs.Count == 0 || graph.nodes.Count == 0)
        				return;

        //get the topLeft position
        	var topLeft = new Point( float.PositiveInfinity, float.PositiveInfinity );
        	foreach ( var n in graph.nodes )
        	{
        		topLeft.X = Math.Min( topLeft.X, n.X );
        		topLeft.Y = Math.Min( topLeft.Y, n.Y );
        	}

        //translate with the topLeft position
            foreach (var n in graph.nodes)
        	{
        		n.X -= topLeft.X;
        		n.Y -= topLeft.Y;
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