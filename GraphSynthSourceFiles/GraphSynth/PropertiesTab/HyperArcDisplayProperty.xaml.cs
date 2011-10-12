using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GraphSynth.GraphDisplay;
using GraphSynth.Representation;

// This will be updated once hyperarcs are inplace.

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for ArcDisplayProperty.xaml
    /// </summary>
    public partial class HyperArcDisplayProperty : UserControl
    {
        private readonly List<AbstractController> multiControllerList = new List<AbstractController>();
        private List<hyperarc> hyperarcs;
        private List<HyperArcController> controllerList = new List<HyperArcController>();
        private GraphGUI gui;

        public HyperArcDisplayProperty()
        {
            InitializeComponent();
        }

        #region StrokeColor

        private void StrokeColorSelector_ValueChanged(object sender, RoutedEventArgs e)
        {
            foreach (hyperarc a in hyperarcs)
            {
                ((HyperArcShape)a.DisplayShape.Shape).Stroke = StrokeColorSelector.Value;
                gui.HyperArcPropertyChanged(a);
            }
        }

        #endregion
        #region Fill Color

        private void FillColorSelector_ValueChanged(object sender, RoutedEventArgs e)
        {
            foreach (hyperarc h in hyperarcs)
            {
                h.DisplayShape.Fill = FillColorSelector.Value;
                gui.HyperArcPropertyChanged(h);
            }
            //  Update();
        }

        #endregion

        #region Stroke Thickness

        private void sldStrokeThickness_ValueChanged(object sender, RoutedEventArgs e)
        {
            foreach (hyperarc h in hyperarcs)
            {
                h.DisplayShape.StrokeThickness = sldStrokeThickness.Value;
                gui.HyperArcPropertyChanged(h);
            }
            // Update();
        }

        #endregion


        #region Update Methods

        internal void Update(List<hyperarc> list, GraphGUI GraphGUI)
        {
            textPrpt.gui = gui = GraphGUI;
            hyperarcs = list;
            textPrpt.Elements = hyperarcs.Cast<graphElement>().ToList();
            Update();
        }

        private void Update()
        {
            var allSame = true;
            var stroke = (Brush)hyperarcs[0].DisplayShape.Stroke;
            for (var i = 1; i < hyperarcs.Count; i++)
                if (!BrushSelector.EqualBrushes(stroke,
                                                (Brush)hyperarcs[i].DisplayShape.Stroke))
                {
                    allSame = false;
                    break;
                }
            if (allSame) StrokeColorSelector.ReadInBrushValue((Brush)hyperarcs[0].DisplayShape.Stroke);
            else StrokeColorSelector.ReadInBrushValue(null);

            allSame = true;
            var thick = hyperarcs[0].DisplayShape.StrokeThickness;
            for (var i = 1; i < hyperarcs.Count; i++)
                if (!thick.Equals(hyperarcs[i].DisplayShape.StrokeThickness))
                {
                    allSame = false;
                    break;
                }
            sldStrokeThickness.UpdateValue(allSame ? hyperarcs[0].DisplayShape.StrokeThickness : double.NaN);


            if (hyperarcs.Count == 1)
            {
                expArcController.Content = ((HyperArcShape)hyperarcs[0].DisplayShape.Shape).Controller;
                expArcController.IsExpanded = true;
            }
            else if ((hyperarcs.Count > 1) && (sameController(hyperarcs)))
            {
                expArcController.Content = ((HyperArcShape)hyperarcs[0].DisplayShape.Shape).Controller;
                expArcController.IsExpanded = true;
                multiControllerList.Clear();
                for (var i = 1; i < hyperarcs.Count; i++)
                    multiControllerList.Add(((HyperArcShape)hyperarcs[i].DisplayShape.Shape).Controller);
            }
            else
            {
                expArcController.Content = "No common arc controller to the selection.";
                expArcController.IsExpanded = false;
                multiControllerList.Clear();
            }
        }

        private static bool sameController(List<hyperarc> selectedArcs)
        {
            var type = ((HyperArcShape)selectedArcs[0].DisplayShape.Shape).Controller.GetType();
            for (var i = 1; i < selectedArcs.Count; i++)
                if (!type.IsInstanceOfType(((HyperArcShape)selectedArcs[i].DisplayShape.Shape).Controller))
                    return false;
            return true;
        }

        private void expArcController_Expanded(object sender, RoutedEventArgs e)
        {
            applyACParameterFrom_To_((HyperArcController)expArcController.Content,
                                     multiControllerList);
        }

        private void applyACParameterFrom_To_(HyperArcController baseAC, IEnumerable<AbstractController> ACs)
        {
            var type = baseAC.GetType();
            var propInfo = type.GetProperties();
            var numProps = propInfo.GetLength(0) - (typeof(HyperArcController)).GetProperties().GetLength(0);
            var baseValues = new object[numProps];
            for (var j = 0; j < numProps; j++)
                baseValues[j] = propInfo[j].GetValue(baseAC, null);

            foreach (HyperArcController a in ACs)
                for (var j = 0; j < numProps; j++)
                    propInfo[j].SetValue(a, baseValues[j], null);

            //Update();
        }

        #endregion
    }
}