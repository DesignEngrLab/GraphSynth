using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GraphSynth.GraphDisplay;

namespace GraphSynth.UI
{
    public class NodeArcComboBoxSelector : ComboBox
    {
        private BindingExpression be;
        private GraphGUI g;

        public NodeArcComboBoxSelector()
        {
            IsEditable = false;
            SelectionChanged += NodeArcComboBoxSelector_SelectionChanged;
        }

        public GraphGUI graphGUI
        {
            private get { return g; }
            set
            {
                if (g != value)
                {
                    g = value;
                    BindSelectCombo();
                    be = BindingOperations.GetBindingExpression(this, ItemsSourceProperty);
                }
                be.UpdateTarget();
                SelectedIndex = 0;
            }
        }

        private void BindSelectCombo()
        {
            BindingOperations.ClearBinding(this, ItemsSourceProperty);
            var ItemsBinding = new Binding();
            ItemsBinding.Source = graphGUI;
            ItemsBinding.Mode = BindingMode.OneWay;
            ItemsBinding.Converter = new ComboBoxSelectItemsConverter();
            SetBinding(ItemsSourceProperty, ItemsBinding);
            DisplayMemberPath = "name";
        }

        private void NodeArcComboBoxSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (graphGUI != null)
            {
                var elements = new List<UIElement>();
                var sItem = SelectedItem as DataRowView;
                if (SelectedIndex > 0)
                {
                    elements.Add(sItem["Shape"] as UIElement);
                    graphGUI.Select(elements);
                }
            }
        }
    }
}