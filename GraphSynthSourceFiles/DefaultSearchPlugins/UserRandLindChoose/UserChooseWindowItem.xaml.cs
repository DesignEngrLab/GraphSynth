using System;
using System.Globalization;
using System.Windows.Controls;
using GraphSynth.Representation;

namespace GraphSynth.UserRandLindChoose
{
    /// <summary>
    ///   Interaction logic for UserChooseWindowItem.xaml
    /// </summary>
    public partial class UserChooseWindowItem : ListBoxItem
    {
        public UserChooseWindowItem(option opt, GlobalSettings settings)
        {
            InitializeComponent();

            txtBOptionString.Text = opt.optionNumber.ToString(CultureInfo.InvariantCulture);
            txTBLocation.SetTextAndLink(opt);
            txtBRuleNo.SetTextAndLink(opt, settings);
            txtBConfluenceString.Text = IntCollectionConverter.Convert(opt.confluence);
        }

        public Boolean IsConfluent { get; set; }

        public override string ToString()
        {
            return "Option: " + txtBOptionString.Text + "; Rule: " + txtBRuleNo.Text
                   + "; Location: " + txTBLocation.StrLocation;
        }
    }
}