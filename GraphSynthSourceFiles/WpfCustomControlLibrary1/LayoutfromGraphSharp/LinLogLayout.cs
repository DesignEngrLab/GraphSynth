using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;


namespace GraphSynth.GraphLayout
{
    public class LinLogLayout : GraphLayoutBaseClass
    {

        #region Helper Classes and Global Variables
        class LinLogVertex
        {
            public int Index;
            //public TVertex OriginalVertex;
            public Representation.node OriginalVertex;
            public LinLogEdge[] Attractions;
            public double RepulsionWeight;
            public Point Position;
        }
        class LinLogEdge
        {
            public LinLogVertex Target;
            public double AttractionWeight;
        }

        class QuadTree
        {
            #region Properties
            private readonly QuadTree[] children = new QuadTree[4];
            public QuadTree[] Children
            {
                get { return children; }
            }

            private int index;
            public int Index
            {
                get { return index; }
            }

            private Point position;

            public Point Position
            {
                get { return position; }
            }

            private double weight;

            public double Weight
            {
                get { return weight; }
            }

            private Point minPos;
            private Point maxPos;

            #endregion

            public double Width
            {
                get
                {
                    return Math.Max(maxPos.X - minPos.X, maxPos.Y - minPos.Y);
                }
            }

            protected const int maxDepth = 20;

            public QuadTree(int index, Point position, double weight, Point minPos, Point maxPos)
            {
                this.index = index;
                this.position = position;
                this.weight = weight;
                this.minPos = minPos;
                this.maxPos = maxPos;
            }

            public void AddNode(int nodeIndex, Point nodePos, double nodeWeight, int depth)
            {
                if (depth > maxDepth)
                    return;

                if (index >= 0)
                {
                    AddNode2(index, position, weight, depth);
                    index = -1;
                }

                position.X = (position.X * weight + nodePos.X * nodeWeight) / (weight + nodeWeight);
                position.Y = (position.Y * weight + nodePos.Y * nodeWeight) / (weight + nodeWeight);
                weight += nodeWeight;

                AddNode2(nodeIndex, nodePos, nodeWeight, depth);
            }

            protected void AddNode2(int nodeIndex, Point nodePos, double nodeWeight, int depth)
            {
                //Debug.WriteLine( string.Format( "AddNode2 {0} {1} {2} {3}", nodeIndex, nodePos, nodeWeight, depth ) );
                int childIndex = 0;
                double middleX = (minPos.X + maxPos.X) / 2;
                double middleY = (minPos.Y + maxPos.Y) / 2;

                if (nodePos.X > middleX)
                    childIndex += 1;

                if (nodePos.Y > middleY)
                    childIndex += 2;

                //Debug.WriteLine( string.Format( "childIndex: {0}", childIndex ) );               


                if (children[childIndex] == null)
                {
                    var newMin = new Point();
                    var newMax = new Point();
                    if (nodePos.X <= middleX)
                    {
                        newMin.X = minPos.X;
                        newMax.X = middleX;
                    }
                    else
                    {
                        newMin.X = middleX;
                        newMax.X = maxPos.X;
                    }
                    if (nodePos.Y <= middleY)
                    {
                        newMin.Y = minPos.Y;
                        newMax.Y = middleY;
                    }
                    else
                    {
                        newMin.Y = middleY;
                        newMax.Y = maxPos.Y;
                    }
                    children[childIndex] = new QuadTree(nodeIndex, nodePos, nodeWeight, newMin, newMax);
                }
                else
                {
                    children[childIndex].AddNode(nodeIndex, nodePos, nodeWeight, depth + 1);
                }
            }

