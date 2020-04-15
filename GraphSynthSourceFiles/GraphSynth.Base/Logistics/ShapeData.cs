/*************************************************************************
 *     This ShapeData file & class is part of the GraphSynth.BaseClasses 
 *     Project which is the foundation of the GraphSynth Application.
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
using System.Collections.Generic;
using System.Globalization;
using GraphSynth.Representation;

namespace GraphSynth
{

    /// <summary>
    /// The ShapeData class stores the data for how to display a particular graph element:
    /// node, arc, or hyperarc. At this level, it is essentially an XML string of data. It is
    /// similar to the GraphSynth.CustomControls.DisplayShape, which inherits from this class.
    /// That class is specific to the WPF viewer programs.
    /// This is a new class as of October 2011. It combines three previous classes (IDisplayShape,
    /// ShapeKey, and DisplayStringShape [from GraphSynthConsole]) into a single class.
    /// </summary>
    public class ShapeData
    {
        #region Fields and Properties
        /// <summary>
        /// The graph element that this data is representing
        /// </summary>
        public readonly graphElement Element;
        /// <summary>
        /// This protected field describes the shape.
        /// </summary>
        protected string _stringShape;
        #endregion
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeData"/> class.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="element">The element.</param>
        public ShapeData(string s, graphElement element)
        {
            _stringShape = s;
            Element = element;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeData"/> class.
        /// </summary>
        /// <param name="element">The element.</param>
        public ShapeData(graphElement element)
        {
            Element = element;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="GraphSynth.ShapeData"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="x">The ShapeData, x.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator string(ShapeData x)
        {
            return x._stringShape;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Copies the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public virtual ShapeData Copy(graphElement element)
        {
            return new ShapeData(this, element);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return _stringShape;
        }

        static string GetValue(string xamlString, string property)
        {
            if (!property.EndsWith("=\"")) property += "=\"";
            var start = xamlString.IndexOf(property, System.StringComparison.Ordinal);
            if (start == -1) return null;
            start += property.Length;
            var end = xamlString.IndexOf("\"", start, System.StringComparison.Ordinal);
            return xamlString.Substring(start, (end - start));
        }

        static void SetValue(ref string xamlString, string property, object newValue)
        {
            if (!property.EndsWith("=\"")) property += "=\"";
            var oldStrValue = GetValue(xamlString, property);
            if (oldStrValue != null) oldStrValue = property + oldStrValue + "\"";

            var newStrValue = newValue.ToString().Trim(new[] { ' ', '\"' });
            if (newValue.ToString().Length > 0)
                newStrValue = property + newStrValue + "\"";
            if (oldStrValue != null)
                xamlString = xamlString.Replace(oldStrValue, newStrValue);
            else
            {
                var i = xamlString.IndexOf(' ');
                if (i == -1) i = xamlString.Length;
                xamlString = xamlString.Insert(i, " " + newStrValue);
            }
        }
        #endregion
        #region Properties of Shape
        /// <summary>
        /// Gets or sets the transform matrix.
        /// </summary>
        /// <value>
        /// The transform matrix.
        /// </value>
        public virtual double[,] TransformMatrix
        {
            get
            {
                var terms = DoubleCollectionConverter.Convert(GetValue(_stringShape, "RenderTransform"));

                return new[,]
                           {
                               {terms[0], terms[1], terms[4]},
                               {terms[2], terms[3], terms[5]},
                               {0, 0, 1}
                           };
            }
            set
            {
                if (value == null) value = new double[,] { { 1, 1, 0 }, { 1, 1, 0 }, { 0, 0, 1 } };

                var replace = DoubleCollectionConverter.Convert(
                    new[]
                            {
                                value[0, 0], value[0, 1], value[1, 0], value[1, 1], value[0, 2], value[1, 2]
                            });
                SetValue(ref _stringShape, "RenderTransform", replace);
            }

        }


        /// <summary>
        /// Gets or sets the fill.
        /// </summary>
        /// <value>
        /// The fill.
        /// </value>
        public virtual object Fill
        {
            get
            {
                return GetValue(_stringShape, "Fill");
            }
            set
            {
                SetValue(ref _stringShape, "Fill", value ?? "#00000000");
            }
        }

        /// <summary>
        /// Gets or sets the stroke.
        /// </summary>
        /// <value>
        /// The stroke.
        /// </value>
        public virtual object Stroke
        {
            get
            {
                return GetValue(_stringShape, "Stroke");

            }
            set
            {
                SetValue(ref _stringShape, "Stroke", value ?? "#00000000");
            }
        }

        /// <summary>
        /// Gets or sets the stroke thickness.
        /// </summary>
        /// <value>
        /// The stroke thickness.
        /// </value>
        public virtual double StrokeThickness
        {
            get
            {
                return double.Parse(GetValue(_stringShape, "StrokeThickness"));
            }
            set
            {
                if (double.IsNaN(value)) value = 0.0;
                SetValue(ref _stringShape, "StrokeThickness",
                                                value.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public virtual double Height
        {
            get
            {
                return double.Parse(GetValue(_stringShape, "Height"));
            }
            set
            {
                if (double.IsNaN(value)) value = 0.0;
                SetValue(ref _stringShape, "Height", value.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public virtual double Width
        {
            get
            {
                return double.Parse(GetValue(_stringShape, "Width"));
            }
            set
            {
                if (double.IsNaN(value)) value = 0.0;
                SetValue(ref _stringShape, "Width", value.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Gets or sets the screen X.
        /// </summary>
        /// <value>
        /// The screen X.
        /// </value>
        public virtual double ScreenX
        {
            get
            {
                try
                {
                    var transform = DoubleCollectionConverter.Convert(
                        GetValue(_stringShape, "RenderTransform"));
                    if (transform.Count > 5)
                        return transform[4] + Width / 2;
                    return double.NaN;
                }
                catch
                {
                    return double.NaN;
                }
            }
            set
            {
                if (double.IsNaN(value)) value = 0.0;
                var renderTStr = GetValue(_stringShape, "RenderTransform");
                var transform = new List<double>(new double[] { 1, 0, 0, -1, 0, 0 });
                if (renderTStr != null)
                    transform = DoubleCollectionConverter.Convert(renderTStr);
                transform[4] = value - Width / 2;
                SetValue(ref _stringShape, "RenderTransform",
                                            DoubleCollectionConverter.Convert(transform));
            }
        }

        /// <summary>
        /// Gets or sets the screen Y.
        /// </summary>
        /// <value>
        /// The screen Y.
        /// </value>
        public virtual double ScreenY
        {
            get
            {
                try
                {
                    var transform = DoubleCollectionConverter.Convert(
                            GetValue(_stringShape, "RenderTransform"));
                    if (transform.Count > 6)
                        return transform[5] + Height / 2;
                    return double.NaN;
                }
                catch
                {
                    return double.NaN;
                }
            }
            set
            {
                if (double.IsNaN(value)) value = 0.0;
                var renderTStr = GetValue(_stringShape, "RenderTransform");
                var transform = new List<double>(new double[] { 1, 0, 0, -1, 0, 0 });
                if (renderTStr != null)
                    transform = DoubleCollectionConverter.Convert(renderTStr);
                transform[5] = value - Height / 2;
                SetValue(ref _stringShape, "RenderTransform",
                                            DoubleCollectionConverter.Convert(transform));
            }
        }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public virtual object Tag
        {
            get
            {
                return GetValue(_stringShape, "Tag");
            }
            set
            {
                if (value == null) value = "";
                SetValue(ref _stringShape, "Tag", value);
            }
        }

        /// <summary>
        /// Gets the shape.
        /// </summary>
        public virtual object Shape { get { return _stringShape; } }
        #endregion

    }
}