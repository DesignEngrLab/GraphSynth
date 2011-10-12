using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml.Linq;
using Microsoft.Win32;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for TemplatePickerWindow.xaml
    /// </summary>
    public partial class TemplatePickerWindow : Window
    {
        public static readonly DependencyProperty ValueProperty
            = DependencyProperty.Register("Value",
                                          typeof(CanvasProperty), typeof(TemplatePickerWindow),
                                          new FrameworkPropertyMetadata(null,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        private int numTemplates;

        public TemplatePickerWindow()
        {
            /* the following is common to all GS window types. */
            InitializeComponent();
            Owner = GSApp.main;
            ShowInTaskbar = false;
            foreach (CommandBinding cb in GSApp.main.CommandBindings)
                CommandBindings.Add(cb);
            foreach (InputBinding ib in GSApp.main.InputBindings)
                InputBindings.Add(ib);
            /***************************************************/
            ReadInTemplates();
        }

        public CanvasProperty Value
        {
            get { return (CanvasProperty)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        #region Event Handling

        //A RoutedEvent using standard RoutedEventArgs, event declaration
        //The actual event routing
        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble,
                                             typeof(RoutedEventHandler), typeof(TemplatePickerWindow));

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

        private void ReadInTemplates(string selectedkey = null)
        {
            var selectedIndex = -1;
            listBoxOfTemplates.Items.Clear();
            numTemplates = 0;
            foreach (object key in Application.Current.Resources.MergedDictionaries[1].Keys)
            {
                var o = Application.Current.Resources[key];
                if (typeof(DataTemplate).IsInstanceOfType(o))
                {
                    object p = ((DataTemplate)o).LoadContent();
                    if (typeof(CanvasProperty).IsInstanceOfType(p))
                    {
                        var name = key.ToString();
                        if (selectedkey == name) selectedIndex = numTemplates;
                        var content = (++numTemplates) + ". " + name;
                        listBoxOfTemplates.Items.Add(
                            new ListBoxItem
                                {
                                    Tag = name,
                                    Content = content
                                });
                    }
                }
            }
            listBoxOfTemplates.Items.Add(
                new ListBoxItem
                    {
                        Tag = "openTemplate",
                        Content = "0. <open from file>"
                    });
            listBoxOfTemplates.SelectedIndex = selectedIndex;
        }

        private void OpenTemplateFromFile()
        {
            var filename = "";
            var fileChooser = new OpenFileDialog
                                  {
                                      Title = "Open a canvas template stored in another file.",
                                      InitialDirectory = GSApp.settings.WorkingDirAbsolute,
                                      Filter = "GraphSynth files|*.gxml;*.grxml;*.rsxml|All xml files|*.xml;*.xaml"
                                  };
            if ((Boolean)fileChooser.ShowDialog())
                filename = fileChooser.FileName;
            if (filename.Length > 0)
            {
                try
                {
                    var cp = ((WPFFiler)GSApp.settings.filer).LoadCanvasProperty(XElement.Load(filename));
                    string key = null;
                    if (cp != null)
                    {
                        Value = cp;
                        key = "from=>" + Path.GetFileNameWithoutExtension(filename);
                        if (!Application.Current.Resources.MergedDictionaries[1].Contains(key))
                        {
                            var xamlString = "<DataTemplate>"
                                              + CanvasProperty.SerializeCanvasToXml(cp) + "</DataTemplate>";

                            var dt = (DataTemplate)MyXamlHelpers.Parse(xamlString);
                            Application.Current.Resources.MergedDictionaries[1].Add(key, dt);
                        }
                    }
                    //}
                    ReadInTemplates(key);
                }
                catch (Exception exc)
                {
                    ErrorLogger.Catch(exc);
                }
            }
        }

        private void listBoxOfTemplates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxOfTemplates.Items.Count == 0) return;
            var selected = ((ListBoxItem)listBoxOfTemplates.SelectedItem).Tag.ToString();
            if (selected.Equals("openTemplate"))
                OpenTemplateFromFile();
            else
            {
                var dt = (DataTemplate)Application.Current.Resources[selected];
                if (dt != null)
                {
                    Value = (CanvasProperty)dt.LoadContent();
                    RaiseValueChangedEvent();
                }
            }
        }


        private void TemplatePicker_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.D0) || (e.Key == Key.NumPad0))
                listBoxOfTemplates.SelectedIndex = listBoxOfTemplates.Items.Count - 1;
        }

        internal static CanvasProperty ShowWindowDialog()
        {
            var tpWin = new TemplatePickerWindow();
            tpWin.ShowDialog();
            return tpWin.Value;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (Value == null)
                MessageBox.Show("Please select a template.", "No template selected.",
                                MessageBoxButton.OK, MessageBoxImage.Hand);
            else Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Value = null;
            Close();
        }
    }
}