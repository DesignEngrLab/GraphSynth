using System;
using System.Data;
using System.Globalization;
using System.Windows.Data;
using GraphSynth.GraphDisplay;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    internal class ComboBoxSelectItemsConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var gd = value as GraphGUI;
            var dt = new DataTable();
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("Shape", typeof(object));

            var dr = dt.NewRow();
            dr["name"] = "Select a single node/arc";
            dr["Shape"] = null;
            dt.Rows.Add(dr);
            foreach (hyperarc h in gd.graph.hyperarcs)
            {
                dr = dt.NewRow();
                dr["name"] = h.name + "\t\t\t(hyperarc)";
                dr["Shape"] = h.DisplayShape.Shape;
                dt.Rows.Add(dr);
            }
            foreach (node n in gd.graph.nodes)
            {
                dr = dt.NewRow();
                dr["name"] = n.name + "\t\t\t(node)";
                dr["Shape"] = n.DisplayShape.Shape;
                dt.Rows.Add(dr);
            }
            foreach (arc a in gd.graph.arcs)
            {
                dr = dt.NewRow();
                dr["name"] = a.name + "\t\t\t(arc)";
                dr["Shape"] = a.DisplayShape.Shape;
                dt.Rows.Add(dr);
            }
            return dt;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This operation is Invalid - SelectionCombo.cs");
        }

        #endregion
    }
}