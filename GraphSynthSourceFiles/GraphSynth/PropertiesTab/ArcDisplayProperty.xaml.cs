using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GraphSynth.GraphDisplay;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for ArcDisplayProperty.xaml
    /// </summary>
    public partial class ArcDisplayProperty : UserControl
    {
        private readonly List<AbstractController> multiControllerList = new List<AbstractController>();
        private List<arc> arcs;
        private List<ArcController> controllerList = new List<ArcController>();
        private GraphGUI gui;

        public ArcDisplayProperty()
        {
            InitializeComponent();
        }

        #region StrokeColor

        private void StrokeColor_ValueChanged(object sender, RoutedEventArgs e)
        {
            foreach (arc a in arcs)
            {
                a.DisplayShape.Stroke = StrokeColorSelector.Value;
                gui.ArcPropertyChanged(a);
            }
        }

        #endregion

        #region Stroke Thickness

        private void sldStrokeThickness_ValueChanged(object sender, RoutedEventArgs e)
        {
            foreach (arc a in arcs)
            {
                a.DisplayShape.StrokeThickness = sldStrokeThickness.Value;
                gui.ArcPropertyChanged(a);
            }
            //  Update();
        }

        #endregion

        #region Show Arrow Heads

        private void chkShowArrowHeads_Click(object sender, RoutedEventArgs e)
        {
            if ((chkShowArrowHeads.IsChecked == null) || (chkShowArrowHeads.IsChecked.Value == false))
                foreach (arc a in arcs)
                {
                    var arcDispShape = (ArcShape)a.DisplayShape.Shape;
                    arcDispShape.ShowArrowHeads = false;
                    gui.ArcPropertyChanged(a);
                }
            else
                foreach (arc a in arcs)
                {
                    var arcDispShape = (ArcShape)a.DisplayShape.Shape;
                    arcDispShape.ShowArrowHeads = true;
                    gui.ArcPropertyChanged(a);
                }
            Update();
        }

        #endregion

        #region Update Methods

        internal void Update(List<arc> list, GraphGUI GraphGUI)
        {
            textPrpt.gui = gui = GraphGUI;
            arcs = list;
            textPrpt.Elements = arcs.Cast<graphElement>().ToList();
            Update();
        }

        private void Update()
        {
            var allSame = true;
            var stroke = (Brush)arcs[0].DisplayShape.Stroke;
            for (var i = 1; i < arcs.Count; i++)
                if (!BrushSelector.EqualBrushes(stroke,
                                                (Brush)arcs[i].DisplayShape.Stroke))
                {
                    allSame = false;
                    break;
                }
            if (allSame) StrokeColorSelector.ReadInBrushValue((Brush)arcs[0].DisplayShape.Stroke);
            else StrokeColorSelector.ReadInBrushValue(null);

            allSame = true;
            var thick = arcs[0].DisplayShape.StrokeThickness;
            for (var i = 1; i < arcs.Count; i++)
                if (!thick.Equals(arcs[i].DisplayShape.StrokeThickness))
                {
                    allSame = false;
                    break;
                }
            if (allSame)
                sldStrokeThickness.UpdateValue(arcs[0].DisplayShape.StrokeThickness);
            else sldStrokeThickness.UpdateValue(double.NaN);


            allSame = true;
            var aBool = ((ArcShape)arcs[0].DisplayShape.Shape).ShowArrowHeads;
            for (var i = 1; i < arcs.Count; i++)
                if (aBool != ((ArcShape)arcs[i].DisplayShape.Shape).ShowArrowHeads)
                {
                    allSame = false;
                    break;
                }
            if (allSame)
                chkShowArrowHeads.IsChecked = ((ArcShape)arcs[0].DisplayShape.Shape).ShowArrowHeads;
            else
                chkShowArrowHeads.IsChecked = null;

            if (arcs.Count == 1)
            {
                expArcController.Content = ((ArcShape)arcs[0].DisplayShape.Shape).Controller;
                expArcController.IsExpanded = true;
            }
            else if ((arcs.Count > 1) && (sameController(arcs)))
            {
                expArcController.Content = ((ArcShape)arcs[0].DisplayShape.Shape).Controller;
                expArcController.IsExpanded = true;
                multiControllerList.Clear();
                for (var i = 1; i < arcs.Count; i++)
                    multiControllerList.Add(((ArcShape)arcs[i].DisplayShape.Shape).Controller);
            }
            else
            {
                expArcController.Content = "No common arc controller to the selection.";
                expArcController.IsExpanded = false;
                multiControllerList.Clear();
            }
        }

        private static bool sameController(List<arc> selectedArcs)
        {
            var type = ((ArcShape)selectedArcs[0].DisplayShape.Shape).Controller.GetType();
            for (var i = 1; i < selectedArcs.Count; i++)
                if (!type.IsInstanceOfType(((ArcShape)selectedArcs[i].DisplayShape.Shape).Controller))
                    return false;
            return true;
        }

        private void expArcController_Expanded(object sender, RoutedEventArgs e)
        {
            applyACParameterFrom_To_((ArcController)expArcController.Content,
                                     multiControllerList);
        }

        private void applyACParameterFrom_To_(ArcController baseAC, IEnumerable<AbstractController> ACs)
        {
            var type = baseAC.GetType();
            var propInfo = type.GetProperties();
            var numProps = propInfo.GetLength(0) - (typeof(ArcController)).GetProperties().GetLength(0);
            var baseValues = new object[numProps];
            for (var j = 0; j < numProps; j++)
                baseValues[j] = propInfo[j].GetValue(baseAC, null);

            foreach (ArcController a in ACs)
                for (var j = 0; j < numProps; j++)
                    propInfo[j].SetValue(a, baseValues[j], null);

            Update();
        }

        #endregion
    }
}