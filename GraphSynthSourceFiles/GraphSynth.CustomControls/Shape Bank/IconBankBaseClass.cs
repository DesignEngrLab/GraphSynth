using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using GraphSynth.Representation;

namespace GraphSynth.GraphDisplay
{
    public abstract class IconBank : DependencyObject, IEnumerable<IconShape>
    {
        protected GraphGUI gd;
        protected List<IconShape> shapes = new List<IconShape>();

        #region Show Text Properties

        public static readonly DependencyProperty FontSizeProperty
            = DependencyProperty.Register("FontSize",
                                          typeof(double), typeof(IconBank),
                                          new FrameworkPropertyMetadata(12.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowNameProperty
            = DependencyProperty.Register("ShowName",
                                          typeof(Boolean), typeof(IconBank),
                                          new FrameworkPropertyMetadata(true,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowLabelsProperty
            = DependencyProperty.Register("ShowLabels",
                                          typeof(Boolean), typeof(IconBank),
                                          new FrameworkPropertyMetadata(true,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty DisplayTextDistanceProperty
            = DependencyProperty.Register("DisplayTextDistance",
                                          typeof(double), typeof(IconBank),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty DisplayTextPositionProperty
            = DependencyProperty.Register("DisplayTextPosition",
                                          typeof(double), typeof(IconBank),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public Boolean ShowName
        {
            get { return (Boolean)GetValue(ShowNameProperty); }
            set { SetValue(ShowNameProperty, value); }
        }

        public Boolean ShowLabels
        {
            get { return (Boolean)GetValue(ShowLabelsProperty); }
            set { SetValue(ShowLabelsProperty, value); }
        }

        public double DisplayTextDistance
        {
            get { return (double)GetValue(DisplayTextDistanceProperty); }
            set { SetValue(DisplayTextDistanceProperty, value); }
        }

        public double DisplayTextPosition
        {
            get { return (double)GetValue(DisplayTextPositionProperty); }
            set { SetValue(DisplayTextPositionProperty, value); }
        }

        #endregion

        #region Constructor

        protected IconBank(GraphGUI gd)
        {
            this.gd = gd;
        }
        #endregion

        #region Iterator & Count

        public int Count
        {
            get { return shapes.Count; }
        }
        public IconShape this[int index]
        {
            get { return shapes[index]; }
            set { shapes[index] = value; }
        }

        #endregion

        #region Methods
        public void Add(IconShape icon, graphElement e)
        {
            ((DisplayShape)e.DisplayShape).icon = icon;
            Add(icon);
        }

        public void Add(IconShape icon)
        {
            if ((icon.ShowName != ShowName) || (icon.ShowLabels != ShowLabels)
                || (double.IsNaN(icon.DisplayTextDistance))
                || (double.IsNaN(icon.DisplayTextPosition))
                || (double.IsNaN(icon.FontSize))
                || (((icon.DisplayTextDistance - this.DisplayTextDistance)
                * (icon.DisplayTextDistance - this.DisplayTextDistance)
                + (icon.DisplayTextPosition - this.DisplayTextPosition)
                * (icon.DisplayTextPosition - this.DisplayTextPosition)
                + (icon.FontSize - this.FontSize) * (icon.FontSize - this.FontSize))
                < 0.01))
                BindTextDisplayProperties(icon);
            else UnbindTextDisplayProperties(icon);
            shapes.Add(icon);
            gd.Children.Add(icon);
            icon.InvalidateVisual();
        }

        public void BindTextDisplayProperties(IconShape icon)
        {
            icon.UniqueTextProperties = false;
            var binding = new Binding
                              {
                                  Source = this,
                                  Mode = BindingMode.OneWay,
                                  Path = new PropertyPath(ShowNameProperty)
                              };
            icon.SetBinding(IconShape.ShowNameProperty, binding);

            binding = new Binding
                            {
                                Source = this,
                                Mode = BindingMode.OneWay,
                                Path = new PropertyPath(ShowLabelsProperty)
                            };
            icon.SetBinding(IconShape.ShowLabelsProperty, binding);

            binding = new Binding
              {
                  Source = this,
                  Mode = BindingMode.OneWay,
                  Path = new PropertyPath(FontSizeProperty)
              };
            icon.SetBinding(IconShape.FontSizeProperty, binding);

            binding = new Binding
            {
                Source = this,
                Mode = BindingMode.OneWay,
                Path = new PropertyPath(DisplayTextDistanceProperty)
            };
            icon.SetBinding(IconShape.DisplayTextDistanceProperty, binding);

            binding = new Binding
            {
                Source = this,
                Mode = BindingMode.OneWay,
                Path = new PropertyPath(DisplayTextPositionProperty)
            };
            icon.SetBinding(IconShape.DisplayTextPositionProperty, binding);

        }

        public void UnbindTextDisplayProperties(IconShape icon)
        {
            icon.UniqueTextProperties = true;
            BindingOperations.ClearBinding(icon, ShowNameProperty);
            BindingOperations.ClearBinding(icon, ShowLabelsProperty);
            BindingOperations.ClearBinding(icon, FontSizeProperty);
            BindingOperations.ClearBinding(icon, DisplayTextDistanceProperty);
            BindingOperations.ClearBinding(icon, DisplayTextPositionProperty);
        }

        public void Remove(IconShape s)
        {
            try
            {
                shapes.Remove(s);
                gd.Children.Remove(s);
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public void RemoveAt(int index)
        {
            try
            {
                Remove(shapes[index]);
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public int IndexOf(IconShape s)
        {
            return shapes.IndexOf(s);
        }

        public Boolean Contains(IconShape s)
        {
            return shapes.Contains(s);
        }

        public void Clear()
        {
            foreach (var s in shapes)
                gd.Children.Remove(s);
            shapes.Clear();
        }
        #endregion

        #region IEnumerable<FrameworkElement> Members

        public IEnumerator<IconShape> GetEnumerator()
        {
            return (shapes as IEnumerable<IconShape>).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (shapes as IEnumerable<IconShape>).GetEnumerator();
        }

        #endregion
    }
}