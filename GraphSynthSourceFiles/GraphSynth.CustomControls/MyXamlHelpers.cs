
using System;
using System.Text;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Markup;
namespace GraphSynth
{
    public static class MyXamlHelpers
    {
        public static string GetValue(string xamlString, string property)
        {
            if (!property.EndsWith("=\"")) property += "=\"";
            var start = xamlString.IndexOf(property);
            if (start == -1) return null;
            start += property.Length;
            var end = xamlString.IndexOf("\"", start);
            return xamlString.Substring(start, (end - start));
        }

        public static void SetValue(ref string xamlString, string property, object newValue)
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
                xamlString = xamlString.Insert(i, " " + newStrValue);
            }
        }

        public static string CleanOutxNulls(string xamlString)
        {
            /* this is used for the XAML Canvases prior to saving. It may be more correct
             * to leave these for the opening of files and fix by simply re-adding the x:
             * namespace or more intelligently reading in the shapes, but the problem is
             * the function, RestoreDisplayShapes is also tied to Cut-n-Paste, and removing
             * these null bindings makes the files smaller and easier to read. */
            var elements = xamlString.Split(new[] { ' ' });
            var returnStr = "";
            for (var i = 0; i < elements.GetLength(0); i++)
                if (!elements[i].Contains("{x:Null}"))
                    returnStr += elements[i] + " ";
            return returnStr;
        }

        public static object Parse(string p, ParserContext context = null)
        {
            try
            {
                if (context == null) context = new ParserContext();
                context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                return XamlReader.Parse(p, context);
                /***** Notice!: If you have crashed Graphsynth here, then
                 * the try-catch failed. This happens due to a setting
                 * in your Visual Studio environment. To fix this:
                 * 1) Go to Debug->Exceptions.
                 * 2) expand Common Language Runtime Exceptions
                 * 3) Scroll Down to System.Windows.Markup.XamlParseException
                 * 4) uncheck the box in the "Thrown" Column. */
            }
            catch (Exception)
            {
                try
                {
                    return XamlReader.Parse(p);
                }
                catch
                {
                    SearchIO.output("XamlReader.Parse failed to translate the string to an object.");
                    return null;
                }
            }
        }


        internal static string XamlOfShape(object Shape, string newTag = "")
        {
            if (typeof(Path).IsInstanceOfType(Shape))
                return XamlOfPath((Path)Shape, newTag);
            if (typeof(Ellipse).IsInstanceOfType(Shape))
                return XamlOfEllipse((Ellipse)Shape, newTag);
            if (typeof(Rectangle).IsInstanceOfType(Shape))
                return XamlOfRectangle((Rectangle)Shape, newTag);
            if (typeof(Polygon).IsInstanceOfType(Shape))
                return XamlOfPolygon((Polygon)Shape, newTag);
            if (typeof(Polyline).IsInstanceOfType(Shape))
                return XamlOfPolyline((Polyline)Shape, newTag);
            if (typeof(Line).IsInstanceOfType(Shape))
                return XamlOfLine((Line)Shape, newTag);
            throw new Exception("Cannot make Xaml String of non-shape (StringOfXamlHelper.XamlOfShape");
        }

        private static string XamlOfPath(Path p, string newTag)
        {
            var sb = new StringBuilder("<Path", 600);
            sb.Append(ShapeDetails(p, newTag));
            sb.AppendLine(" >");
            sb.AppendLine("<Path.Data>");
            if (typeof(PathGeometry).IsInstanceOfType(p.Data)) XamlofPathGeometry(sb, (PathGeometry)p.Data);
            else if (typeof(EllipseGeometry).IsInstanceOfType(p.Data)) XamlofEllipseGeometry(sb, (EllipseGeometry)p.Data);
            else if (typeof(RectangleGeometry).IsInstanceOfType(p.Data)) XamlofRectangleGeometry(sb, (RectangleGeometry)p.Data);
            sb.AppendLine("</Path.Data>");
            sb.AppendLine("</Path>");

            return sb.ToString();
        }

        private static void XamlofRectangleGeometry(StringBuilder sb, RectangleGeometry rectGeom)
        {
            sb.AppendFormat("<RectangleGeometry Rect=\"{0},{1},{2},{3}\" RadiusX=\"{4}\" RadiusY=\"{5}\" Transform=\"{6},{7},{8},{9},{10},{11}\" />",
                rectGeom.Rect.X, rectGeom.Rect.Y, rectGeom.Rect.Width, rectGeom.Rect.Height,
                rectGeom.RadiusX, rectGeom.RadiusY,
                rectGeom.Transform.Value.M11, rectGeom.Transform.Value.M12,
                rectGeom.Transform.Value.M21, rectGeom.Transform.Value.M22,
                rectGeom.Transform.Value.OffsetX, rectGeom.Transform.Value.OffsetY);
        }

        private static void XamlofEllipseGeometry(StringBuilder sb, EllipseGeometry ellipseGeom)
        {
            sb.AppendFormat("<EllipseGeometry Center=\"{0},{1}\" RadiusX=\"{2}\" RadiusY=\"{3}\" Transform=\"{4},{5},{6},{7},{8},{9}\" />",
                ellipseGeom.Center.X, ellipseGeom.Center.Y, ellipseGeom.RadiusX, ellipseGeom.RadiusY,
                ellipseGeom.Transform.Value.M11, ellipseGeom.Transform.Value.M12,
                ellipseGeom.Transform.Value.M21, ellipseGeom.Transform.Value.M22,
                ellipseGeom.Transform.Value.OffsetX, ellipseGeom.Transform.Value.OffsetY);
        }

        private static void XamlofPathGeometry(StringBuilder sb, PathGeometry pathGeom)
        {
            sb.AppendLine("<PathGeometry>");
            sb.AppendLine("<PathGeometry.Figures>");
            foreach (var pf in pathGeom.Figures)
            {
                sb.AppendFormat("<PathFigure StartPoint=\"{0},{1}\" IsFilled=\"{2}\" IsClosed=\"{3}\" >",
                                pf.StartPoint.X, pf.StartPoint.Y, pf.IsFilled, pf.IsClosed);
                sb.AppendLine("<PathFigure.Segments>");
                foreach (var seg in pf.Segments)
                {
                    if (typeof(ArcSegment).IsInstanceOfType(seg))
                        sb.AppendFormat(
                            "<ArcSegment Point=\"{0},{1}\" Size=\"{2},{3}\" IsLargeArc=\"{4}\" SweepDirection=\"{5}\" />",
                            ((ArcSegment)seg).Point.X, ((ArcSegment)seg).Point.Y, ((ArcSegment)seg).Size.Width,
                            ((ArcSegment)seg).Size.Height, ((ArcSegment)seg).IsLargeArc,
                            ((ArcSegment)seg).SweepDirection);
                    else if (typeof(BezierSegment).IsInstanceOfType(seg))
                        sb.AppendFormat(
                            "<BezierSegment Point1=\"{0},{1}\" Point2=\"{2},{3}\" Point3=\"{4},{5}\" />",
                            ((BezierSegment)seg).Point1.X, ((BezierSegment)seg).Point1.Y,
                            ((BezierSegment)seg).Point2.X, ((BezierSegment)seg).Point2.Y,
                            ((BezierSegment)seg).Point3.X, ((BezierSegment)seg).Point3.Y);
                    else if (typeof(LineSegment).IsInstanceOfType(seg))
                        sb.AppendFormat(
                            "<LineSegment Point=\"{0},{1}\" />", ((LineSegment)seg).Point.X, ((LineSegment)seg).Point.Y);
                    else if (typeof(PolyLineSegment).IsInstanceOfType(seg))
                        sb.AppendFormat("<PolyLineSegment Points=\"{0}\" />", ((PolyLineSegment)seg).Points);
                }
                sb.AppendLine("</PathFigure.Segments>");
                sb.AppendLine("</PathFigure>");
            }
            sb.AppendLine("</PathGeometry.Figures>");
            sb.AppendLine("</PathGeometry>");
        }

        private static string XamlOfEllipse(Ellipse p, string newTag)
        {
            var sb = new StringBuilder("<Ellipse", 200);
            sb.Append(ShapeDetails(p, newTag));
            sb.AppendLine(" />");
            return sb.ToString();
        }


        private static string XamlOfRectangle(Rectangle p, string newTag)
        {
            var sb = new StringBuilder("<Rectangle", 200);
            sb.Append(ShapeDetails(p, newTag));
            AddElement(sb, "RadiusX", p.RadiusX);
            AddElement(sb, "RadiusY", p.RadiusY);
            sb.AppendLine(" />");
            return sb.ToString();
        }

        private static string XamlOfPolygon(Polygon p, string newTag)
        {
            var sb = new StringBuilder("<Polygon", 200);
            sb.Append(ShapeDetails(p, newTag));
            AddElement(sb, "FillRule", p.FillRule);
            AddElement(sb, "Points", p.Points);
            sb.AppendLine(" />");
            return sb.ToString();
        }

        private static string XamlOfPolyline(Polyline p, string newTag)
        {
            var sb = new StringBuilder("<Polyline", 200);
            sb.Append(ShapeDetails(p, newTag));
            AddElement(sb, "FillRule", p.FillRule);
            AddElement(sb, "Points", p.Points);
            sb.AppendLine(" />");
            return sb.ToString();
        }

        private static string XamlOfLine(Line p, string newTag)
        {
            var sb = new StringBuilder("<Line", 200);
            sb.Append(ShapeDetails(p, newTag));
            AddElement(sb, "X1", p.X1);
            AddElement(sb, "X2", p.X2);
            AddElement(sb, "Y1", p.Y1);
            AddElement(sb, "Y2", p.Y2);
            sb.AppendLine(" />");
            return sb.ToString();
        }

        private static string ShapeDetails(Shape p, string newTag)
        {
            var sb = new StringBuilder();
            AddElement(sb, "Stretch", p.Stretch);
            AddElement(sb, "Fill", p.Fill);
            AddElement(sb, "Stroke", p.Stroke);

            AddElement(sb, "StrokeThickness", p.StrokeThickness);
            AddElement(sb, "StrokeStartLineCap", p.StrokeStartLineCap);
            AddElement(sb, "StrokeEndLineCap", p.StrokeEndLineCap);
            AddElement(sb, "StrokeDashCap", p.StrokeDashCap);
            AddElement(sb, "StrokeLineJoin", p.StrokeLineJoin);
            AddElement(sb, "StrokeMiterLimit", p.StrokeMiterLimit);
            AddElement(sb, "StrokeDashOffset", p.StrokeDashOffset);
            AddElement(sb, "StrokeDashArray", p.StrokeDashArray);
            AddElement(sb, "Tag", !string.IsNullOrWhiteSpace(newTag) ? newTag : p.Tag);
            AddElement(sb, "LayoutTransform", p.LayoutTransform);
            AddElement(sb, "Width", p.Width);
            AddElement(sb, "Height", p.Height);
            AddElement(sb, "Margin", p.Margin);
            AddElement(sb, "HorizontalAlignment", p.HorizontalAlignment);
            AddElement(sb, "VerticalAlignment", p.VerticalAlignment);
            AddElement(sb, "RenderTransform", p.RenderTransform);
            AddElement(sb, "RenderTransformOrigin", p.RenderTransformOrigin);
            AddElement(sb, "Opacity", p.Opacity);
            AddElement(sb, "Visibility", p.Visibility);
            AddElement(sb, "SnapsToDevicePixels", p.SnapsToDevicePixels);
            return sb.ToString();
        }

        private static void AddElement(StringBuilder sb, string name, object value)
        {
            if (value == null || (typeof(double).IsInstanceOfType(value) && double.IsNaN((double)value))) return;
            var valString = value.ToString();
            if (string.IsNullOrWhiteSpace(valString)) return;
            sb.AppendFormat(" " + name + "=\"{0}\"", valString);
        }



    }
}