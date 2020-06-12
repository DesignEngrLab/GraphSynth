/*************************************************************************
 *     This file includes MatrixMath functions and is part of the 
 *     GraphSynth.BaseClasses Project which is the foundation of the 
 *     GraphSynth Application.
 *     GraphSynth.BaseClasses is protected and copyright under the MIT
 *     License.
 *     Copyright (c) 2011 Matthew Ira Campbell, PhD.
 *
 *     Permission is hereby granted, free of charge, to any person obtain-
 *     ing a copy of this software and associated documentation files 
 *     (the "Software"), to deal in the Software without restriction, incl-
 *     uding without limitation the rights to use, copy, modify, merge, 
 *     publish, distribute, sublicense, and/or sell copies of the Software, 
 *     and to permit persons to whom the Software is furnished to do so, 
 *     subject to the following conditions:
 *     
 *     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 *     EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
 *     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGE-
 *     MENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
 *     FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 *     CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
 *     WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *     
 *     Similar functions have been created in a more involved matrix library
 *     written by the author known as StarMath (http://starmath.codeplex.com/).
 *     Please find further details and contact information on GraphSynth
 *     at http://www.GraphSynth.com.
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphSynth
{
    internal static class MatrixMath
    {
        /// <summary>
        ///   This is used below in the close enough to zero booleans to match points
        ///   (see below: sameCloseZero). In order to avoid strange round-off issues - 
        ///   even with doubles - I have implemented this function when comparing the
        ///   position of points (mostly in checking for a valid transformation (see
        ///   ValidTransformation) and if other nodes comply (see otherNodesComply).
        /// </summary>
        private const double epsilon = 0.00001;

        internal static Boolean sameCloseZero(this double x1)
        {
            return Math.Abs(x1) < epsilon;
        }

        internal static Boolean sameCloseZero(this double x1, double x2)
        {
            return sameCloseZero(x1 - x2);
        }

        internal static double[,] Identity(int size)
        {
            var identity = new double[size, size];
            for (var i = 0; i < size; i++)
                identity[i, i] = 1.0;
            return identity;
        }

        internal static double[] multiply(this double[,] A, double[] x, int size)
        {
            var b = new double[size];

            for (int m = 0; m != size; m++)
            {
                b[m] = 0.0;
                for (int n = 0; n != size; n++)
                    b[m] += A[m, n] * x[n];
            }
            return b;
        }

        internal static double[,] multiply(this double[,] A, double[,] B, int size)
        {
            var C = new double[size, size];

            for (int m = 0; m != size; m++)
                for (int n = 0; n != size; n++)
                {
                    C[m, n] = 0.0;
                    for (int p = 0; p != size; p++)
                        C[m, n] += A[m, p] * B[p, n];
                }
            return C;
        }

        /// <summary>
        /// The cross product of two double vectors, A and B, which are of length, 3.
        /// This is equivalent to calling crossProduct, but a slight speed advantage
        /// may exist in skipping directly to this sub-function.
        /// </summary>
        /// <param name = "A">1D double Array, A</param>
        /// <param name = "B">1D double Array, B</param>
        /// <returns></returns>
        internal static double[] crossProduct3(this double[] A, double[] B)
        {
            return new[]
                       {
                           A[1]*B[2] - B[1]*A[2],
                           A[2]*B[0] - B[2]*A[0],
                           A[0]*B[1] - B[0]*A[1]
                       };
        }

        /// <summary>
        /// Returns to 2-norm (square root of the sum of squares of all terms)
        /// of the vector, x.
        /// </summary>
        /// <param name="x">The vector, x.</param>
        /// <param name="size">The size or length of the array.</param>
        /// <param name="dontDoSqrt">if set to <c>true</c> [don't take the square root].</param>
        /// <returns>
        /// Scalar value of 2-norm.
        /// </returns>
        /// <exception cref="System.Exception">The vector, x, is null.</exception>
        public static double norm2(this double[] x, int size = -1, Boolean dontDoSqrt = false)
        {
            if (size == -1) size = x.GetLength(0);
            if (x == null) throw new Exception("The vector, x, is null.");
            var value = 0.0;
            for (int i = 0; i < size; i++)
                value += x[i] * x[i];
            return dontDoSqrt ? value : Math.Sqrt(value);
        }

    }
}
