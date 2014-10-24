
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

        private static string AdditionalShapeDetails;
        internal static string XamlOfShape(object Shape, string newTag = "")
        {
            AdditionalShapeDetails = "";
            if (Shape is Path)
                return XamlOfPath((Path)Shape, newTag);
            if (Shape is Ellipse)
                return XamlOfEllipse((Ellipse)Shape, newTag);
            if (Shape is Rectangle)
                return XamlOfRectangle((Rectangle)Shape, newTag);
            if (Shape is Polygon)
                return XamlOfPolygon((Polygon)Shape, newTag);
            if (Shape is Polyline)
                return XamlOfPolyline((Polyline)Shape, newTag);
            if (Shape is Line)
                return XamlOfLine((Line)Shape, newTag);
            throw new Exception("Cannot make Xaml String of non-shape (StringOfXamlHelper.XamlOfShape");
        }

        private static string XamlOfPath(Path p, string newTag)
        {
            var sb = new StringBuilder("<Path", 600);
            sb.Append(ShapeDetails(p, newTag));
            sb.AppendLine(" >");
            sb.AppendLine("<Path.Data>");
            if (p.Data is PathGeometry) XamlofPathGeometry(sb, (PathGeometry)p.Data);
            else if (p.Data is EllipseGeometry) XamlofEllipseGeometry(sb, (EllipseGeometry)p.Data);
            else if (p.Data is RectangleGeometry) XamlofRectangleGeometry(sb, (RectangleGeometry)p.Data);
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
                    if (seg is ArcSegment)
                        sb.AppendFormat(
                            "<ArcSegment Point=\"{0},{1}\" Size=\"{2},{3}\" IsLargeArc=\"{4}\" SweepDirection=\"{5}\" />",
                            ((ArcSegment)seg).Point.X, ((ArcSegment)seg).Point.Y, ((ArcSegment)seg).Size.Width,
                            ((ArcSegment)seg).Size.Height, ((ArcSegment)seg).IsLargeArc,
                            ((ArcSegment)seg).SweepDirection);
                    else if (seg is BezierSegment)
                        sb.AppendFormat(
                            "<BezierSegment Point1=\"{0},{1}\" Point2=\"{2},{3}\" Point3=\"{4},{5}\" />",
                            ((BezierSegment)seg).Point1.X, ((BezierSegment)seg).Point1.Y,
                            ((BezierSegment)seg).Point2.X, ((BezierSegment)seg).Point2.Y,
                            ((BezierSegment)seg).Point3.X, ((BezierSegment)seg).Point3.Y);
                    else if (seg is LineSegment)
                        sb.AppendFormat(
                            "<LineSegment Point=\"{0},{1}\" />", ((LineSegment)seg).Point.X, ((LineSegment)seg).Point.Y);
                    else if (seg is PolyLineSegment)
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
            sb.AppendLine(" >");
            sb.Append(AdditionalShapeDetails);
            sb.AppendLine("</Ellipse>");
            return sb.ToString();
        }


        private static string XamlOfRectangle(Rectangle p, string newTag)
        {
            var sb = new StringBuilder("<Rectangle", 200);
            sb.Append(ShapeDetails(p, newTag));
            AddElement(sb, "RadiusX", p.RadiusX, p);
            AddElement(sb, "RadiusY", p.RadiusY, p);
            sb.AppendLine(" >");
            sb.Append(AdditionalShapeDetails);
            sb.AppendLine("</Rectangle>");
            return sb.ToString();
        }

        private static string XamlOfPolygon(Polygon p, string newTag)
        {
            var sb = new StringBuilder("<Polygon", 200);
            sb.Append(ShapeDetails(p, newTag));
            AddElement(sb, "FillRule", p.FillRule, p);
            AddElement(sb, "Points", p.Points, p);
            sb.AppendLine(" >");
            sb.Append(AdditionalShapeDetails);
            sb.AppendLine("</Polygon>");
            return sb.ToString();
        }

        private static string XamlOfPolyline(Polyline p, string newTag)
        {
            var sb = new StringBuilder("<Polyline", 200);
            sb.Append(ShapeDetails(p, newTag));
            AddElement(sb, "FillRule", p.FillRule, p);
            AddElement(sb, "Points", p.Points, p);
            sb.AppendLine(" >");
            sb.Append(AdditionalShapeDetails);
            sb.AppendLine("</Polyline>");
            return sb.ToString();
        }

        private static string XamlOfLine(Line p, string newTag)
        {
            var sb = new StringBuilder("<Line", 200);
            sb.Append(ShapeDetails(p, newTag));
            AddElement(sb, "X1", p.X1, p);
            AddElement(sb, "X2", p.X2, p);
            AddElement(sb, "Y1", p.Y1, p);
            AddElement(sb, "Y2", p.Y2, p);
            sb.AppendLine(" >");
            sb.Append(AdditionalShapeDetails);
            sb.AppendLine("</Line>");
            return sb.ToString();
        }

        private static string ShapeDetails(Shape p, string newTag)
        {
            var sb = new StringBuilder();
            AddElement(sb, "Stretch", p.Stretch, p);
            AddElement(sb, "Fill", p.Fill, p);
            AddElement(sb, "Stroke", p.Stroke, p);

            AddElement(sb, "StrokeThickness", p.StrokeThickness, p);
            AddElement(sb, "StrokeStartLineCap", p.StrokeStartLineCap, p);
            AddElement(sb, "StrokeEndLineCap", p.StrokeEndLineCap, p);
            AddElement(sb, "StrokeDashCap", p.StrokeDashCap, p);
            AddElement(sb, "StrokeLineJoin", p.StrokeLineJoin, p);
            AddElement(sb, "StrokeMiterLimit", p.StrokeMiterLimit, p);
            AddElement(sb, "StrokeDashOffset", p.StrokeDashOffset, p);
            AddElement(sb, "StrokeDashArray", p.StrokeDashArray, p);
            AddElement(sb, "Tag", !string.IsNullOrWhiteSpace(newTag) ? newTag : p.Tag, p);
            AddElement(sb, "LayoutTransform", p.LayoutTransform, p);
            AddElement(sb, "Width", p.Width, p);
            AddElement(sb, "Height", p.Height, p);
            AddElement(sb, "Margin", p.Margin, p);
            AddElement(sb, "HorizontalAlignment", p.HorizontalAlignment, p);
            AddElement(sb, "VerticalAlignment", p.VerticalAlignment, p);
            AddElement(sb, "RenderTransform", p.RenderTransform, p);
            AddElement(sb, "RenderTransformOrigin", p.RenderTransformOrigin, p);
            AddElement(sb, "Opacity", p.Opacity, p);
            AddElement(sb, "Visibility", p.Visibility, p);
            AddElement(sb, "SnapsToDevicePixels", p.SnapsToDevicePixels, p);
            return sb.ToString();
        }

        private static void AddElement(StringBuilder sb, string name, object value, Shape p)
        {
            if (value == null
                || (value is double && double.IsNaN((double)value))) return;
            var valString = value.ToString();
            if (valString.StartsWith("System.Windows.Media"))
            {
                AdditionalShapeDetails += "<" + p.GetType().Name + "." + name + ">";
                AdditionalShapeDetails += XamlWriter.Save(value);
                AdditionalShapeDetails += "</" + p.GetType().Name + "." + name + ">";
            }
            else if (string.IsNullOrWhiteSpace(valString)) return;
            else sb.AppendFormat(" " + name + "=\"{0}\"", valString);
        }



    }
}