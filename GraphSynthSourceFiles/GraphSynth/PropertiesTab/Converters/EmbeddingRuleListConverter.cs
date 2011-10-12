using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows.Data;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    internal class EmbeddingRuleListConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var eRules = (List<embeddingRule>)value;
            var dt = new DataTable();
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("rule", typeof(object));

            foreach (embeddingRule e in eRules)
            {
                var name = "<";
                foreach (string a in e.freeArcLabels)
                    name += a + ",";
                foreach (string a in e.freeArcNegabels)
                    name += "~" + a + ",";
                //this is to remove the last comma
                name.Remove(name.Length - 1);
                name += "><" + e.LNodeName + "><";
                foreach (string a in e.neighborNodeLabels)
                    name += a + ",";
                foreach (string a in e.neighborNodeNegabels)
                    name += "~" + a + ",";
                //this is to remove the last comma
                name.Remove(name.Length - 1);
                name += "><" + e.originalDirection + ">";
                name += "[" + e.RNodeName + "][" + e.newDirection + "]";
                if (e.allowArcDuplication)
                    name += "(duplicate)";
                var dr = dt.NewRow();
                dr["name"] = name;
                dr["rule"] = e;
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