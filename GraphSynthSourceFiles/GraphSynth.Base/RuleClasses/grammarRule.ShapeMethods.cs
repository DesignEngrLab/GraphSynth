/*************************************************************************
 *     This grammarRule.ShapeMethods.cs file partially defines the 
 *     grammarRule class (also partially defined in grammarRule.Basic.cs, 
 *     grammarRule.RecognizeApply.cs and grammarRule.NegativeRecognize.cs)
 *     and is part of the GraphSynth.BaseClasses Project which is the 
 *     foundation of the GraphSynth Application.
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
 *     Please find further details and contact information on GraphSynth
 *     at http://www.GraphSynth.com.
 *************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphSynth.Representation
{
    /* Here is the new overload of grammarRule that includes the ability to view
     * the graph as a 2-D shape where nodes are points or vertices, and arcs are
     * lines or edges. */

    public partial class grammarRule
    {
        #region Properties and Fields
        #region Regularization Matrix

        /// <summary>
        ///   this matrix determines the transform to place the first node at (0,0) and the 
        ///   second node at (r,0). This is not stored in the file since it can be quickly 
        ///   determined.
        /// </summary>                                
        private double[,] _regularizationMatrix;
        private double[,] _inverseRegMatrix;

        private double[,] RegularizationMatrix
        {
            get
            {
                if (_regularizationMatrix == null)
                {
                    if (_threeDimensional) calculateRegularizationMatrix3D();
                    else calculateRegularizationMatrix2D();
                }
                return _regularizationMatrix;
            }
        }

        /// <summary>
        ///   Resets the regularization matrix.
        /// </summary>
        public void ResetRegularizationMatrix()
        {
            _regularizationMatrix = null;
        }

        /// <summary>
        /// Calculates the regularization matrix. This is a matrix that is used to simplify the 
        /// shape recognition process. Multiplying the LHS by R (the regularization matrix), the
        /// first node will now be at {0,0,0}, the second is on the x-axis {d1, 0, 0}, then third
        /// is on the x-y plane {d2, d3, 0}. This simplifies the calculations in finding the transformation
        /// of the LHS to get it to match the host shape.
        /// </summary>
        private void calculateRegularizationMatrix3D()
        {
            double[,] quaternion1, quaternion2;
            // start with the regularization matrix at the origin.
            _regularizationMatrix = MatrixMath.Identity(4);
            _inverseRegMatrix = MatrixMath.Identity(4);
            // if there are no nodes, then just return with identity
            if (L.nodes.Count == 0) return;
            // for the first node, simply add translation terms
            _regularizationMatrix[0, 3] = -L.nodes[0].X;
            _regularizationMatrix[1, 3] = -L.nodes[0].Y;
            _regularizationMatrix[2, 3] = -L.nodes[0].Z;
            _inverseRegMatrix[0, 3] = L.nodes[0].X;
            _inverseRegMatrix[1, 3] = L.nodes[0].Y;
            _inverseRegMatrix[2, 3] = L.nodes[0].Z;
            if (L.nodes.Count == 1) return;
            // if two or more nodes, then we add some rotation to move the second node (L.nodes[1])
            // to the x axis.
            var xAxis = new[] { 1.0, 0.0, 0.0 };
            var vL1 = _regularizationMatrix.multiply(new[] { L.nodes[1].X, L.nodes[1].Y, L.nodes[1].Z, 1 }, 4);
            double angle = 0.0;
            var axis = xAxis;
            if (!(vL1[1].sameCloseZero() && vL1[2].sameCloseZero()))
            {
                var vL1_length = vL1.norm2();
                angle = Math.Acos(vL1[0] / vL1_length);
                axis = vL1.crossProduct3(xAxis);
                quaternion1 = makeQuaternion(axis, angle);
                _regularizationMatrix = quaternion1.multiply(_regularizationMatrix, 4);
                quaternion1 = makeQuaternion(axis, -angle);
                _inverseRegMatrix = _inverseRegMatrix.multiply(quaternion1, 4);
            }
            if (L.nodes.Count == 2) return;
            // if three or more, then we move the third node (L.nodes[2]) to the x-y plane/
            var vL2 = _regularizationMatrix.multiply(new[] { L.nodes[2].X, L.nodes[2].Y, L.nodes[2].Z, 1 }, 4);
            // now, how much do we have to rotate about the x-axis to move the 3rd point into the x-y plane (s.t. it's z-value will be zero)
            var theta = Math.Atan2(vL2[2], vL2[1]);
            quaternion2 = makeQuaternion(xAxis, theta);
            _regularizationMatrix = quaternion2.multiply(_regularizationMatrix, 4);
            quaternion2 = makeQuaternion(xAxis, -theta);
            _inverseRegMatrix = _inverseRegMatrix.multiply(quaternion2, 4);
            _threeDimensional = false;
            for (int i = 3; i < L.nodes.Count; i++)
            {
                var vLi = _regularizationMatrix.multiply(new[] { L.nodes[i].X, L.nodes[i].Y, L.nodes[i].Z, 1 }, 4);
                if (!vLi[3].sameCloseZero())
                {
                    _threeDimensional = true;
                    if (Skew != transfromType.Prohibited || Projection != transfromType.Prohibited)
                        throw new Exception("Cannot accommodate Skewing or Projection transformation for 3D left-hand-sides.");
                }
            }
        }
        /// <summary>
        ///   Calculates the regularization matrix.
        /// </summary>
        private void calculateRegularizationMatrix2D()
        {
            _regularizationMatrix = new double[4, 4];
            double a = 1.0, b = 0.0, c = 0.0, d = 1.0, tauX = 0.0, tauY = 0.0;
            double length = 1;


            if (L.nodes.Count >= 1)
            {
                tauX = -L.nodes[0].X;
                tauY = -L.nodes[0].Y;
            }
            if (L.nodes.Count >= 2)
            {
                var theta = -Math.Atan2((L.nodes[1].Y - L.nodes[0].Y), (L.nodes[1].X - L.nodes[0].X));
                if (MatrixMath.sameCloseZero(Math.Abs(theta), Math.PI / 2)) // theta is 90-degrees
                {
                    a = d = 0.0;
                    b = (theta > 0) ? -1 : 1;
                    c = -b;
                    length = Math.Abs(L.nodes[1].Y - L.nodes[0].Y);
                }
                else if (MatrixMath.sameCloseZero(theta))//theta is 0-degrees
                {
                    a = d = 1;
                    b = c = 0;
                    length = Math.Abs(L.nodes[1].X - L.nodes[0].X);
                }
                else
                {
                    a = d = Math.Cos(theta);
                    length = (L.nodes[1].X - L.nodes[0].X) / a;
                    b = -Math.Sin(theta);
                    c = -b;
                }
            }
            _regularizationMatrix[0, 0] = a / length;
            _regularizationMatrix[0, 1] = b / length;
            _regularizationMatrix[0, 2] = (a * tauX + b * tauY) / length;
            _regularizationMatrix[1, 0] = c / length;
            _regularizationMatrix[1, 1] = d / length;
            _regularizationMatrix[1, 2] = (c * tauX + d * tauY) / length;
            _regularizationMatrix[2, 2] = 1.0;
            _regularizationMatrix[3, 0] = 0.0;
            _regularizationMatrix[3, 1] = 0.0;
            _regularizationMatrix[3, 2] = 0.0;
            _regularizationMatrix[3, 3] = 1.0;
        }

        #endregion

        private transfromType _flip = transfromType.Prohibited;
        private transfromType _projection = transfromType.Prohibited;
        private transfromType _rotate = transfromType.XYZIndependent;
        private transfromType _scale = transfromType.XYZIndependent;
        private transfromType _skew = transfromType.Prohibited;
        private transfromType _translate = transfromType.XYZIndependent;
        private Boolean _threeDimensional;

        private Boolean _useShapeRestrictions;
        private Boolean _restrictToNodeShapeMatch;
        private Boolean _transformNodeShapes = false;
        private Boolean _transformNodePositions = true;

        /// <summary>
        ///   Gets or sets a value indicating whether [use shape restrictions].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use shape restrictions]; otherwise, <c>false</c>.
        /// </value>
        public Boolean UseShapeRestrictions
        {
            get { return _useShapeRestrictions; }
            set { _useShapeRestrictions = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [restrict to node shape match].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [restrict to node shape match]; otherwise, <c>false</c>.
        /// </value>
        public Boolean RestrictToNodeShapeMatch
        {
            get { return _restrictToNodeShapeMatch; }
            set { _restrictToNodeShapeMatch = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [transform node positions].
        /// </summary>
        /// <value><c>true</c> if [transform node positions]; otherwise, <c>false</c>.
        /// </value>
        public Boolean TransformNodePositions
        {
            get { return _transformNodePositions; }
            set { _transformNodePositions = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the node shapes are also transformed or simply their position.
        /// </summary>
        /// <value><c>true</c> if [transform node shapes]; otherwise, <c>false</c>.</value>
        public Boolean TransformNodeShapes
        {
            get { return _transformNodeShapes; }
            set { _transformNodeShapes = value; }
        }

        /// <summary>
        ///   Gets or sets the translate transformation allowance.
        /// </summary>
        /// <value>The translate.</value>
        public transfromType Translate
        {
            get { return _translate; }
            set { _translate = value; }
        }

        /// <summary>
        ///   Gets or sets the scale transformation allowance.
        /// </summary>
        /// <value>The scale.</value>
        public transfromType Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        /// <summary>
        ///   Gets or sets the skew transformation allowance.
        /// </summary>
        /// <value>The skew.</value>
        public transfromType Skew
        {
            get { return _skew; }
            set { _skew = value; }
        }

        /// <summary>
        ///   Gets or sets the flip transformation allowance.
        /// </summary>
        /// <value>The flip.</value>
        public transfromType Flip
        {
            get { return _flip; }
            set { _flip = value; }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether this <see cref = "grammarRule" /> allows a rotation transformation.
        /// </summary>
        /// <value><c>true</c> if rotate; otherwise, <c>false</c>.</value>
        public transfromType Rotate
        {
            get { return _rotate; }
            set { _rotate = value; }
        }

        /// <summary>
        ///   Gets or sets the projection transformation allowance.
        /// </summary>
        /// <value>The projection.</value>
        public transfromType Projection
        {
            get { return _projection; }
            set { _projection = value; }
        }



        #endregion

        #region Recognize Methods

        private Boolean findTransform(IList<node> locatedNodes, out double[,] T)
        {
            if (UseShapeRestrictions && Skew == transfromType.Prohibited && Projection == transfromType.Prohibited)
                return find3DTransform(locatedNodes, out T);
            var result = General2DTransform(locatedNodes, out T);
            if (result)
            {
                if (_threeDimensional) return false;
                var zCoord = locatedNodes[0].Z;
                if (locatedNodes.All(n => n.Z == zCoord)) return false;
                return true;
            }
            return false;
        }

        private Boolean General2DTransform(IList<node> locatedNodes, out double[,] T)
        {
            T = MatrixMath.Identity(4);
            /* if there are no nodes, simply return the identity matrix */
            if (locatedNodes.Count == 0) return true;

            #region Variable Set-up

            /* Variable Set-up: This seems a little verbose, but it is necessary
             * to ease the calculations later and to avoid compile errors. */
            double x1, x2, x3, x4, y1, y2, y3, y4;
            x2 = x3 = x4 = y2 = y3 = y4 = 0;
            double tx, ty, wX, wY, a, b, c, d;
            tx = ty = wX = wY = a = b = c = d = 0;
            double k1, k2, k3, k4;
            k1 = k2 = k3 = k4 = 0;
            double u3, u4, v3, v4;
            u3 = u4 = v3 = v4 = 0;

            /* This x1 and y1 are matched with the position of L.nodes[0].X and .Y, 
             * which, given the Regularization concept, is effectively at 0,0. This is
             * what Regularization does. It's as if the first node in L is moved to zero
             * without loss of generality, and all the other nodes are translated accord-
             * ingly. So, u1 = v1 = 0. */
            x1 = locatedNodes[0].X;
            y1 = locatedNodes[0].Y;
            if (locatedNodes.Count >= 2)
            {
                x2 = locatedNodes[1].X;
                y2 = locatedNodes[1].Y;
                /* Given regularization, this second point is scaled and rotated to 1, 0. */
            }
            if (locatedNodes.Count >= 3)
            {
                x3 = locatedNodes[2].X;
                y3 = locatedNodes[2].Y;
                var temp = new[] { L.nodes[2].X, L.nodes[2].Y, 0.0, 1.0 };
                temp = RegularizationMatrix.multiply(temp, 4);
                u3 = temp[0];
                v3 = temp[1];
            }
            if (locatedNodes.Count >= 4)
            {
                x4 = locatedNodes[3].X;
                y4 = locatedNodes[3].Y;
                var temp = new[] { L.nodes[3].X, L.nodes[3].Y, 1.0 };
                temp = RegularizationMatrix.multiply(temp, 3);
                u4 = temp[0];
                v4 = temp[1];
            }

            #endregion

            // set values for tx, and ty
            tx = x1;
            ty = y1;

            #region Calculate Projection Terms

            if ((locatedNodes.Count <= 3) || ((v3 * v4).sameCloseZero()))
                wX = wY = 0;
            else
            {
                //calculate intermediate values used only in this class or method
                //k1 = (u4 * (y4 - y2) / v4 - u3 * (y3 - y2) / v3);   //(Equation 3 of program)
                k1 = u4 * v3 * (y4 - y2) - u3 * v4 * (y3 - y2);
                if (k1.sameCloseZero()) k1 = 0;
                else k1 /= v3 * v4;

                //k2 = (y3 - y2 * u3 + ty * u3 - ty) / v3 + (-y4 - ty * u4 + y2 * u4 + ty) / v4;  //(Equation 4 of program)
                k2 = v4 * (y3 - y2 * u3 + ty * u3 - ty) + v3 * (-y4 - ty * u4 + y2 * u4 + ty);
                if (k2.sameCloseZero()) k2 = 0;
                else k2 /= v3 * v4;

                //k3 = (u3 * (x3 - x2) / v3 - u4 * (x4 - x2) / v4);
                k3 = u3 * v4 * (x3 - x2) - u4 * v3 * (x4 - x2);
                if (k3.sameCloseZero()) k3 = 0;
                else k3 /= v3 * v4;

                //k4 = (x4 - x2 * u4 + tx * u4 - tx) / v4 - (x3 + tx * u3 - x2 * u3 - tx) / v3;
                k4 = v3 * (x4 - x2 * u4 + tx * u4 - tx) - v4 * (x3 + tx * u3 - x2 * u3 - tx);
                if (k4.sameCloseZero()) k4 = 0;
                else k4 /= v3 * v4;

                //calculate wY, and wX
                wY = (k1 * k4) - (k2 * k3);
                if (wY.sameCloseZero()) wY = 0;
                else wY /= k3 * (y3 - y4) + k1 * (x3 - x4); //(Equation 7 of program)

                wX = wY * (y3 - y4) + k2;
                if (wX.sameCloseZero()) wX = 0;
                else wX /= k1; //is (Equation 8 of program) which is rewritten for program's accuracy
            }

            #endregion

            #region Calculate rotate, scale, skew terms

            if (locatedNodes.Count <= 1)
            {
                a = d = 1;
                b = c = 0;
            }
            else
            {
                // calculate a 
                a = x2 * (wX + 1) - tx;
                //calculate c
                c = y2 * (wX + 1) - ty;


                if ((locatedNodes.Count <= 2) || (LnodesAreCollinear()))
                {
                    /* in order for the validTransform to function, b and d are set as
                     * if there is a rotation as opposed to a Skew in X. It is likely that
                     * isotropic transformations like rotation are more often intended than skews. */
                    // var theta = Math.Atan2(-c, a);
                    b = -c;
                    d = a;
                }
                else
                {
                    //calculate b
                    b = x3 * (wX * u3 + wY * v3 + 1) - a * u3 - tx;
                    if (b.sameCloseZero()) b = 0;
                    else b /= v3;
                    //calculate d
                    d = y3 * (wX * u3 + wY * v3 + 1) - c * u3 - ty;
                    if (d.sameCloseZero()) d = 0;
                    else d /= v3;
                }
            }

            #endregion


            T[0, 0] = a;
            T[0, 1] = b;
            T[0, 3] = tx;
            T[1, 0] = c;
            T[1, 1] = d;
            T[1, 3] = ty;
            T[3, 0] = wX;
            T[3, 1] = wY;
            T[3, 3] = 1;
            T = T.multiply(RegularizationMatrix, 4);
            T[0, 0] /= T[3, 3];
            T[0, 1] /= T[3, 3];
            T[0, 2] = 0.0;
            T[0, 3] /= T[3, 3];
            T[1, 0] /= T[3, 3];
            T[1, 1] /= T[3, 3];
            T[1, 2] = 0.0;
            T[1, 3] /= T[3, 3];
            T[2, 0] = 0.0;
            T[2, 1] = 0.0;
            T[2, 2] = 1.0;
            T[2, 3] = 0.0;
            T[3, 0] /= T[3, 3];
            T[3, 1] /= T[3, 3];
            T[3, 2] = 0.0;
            T[3, 3] = 1;
            snapToIntValues(T);
            //if (RestrictToNodeShapeMatch && T[0,0]==1 && T[)
            return validTransform(T);
        }

        private Boolean find3DTransform(IList<node> locatedNodes, out double[,] T)
        {
            // T = trans*Quat2*Quat1*Scale*R*LHS
            T = MatrixMath.Identity(4);
            // if there are no nodes, just return the identity matrix
            if (locatedNodes.Count == 0) return true;
            /* move the first node into location by translation */
            var transMatrix = MatrixMath.Identity(4);
            var refPt = new double[3];
            transMatrix[0, 3] = refPt[0] = locatedNodes[0].X;
            transMatrix[1, 3] = refPt[1] = locatedNodes[0].Y;
            transMatrix[2, 3] = refPt[2] = locatedNodes[0].Z;
            if (locatedNodes.Count == 1)
            {
                T = transMatrix;
                return ValidTranslation(transMatrix.multiply(RegularizationMatrix, 4));
            }
            // if there is just one node find the proper translation matrix (this T matrix * Regularization) and return

            /* for 2 or more we first find scale factors needed. */
            /* first, figure out how much to scale the shape */
            var vHost = new[]
                    {
                        locatedNodes[1].X - refPt[0],
                        locatedNodes[1].Y - refPt[1],
                        locatedNodes[1].Z - refPt[2]
                    };
            var vHost_length = vHost.norm2();


            var vL = RegularizationMatrix.multiply(new[] { L.nodes[1].X, L.nodes[1].Y, L.nodes[1].Z, 1 }, 4);
            var vL_length = vL[0];
            var xScale = vHost_length / vL_length;
            vL = new[] { 1.0, 0.0, 0.0 }; // turn vL into a unit vector - this is simply 1,0,0 since - after regularization
            // the point is on the x-axisw
            var axis = vL.crossProduct3(vHost);
            double angle;
            if (axis.Sum().sameCloseZero())
            {
                // if the two vectors are the same then the cross product will be all zeroes
                axis = vHost;
                angle = vHost[0] < 0 ? Math.PI : 0.0;
            }
            else angle = Math.Acos(vHost[0] / vHost_length); // essentially, the dot product to find the angle
            //now construct the quaternion for this rotation and multiply by T
            double[,] quaternion1 = (angle.sameCloseZero()) ? MatrixMath.Identity(4) : makeQuaternion(axis, angle);
            var scaleMatrix = MatrixMath.Identity(4);
            scaleMatrix[0, 0] = xScale;
            if (locatedNodes.Count == 2)
            {
                if (ValidScaling(_inverseRegMatrix.multiply(scaleMatrix, 4)))
                    T = scaleMatrix.multiply(RegularizationMatrix, 4);
                else
                {
                    scaleMatrix[1, 1] = scaleMatrix[2, 2] = xScale;
                    if (ValidScaling(_inverseRegMatrix.multiply(scaleMatrix, 4)))
                        T = scaleMatrix.multiply(RegularizationMatrix, 4);
                    else return false;
                }
                T = quaternion1.multiply(T, 4);
                T = transMatrix.multiply(T, 4);
                snapToIntValues(T);
                return ValidRotation(T, _inverseRegMatrix.multiply(scaleMatrix, 4));
            }
            /* if there are 3 or more points, then we find a new Quaternion that will multiply
             * the former result. In order to keep the second node in place, we use the vHost
             * as the axis of rotation. */
            // T = trans*Quat2*Quat1*Scale*R*LHS
            axis = vHost;
            var axisUnitVector = new[] { axis[0] / vHost_length, axis[1] / vHost_length, axis[2] / vHost_length };
            // move the third L point (L.nodes[2] to the proper orientation. Note that it is not multiplied 
            // by translation since it is essentially the delta from the reference point - without translation, this
            // is zero given the way Regularization puts the first node at {0,0,0}.
            vL = RegularizationMatrix.multiply(new[] { L.nodes[2].X, L.nodes[2].Y, L.nodes[2].Z, 1 }, 4);
            vL = scaleMatrix.multiply(vL, 4);
            vL = quaternion1.multiply(vL, 4);
            // dxAlongAxis is the dot project of where this point vL is projected to the the axis.
            var dxAlongAxisL = vL[0] * axisUnitVector[0] + vL[1] * axisUnitVector[1] + vL[2] * axisUnitVector[2];
            // set up a new host vector from the reference point, which is the location of the first node
            vHost = new[]
            {
                locatedNodes[2].X - refPt[0],
                locatedNodes[2].Y - refPt[1],
                locatedNodes[2].Z - refPt[2]
            };
            // find the distance (dx) along the axis for this one as well.
            // todo: what if dxAlongAxisL and dxAlongAxisHost are different? then I suppose there is a skew x w.r.t.y we could calculate 
            // var dxAlongAxisHost = vHost[0] * axisUnitVector[0] + vHost[1] * axisUnitVector[1] + vHost[2] * axisUnitVector[2];

            // reformulate vL as the vector to the L-node from the axis.
            vL = new[]
            {
                vL[0] - (dxAlongAxisL*axisUnitVector[0]),
                vL[1] - (dxAlongAxisL*axisUnitVector[1]),
                vL[2] - (dxAlongAxisL*axisUnitVector[2])
            };
            vL_length = vL.norm2(3);
            // similarly reformulate vHost as the vector to the host-node from the axis.
            vHost = new[]
            {
                vHost[0] - (dxAlongAxisL*axisUnitVector[0]),
                vHost[1] - (dxAlongAxisL*axisUnitVector[1]),
                vHost[2] - (dxAlongAxisL*axisUnitVector[2])
            };
            vHost_length = vHost.norm2();
            // the ratio of these new lengths is the scale-Y term (well, this scale matrix if adulterated by the regularization, so it is
            // not the true scale-y
            var yScale = vHost_length / vL_length;
            scaleMatrix[1, 1] = yScale;
            // by using the dot-product equals cos(angle) identity, solve for the angle for the second quaternion operation
            vL = new[] { vL[0] / vL_length, vL[1] / vL_length, vL[2] / vL_length };
            vHost = new[] { vHost[0] / vHost_length, vHost[1] / vHost_length, vHost[2] / vHost_length };
            var dot = vL[0] * vHost[0] + vL[1] * vHost[1] + vL[2] * vHost[2];
            angle = dot >= 1.0 ? 0.0
               : dot <= -1.0 ? Math.PI
                : Math.Acos(dot);
            var quaternion2 = (angle.sameCloseZero()) ? MatrixMath.Identity(4) : makeQuaternion(axis, angle);
            if (locatedNodes.Count == 3)
            {
                if (ValidScaling(_inverseRegMatrix.multiply(scaleMatrix, 4)))
                    T = scaleMatrix.multiply(RegularizationMatrix, 4);
                else
                {
                    scaleMatrix[2, 2] = yScale;
                    if (ValidScaling(_inverseRegMatrix.multiply(scaleMatrix, 4)))
                        T = scaleMatrix.multiply(RegularizationMatrix, 4);
                    else return false;
                }
                T = quaternion1.multiply(T, 4);
                T = quaternion2.multiply(T, 4);
                T = transMatrix.multiply(T, 4);
                snapToIntValues(T);
                return ValidRotation(T, _inverseRegMatrix.multiply(scaleMatrix, 4));
            }
            // else, there are 4 or more 
            vL = scaleMatrix.multiply(RegularizationMatrix, 4).multiply(new[] { L.nodes[3].X, L.nodes[3].Y, L.nodes[3].Z, 1 }, 4);
            vL_length = vL.norm2();
            vHost = new[]
            {
                locatedNodes[3].X - locatedNodes[0].X,
                locatedNodes[3].Y - locatedNodes[0].Y,
                locatedNodes[3].Z - locatedNodes[0].Z
            };
            vHost_length = vHost.norm2();
            var zScale = vHost_length / vL_length;
            scaleMatrix[2, 2] = zScale;

            T = scaleMatrix.multiply(RegularizationMatrix, 4);
            T = quaternion1.multiply(T, 4);
            T = quaternion2.multiply(T, 4);
            T = transMatrix.multiply(T, 4);
            snapToIntValues(T);
            return ValidScaling(_inverseRegMatrix.multiply(scaleMatrix, 4))
                && ValidRotation(T, _inverseRegMatrix.multiply(scaleMatrix, 4));
        }

        private bool ValidRotation(double[,] T, double[,] scaleMatrix)
        {
            if (Rotate == transfromType.XYZIndependent || Rotate == transfromType.BothIndependent) return true;
            var inverseScale = MatrixMath.Identity(4);
            inverseScale[0, 0] = 1 / scaleMatrix[0, 0];
            inverseScale[2, 1] = 1 / scaleMatrix[1, 1];
            inverseScale[2, 2] = 1 / scaleMatrix[2, 2];
            var t = inverseScale.multiply(T, 4);
            // http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToEuler/index.htm
            double rotX, rotY, rotZ;
            if (t[1, 0].sameCloseZero(1))
            { // singularity at north pole
                rotX = Math.Atan2(t[0, 2], t[2, 2]);
                rotY = Math.PI / 2;
                rotZ = 0;
            }
            else if (t[1, 0].sameCloseZero(-1))
            { // singularity at south pole
                rotX = Math.Atan2(t[0, 2], t[2, 2]);
                rotY = -Math.PI / 2;
                rotZ = 0;
            }
            else
            {
                rotX = Math.Atan2(-t[2, 0], t[0, 0]);
                rotZ = Math.Atan2(-t[1, 2], t[1, 1]);
                rotY = Math.Asin(t[1, 0]);
            }
            if (Rotate == transfromType.XYZUniform || Rotate == transfromType.BothUniform)
                return (rotX.sameCloseZero(rotY) && rotY.sameCloseZero(rotZ));
            if (Rotate == transfromType.Prohibited)
                return (rotX.sameCloseZero() && rotY.sameCloseZero() && rotZ.sameCloseZero());
            if (Rotate == transfromType.OnlyX)
                return (rotY.sameCloseZero() && rotZ.sameCloseZero());
            if (Rotate == transfromType.OnlyY)
                return (rotX.sameCloseZero() && rotZ.sameCloseZero());
            if (Rotate == transfromType.OnlyZ)
                return (rotY.sameCloseZero() && rotX.sameCloseZero());
            return false;
        }

        private bool ValidRotation(double[] vHost)
        {
            if (Rotate == transfromType.XYZIndependent) return true;
            var vL = new[] { L.nodes[1].X - L.nodes[0].X, L.nodes[1].Y - L.nodes[0].Y, L.nodes[1].Z - L.nodes[0].Z };
            return (vL.crossProduct3(vHost).norm2().sameCloseZero()
                && (0 < vL[0] * vHost[0] + vL[1] * vHost[1] + vL[2] * vHost[2]));
        }

        private Boolean ValidRotation(double angle)
        {
            if (Rotate == transfromType.XYZIndependent) return true;
            return (angle.sameCloseZero());
        }

        private Boolean ValidTranslation(double[,] T)
        {
            var tx = T[0, 3];
            var ty = T[1, 3];
            var tz = T[2, 3];
            switch (Translate)
            {
                case transfromType.Prohibited:
                    return (tx.sameCloseZero() && ty.sameCloseZero() && tz.sameCloseZero());
                case transfromType.OnlyX:
                    return (ty.sameCloseZero() && tz.sameCloseZero());
                case transfromType.OnlyY:
                    return (tx.sameCloseZero() && tz.sameCloseZero());
                case transfromType.OnlyZ:
                    return (tx.sameCloseZero() && ty.sameCloseZero());
                case transfromType.XYZUniform:
                    return (tx.sameCloseZero(ty) && ty.sameCloseZero(tz));
                default:
                    return true;
            }
        }
        private Boolean ValidScaling(double[,] T)
        {
            var sx = T[0, 0];
            var sy = T[1, 1];
            var sz = T[2, 2];
            switch (Scale)
            {
                case transfromType.Prohibited:
                    if (Math.Abs(sx).sameCloseZero(1) && Math.Abs(sy).sameCloseZero(1)
                        && Math.Abs(sz).sameCloseZero(1))
                        break;
                    else return false;
                case transfromType.OnlyX:
                    if (Math.Abs(sy).sameCloseZero(1) && Math.Abs(sz).sameCloseZero(1)) break;
                    else return false;
                case transfromType.OnlyY:
                    if (Math.Abs(sx).sameCloseZero(1) && Math.Abs(sz).sameCloseZero(1)) break;
                    else return false;
                case transfromType.OnlyZ:
                    if (Math.Abs(sx).sameCloseZero(1) && Math.Abs(sy).sameCloseZero(1)) break;
                    else return false;
                case transfromType.XYZUniform:
                    if (Math.Abs(sx).sameCloseZero(Math.Abs(sy)) && Math.Abs(sy).sameCloseZero(Math.Abs(sz))) break;
                    else return false;
            }
            switch (Flip)
            {
                case transfromType.Prohibited:
                    return (sx >= 0 && sy >= 0 && sz >= 0);
                case transfromType.OnlyX:
                    return (sy >= 0 && sz >= 0);
                case transfromType.OnlyY:
                    return (sx >= 0 && sz >= 0);
                case transfromType.OnlyZ:
                    return (sx >= 0 && sy >= 0);
                case transfromType.XYZUniform:
                    return ((sx >= 0 && sy >= 0 && sz >= 0) || (sx <= 0 && sy <= 0 && sz <= 0));
                default:
                    return true;
            }
        }

        private double[,] makeQuaternion(double[] axis, double angle)
        {
            /* this is informed by http://www.cprogramming.com/tutorial/3d/quaternions.html */
            var length = axis.norm2();
            var axisNormalized = axis.Select(value => value / length).ToArray();
            var halfAngle = -angle / 2;
            var w = Math.Cos(halfAngle);
            var x = axisNormalized[0] * Math.Sin(halfAngle);
            var y = axisNormalized[1] * Math.Sin(halfAngle);
            var z = axisNormalized[2] * Math.Sin(halfAngle);
            length = Math.Sqrt(w * w + x * x + y * y + z * z);
            //length = 1;
            w /= length;
            x /= length;
            y /= length;
            z /= length;
            /* | 1-2yy-2zz       2xy-2wz        2xz+2wy      0 |
               |  2xy+2wz       1-2xx-2zz       2yz+2wx      0 |
               |  2xz-2wy        2yz-2wx       1-2xx-2yy     0 |
               |     0              0              0         1 |  */
            var q = new double[4, 4];
            //hmm, are these transposed?
            q[0, 0] = 1 - 2 * y * y - 2 * z * z;
            q[1, 0] = 2 * x * y - 2 * w * z;
            q[2, 0] = 2 * x * z + 2 * w * y;
            q[0, 1] = 2 * x * y + 2 * w * z;
            q[1, 1] = 1 - 2 * x * x - 2 * z * z;
            q[2, 1] = 2 * y * z + 2 * w * x;
            q[0, 2] = 2 * x * z - 2 * w * y;
            q[1, 2] = 2 * y * z - 2 * w * x;
            q[2, 2] = 1 - 2 * x * x - 2 * y * y;
            q[3, 3] = 1.0;
            return q;
        }

        private static void snapToIntValues(double[,] T)
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    if (T[i, j].sameCloseZero(1)) T[i, j] = 1;
                    else if (T[i, j].sameCloseZero()) T[i, j] = 0;
                    else if (T[i, j].sameCloseZero(-1)) T[i, j] = -1;
                }
        }

        private Boolean LnodesAreCollinear()
        {
            var n1X = L.nodes[0].X;
            var n1Y = L.nodes[0].Y;
            var tNodes = new List<node>(L.nodes);
            tNodes.RemoveAt(0);
            if (tNodes.Count > 3) tNodes.RemoveRange(3, tNodes.Count - 3);
            if (tNodes.TrueForAll(n => n.Y == n1Y)) return true;
            if (tNodes.TrueForAll(n => n.X == n1X)) return true;
            var m1 = (tNodes[0].Y - n1Y) / (tNodes[0].X - n1X);
            tNodes.RemoveAt(0);
            return tNodes.TrueForAll(n => (m1 == (n.Y - n1Y) / (n.X - n1X)));
        }

        private Boolean validTransform(double[,] T)
        {
            /* In this function the candidate transform, T, "runs the gauntlet. 
             * the long set of if statements each return false, and if T makes it all
             * the way through, we return true. */

            /* It's easy to check the translation and projection constraints first. Since there's
             * a one-to-one match with variables in the matrix and the flags. */
            /* if Tx is not zero... */
            if ((!T[0, 3].sameCloseZero())
                && ((Translate == transfromType.OnlyY) || (Translate == transfromType.OnlyZ) || (Translate == transfromType.Prohibited)))
                return false;
            if ((!T[1, 3].sameCloseZero())
                     && ((Translate == transfromType.OnlyX) || (Translate == transfromType.OnlyZ) || (Translate == transfromType.Prohibited)))
                return false;
            if ((!T[2, 3].sameCloseZero())
                     && ((Translate == transfromType.OnlyY) || (Translate == transfromType.OnlyZ) || (Translate == transfromType.Prohibited)))
                return false;
            if ((!T[0, 3].sameCloseZero(T[1, 3])) && (!T[1, 3].sameCloseZero(T[2, 3])) && (Translate == transfromType.XYZUniform))
                return false;

            /* now for projection. */
            if ((!T[3, 0].sameCloseZero())
                && ((Projection == transfromType.OnlyY) || (Projection == transfromType.OnlyZ) || (Projection == transfromType.Prohibited)))
                return false;
            if ((!T[3, 1].sameCloseZero())
                     && ((Projection == transfromType.OnlyX) || (Projection == transfromType.OnlyZ) || (Projection == transfromType.Prohibited)))
                return false;
            if ((!T[3, 2].sameCloseZero())
                     && ((Projection == transfromType.OnlyX) || (Projection == transfromType.OnlyY) || (Projection == transfromType.Prohibited)))
                return false;
            if ((!T[3, 0].sameCloseZero(T[3, 1])) && (!T[3, 1].sameCloseZero(T[3, 2])) && (Projection == transfromType.XYZUniform))
                return false;

            /* Now, it's a little more complicated since the rotation occupies the same cells
         * in T as skewX, skewY, scaleX, and scaleY. The approach taken here is to solve 
         * for theta (the amount of rotation) and then call/return what the overload produces
         * which requires theta and solves for skewX, skewY, scaleX, and scaleY. */
            if (Rotate == transfromType.Prohibited || Rotate == transfromType.OnlyX || Rotate == transfromType.OnlyY) return validTransform(T, 0.0);
            /* Skew restrictions are easier than Scale, because they default to (as in the 
         * identity matrix) 0 whereas Scale is 1. */
            if ((Skew == transfromType.Prohibited) || (Skew == transfromType.OnlyY))
                return validTransform(T, Math.Atan2(T[0, 1], T[1, 1]));
            if (Skew == transfromType.OnlyX)
                return validTransform(T, Math.Atan2(-T[1, 0], T[0, 0]));
            if (Skew == transfromType.XYZUniform)
                return validTransform(T, Math.Atan2((T[0, 1] - T[1, 0]), (T[0, 0] + T[1, 1])));

            /* Lastly, and most challenging, we look at Scale Restrictions. Flip is basically
         * the same and handled in the overload below. */
            if ((Scale == transfromType.Prohibited) || (Scale == transfromType.OnlyY))
            {
                var Too2PlusTio2 = T[0, 0] * T[0, 0] + T[1, 0] * T[1, 0];
                var sqrtt2pt2 = Math.Sqrt(Too2PlusTio2);
                var Ky = Math.Sqrt(Too2PlusTio2 - 1);
                return validTransform(T, Math.Acos(T[0, 0] / sqrtt2pt2) +
                                         Math.Atan2(Ky, 1));
            }
            if (Scale == transfromType.OnlyY)
            {
                var Toi2PlusTii2 = T[0, 1] * T[0, 1] + T[1, 1] * T[1, 1];
                var sqrtt2pt2 = Math.Sqrt(Toi2PlusTii2);
                var Kx = Math.Sqrt(Toi2PlusTii2 - 1);
                return validTransform(T, Math.Acos(T[0, 1] / sqrtt2pt2) +
                                         Math.Atan2(1, Kx));
            }
            if (_scale == transfromType.XYZUniform)
                return validTransform(T, Math.Atan2((T[0, 0] - T[1, 1]), (T[0, 1] + T[1, 0])));
            return true;
        }

        private Boolean validTransform(double[,] T, double theta)
        {
            /* now with theta known, we can find the values for Sx, Sy, Kx, and Ky. */
            var Kx = T[0, 1] * Math.Cos(theta) - T[1, 1] * Math.Sin(theta);
            var Ky = T[0, 0] * Math.Sin(theta) + T[1, 0] * Math.Cos(theta);
            var Sx = T[0, 0] * Math.Cos(theta) - T[1, 0] * Math.Sin(theta);
            var Sy = T[0, 1] * Math.Sin(theta) + T[1, 1] * Math.Cos(theta);

            /* now check the skew restrictions, once an error is found return false. */
            if ((!Kx.sameCloseZero()) &&
                ((Skew == transfromType.Prohibited) || (Skew == transfromType.OnlyY)))
                return false;
            if ((!Ky.sameCloseZero()) &&
                     ((Skew == transfromType.Prohibited) || (Skew == transfromType.OnlyY)))
                return false;
            if ((!Kx.sameCloseZero(Ky)) && (Skew == transfromType.XYZUniform))
                return false;

            /* now we check scaling restrictions. */
            if ((!Math.Abs(Sx).sameCloseZero(1)) &&
                     ((Scale == transfromType.Prohibited) || (Scale == transfromType.OnlyY)))
                return false;
            if ((!Math.Abs(Sy).sameCloseZero(1)) &&
                     ((Scale == transfromType.Prohibited) || (Scale == transfromType.OnlyX)))
                return false;
            if ((!Math.Abs(Sx).sameCloseZero(Math.Abs(Sy))) && (Scale == transfromType.XYZUniform))
                return false;

            /* finally, we check if the shape has to be flipped. */
            if ((Sx < 0) &&
                ((Flip == transfromType.Prohibited) || (Flip == transfromType.OnlyY)))
                return false;
            if ((Sy < 0) &&
                ((Flip == transfromType.Prohibited) || (Flip == transfromType.OnlyX)))
                return false;
            if ((Sx * Sy < 0) && (Flip == transfromType.XYZUniform))
                return false;
            return true;
        }

        private Boolean otherNodesComply(double[,] T, IList<node> locatedNodes)
        {
            if (locatedNodes.Count <= 2) return true;
            for (var i = 2; i != locatedNodes.Count; i++)
            {
                var vLVect = new[] { L.nodes[i].X, L.nodes[i].Y, L.nodes[i].Z, 1.0 };
                vLVect = T.multiply(vLVect, 4);
                vLVect[0] /= vLVect[3];
                vLVect[1] /= vLVect[3];
                vLVect[2] /= vLVect[3];
                var vHostVect = new[] { locatedNodes[i].X, locatedNodes[i].Y, locatedNodes[i].Z, 1.0 };
                if ((!vLVect[0].sameCloseZero(vHostVect[0]))
                    || (!vLVect[1].sameCloseZero(vHostVect[1]))
                    || (!vLVect[2].sameCloseZero(vHostVect[2])))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Reorders the nodes for best shape transform. This is to put all NOT-exist nodes
        /// at the end of the list and to avoid unlikely problems when first 3 or 4 nodes 
        /// are collinear or sitting on top of each other.
        /// </summary>
        public void ReorderNodes()
        {
            /* put not-exist nodes at the end of the list. */
            var notExistNodes = L.nodes.Where(n => ((ruleNode)n).NotExist).ToList();
            L.nodes.RemoveAll(notExistNodes.Contains);

            /* if all the nodes are collinear, there's nothing we can do. */
            if ((L.nodes.Count < 3) || (LnodesAreCollinear()))
            {
                L.nodes.AddRange(notExistNodes);
                return;
            }
            /*take off the node with the lowest x, call it nodeMinX */
            var minX = L.nodes.Min(n => n.X);
            var nodeMinX = L.nodes.First(n => n.X == minX);
            L.nodes.Remove(nodeMinX);
            /*take off the node with the largest x, call it nodeMaxX */
            var maxX = L.nodes.Max(n => n.X);
            var nodeMaxX = L.nodes.First(n => n.X == maxX);
            L.nodes.Remove(nodeMaxX);
            /*take off the node with the next lowest x, call it nodeMinXX */
            var minXX = L.nodes.Min(n => n.X);
            var nodeMinXX = L.nodes.First(n => n.X == minXX);
            L.nodes.Remove(nodeMinXX);
            if (L.nodes.Count > 0)
            {
                /* if you have four or more nodes, find a fourth point, 
                 * again at max X. */
                var maxXX = L.nodes.Max(n => n.X);
                var nodeMaxXX = L.nodes.First(n => n.X == maxXX);
                L.nodes.Remove(nodeMaxXX);
                L.nodes.Insert(0, nodeMaxXX);
            }
            L.nodes.Insert(0, nodeMinXX);
            L.nodes.Insert(0, nodeMaxX);
            L.nodes.Insert(0, nodeMinX);
            L.nodes.AddRange(notExistNodes);
        }
        #endregion

        #region Apply Method

        /// <summary>
        ///   Updates the position of a node.
        /// </summary>
        /// <param name = "update">The node to update.</param>
        /// <param name = "T">The Transformation matrix, T.</param>
        /// <param name = "given">The given rule node.</param>
        private static void TransformPositionOfNode(node update, double[,] T, node given)
        {
            var pt = new[] { given.X, given.Y, given.Z, 1 };
            pt = T.multiply(pt, 4);
            var newT = MatrixMath.Identity(4);
            newT[0, 3] = update.X = pt[0] / pt[3];
            newT[1, 3] = update.Y = pt[1] / pt[3];
            newT[2, 3] = update.Z = pt[2] / pt[3];

            if (update.DisplayShape != null)
                update.DisplayShape.TransformMatrix = newT;
        }


        /// <summary>
        /// Transfroms the shape of node.
        /// </summary>
        /// <param name="update">The update.</param>
        /// <param name="T">The T.</param>
        private static void TransfromShapeOfNode(node update, double[,] T)
        {
            var newT = (double[,])T.Clone();
            newT[0, 3] = update.X;
            newT[1, 3] = update.Y;
            newT[2, 3] = update.Y;
            if (update.DisplayShape != null)
                update.DisplayShape.TransformMatrix = newT;
        }

        #endregion
    }
}