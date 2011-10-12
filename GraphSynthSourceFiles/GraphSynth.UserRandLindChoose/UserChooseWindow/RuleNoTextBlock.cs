using System.Windows.Controls;
using System.Windows.Input;
using GraphSynth.Representation;

namespace GraphSynth.UserRandLindChoose
{
    public class RuleNoTextBlock : TextBlock
    {
        #region Fields

        private option opt;
        private GlobalSettings settings;

        #endregion

        public void SetTextAndLink(option opt, GlobalSettings settings)
        {
            this.opt = opt;
            this.settings = settings;
            Text = opt.ruleNumber.ToString();
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                var rs = (ruleSet)settings.rulesets[opt.ruleSetIndex];
                object[] tempRuleObj = settings.filer.Open(
                    rs.rulesDir + rs.ruleFileNames[opt.ruleNumber - 1]);
                SearchIO.addAndShowRuleWindow(tempRuleObj,
                                              "Rule for Option " + opt.optionNumber + " from RuleSet " +
                                              opt.ruleSetIndex
                                              + " Rule #" + opt.ruleNumber);
            }
            base.OnPreviewMouseDown(e);
        }
    }
}