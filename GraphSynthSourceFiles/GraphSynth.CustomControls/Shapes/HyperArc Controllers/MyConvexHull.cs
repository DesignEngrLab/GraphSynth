using System;
using System.Collections.Generic;
using GraphSynth.Representation;
using System.Windows;
using System.Windows.Media;

namespace GraphSynth.GraphDisplay
{
    public static class MIConvexHull
    {
        public static List<Point> Find(PointCollection nodes)
        {
            var oldNodes = new List<Point>(nodes);
            var newOrder = new List<Point>();

            #region Step 1 : Define Convex Octogon

            var maxX = double.NegativeInfinity;
            var maxY = double.NegativeInfinity;
            var maxSum = double.NegativeInfinity;
            var maxDiff = double.NegativeInfinity;
            var minX = double.PositiveInfinity;
            var minY = double.PositiveInfinity;
            var minSum = double.PositiveInfinity;
            var minDiff = double.PositiveInfinity;
            Point nodeMaxX = new Point();
            Point nodeMaxY = new Point(); Point nodeMaxSum = new Point();
            Point nodeMaxDiff = new Point(); Point nodeMinX = new Point();
            Point nodeMinY = new Point(); Point nodeMinSum = new Point(); Point nodeMinDiff = new Point();
            foreach (var n in oldNodes)
            {
                if (n.X > maxX)
                {
                    nodeMaxX = n;
                    maxX = n.X;
                }
                if (n.Y > maxY)
                {
                    nodeMaxY = n;
                    maxY = n.Y;
                }
                if ((n.X + n.Y) > maxSum)
                {
                    nodeMaxSum = n;
                    maxSum = n.X + n.Y;
                }
                if ((n.X - n.Y) > maxDiff)
                {
                    nodeMaxDiff = n;
                    maxDiff = n.X - n.Y;
                }
                if (n.X < minX)
                {
                    nodeMinX = n;
                    minX = n.X;
                }
                if (n.Y < minY)
                {
                    nodeMinY = n;
                    minY = n.Y;
                }
                if ((n.X + n.Y) < minSum)
                {
                    nodeMinSum = n;
                    minSum = n.X + n.Y;
                }
                if ((n.X - n.Y) < minDiff)
                {
                    nodeMinDiff = n;
                    minDiff = n.X - n.Y;
                }
            }
            newOrder.Add(nodeMinX);
            oldNodes.Remove(nodeMinX);
            if (!newOrder.Contains(nodeMinSum))
            {
                newOrder.Add(nodeMinSum);
                oldNodes.Remove(nodeMinSum);
            }
            if (!newOrder.Contains(nodeMinY))
            {
                newOrder.Add(nodeMinY);
                oldNodes.Remove(nodeMinY);
            }
            if (!newOrder.Contains(nodeMaxDiff))
            {
                newOrder.Add(nodeMaxDiff);
                oldNodes.Remove(nodeMaxDiff);
            }
            if (!newOrder.Contains(nodeMaxX))
            {
                newOrder.Add(nodeMaxX);
                oldNodes.Remove(nodeMaxX);
            }
            if (!newOrder.Contains(nodeMaxSum))
            {
                newOrder.Add(nodeMaxSum);
                oldNodes.Remove(nodeMaxSum);
            }
            if (!newOrder.Contains(nodeMaxY))
            {
                newOrder.Add(nodeMaxY);
                oldNodes.Remove(nodeMaxY);
            }
            if (!newOrder.Contains(nodeMinDiff))
            {
                newOrder.Add(nodeMinDiff);
                oldNodes.Remove(nodeMinDiff);
            }

            #endregion

            var oldNum = oldNodes.Count;
            var newNum = newOrder.Count;
            var last = newNum - 1;

            #region Step 2 : Find Signed-Distance to each convex edge

            var convexVectInfo = new double[newNum, 3];

            for (var i = 0; i < last; i++)
            {
                convexVectInfo[i, 0] = newOrder[i + 1].X - newOrder[i].X;
                convexVectInfo[i, 1] = newOrder[i + 1].Y - newOrder[i].Y;
                convexVectInfo[i, 2] = Math.Sqrt(convexVectInfo[i, 0] * convexVectInfo[i, 0] +
                                                 convexVectInfo[i, 1] * convexVectInfo[i, 1]);
            }
            convexVectInfo[last, 0] = newOrder[0].X - newOrder[last].X;
            convexVectInfo[last, 1] = newOrder[0].Y - newOrder[last].Y;
            convexVectInfo[last, 2] = Math.Sqrt(convexVectInfo[last, 0] * convexVectInfo[last, 0] +
                                                convexVectInfo[last, 1] * convexVectInfo[last, 1]);
            var hullCands = new List<Tuple<Point, double>>[newNum];
            for (var j = 0; j < newNum; j++) hullCands[j] = new List<Tuple<Point, double>>();

            for (var i = 0; i < oldNum; i++)
            {
                for (var j = 0; j < newNum; j++)
                {
                    var bX = oldNodes[i].X - newOrder[j].X;
                    var bY = oldNodes[i].Y - newOrder[j].Y;
                    if (signedDistance(convexVectInfo[j, 0], convexVectInfo[j, 1], bX, bY, convexVectInfo[j, 2]) <= 0)
                    {
                        var newSideCand = Tuple.Create(oldNodes[i],
                                                       positionAlong(convexVectInfo[j, 0], convexVectInfo[j, 1], bX, bY,
                                                                     convexVectInfo[j, 2]));
                        var k = 0;
                        while ((k < hullCands[j].Count) && (newSideCand.Item2 > hullCands[j][k].Item2)) k++;
                        hullCands[j].Insert(k, newSideCand);
                        break;
                    }
                }
            }

            #endregion

            #region Step 3: now check the remaining hull candidates

            for (var j = newNum; j > 0; j--)
            {
                if (hullCands[j - 1].Count == 1)
                    newOrder.Insert(j, hullCands[j - 1][0].Item1);
                else if (hullCands[j - 1].Count > 1)
                {
                    var hc = hullCands[j - 1];
                    hc.Insert(0, Tuple.Create(newOrder[j - 1], double.NaN));
                    if (j == newNum) hc.Add(Tuple.Create(newOrder[0], double.NaN));
                    else hc.Add(Tuple.Create(newOrder[j], double.NaN));
                    var i = hc.Count - 2;
                    while (i > 0)
                    {
                        var asin = crossProduct(hc[i].Item1.X - hc[i - 1].Item1.X,
                                                hc[i].Item1.Y - hc[i - 1].Item1.Y, hc[i + 1].Item1.X - hc[i].Item1.X,
                                                hc[i + 1].Item1.Y - hc[i].Item1.Y);
                        if (asin < 0)
                        {
                            hc.RemoveAt(i);
                            if (i == hc.Count - 1) i--;
                        }
                        else i--;
                    }
                    for (i = hc.Count - 2; i > 0; i--)
                        newOrder.Insert(j, hc[i].Item1);
                }
            }

            #endregion
            return newOrder;
        }

        /// <summary>
        ///   Returns the signed distance from edge, a. Where aMag is the magnitude of vector 
        ///   a. Thus the result is basically = |b|*sin(theta)
        /// </summary>
        /// <param name = "aX">X-component of the A vector.</param>
        /// <param name = "aY">Y-component of the A vector..</param>
        /// <param name = "bX">X-component of the B vector.</param>
        /// <param name = "bY">Y-component of the B vector.</param>
        /// <param name = "aMag">magnitude of A vector.</param>
        /// <returns></returns>
        private static double signedDistance(double aX, double aY, double bX, double bY, double aMag)
        {
            return crossProduct(aX, aY, bX, bY) / aMag;
        }

        private static double crossProduct(double aX, double aY, double bX, double bY)
        {
            return (aX * bY - bX * aY);
        }

        private static double positionAlong(double aX, double aY, double bX, double bY, double Mag)
        {
            var dotProduct = aX * bX + aY * bY;
            return dotProduct / Mag; //basically = |b|*cos(theta)
        }
    }
}