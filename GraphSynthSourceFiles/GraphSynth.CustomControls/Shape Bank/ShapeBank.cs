using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;

namespace GraphSynth.GraphDisplay
{
    public class ShapeBank : DependencyObject, IEnumerable<FrameworkElement>
    {
        #region Fields & Properties

        public static readonly DependencyProperty OpacityProperty
            = DependencyProperty.Register("Opacity",
                                          typeof(double), typeof(ShapeBank),
                                          new FrameworkPropertyMetadata(1.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        protected GraphGUI gd;
        protected List<FrameworkElement> shapes = new List<FrameworkElement>();


        public double Opacity
        {
            get { return (double)GetValue(OpacityProperty); }
            set { SetValue(OpacityProperty, value); }
        }

        #endregion

        #region Constructor

        public ShapeBank(GraphGUI gd)
        {
            this.gd = gd;
            var binding = new Binding
                              {
                                  Source = gd,
                                  Mode = BindingMode.OneWay,
                                  Path = new PropertyPath(OpacityProperty)
                              };
            BindingOperations.SetBinding(this, OpacityProperty, binding);

        }

        #endregion

        #region Iterator & Count
        public int Count
        {
            get { return shapes.Count; }
        }

        public FrameworkElement this[int index]
        {
            get { return shapes[index]; }
            set { shapes[index] = value; }
        }

        #endregion

        #region Methods

        public virtual void Add(FrameworkElement s)
        {
            try
            {
                var binding = new Binding
                {
                    Source = this,
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath(OpacityProperty)
                };
                s.SetBinding(UIElement.OpacityProperty, binding);

                shapes.Add(s);
                gd.Children.Add(s);
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public void Remove(FrameworkElement s)
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

        public int IndexOf(FrameworkElement s)
        {
            return shapes.IndexOf(s);
        }

        public Boolean Contains(FrameworkElement s)
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

        public IEnumerator<FrameworkElement> GetEnumerator()
        {
            return (shapes as IEnumerable<FrameworkElement>).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (shapes as IEnumerable<FrameworkElement>).GetEnumerator();
        }

        #endregion
    }
}