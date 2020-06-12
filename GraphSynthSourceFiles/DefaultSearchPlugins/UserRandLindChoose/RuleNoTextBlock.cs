using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using GraphSynth.Representation;

namespace GraphSynth.UserRandLindChoose
{
    public class RuleNoTextBlock : TextBlock
    {
        #region Fields

        private option _opt;
        private GlobalSettings _settings;

        #endregion

        public void SetTextAndLink(option opt, GlobalSettings settings)
        {
            _opt = opt;
            _settings = settings;
            Text = opt.ruleNumber.ToString(CultureInfo.InvariantCulture);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                var rs = _settings.rulesets[_opt.ruleSetIndex];
                object[] tempRuleObj = _settings.filer.Open(
                    rs.rulesDir + rs.ruleFileNames[_opt.ruleNumber - 1]);
                SearchIO.addAndShowRuleWindow(tempRuleObj,
                                              "Rule for Option " + _opt.optionNumber + " from RuleSet " +
                                              _opt.ruleSetIndex
                                              + " Rule #" + _opt.ruleNumber);
            }
            base.OnPreviewMouseDown(e);
        }
    }
}