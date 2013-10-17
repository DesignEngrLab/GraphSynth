using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphSynth.Representation;
using GraphSynth.UI;

namespace GraphSynth.GraphDisplay
{
    public enum ShapeRepresents
    { Node, Arc, HyperArc }

    /// <summary>
    /// </summary>
    public sealed class DisplayShape : ShapeData
    {
        #region Fields and Properties
        public IconShape icon { get; set; }
        public Boolean StringNeedsUpdating;
        private Shape _shape;
        private readonly ShapeRepresents shapeRepresents;
        #endregion

        #region Constructor

        public DisplayShape(string s, ShapeRepresents shapeRepresents, graphElement element)
            :base(s,element)
        {
            this.shapeRepresents = shapeRepresents;
        }

        public DisplayShape(Shape s, ShapeRepresents shapeRepresents, graphElement element)
            :base(element)
        {
            _shape = s;
            StringNeedsUpdating = true;
            this.shapeRepresents = shapeRepresents;
        }

        #endregion

        #region Moving between Shape and string Functions

        private Boolean StringIsUpToDateAndShapeIsNotAccessible
        {
            get
            {
                return (!StringNeedsUpdating
                    && ((_shape == null)
                    || (!_shape.Dispatcher.CheckAccess())));
            }
        }

        /// <summary>
        /// Gets the shape.
        /// </summary>
        public override object Shape
        {
            get
            {
                if (_shape != null) return _shape;
                switch (shapeRepresents)
                {
                    case ShapeRepresents.Arc:
                        _shape = new ArcShape((Path)MyXamlHelpers.Parse(_stringShape));
                        break;
                    case ShapeRepresents.HyperArc:
                        _shape = new HyperArcShape((Shape)MyXamlHelpers.Parse(_stringShape));
                        break;
                    default:
                        _shape = (Shape)MyXamlHelpers.Parse(_stringShape);
                        break;
                }
                return _shape;
            }
        }

        public string String
        {
            get
            {
                if (!StringNeedsUpdating) return _stringShape;
                if (_shape.Dispatcher.CheckAccess())
                {
                    switch (shapeRepresents)
                    {
                        case ShapeRepresents.Arc:
                            _stringShape = ((ArcShape)_shape).XamlWrite();
                            break;
                        case ShapeRepresents.HyperArc:
                            _stringShape = ((HyperArcShape)_shape).XamlWrite();
                            break;
                        default:
                            if (icon!=null)
                                _stringShape = MyXamlHelpers.XamlOfShape(_shape, icon.UpdateTag());
                            else _stringShape = MyXamlHelpers.XamlOfShape(_shape, (string)this.Tag);
                            break;
                    }
                }
                else
                    _shape.Dispatcher.Invoke(
                         (ThreadStart)delegate
                         {
                             switch (shapeRepresents)
                             {
                                 case ShapeRepresents.Arc:
                                     _stringShape = ((ArcShape)_shape).XamlWrite();
                                     break;
                                 case ShapeRepresents.HyperArc:
                                     _stringShape = ((HyperArcShape)_shape).XamlWrite();
                                     break;
                                 default:
                                     _stringShape = MyXamlHelpers.XamlOfShape(_shape, icon.UpdateTag());
                                     break;
                             }
                         });
                StringNeedsUpdating = false;
                return _stringShape;
            }
        }


        #endregion

        /// <summary>
        /// Copies the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public override ShapeData Copy(graphElement element)
        {
            return new DisplayShape(String, shapeRepresents, element);
        }

        #region Properties of Shape and shape String
        /// <summary>
        /// Gets or sets the transform matrix.
        /// </summary>
        /// <value>
        /// The transform matrix.
        /// </value>
        public override double[,] TransformMatrix
        {
            get
            {
                List<double> terms;
                if (StringIsUpToDateAndShapeIsNotAccessible)
                    terms = DoubleCollectionConverter.convert(
                        MyXamlHelpers.GetValue(_stringShape, "RenderTransform"));
                else terms = new List<double>
                                {
                                    ((Shape) _shape).RenderTransform.Value.M11,
                                    ((Shape) _shape).RenderTransform.Value.M12,
                                    ((Shape) _shape).RenderTransform.Value.M21,
                                    ((Shape) _shape).RenderTransform.Value.M22,
                                    ((Shape) _shape).RenderTransform.Value.OffsetX,
                                    ((Shape) _shape).RenderTransform.Value.OffsetY
                                };
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
                if (StringIsUpToDateAndShapeIsNotAccessible)
                {
                    var replace = DoubleCollectionConverter.convert(
                        new[]
                            {
                                value[0, 0], value[0, 1], value[1, 0], value[1, 1], value[0, 2], value[1, 2]
                            });
                    MyXamlHelpers.SetValue(ref _stringShape, "RenderTransform", replace);
                }
                else
                {
                    ((Shape)_shape).RenderTransform
                        = new MatrixTransform(value[0, 0], value[0, 1], value[1, 0], value[1, 1],
                                             value[0, 2], value[1, 2]);
                    StringNeedsUpdating = true;
                }

            }
        }

