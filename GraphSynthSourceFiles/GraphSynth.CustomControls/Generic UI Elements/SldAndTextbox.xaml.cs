using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for SldAndTextbox.xaml
    /// </summary>
    public partial class SldAndTextbox : UserControl
    {
        /// <summary>
        ///   This is used below in the close enough to zero booleans to match points
        ///   (see below: sameCloseZero). In order to avoid strange round-off issues - 
        ///   even with doubles - I have implemented this function when comparing the
        ///   position of points (mostly in checking for a valid transformation (see
        ///   ValidTransformation) and if other nodes comply (see otherNodesComply).
        /// </summary>
        private const double epsilon = 0.00001;

        public static readonly DependencyProperty ValueProperty
            = DependencyProperty.Register("Value",
                                          typeof(double), typeof(SldAndTextbox),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        private Boolean initialized;

        #region Constructor

        public SldAndTextbox()
        {
            InitializeComponent();
            Maximum = 1;
            Minimum = 0;
            LargeChange = 0.1;
            SmallChange = 0.05;
            TickPlacement = TickPlacement.BottomRight;
            TickFrequency = 0.05;

            var txtValueBinding = new Binding();
            txtValueBinding.Source = txtTextBox;
            txtValueBinding.Converter = new TextToDoubleConverter();
            txtValueBinding.Mode = BindingMode.TwoWay;
            txtValueBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            txtValueBinding.Path = new PropertyPath(TextBox.TextProperty);
            SetBinding(ValueProperty, txtValueBinding);
        }

        #endregion

        #region Properties

        IValueConverter _converter;
        public IValueConverter Converter
        {
            private get {return _converter;}
            set
            {
                _converter=value;

                var sldTxtbinding = new Binding();
                sldTxtbinding.Source = sldSlider;
                sldTxtbinding.Converter = _converter;
                sldTxtbinding.Mode = BindingMode.TwoWay;
                sldTxtbinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                sldTxtbinding.Path = new PropertyPath(RangeBase.ValueProperty);
                txtTextBox.SetBinding(TextBox.TextProperty, sldTxtbinding);
            }
        }

        public double Maximum
        {
            get { return sldSlider.Maximum; }
            set { sldSlider.Maximum = value; }
        }

        public double Minimum
        {
            get { return sldSlider.Minimum; }
            set { sldSlider.Minimum = value; }
        }

        public double SmallChange
        {
            get { return sldSlider.SmallChange; }
            set { sldSlider.SmallChange = value; }
        }

        public double LargeChange
        {
            get { return sldSlider.LargeChange; }
            set { sldSlider.LargeChange = value; }
        }

        public TickPlacement TickPlacement
        {
            get { return sldSlider.TickPlacement; }
            set { sldSlider.TickPlacement = value; }
        }

        public double TickFrequency
        {
            get { return sldSlider.TickFrequency; }
            set { sldSlider.TickFrequency = value; }
        }

        public string Label
        {
            get { return (string)nameLabel.Content; }
            set { nameLabel.Content = value; }
        }

        #endregion

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            private set
            {
                if (Value == value) return;
                SetValue(ValueProperty, value);
            }
        }

        public void UpdateValue(double v)
        {
            if (double.IsNaN(v)) AppearToDisable();
            else AppearToEnable();

            if ((!double.IsNaN(v)) && !sameCloseZero(Value, v))
            {
                Value = v;
            }
        }

        private static Boolean sameCloseZero(double x1)
        {
            return Math.Abs(x1) < epsilon;
        }

        private Boolean sameCloseZero(double x1, double x2)
        {
            return sameCloseZero(x1 - x2);
        }

        private void txtTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            txtTextBoxInput(sender, null);
        }

        private void txtTextBoxInput(object sender, TextCompositionEventArgs e)
        {
            double dummy;
            if (double.TryParse(txtTextBox.Text, out dummy))
                RaiseValueChangedEvent();
        }

        private void sldSlider_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var fraction = e.GetPosition(sldSlider).X / sldSlider.ActualWidth;
            sldSlider.Value = fraction * (Maximum - Minimum) + Minimum;
            RaiseValueChangedEvent();
        }

        private void sldSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (initialized)
            {

                Value = (double)Converter.Convert(sldSlider.Value, null, null, null);
                     /* i realize that this line is not needed, but the binding
                                          * does not respond quick enough since the slider updates 
                                          * the textbox and the textbox updates the Value. Here we
                                          * "cut out the middle man" and update Value based on slider
                                          * this is especially important for quick/big jumps in the
                                          * slider. */
                
                RaiseValueChangedEvent();
            }
            else initialized = true;
        }

        #region For Multiple Different Selections

        private void clickToSetLabel_Click(object sender, MouseButtonEventArgs e)
        {
            AppearToEnable();
        }

        private void AppearToEnable()
        {
            clickToSetLabel.Visibility = Visibility.Hidden;
            txtTextBox.Foreground = Brushes.Black;
        }

        private void AppearToDisable()
        {
            txtTextBox.Foreground = Brushes.Transparent;
            clickToSetLabel.Visibility = Visibility.Visible;
        }

        #endregion

        #region Event Handling

        //A RoutedEvent using standard RoutedEventArgs, event declaration
        //The actual event routing
        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble,
                                             typeof(RoutedEventHandler), typeof(SldAndTextbox));

        // Provides accessors for the event
        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        // This method raises the valueChanged event
        private void RaiseValueChangedEvent()
        {
            var newEventArgs = new RoutedEventArgs(ValueChangedEvent);
            RaiseEvent(newEventArgs);
        }

        //************************************************************************

        #endregion

    }
}