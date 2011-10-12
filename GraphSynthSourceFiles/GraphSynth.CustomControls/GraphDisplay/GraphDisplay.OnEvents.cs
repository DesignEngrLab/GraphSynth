using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GraphSynth.Representation;

namespace GraphSynth.GraphDisplay
{
    public partial class GraphGUI : InkCanvas
    {
        #region Synchronize shape and shape string

        private readonly DispatcherTimer shapeSyncTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(150000000),
            IsEnabled = true
        };

        private void shapeSyncTimer_Tick(object sender, EventArgs e)
        {
            RedrawResizeAndReposition();
        }

        //public void SynchronizeShapeAndStringDescription()
        //{
        //    try
        //    {
        //        foreach (var n in graph.nodes)
        //            ((DisplayShape)n.DisplayShape).ShapeToString();
        //        foreach (var a in graph.arcs)
        //            ((DisplayShape)a.DisplayShape).ShapeToString();
        //        foreach (var a in graph.hyperarcs)
        //            ((DisplayShape)a.DisplayShape).ShapeToString();
        //    }
        //    catch (Exception exc)
        //    {
        //        ErrorLogger.Catch(exc);
        //    }
        //}
        #endregion

        #region Selection Events

        /* the following functions are odd. I won't say hacky, because shape moving and
         * resizing doesn't work as expected in the InkCanvas. This is partly due to the
         * reliance on LeftProperty, etc. as opposed to RenderTransform which we use here. */

        private Boolean preventSelectionRecursion;