        /// <summary>
        /// Gets or sets the fill.
        /// </summary>
        /// <value>
        /// The fill.
        /// </value>
        public override object Fill
        {
            get
            {
                if (StringIsUpToDateAndShapeIsNotAccessible)
                    return MyXamlHelpers.GetValue(_stringShape, "Fill");
                return ((Shape)_shape).Fill;
            }
            set
            {
                if (value == null) value = Brushes.Transparent;
                if (StringIsUpToDateAndShapeIsNotAccessible)
                    MyXamlHelpers.SetValue(ref _stringShape, "Fill", value);
                else
                {
                    if ((value is string))
                        ((Shape)_shape).Fill = BrushSelector.GetBrushFromString((string)value);
                    else if ((value is Brush)) ((Shape)_shape).Fill = (Brush)value;
                    else throw new Exception("Fill cannot be set to type:" + value.GetType());
                    StringNeedsUpdating = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the stroke.
        /// </summary>
        /// <value>
        /// The stroke.
        /// </value>
        public override object Stroke
        {
            get
            {
                if (StringIsUpToDateAndShapeIsNotAccessible)
                    return MyXamlHelpers.GetValue(_stringShape, "Stroke");
                return ((Shape)_shape).Stroke;
            }
            set
            {
                if (value == null) value = Brushes.Transparent;
                if (StringIsUpToDateAndShapeIsNotAccessible)
                    MyXamlHelpers.SetValue(ref _stringShape, "Stroke", value);
                else
                {
                    if ((value is string))
                        ((Shape)_shape).Stroke = BrushSelector.GetBrushFromString((string)value);
                    else if ((value is Brush)) ((Shape)_shape).Stroke = (Brush)value;
                    else throw new Exception("Stroke cannot be set to type:" + value.GetType());
                    StringNeedsUpdating = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the stroke thickness.
        /// </summary>
        /// <value>
        /// The stroke thickness.
        /// </value>
        public override double StrokeThickness
        {
            get
            {
                if (StringIsUpToDateAndShapeIsNotAccessible)
                    return double.Parse(MyXamlHelpers.GetValue(_stringShape, "StrokeThickness"));
                return ((Shape)_shape).StrokeThickness;
            }
            set
            {
                if (double.IsNaN(value)) value = 0.0;
                if (StringIsUpToDateAndShapeIsNotAccessible)
                    MyXamlHelpers.SetValue(ref _stringShape, "StrokeThickness",
                                                value.ToString(CultureInfo.InvariantCulture));
                else
                {
                    ((Shape)_shape).StrokeThickness = value;
                    StringNeedsUpdating = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public override double Height
        {
            get
            {
                if (StringIsUpToDateAndShapeIsNotAccessible)
                    return double.Parse(MyXamlHelpers.GetValue(_stringShape, "Height"));
                return ((Shape)_shape).Height;
            }
            set
            {
                if (double.IsNaN(value)) value = 0.0;
                if (StringIsUpToDateAndShapeIsNotAccessible)
                    MyXamlHelpers.SetValue(ref _stringShape, "Height", value.ToString(CultureInfo.InvariantCulture));
                else
                {
                    ((Shape)_shape).Height = value;
                    StringNeedsUpdating = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public override double Width
        {
            get
            {
                if (StringIsUpToDateAndShapeIsNotAccessible)
                    return double.Parse(MyXamlHelpers.GetValue(_stringShape, "Width"));
                return ((Shape)_shape).Width;
            }
            set
            {
                if (double.IsNaN(value)) value = 0.0;
                if (StringIsUpToDateAndShapeIsNotAccessible)
                    MyXamlHelpers.SetValue(ref _stringShape, "Width", value.ToString(CultureInfo.InvariantCulture));
                else
                {
                    ((Shape)_shape).Width = value;
                    StringNeedsUpdating = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the screen X.
        /// </summary>
        /// <value>
        /// The screen X.
        /// </value>
        public override double ScreenX
        {
            get
            {
                try
                {
                    if (StringIsUpToDateAndShapeIsNotAccessible)
                    {
                        var transform = DoubleCollectionConverter.convert(
                            MyXamlHelpers.GetValue(_stringShape, "RenderTransform"));
                        if (transform.Count > 5)
                            return transform[4] + Width / 2;
                        return double.NaN;
                    }
                    return ((Shape)_shape).RenderTransform.Value.OffsetX + Width / 2;
                }
                catch
                {
                    return double.NaN;
                }
            }
            set
            {
                if (double.IsNaN(value)) value = 0.0;
                if (StringIsUpToDateAndShapeIsNotAccessible)
                {
                    var renderTStr = MyXamlHelpers.GetValue(_stringShape, "RenderTransform");
                    var transform = new List<double>(new double[] { 1, 0, 0, -1, 0, 0 });
                    if (renderTStr != null)
                        transform = DoubleCollectionConverter.convert(renderTStr);
                    transform[4] = value - Width / 2;
                    MyXamlHelpers.SetValue(ref _stringShape, "RenderTransform",
                                                DoubleCollectionConverter.convert(transform));
                }
                else
                {
                    ((Shape)_shape).RenderTransform = new MatrixTransform(
                        ((Shape)_shape).RenderTransform.Value.M11,
                        ((Shape)_shape).RenderTransform.Value.M12,
                        ((Shape)_shape).RenderTransform.Value.M21,
                        ((Shape)_shape).RenderTransform.Value.M22,
                        value - Width / 2,
                        ((Shape)_shape).RenderTransform.Value.OffsetY);
                    StringNeedsUpdating = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the screen Y.
        /// </summary>
        /// <value>
        /// The screen Y.
        /// </value>
        public override double ScreenY
        {
            get
            {
                try
                {
                    if (StringIsUpToDateAndShapeIsNotAccessible)
                    {
                        var transform = DoubleCollectionConverter.convert(
                            MyXamlHelpers.GetValue(_stringShape, "RenderTransform"));
                        if (transform.Count > 6)
                            return transform[5] + Height / 2;
                        return double.NaN;
                    }
                    return ((Shape)_shape).RenderTransform.Value.OffsetY + Height / 2;
                }
                catch
                {
                    return double.NaN;
                }
            }
            set
            {
                if (double.IsNaN(value)) value = 0.0;
                if (StringIsUpToDateAndShapeIsNotAccessible)
                {
                    var renderTStr = MyXamlHelpers.GetValue(_stringShape, "RenderTransform");
                    var transform = new List<double>(new double[] { 1, 0, 0, -1, 0, 0 });
                    if (renderTStr != null)
                        transform = DoubleCollectionConverter.convert(renderTStr);
                    transform[5] = value - Height / 2;
                    MyXamlHelpers.SetValue(ref _stringShape, "RenderTransform",
                                                DoubleCollectionConverter.convert(transform));
                }
                else
                {
                    ((Shape)_shape).RenderTransform = new MatrixTransform(
                        ((Shape)_shape).RenderTransform.Value.M11,
                        ((Shape)_shape).RenderTransform.Value.M12,
                        ((Shape)_shape).RenderTransform.Value.M21,
                        ((Shape)_shape).RenderTransform.Value.M22,
                        ((Shape)_shape).RenderTransform.Value.OffsetX,
                        value - Height / 2);
                    StringNeedsUpdating = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public override object Tag
        {
            get
            {
                if (StringIsUpToDateAndShapeIsNotAccessible)
                    return MyXamlHelpers.GetValue(_stringShape, "Tag");
                return ((Shape)_shape).Tag;
            }
            set
            {
                if (value == null) value = "";
                if (StringIsUpToDateAndShapeIsNotAccessible)
                    MyXamlHelpers.SetValue(ref _stringShape, "Tag", value);
                else
                {
                    ((Shape)_shape).Tag = value;
                    StringNeedsUpdating = true;
                }
            }
        }
        #endregion

    }
}