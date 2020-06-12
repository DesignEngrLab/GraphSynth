using System;
using System.Linq;
using System.Threading;
using System.Windows;

namespace GraphSynth.GraphLayout
{
    public class IsometricScaling : GraphLayoutBaseClass
    {
        #region Layout Declaration, Sliders
        public IsometricScaling()
        {
            MakeSlider(YRotationProperty, "Y Rotation", "Rotation about the vertical axis", -5, 5, 0.1, 0, false, 0);
            MakeSlider(XRotationProperty, "X Rotation", "Rotation about the horizontal axis", -5, 5, 0.1, 0, false, 0);
        }

        public override string text
        {
            get { return "Isometric Scaling"; }
        }
        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty YRotationProperty
            = DependencyProperty.Register("Y Rotation",
                                          typeof(double), typeof(IsometricScaling),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty XRotationProperty
            = DependencyProperty.Register("X Rotation",
                                          typeof(double), typeof(IsometricScaling),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public double YRotation
        {
            get
            {
                var val = 0.0;
                Dispatcher.Invoke((ThreadStart)delegate { val = (double)GetValue(YRotationProperty); });
                return val;
            }
            set
            {
                SetValue(YRotationProperty, value);
            }
        }
        public double XRotation
        {
            get
            {
                var val = 0.0;
                Dispatcher.Invoke((ThreadStart)delegate { val = (double)GetValue(XRotationProperty); });
                return val;
            }
            set
            {
                SetValue(XRotationProperty, value);
            }
        }

        private double prevXRotation;
        private double prevYRotation;

        #endregion

        #region Layout Methods / Algorithm
        protected override bool RunLayout()
        {
            try
            {
                foreach (var n in graph.nodes)
                {
                    n.X += (YRotation - prevYRotation) * n.Z;
                    n.Y += (XRotation - prevXRotation) * n.Z;
                }

                Dispatcher.Invoke((ThreadStart)delegate
                {
                    prevYRotation = YRotation;
                    prevXRotation = XRotation;
                });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
    }
}
