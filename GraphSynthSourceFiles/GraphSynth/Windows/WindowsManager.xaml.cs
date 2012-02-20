using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using GraphSynth.GraphDisplay;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for WindowsManager.xaml
    /// </summary>
    public partial class WindowsManager : UserControl
    {
        /*
         * Refer
         * http://blog.paranoidferret.com/index.php/2008/10/28/wpf-tutorial-using-the-listview-part-2-sorting/
         * http://blog.paranoidferret.com/index.php/2008/02/28/wpf-tutorial-using-the-listview-part-1/         * 
         * http://msdn.microsoft.com/en-us/library/ms771417.aspx
         */
        private readonly ObservableCollection<WinData> _WinCollection = new ObservableCollection<WinData>();
        private SortAdorner _CurAdorner;
        private GridViewColumnHeader _CurSortCol;
        private int _searchProcessID;
        private GlobalSettingWindow gSW;
        private Boolean ignoreSelectionChange;
        private Boolean isGlobalSettingWindowOpen;

        public WindowsManager()
        {
            InitializeComponent();
        }

        public GraphGUI activeGraphCanvas { get; private set; }

        public Window activeWindow
        {
            get
            {
                if (WinCollection.Count > 0) return WinCollection[0].Win;
                return GSApp.main;
            }
        }

        public int SearchProcessID
        {
            get { return _searchProcessID++; }
        }

        private static MainWindow main
        {
            get { return GSApp.main; }
        }

        public int NumActiveSPC
        {
            get { return WinCollection.Count(w => w.WinType == WindowType.SearchProcessController); }
        }

        public ObservableCollection<WinData> WinCollection
        {
            get { return _WinCollection; }
        }

        public ObservableCollection<WinData> GraphWindows
        {
            get
            {
                var wins = (from w in _WinCollection
                            where (w.WinType == WindowType.Graph)
                            select w);
                return new ObservableCollection<WinData>(wins);
            }
        }

        public ObservableCollection<WinData> RuleWindows
        {
            get
            {
                var wins = (from w in _WinCollection
                            where (w.WinType == WindowType.Rule)
                            select w);
                return new ObservableCollection<WinData>(wins);
            }
        }

        public int NumberOfWindows
        {
            get { return WinCollection.Count; }
        }


        public void AddandShowWindow(Window w)
        {
            if (w is graphWindow)
            {
                WinCollection.Insert(0, new WinData(w, WindowType.Graph, ((graphWindow)w).filename));
                activeGraphCanvas = ((graphWindow)w).graphGUI;
            }
            else if (w is ruleWindow)
            {
                WinCollection.Insert(0, new WinData(w, WindowType.Rule, ((ruleWindow)w).filename));
                activeGraphCanvas = ((ruleWindow)w).graphCanvasK;
            }
            else if (w is ruleSetWindow)
                WinCollection.Insert(0, new WinData(w, WindowType.RuleSet, ((ruleSetWindow)w).filename));
            else if (w is searchProcessController)
                WinCollection.Insert(0, new WinData(w, WindowType.SearchProcessController,
                                                    "SearchProcessController" + _searchProcessID));
            else if (w is GlobalSettingWindow)
            {
                gSW = (GlobalSettingWindow)w;
                WinCollection.Insert(0, new WinData(w, WindowType.GlobalSetting,
                                                    "GlobalSettingWindow"));
            }
            w.Closed += Window_Closed;

            w.Show();
        }

        public void RemoveWindow(Window w)
        {
            var remWd = (from wd in WinCollection
                         where (wd.Win == w)
                         select wd).FirstOrDefault();
            WinCollection.Remove(remWd);
        }

        public void Window_Closed(object sender, EventArgs e)
        {
            var remWd = (from wd in WinCollection
                         where (wd.Win == sender)
                         select wd).FirstOrDefault();
            WinCollection.Remove(remWd);
            if (remWd.WinType == WindowType.GlobalSetting)
            {
                isGlobalSettingWindowOpen = false;
                gSW = null;
            }
            if (activeWindow is graphWindow)
                activeGraphCanvas = ((graphWindow)activeWindow).graphGUI;
            else if (activeWindow is ruleWindow)
                activeGraphCanvas = ((ruleWindow)activeWindow).graphCanvasK;
            else activeGraphCanvas = null;
            main.propertyUpdate();
        }

        internal void SetAsActive(Window win)
        {
            main.MoveFocus(new TraversalRequest(FocusNavigationDirection.Last));
            var activeWd = (from wd in WinCollection
                            where (wd.Win == win)
                            select wd).FirstOrDefault();
            if (WinCollection[0] == activeWd) return;
            ignoreSelectionChange = true;
            WinCollection.Remove(activeWd);
            WinCollection.Insert(0, activeWd);
            ignoreSelectionChange = false;
        }

        internal void SetActiveGraphCanvas(GraphGUI graphCanvas)
        {
            activeGraphCanvas = graphCanvas;
            main.propertyUpdate();
            Keyboard.Focus(graphCanvas);
        }

        public Boolean FindAndFocusFileInCollection(XmlDocument doc, XmlNamespaceManager nsMgr,
                                                    string name)
        {
            if (WinCollection.Count == 0) return false;
            var wT = WindowType.Invalid;

            if (doc.SelectNodes("/ruleSet", nsMgr).Count > 0)
                wT = WindowType.RuleSet;
            else if (doc.SelectNodes("/grammarRule", nsMgr).Count > 0)
                wT = WindowType.Rule;
            else if ((doc.SelectNodes("/designGraph", nsMgr).Count > 0)
                     || (doc.SelectNodes("/candidate", nsMgr).Count > 0)
                     || (doc.DocumentElement.Attributes["Tag"].Value == "Graph"))
                wT = WindowType.Graph;
            else if (doc.DocumentElement.Attributes["Tag"].Value == "Rule")
                wT = WindowType.Rule;
            return FindAndFocusFileInCollection(name, wT);
        }

        public Boolean FindAndFocusFileInCollection(string name, WindowType wT)
        {
            var wD = (
                         from p in WinCollection
                         where (p.WinPath == name) && (p.WinType == wT)
                         select p).FirstOrDefault();
            if (wD == null) return false;
            wD.Win.Activate();
            wD.Win.Focus();
            return true;
        }

        internal void FocusNextWindow()
        {
            ignoreSelectionChange = true;
            var lastWd = WinCollection[0];
            WinCollection.RemoveAt(0);
            WinCollection.Insert(NumberOfWindows, lastWd);
            activeWindow.Focus();
            ignoreSelectionChange = false;
        }

        internal void OpenAndShowSettings()
        {
            try
            {
                if (!isGlobalSettingWindowOpen)
                {
                    isGlobalSettingWindowOpen = true;
                    AddandShowWindow(new GlobalSettingWindow());
                }
                gSW.WindowState = WindowState.Normal;
                gSW.Focus();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        internal void CloseAllGraphWindows()
        {
            try
            {
                var listWins = new List<WinData>(WinCollection);
                for (var i = 0; i < listWins.Count; i++)
                    if (listWins[i].WinType == WindowType.Graph)
                        listWins[i].Win.Close();
            }
            catch (Exception e)
            {
                ErrorLogger.Catch(e);
            }
        }

        internal void ExpandOrCollapse()
        {
            expanderWM.IsExpanded = (activeGraphCanvas == null);
        }

        #region Minimize and Restore Windows

        internal Boolean AllWindowsMinimized()
        {
            return (WinCollection.All(w => (w.Win.WindowState == WindowState.Minimized)));
        }

        internal void RestoreWindows()
        {
            for (int i = 0; i < WinCollection.Count; i++)
                WinCollection[i].Win.WindowState = WindowState.Normal;
                //foreach (WinData t in WinCollection)
                //    t.Win.WindowState = WindowState.Normal;
        }

        internal void MinimizeWindows()
        {
            for (int i = 0; i < WinCollection.Count; i++)
                WinCollection[i].Win.WindowState = WindowState.Minimized;
            for (int i = 0; i < WinCollection.Count; i++)
                WinCollection[i].Win.WindowState = WindowState.Minimized;
            //foreach (WinData t in WinCollection)
            //    t.Win.WindowState = WindowState.Minimized;
        }

        #endregion

        #region Events

        private void SortClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var column = sender as GridViewColumnHeader;
                var field = column.Tag as String;

                if (_CurSortCol != null)
                {
                    AdornerLayer.GetAdornerLayer(
                        _CurSortCol).Remove(_CurAdorner);
                    WinMgrView.Items.SortDescriptions.Clear();
                }

                var newDir = ListSortDirection.Ascending;
                if (_CurSortCol == column &&
                    _CurAdorner.Direction == newDir)
                    newDir = ListSortDirection.Descending;

                _CurSortCol = column;
                _CurAdorner = new SortAdorner(_CurSortCol, newDir);
                AdornerLayer.GetAdornerLayer(
                    _CurSortCol).Add(_CurAdorner);
                WinMgrView.Items.SortDescriptions.Add(
                    new SortDescription(field, newDir));
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private void WinMgrView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ignoreSelectionChange) return;
            try
            {
                var wd = (WinData)WinMgrView.SelectedItem;
                wd.Win.Focus();
                wd.Win.WindowState = WindowState.Normal;
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private void WinMgrView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var wd = (WinData)WinMgrView.SelectedItem;
            if (wd == null) return;
            switch (wd.Win.WindowState)
            {
                case WindowState.Normal:
                    wd.Win.WindowState = WindowState.Minimized;
                    break;
                case WindowState.Minimized:
                    wd.Win.WindowState = WindowState.Normal;
                    break;
            }
            wd.Win.Focus();
        }

        #endregion
    }

    public class SortAdorner : Adorner
    {
        private static readonly Geometry _AscGeometry =
            Geometry.Parse("M 0,0 L 10,0 L 5,5 Z");

        private static readonly Geometry _DescGeometry =
            Geometry.Parse("M 0,5 L 10,5 L 5,0 Z");

        public SortAdorner(UIElement element,
                           ListSortDirection dir)
            : base(element)
        {
            Direction = dir;
        }

        public ListSortDirection Direction { get; private set; }

        protected override void OnRender(
            DrawingContext drawingContext)
        {
            try
            {
                base.OnRender(drawingContext);

                if (AdornedElement.RenderSize.Width < 20)
                    return;

                drawingContext.PushTransform(
                    new TranslateTransform(
                        AdornedElement.RenderSize.Width - 15,
                        (AdornedElement.RenderSize.Height - 5) / 2));

                drawingContext.DrawGeometry(Brushes.Black, null,
                                            Direction == ListSortDirection.Ascending
                                                ? _AscGeometry
                                                : _DescGeometry);

                drawingContext.Pop();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }
    }
}