            /// <summary>
            /// Az adott rész pozícióját újraszámítja, levonva bel?le a mozgatott node részét.
            /// </summary>
            /// <param name="oldPos"></param>
            /// <param name="newPos"></param>
            /// <param name="nodeWeight"></param>
            public void MoveNode(Point oldPos, Point newPos, double nodeWeight)
            {
                position += ((newPos - oldPos) * (nodeWeight / weight));

                int childIndex = 0;
                double middleX = (minPos.X + maxPos.X) / 2;
                double middleY = (minPos.Y + maxPos.Y) / 2;

                if (oldPos.X > middleX)
                    childIndex += 1;
                if (oldPos.Y > middleY)
                    childIndex += 1 << 1;

                if (children[childIndex] != null)
                    children[childIndex].MoveNode(oldPos, newPos, nodeWeight);
            }
        }

        private LinLogVertex[] vertices;
        private Point baryCenter;
        private double repulsionMultiplier;
        private IDictionary<Representation.node, Point> VertexPositions = new Dictionary<Representation.node, Point>();

        //PARAMETERS THAT ARE ADJUSTABLE?
        private double repulsiveExponent = 0;
        private double attractionExponent = 1.0;
        private int iterationCount = 100;
        private double gravitationMultiplier = 0.1;
        #endregion

        #region Layout declaration, Sliders

        public LinLogLayout()
        {
            MakeSlider(SpacingProperty, "Spacing",
                           "The spacing between nodes",
                          0.1, 3, 1, 10, true, 0);
        }

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
        
