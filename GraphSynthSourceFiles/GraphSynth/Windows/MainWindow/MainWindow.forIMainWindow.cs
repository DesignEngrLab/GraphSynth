using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using GraphSynth.GraphDisplay;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMainWindow
    {
        #region Fields and Properties

        private int _selectedAddItem;
        public List<string> SelectedAddItems { get; private set; }
        public Key[] shortCutKeys { get; private set; }

        public string SelectedAddItem
        {
            get { return SelectedAddItems[_selectedAddItem]; }
        }

        public Boolean SetSelectedAddItem(int index)
        {
            if ((index >= 0) && (index < SelectedAddItems.Count))
            {
                _selectedAddItem = index;
                return true;
            }
            else
            {
                _selectedAddItem = 0;
                return false;
            }
        }

        public Boolean stayOn { get; private set; }

        public Boolean SetSelectedAddItem(string key)
        {
            if (SelectedAddItems.Contains(key))
            {
                _selectedAddItem = SelectedAddItems.IndexOf(key);
                return true;
            }
            else
            {
                key = key.Replace("Button", "");
                if (SelectedAddItems.Contains(key))
                {
                    _selectedAddItem = SelectedAddItems.IndexOf(key);
                    return true;
                }
                else
                {
                    _selectedAddItem = 0;
                    return false;
                }
            }
        }

        #endregion

        #region Add and Show a Window

        public void addAndShowGraphWindow(object obj, string title = null)
        {
            var dG = new designGraph();
            var canvas = SeedCanvas;
            string filename = null;
            if (obj == null) dG = null;
            else if ((typeof(designGraph)).IsInstanceOfType(obj))
                dG = (designGraph)obj;
            else if ((typeof(object[])).IsInstanceOfType(obj))
            {
                var objArray = (object[])obj;
                if (objArray.GetLength(0) > 0)
                {
                    if ((typeof(designGraph)).IsInstanceOfType(objArray[0]))
                        dG = (designGraph)objArray[0];
                    else if ((typeof(CanvasProperty)).IsInstanceOfType(objArray[0]))
                        canvas = (CanvasProperty)objArray[0];
                    else if ((typeof(string)).IsInstanceOfType(objArray[0]))
                        filename = (string)objArray[0];
                }
                if (objArray.GetLength(0) > 1)
                {
                    if ((typeof(designGraph)).IsInstanceOfType(objArray[1]))
                        dG = (designGraph)objArray[1];
                    else if ((typeof(CanvasProperty)).IsInstanceOfType(objArray[1]))
                        canvas = (CanvasProperty)objArray[1];
                    else if ((typeof(string)).IsInstanceOfType(objArray[1]))
                        filename = (string)objArray[1];
                }
                if (objArray.GetLength(0) > 2)
                {
                    if ((typeof(designGraph)).IsInstanceOfType(objArray[2]))
                        dG = (designGraph)objArray[2];
                    else if ((typeof(CanvasProperty)).IsInstanceOfType(objArray[2]))
                        canvas = (CanvasProperty)objArray[2];
                    else if ((typeof(string)).IsInstanceOfType(objArray[2]))
                        filename = (string)objArray[2];
                }
            }
            if (title != null && title.StartsWith("SEED")) SeedCanvas = canvas;
            if (!windowsMgr.FindAndFocusFileInCollection(filename, WindowType.Graph))
            {
                var gW = new graphWindow(dG, canvas, filename, title);
                windowsMgr.AddandShowWindow(gW);
            }
        }

        public void addAndShowRuleWindow(object obj, string title = null)
        {
            var gR = new grammarRule();
            var canvas = new CanvasProperty();
            string filename = null;
            if (obj == null) gR = null;
            else if ((typeof(grammarRule)).IsInstanceOfType(obj))
                gR = (grammarRule)obj;
            else if ((typeof(object[])).IsInstanceOfType(obj))
            {
                var objArray = (object[])obj;
                if (objArray.GetLength(0) > 0)
                {
                    if ((typeof(grammarRule)).IsInstanceOfType(objArray[0]))
                        gR = (grammarRule)objArray[0];
                    else if ((typeof(CanvasProperty)).IsInstanceOfType(objArray[0]))
                        canvas = (CanvasProperty)objArray[0];
                    else if ((typeof(string)).IsInstanceOfType(objArray[0]))
                        filename = (string)objArray[0];
                }
                if (objArray.GetLength(0) > 1)
                {
                    if ((typeof(grammarRule)).IsInstanceOfType(objArray[1]))
                        gR = (grammarRule)objArray[1];
                    else if ((typeof(CanvasProperty)).IsInstanceOfType(objArray[1]))
                        canvas = (CanvasProperty)objArray[1];
                    else if ((typeof(string)).IsInstanceOfType(objArray[1]))
                        filename = (string)objArray[1];
                }
                if (objArray.GetLength(0) > 2)
                {
                    if ((typeof(grammarRule)).IsInstanceOfType(objArray[2]))
                        gR = (grammarRule)objArray[2];
                    else if ((typeof(CanvasProperty)).IsInstanceOfType(objArray[2]))
                        canvas = (CanvasProperty)objArray[2];
                    else if ((typeof(string)).IsInstanceOfType(objArray[2]))
                        filename = (string)objArray[2];
                }
            }
            if (!windowsMgr.FindAndFocusFileInCollection(filename, WindowType.Rule))
            {
                var rW = new ruleWindow(gR, canvas, filename, title);
                windowsMgr.AddandShowWindow(rW);
            }
        }
        
        public void addAndShowRuleSetWindow(object obj, string title=null)
        {
            var rs = new ruleSet();
            string filename = null;
            if (obj == null) rs = null;
            else if ((typeof(ruleSet)).IsInstanceOfType(obj))
                rs = (ruleSet)obj;
            else if ((typeof(object[])).IsInstanceOfType(obj))
            {
                var objArray = (object[]) obj;
                if (objArray.GetLength(0) > 0)
                {
                    if ((typeof (ruleSet)).IsInstanceOfType(objArray[0]))
                        rs = (ruleSet) objArray[0];
                    else if ((typeof (string)).IsInstanceOfType(objArray[0]))
                        filename = (string) objArray[0];
                }
                if (objArray.GetLength(0) > 1)
                {
                    if ((typeof (ruleSet)).IsInstanceOfType(objArray[1]))
                        rs = (ruleSet) objArray[1];
                    else if ((typeof (string)).IsInstanceOfType(objArray[1]))
                        filename = (string) objArray[1];
                }
            }
            if (title == null) title = rs.name;
            if (!windowsMgr.FindAndFocusFileInCollection(filename, WindowType.RuleSet))
            {
                var rSW = new ruleSetWindow(rs, filename,title);
                windowsMgr.AddandShowWindow(rSW);
            }
        }

        public void addAndShowSearchController(searchProcessController controller, Boolean playOnStart)
        {
            try
            {
                windowsMgr.AddandShowWindow(controller);
                if (playOnStart) controller.btnPlay_Click(null, null);
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        #endregion

        #region IMainWindow Members

        public void FocusOnLabelEntry(object o)
        {
            propertyUpdate(o);
            if (((GraphGUI)o).Selection.SelectedArc != null)
                property.ArcPrpt.txtLabels.Focus();
            else if (((GraphGUI)o).Selection.SelectedNode != null)
                property.NodePrpt.txtLabels.Focus();
        }

        public void propertyUpdate(object o = null)
        {
            if ((o != null) &&
                (typeof(GraphGUI).IsInstanceOfType(o)))
                windowsMgr.SetActiveGraphCanvas((GraphGUI)o);
            property.Update();
        }

        public void SetCanvasPropertyScaleFactor(double scale, Boolean? zoomToFit)
        {
            if (property.CanvasProp != null)
            {
                if (property.CanvasProp.ScaleFactor != scale)
                    property.CanvasProp.ScaleFactor = scale;
                if (zoomToFit != null)
                    property.CanvasProp.ZoomToFit = (Boolean)zoomToFit;
            }
        }

        #endregion
    }
}