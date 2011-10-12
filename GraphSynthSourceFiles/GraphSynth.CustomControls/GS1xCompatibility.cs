using System;
using System.Collections.Generic;
using System.Windows;
using GraphSynth.GraphDisplay;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    public static class GS1xCompatibility
    {
        public static void UpdateArcShape(arc a)
        {
            string strShape;
            var shapeData = new List<string>();
            if (a.DisplayShape != null)
                shapeData = new List<string>(a.DisplayShape.ToString().Split(new[] { ',', '.' }));
            if (shapeData.Count == 0) strShape = updateArcShapeKeyString("");
            else strShape = updateArcShapeKeyString(shapeData[0]);
            if (shapeData.Count > 1)
            {
                MyXamlHelpers.SetValue(ref strShape, "Stroke",
                                            BrushSelector.GetColorFromString(shapeData[1]).ToString());
                MyXamlHelpers.SetValue(ref strShape, "StrokeThickness", shapeData[2]);
            }
            MyXamlHelpers.SetValue(ref strShape, "Tag", a.name);
            a.DisplayShape = new DisplayShape(strShape, ShapeRepresents.Arc, a);
        }

        public static void UpdateNodeShape(node n)
        {
            string strShape;
            List<string> shapeData;
            if (n.DisplayShape == null) shapeData = new List<string>();
            else
                shapeData
                    = new List<string>(n.DisplayShape.ToString().Split(new[] { ',', '.' }));
            if (shapeData.Count == 0) strShape = updateNodeShapeKeyString("");
            else strShape = updateNodeShapeKeyString(shapeData[0]);
            if (shapeData.Count > 1)
            {
                MyXamlHelpers.SetValue(ref strShape, "Fill",
                                            BrushSelector.GetColorFromString(shapeData[1]).ToString());
                MyXamlHelpers.SetValue(ref strShape, "Width", shapeData[2]);
                MyXamlHelpers.SetValue(ref strShape, "Height", shapeData[3]);
            }
            MyXamlHelpers.SetValue(ref strShape, "Tag", n.name);
            n.DisplayShape = new DisplayShape(strShape, ShapeRepresents.Node, n);
        }

        private static string updateNodeShapeKeyString(string shapeKey)
        {
            if (shapeKey.StartsWith("<") && shapeKey.EndsWith(">"))
                return shapeKey;
            if (Application.Current.Resources.Contains(shapeKey))
                return (string)Application.Current.Resources[shapeKey];
            if ((string.IsNullOrWhiteSpace(shapeKey)) ||
                     (shapeKey == "4F878611-3196-4d12-BA36-705F502C8A6B") || (shapeKey == "smallCircleNode") ||
                     (shapeKey == "3") || (shapeKey == "c") || (shapeKey == "BasicShapes.smallCircleNode"))
                return updateNodeShapeKeyString("SmallCircleNode");
            if ((shapeKey == "b2178640-076f-4520-b33c-c603466bc2fc") ||
                     (shapeKey == "medCircleNode") || (shapeKey == "2") || (shapeKey == "b") ||
                     (shapeKey == "BasicShapes.medCircleNode"))
                return updateNodeShapeKeyString("MedCircleNode");
            if ((shapeKey == "6E92FCD0-75DF-4f8f-A5B2-2927E22F4F0F") ||
                     (shapeKey == "largeCircleNode") || (shapeKey == "1") || (shapeKey == "n") ||
                     (shapeKey == "BasicShapes.largeCircleNode"))
                return updateNodeShapeKeyString("LargeCircleNode");
            if ((shapeKey == "19730425") ||
                     (shapeKey == "ovalNode") || (shapeKey == "4") || (shapeKey == "d") ||
                     (shapeKey == "BasicShapes.ovalNode"))
                return updateNodeShapeKeyString("OvalNode");
            if ((shapeKey == "57AF94BA-4129-45dc-B8FD-F82CA3B4433E") || (shapeKey == "simpleNode") ||
                     (shapeKey == "roundtangleNode") || (shapeKey == "BasicShapes.roundtangleNode") ||
                     (shapeKey == "5") || (shapeKey == "e") || (shapeKey == "BasicShapes.simpleNode"))
                return updateNodeShapeKeyString("RoundtangleNode");
            if ((shapeKey == "8ED1469D-90B2-43ab-B000-4FF5C682F530") ||
                     (shapeKey == "squareNode") || (shapeKey == "SquareNode") || (shapeKey == "rectangleNode")
                     || (shapeKey == "7") || (shapeKey == "g") || (shapeKey == "BasicShapes.squareNode"))
                return updateNodeShapeKeyString("RectangleNode");
            throw new Exception("Error in string to node shape interpreter (WPFFiler.Basic.cs)" +
                                    " Unable to interpret node shape's textual description (k.e. stringShape)");
        }

        private static string updateArcShapeKeyString(string shapeKey)
        {
            if (shapeKey.StartsWith("<") && shapeKey.EndsWith(">"))
                return shapeKey;
            if (Application.Current.Resources.Contains(shapeKey))
                return (string)Application.Current.Resources[shapeKey];
            if ((string.IsNullOrWhiteSpace(shapeKey)) || (shapeKey == "Straight"))
                return updateArcShapeKeyString("StraightArc");
            if ((shapeKey == "Bezier"))
                return updateArcShapeKeyString("BezierArc");
            if ((shapeKey == "Rectalinear") || (shapeKey == "Rectilinear"))
                return updateArcShapeKeyString("RectilinearArc");
            if ((shapeKey == "CircleArc") || (shapeKey == "CircularArc"))
                return updateArcShapeKeyString("CircleArc");
            throw new Exception("Error in string to arc shape interpreter (WPFFiler.Basic.cs)." +
                                    "Unable to interpret arc shape's textual description (k.e. stringShape)");
        }
    }
}