        public override string text
        {
            get { return "LinLog Layout"; }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty SpacingProperty
            = DependencyProperty.Register("Spacing",
                                          typeof(double), typeof(LinLogLayout),
                                          new FrameworkPropertyMetadata(0.0,
                                                                    FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region Layout Methods / Algorithm
        protected override bool RunLayout()
		{
            if (graph.nodes.Count <= 1)
            {
                return false;
            }
            VertexPositions.Clear();
            populateVertexDictionary();
            
            backgroundWorker.ReportProgress(30);
            if (backgroundWorker.CancellationPending) return false;

			InitAlgorithm();
			QuadTree quadTree;
			double finalRepuExponent = repulsiveExponent;
			double finalAttrExponent = attractionExponent;
            
			for ( int step = 1; step <= iterationCount; step++ )
			{
				ComputeBaryCenter();
				quadTree = BuildQuadTree();

				#region hûlési függvény meghatározása
				if ( iterationCount >= 50 && finalRepuExponent < 1.0 )
				{
					attractionExponent = finalAttrExponent;
				    repulsiveExponent = finalRepuExponent;
					if ( step <= 0.6 * iterationCount )
					{
						// use energy model with few local minima 
						attractionExponent += 1.1 * ( 1.0 - finalRepuExponent );
						repulsiveExponent += 0.9 * ( 1.0 - finalRepuExponent );
					}
					else if ( step <= 0.9 * iterationCount )
					{
						// gradually move to final energy model
						attractionExponent +=
							1.1 * ( 1.0 - finalRepuExponent ) * ( 0.9 - step / (double)iterationCount ) / 0.3;
						repulsiveExponent +=
							0.9 * ( 1.0 - finalRepuExponent ) * ( 0.9 - step / (double)iterationCount ) / 0.3;
					}
				}
				#endregion

				#region Move each node
				for ( int i = 0; i < vertices.Length; i++ )
				{
					var v = vertices[i];
					double oldEnergy = GetEnergy( i, quadTree );

					// compute direction of the move of the node
					Vector bestDir;
					GetDirection( i, quadTree, out bestDir );

					// line search: compute length of the move
					Point oldPos = v.Position;

					double bestEnergy = oldEnergy;
					int bestMultiple = 0;
                    bestDir /= 32;
					//kisebb mozgatások esetén a legjobb eset meghatározása
					for ( int multiple = 32;
					      multiple >= 1 && ( bestMultiple == 0 || bestMultiple / 2 == multiple );
					      multiple /= 2 )
					{
						v.Position = oldPos + bestDir * multiple;
						double curEnergy = GetEnergy( i, quadTree );
						if ( curEnergy < bestEnergy )
						{
							bestEnergy = curEnergy;
							bestMultiple = multiple;
						}
					}

					//nagyobb mozgatás esetén van-e jobb megoldás?
					for ( int multiple = 64;
					      multiple <= 128 && bestMultiple == multiple / 2;
					      multiple *= 2 )
					{
						v.Position = oldPos + bestDir * multiple;
						double curEnergy = GetEnergy( i, quadTree );
						if ( curEnergy < bestEnergy )
						{
							bestEnergy = curEnergy;
							bestMultiple = multiple;
						}
					}

					//legjobb megoldással mozgatás
					v.Position = oldPos + bestDir * bestMultiple;
					if ( bestMultiple > 0 )
					{
						quadTree.MoveNode( oldPos, v.Position, v.RepulsionWeight );
					}
				}
				#endregion
			}
            backgroundWorker.ReportProgress(90);
            if (backgroundWorker.CancellationPending) return false;
			CopyPositions();
	        int index = 0;
            foreach (Point point in VertexPositions.Values)
            {
                graph.nodes[index].X = point.X * Spacing;
                graph.nodes[index].Y = point.Y * Spacing;
                index++;
            }
            return true;
		}

		protected void CopyPositions()
		{
			// Copy positions
			foreach ( var v in vertices )
				VertexPositions[v.OriginalVertex] = v.Position;
		}


		private void GetDirection( int index, QuadTree quadTree, out Vector dir )
		{
			dir = new Vector( 0, 0 );

			double dir2 = AddRepulsionDirection( index, quadTree, ref dir );
			dir2 += AddAttractionDirection( index, ref dir );
			dir2 += AddGravitationDirection( index, ref dir );

			if ( dir2 != 0.0 )
			{
				dir /= dir2;

				double length = dir.Length;
				if ( length > quadTree.Width / 8 )
				{
					length /= quadTree.Width / 8;
					dir /= length;
				}
			}
			else { dir = new Vector( 0, 0 ); }
		}

		private double AddGravitationDirection( int index, ref Vector dir )
		{
			var v = vertices[index];
			Vector gravitationVector = ( baryCenter - v.Position );
			double dist = gravitationVector.Length;
			double tmp = gravitationMultiplier * repulsionMultiplier * Math.Max( v.RepulsionWeight, 1 ) * Math.Pow( dist, attractionExponent - 2 );
			dir += gravitationVector * tmp;

			return tmp * Math.Abs(attractionExponent - 1);
		}

		private double AddAttractionDirection( int index, ref Vector dir )
		{
			double dir2 = 0.0;
			var v = vertices[index];
			foreach ( var e in v.Attractions )
			{
				//onhurkok elhagyasa
				if ( e.Target == v )
					continue;

				Vector attractionVector = ( e.Target.Position - v.Position );
				double dist = attractionVector.Length;
				if ( dist <= 0 )
					continue;

				double tmp = e.AttractionWeight * Math.Pow( dist, attractionExponent - 2 );
                dir2 += tmp * Math.Abs(attractionExponent - 1);
				dir += ( e.Target.Position - v.Position ) * tmp;
			}
			return dir2;
		}

		/// <summary>
		/// Kiszámítja az <code>index</code> sorszámú pontra ható erõt a 
		/// quadTree segítségével.
		/// </summary>
		/// <param name="index">A node sorszáma, melyre a repulzív erõt számítani akarjuk.</param>
		/// <param name="quadTree"></param>
		/// <param name="dir">A repulzív erõt hozzáadja ehhez a Vectorhoz.</param>
		/// <returns>Becsült második deriváltja a repulzív energiának.</returns>
		private double AddRepulsionDirection( int index, QuadTree quadTree, ref Vector dir )
		{
			var v = vertices[index];

			if ( quadTree == null || quadTree.Index == index || v.RepulsionWeight <= 0 )
				return 0.0;

			Vector repulsionVector = ( quadTree.Position - v.Position );
			double dist = repulsionVector.Length;
			if ( quadTree.Index < 0 && dist < 2.0 * quadTree.Width )
			{
				double dir2 = 0.0;
				for ( int i = 0; i < quadTree.Children.Length; i++ )
					dir2 += AddRepulsionDirection( index, quadTree.Children[i], ref dir );
				return dir2;
			}

			if ( dist != 0.0 )
			{
				double tmp = repulsionMultiplier * v.RepulsionWeight * quadTree.Weight
				             * Math.Pow( dist, repulsiveExponent - 2 );
				dir -= repulsionVector * tmp;
				return tmp * Math.Abs(repulsiveExponent - 1 );
			}

			return 0.0;
		}

		private double GetEnergy( int index, QuadTree q )
		{
			return GetRepulsionEnergy( index, q )
			       + GetAttractionEnergy( index ) + GetGravitationEnergy( index );
		}

		private double GetGravitationEnergy( int index )
		{
			var v = vertices[index];

			double dist = ( v.Position - baryCenter ).Length;
			return gravitationMultiplier * repulsionMultiplier * Math.Max( v.RepulsionWeight, 1 )
			       * Math.Pow( dist, attractionExponent ) / attractionExponent;
		}

		private double GetAttractionEnergy( int index )
		{
			double energy = 0.0;
			var v = vertices[index];
			foreach ( var e in v.Attractions )
			{
				if ( e.Target == v )
					continue;

				double dist = ( e.Target.Position - v.Position ).Length;
				energy += e.AttractionWeight * Math.Pow( dist, attractionExponent ) / attractionExponent;
			}
			return energy;
		}

		private double GetRepulsionEnergy( int index, QuadTree tree )
		{
			if ( tree == null || tree.Index == index || index >= vertices.Length )
				return 0.0;

			var v = vertices[index];

			double dist = ( v.Position - tree.Position ).Length;
			if ( tree.Index < 0 && dist < ( 2 * tree.Width ) )
			{
				double energy = 0.0;
				for ( int i = 0; i < tree.Children.Length; i++ )
					energy += GetRepulsionEnergy( index, tree.Children[i] );

				return energy;
			}

			if (repulsiveExponent == 0.0 )
				return -repulsionMultiplier * v.RepulsionWeight * tree.Weight * Math.Log( dist );

			return -repulsionMultiplier * v.RepulsionWeight * tree.Weight
			       * Math.Pow( dist, repulsiveExponent ) / repulsiveExponent;
		}

		private void InitAlgorithm()
		{
			vertices = new LinLogVertex[graph.nodes.Count];
            var vertexMap = new Dictionary<Representation.node, LinLogVertex>();
			int i = 0;
			foreach (Representation.node v in graph.nodes )
			{
				vertices[i] = new LinLogVertex
				              	{
				              		Index = i,
				              		OriginalVertex = v,
				              		Attractions = new LinLogEdge[v.degree],
				              		RepulsionWeight = 0,
				              		Position = VertexPositions[v]
				              	};
				vertexMap[v] = vertices[i];
				i++;
			}
            backgroundWorker.ReportProgress(40);
            if (backgroundWorker.CancellationPending) return;
			//minden vertex-hez felépíti az attractionWeights, attractionIndexes,
			//és a repulsionWeights struktúrát, valamint átmásolja a pozícióját a VertexPositions-ból
			foreach ( var v in vertices )
			{
				int attrIndex = 0;
                foreach (var e in v.OriginalVertex.arcsTo)
				{
                    double weight = 1;
                    v.Attractions[attrIndex] = new LinLogEdge
					                           	{
					                           		Target = vertexMap[e.From],
					                           		AttractionWeight = weight
					                           	};
					v.RepulsionWeight += 1;
					attrIndex++;
				}

                foreach (var e in v.OriginalVertex.arcsFrom)
				{
				
                    double weight = 1;
                    v.Attractions[attrIndex] = new LinLogEdge
					                           	{
					                           		Target = vertexMap[e.To],
					                           		AttractionWeight = weight
					                           	};
			
					v.RepulsionWeight += 1;
					attrIndex++;
				}
				v.RepulsionWeight = Math.Max( v.RepulsionWeight, gravitationMultiplier );
			    
            }
            backgroundWorker.ReportProgress(50);
            if (backgroundWorker.CancellationPending) return;
			repulsionMultiplier = ComputeRepulsionMultiplier();
		}

		private void ComputeBaryCenter()
		{
			baryCenter = new Point( 0, 0 );
			double repWeightSum = 0.0;
			foreach ( var v in vertices )
			{
				repWeightSum += v.RepulsionWeight;
				baryCenter.X += v.Position.X * v.RepulsionWeight;
				baryCenter.Y += v.Position.Y * v.RepulsionWeight;
			}
			if ( repWeightSum > 0.0 )
			{
				baryCenter.X /= repWeightSum;
				baryCenter.Y /= repWeightSum;
			}
		}

		private double ComputeRepulsionMultiplier()
		{
			double attractionSum = vertices.Sum( v => v.Attractions.Sum( e => e.AttractionWeight ) );
			double repulsionSum = vertices.Sum( v => v.RepulsionWeight );

			if ( repulsionSum > 0 && attractionSum > 0 )
				return attractionSum / Math.Pow( repulsionSum, 2 ) * Math.Pow( repulsionSum, 0.5 * ( attractionExponent - repulsiveExponent ) );

			return 1;
		}

		/// <summary>
		/// Felépít egy QuadTree-t (olyan mint az OctTree, csak 2D-ben).
		/// </summary>
		private QuadTree BuildQuadTree()
		{
			//a minimális és maximális pozíció számítása
			var minPos = new Point( double.MaxValue, double.MaxValue );
			var maxPos = new Point( -double.MaxValue, -double.MaxValue );

			foreach ( var v in vertices )
			{
				if ( v.RepulsionWeight <= 0 )
					continue;

				minPos.X = Math.Min( minPos.X, v.Position.X );
				minPos.Y = Math.Min( minPos.Y, v.Position.Y );
				maxPos.X = Math.Max( maxPos.X, v.Position.X );
				maxPos.Y = Math.Max( maxPos.Y, v.Position.Y );
			}

			//a nemnulla repulsionWeight-el rendelkezõ node-ok hozzáadása a QuadTree-hez.
			QuadTree result = null;
			foreach ( var v in vertices )
			{
				if ( v.RepulsionWeight <= 0 )
					continue;

				if ( result == null )
					result = new QuadTree( v.Index, v.Position, v.RepulsionWeight, minPos, maxPos );
				else
					result.AddNode( v.Index, v.Position, v.RepulsionWeight, 0 );
			}
			return result;
		}

        private void populateVertexDictionary()
        {
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                Point newPoint = new Point();
                newPoint.X = graph.nodes[i].X / 100;
                newPoint.Y = graph.nodes[i].Y / 100;
                VertexPositions.Add(graph.nodes[i], newPoint);
            }
        }


        public class WeightedEdge<Node>
	{
            public Node Source { get; set; }
            public Node Target { get; set; }
            public double Weight { get; private set; }

        public WeightedEdge(Node source, Node target)
			: this(source, target, 1) {}

        public WeightedEdge(Node source, Node target, double weight)
        {
            Source = source;
            Target = target;
            this.Weight = weight;
        }
	}

        private void NormalizePositions()
        {
            if (VertexPositions == null || VertexPositions.Count == 0)
                return;

            //get the topLeft position
            var topLeft = new Point(float.PositiveInfinity, float.PositiveInfinity);
            foreach (var pos in VertexPositions.Values.ToArray())
            {
                topLeft.X = Math.Min(topLeft.X, pos.X);
                topLeft.Y = Math.Min(topLeft.Y, pos.Y);
            }

            //translate with the topLeft position
            foreach (var v in VertexPositions.Keys.ToArray())
            {
                var pos = VertexPositions[v];
                pos.X -= topLeft.X;
                pos.Y -= topLeft.Y;
                VertexPositions[v] = pos;
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