        protected override void OnSelectionMoved(EventArgs e)
        {
            try
            {
                if (Selection.SelectedShapes.Count > 0)
                {
                    var moveX = (double)Selection.SelectedShapes[0].GetValue(LeftProperty);
                    var moveY = (double)Selection.SelectedShapes[0].GetValue(TopProperty);
                    if (SnapToGrid)
                    {
                        var selectionPoint = new Point(Selection.ReferencePoint.X + moveX,
                                                       Selection.ReferencePoint.Y + moveY);
                        selectionPoint = GoToNearestGridIntersection(selectionPoint);
                        moveX = selectionPoint.X - Selection.ReferencePoint.X;
                        moveY = selectionPoint.Y - Selection.ReferencePoint.Y;
                    }

                    /* for each element that is a node shape, take the move amount
                     * out of the LeftProperty and TopProperty and put it in the 
                     * RenderTransform.  Arcs  and nodeIcons take 
                     * care of themselves through bindings. */
                    foreach (FrameworkElement s in Selection.SelectedShapes)
                    {
                        if (nodeShapes.Contains(s))
                        {
                            s.RenderTransform = new MatrixTransform(
                                s.RenderTransform.Value.M11,
                                s.RenderTransform.Value.M12,
                                s.RenderTransform.Value.M21,
                                s.RenderTransform.Value.M22,
                                s.RenderTransform.Value.OffsetX + moveX,
                                s.RenderTransform.Value.OffsetY + moveY);
                            UpdateXYCoordinatesInNodes(getNodeFromShape(s));
                        }
                        s.SetValue(LeftProperty, double.NaN);
                        s.SetValue(TopProperty, double.NaN);
                    }
                }
                Select(Selection.SelectedShapes);
                RedrawResizeAndReposition(true);
                mainObject.propertyUpdate();
                storeOnUndoStack();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        protected override void OnSelectionResizing(InkCanvasSelectionEditingEventArgs e)
        {
            try
            {
                var moveX = e.NewRectangle.Left - e.OldRectangle.Left;
                var moveY = e.NewRectangle.Top - e.OldRectangle.Top;
                /* similar to moving, the rectangle top and left need to affect the 
                 * RenderTransform of all node shapes. */
                //foreach (FrameworkElement element in
                //    Selection.SelectedShapes.Cast<FrameworkElement>().Where(element => nodeShapes.Contains(element)))
                //{
                //    element.SetValue(LeftProperty, double.NaN);
                //    element.SetValue(TopProperty, double.NaN);
                //    element.SetValue(RightProperty, double.NaN);
                //    element.SetValue(BottomProperty, double.NaN);
                //    element.RenderTransform = new MatrixTransform(
                //        element.RenderTransform.Value.M11,
                //        element.RenderTransform.Value.M12,
                //        element.RenderTransform.Value.M21,
                //        element.RenderTransform.Value.M22,
                //        element.RenderTransform.Value.OffsetX + moveX,
                //        element.RenderTransform.Value.OffsetY + moveY);
                //}
                base.OnSelectionResizing(e);
                storeOnUndoStack();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        protected override void OnSelectionResized(EventArgs e)
        {
            try
            {
                /* I don't know why this one is necessary, but it is. If you remove it,
                 * nodes won't position right. I guess the Setting of Values in 
                 * Inkcanvas is happening all the time, and if we don't do this, it
                 * will add to the renderTranform. */
                foreach (FrameworkElement element in
                    Selection.SelectedShapes.Cast<FrameworkElement>().Where(element => nodeShapes.Contains(element)))
                {
                    element.SetValue(LeftProperty, double.NaN);
                    element.SetValue(TopProperty, double.NaN);
                    element.SetValue(RightProperty, double.NaN);
                    element.SetValue(BottomProperty, double.NaN);
                    // change X, Y value in .nodes
                    var n = getNodeFromShape(element);
                    UpdateXYCoordinatesInNodes(n);
                    NodePropertyChanged(n);
                    element.InvalidateVisual();
                }
                RedrawResizeAndReposition(true);
                base.OnSelectionResized(e);
                storeOnUndoStack();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        protected override void OnSelectionChanged(EventArgs e)
        {
            userChanged = true;
            try
            {
                if (!preventSelectionRecursion)
                {
                    preventSelectionRecursion = true;
                    Selection.UpdateSelection();
                    mainObject.propertyUpdate();
                    base.OnSelectionChanged(e);
                }
                preventSelectionRecursion = false;
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        #endregion

        #region Mouse Events

        private int TimeStampLeftButtonDown;

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            try
            {
                var shapeKey = mainObject.SelectedAddItem;
                if (activeNullNode != null) TimeStampLeftButtonDown = 0;
                else if (startingNewHyperArcConnection())
                {
                    TimeStampLeftButtonDown = e.Timestamp;
                }
                else if (shapeKey.EndsWith("arc", true, null))
                {
                    beginNewArc(shapeKey, MouseLocation);
                    TimeStampLeftButtonDown = e.Timestamp;
                }
                else
                {
                    var n = getNodeIconFromPoint(MouseLocation);
                    if (n != null)
                        Select(new List<UIElement> { (UIElement)n.GraphElement.DisplayShape.Shape });
                    TimeStampLeftButtonDown = int.MaxValue;
                }
                base.OnPreviewMouseLeftButtonDown(e);
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            try
            {
                if ((e.Timestamp - TimeStampLeftButtonDown > 200) && (activeNullNode != null))
                {
                    if (typeof(arc).IsInstanceOfType(activeNullNode.GraphElement))
                        completeNewArc((arc)activeNullNode.GraphElement, getNodeFromPoint(MouseLocation),
                            activeNullNode.AttachedToHead);
                    else
                        completeNewHyperArcConnection((hyperarc)activeNullNode.GraphElement, getNodeFromPoint(MouseLocation));
                }
                else
                {
                    var shapeKey = mainObject.SelectedAddItem;
                    /* if the shapeKey ends in arc than we start the process of adding a new arc, 
                     * else we add a node or macro below.*/
                    //if (shapeKey.EndsWith("arc", true, null)) beginNewArc(shapeKey, mouseLocation);
                    //else 
                    if (shapeKey.EndsWith("node", true, null)) addNewNode(shapeKey, MouseLocation);
                    else if (shapeKey.EndsWith("hyper", true, null)) addNewHyperArc(shapeKey, MouseLocation);
                }

                if (!mainObject.stayOn)
                {
                    /*if one of the rules has been set to "stay on" as in adding more than
                    * one arc or node, then this step is skipped. Otherwise, the default 
                    * is to return the current state to None; */
                    mainObject.SetSelectedAddItem(-1);
                    Cursor = Cursors.Cross;
                }

                base.OnMouseLeftButtonUp(e);
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            MouseLocation = Mouse.GetPosition(this);
            if (activeNullNode != null)
                activeNullNode.Center = MouseLocation;

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            MouseLocation = new Point(double.NaN, double.NaN);
            shapeSyncTimer_Tick(null, e);
            base.OnMouseLeave(e);
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            try
            {
                base.OnMouseWheel(e);
                if (e.Delta > 0)
                    zoomIn();
                else if (e.Delta < 0)
                    zoomOut();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }
        #endregion

        #region Keyboard Events

        protected override void OnKeyUp(KeyEventArgs e)
        {
            try
            {
                base.OnKeyUp(e);
                switch (e.Key)
                {
                    case Key.Up:
                        nudgeUp();
                        break;
                    case Key.Down:
                        nudgeDown();
                        break;
                    case Key.Left:
                        nudgeLeft();
                        break;
                    case Key.Right:
                        nudgeRight();
                        break;
                    case Key.F2:
                        mainObject.FocusOnLabelEntry(this);
                        break;
                    default:
                        if ((e.KeyboardDevice.Modifiers == ModifierKeys.None) &&
                            ((int)e.Key >= (int)Key.D0) && ((int)e.Key <= (int)Key.Divide) &&
                            (e.Key != Key.LWin) && (e.Key != Key.RWin) && (e.Key != Key.Apps)
                            && (e.Key != Key.Sleep))
                            HandleKeyboardShortcuts(e.Key, MouseLocation, e.Source);
                        break;
                }
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        #endregion
    }
}