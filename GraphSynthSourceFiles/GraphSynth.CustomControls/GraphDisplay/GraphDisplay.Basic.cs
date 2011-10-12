using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GraphSynth.Representation;

namespace GraphSynth.GraphDisplay
{
    public partial class GraphGUI : InkCanvas
    {
        #region Fields & Properties
        public SelectionClass Selection;

        public NullNodeIconBank nullNodeIcons;
        public NodeIconBank nodeIcons;
        public ArcIconBank arcIcons;
        public HyperArcIconBank hyperarcIcons;

        public ShapeBank nodeShapes;
        public ShapeBank arcShapes;
        public ShapeBank hyperarcShapes;

        #region drawing a new arc

        public NullNodeIconShape activeNullNode { get; set; }

        /// <summary>
        ///   this is used to keep track of whether an arc has been started (n From node
        ///   has been chosen. Thus we are currently dragging the free-end about the screen.
        ///   </summary>
        //public arc newArc { get; set; }
        //public Boolean draggingArcTail { get; set; }
        //public Boolean draggingArcHead { get; set; }
      //  public HyperArcIconShape draggingHyperConnect { get; set; }
        #endregion


        #region Grid

        private const double defaultLength = 72.0;
        public GridAndAxes gridAndAxes;

        public Boolean SnapToGrid { get; set; }

        #endregion

        protected Point MouseLocation
        {
            get { return (Point)GetValue(MouseLocationProperty); }
            set { SetValue(MouseLocationProperty, value); }
        }

        public static readonly DependencyProperty MouseLocationProperty =
            DependencyProperty.Register("MouseLocation",
                                        typeof(Point), typeof(GraphGUI),
                                        new FrameworkPropertyMetadata(new Point(),
                                                                      FrameworkPropertyMetadataOptions.AffectsRender));

        public designGraph graphObject { get; set; }

        public IMainWindow mainObject
        {
            get { return OwnerWindow.Owner as IMainWindow; }
        }

        public Window OwnerWindow
        {
            get
            {
                var parent = (FrameworkElement)Parent;
                while (parent as Window == null)
                    parent = (FrameworkElement)parent.Parent;
                return (Window)parent;
            }
        }

        public designGraph graph
        {
            get { return graphObject; }
            set { graphObject = value; }
        }

        public Boolean userChanged { get; set; }

        #endregion

        #region Constructor

        public GraphGUI()
        {
            Selection = new SelectionClass(this);

            EditingMode = InkCanvasEditingMode.Select;
            var formats = new[]
                              {
                                  InkCanvasClipboardFormat.Xaml,
                                  InkCanvasClipboardFormat.InkSerializedFormat
                              };
            PreferredPasteFormats = formats;

            nodeShapes = new ShapeBank(this);
            arcShapes = new ShapeBank(this);
            hyperarcShapes = new ShapeBank(this);

            nullNodeIcons = new NullNodeIconBank(this);
            nodeIcons = new NodeIconBank(this);
            arcIcons = new ArcIconBank(this);
            hyperarcIcons = new HyperArcIconBank(this);

            gridAndAxes = new GridAndAxes(defaultLength);
            Children.Add(gridAndAxes);

            shapeSyncTimer.Tick += shapeSyncTimer_Tick;
            shapeSyncTimer.Start();
        }

        #endregion

        #region Keyboard Shortcuts

        public void HandleKeyboardShortcuts(Key key, Point p, object eventSource)
        {
            if (!mainObject.shortCutKeys.Contains(key)) return;
            var keyInt = Array.IndexOf(mainObject.shortCutKeys, key);
            mainObject.SetSelectedAddItem(keyInt);

            if (mainObject.SelectedAddItem.ToLowerInvariant().Contains("node"))
                addNewNode(mainObject.SelectedAddItem, p);
            else if (mainObject.SelectedAddItem.ToLowerInvariant().Contains("arc"))
            {
                beginNewArc(mainObject.SelectedAddItem, p);
                //((ArcShape)newArc.DisplayShape.Shape).ToShape.RenderTransform
                //    = new TranslateTransform(mouseLocation.X, mouseLocation.Y);
            }
            else if (mainObject.SelectedAddItem.ToLowerInvariant().Contains("hyper"))
            {
                addNewHyperArc(mainObject.SelectedAddItem, p);
            }
            mainObject.SetSelectedAddItem(-1);
        }

        #endregion
    }
}