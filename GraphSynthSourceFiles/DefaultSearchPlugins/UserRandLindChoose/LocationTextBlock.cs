using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using GraphSynth.Representation;

namespace GraphSynth.UserRandLindChoose
{
    public class LocationTextBlock : TextBlock
    {
        #region Fields

        private option _opt;
        public string StrLocation;

        #endregion

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
                SearchIO.addAndShowGraphWindow(_opt.copy(),
                                               "Location for Option " + _opt.optionNumber + " from RuleSet " +
                                               _opt.ruleSetIndex
                                               + " Rule #" + _opt.ruleNumber + ": " + StrLocation);
            base.OnPreviewMouseDown(e);
        }

        internal void SetTextAndLink(option opt)
        {
            _opt = opt;
            var remLocNames = new List<string>();
            var remRuleNames = new List<string>();
            var LStr = "";
            var KStr = "";
            var RStr = "";

            for (var i = 0; i < opt.nodes.Count; i++)
            {
                var elt = opt.nodes[i];
                if (elt == null) continue;
                remLocNames.Add("n:" + elt.name);
                remRuleNames.Add("n:" + opt.rule.L.nodes[i].name);
            }
            for (var i = 0; i < opt.arcs.Count; i++)
            {
                var elt = opt.arcs[i];
                if (elt == null) continue;
                remLocNames.Add("a:" + elt.name);
                remRuleNames.Add("a:" + opt.rule.L.arcs[i].name);
            }
            for (var i = 0; i < opt.hyperarcs.Count; i++)
            {
                var elt = opt.hyperarcs[i];
                if (elt == null) continue;
                remLocNames.Add("h:" + elt.name);
                remRuleNames.Add("h:" + opt.rule.L.hyperarcs[i].name);
            }
            foreach (node n in opt.rule.R.nodes)
            {
                var temp = "n:" + n.name;
                if (remRuleNames.Contains(temp))
                {
                    var i = remRuleNames.FindIndex(s => s.Equals(temp));
                    KStr += " " + remLocNames[i] + ",";
                    remLocNames.RemoveAt(i);
                    remRuleNames.RemoveAt(i);
                }
                else RStr += " " + temp + ",";
            }
            foreach (arc a in opt.rule.R.arcs)
            {
                var temp = "a:" + a.name;
                if (remRuleNames.Contains(temp))
                {
                    var i = remRuleNames.FindIndex(s => s.Equals(temp));
                    KStr += " " + remLocNames[i] + ",";
                    remLocNames.RemoveAt(i);
                    remRuleNames.RemoveAt(i);
                }
                else RStr += " " + temp + ",";
            }
            foreach (hyperarc h in opt.rule.R.hyperarcs)
            {
                var temp = "h:" + h.name;
                if (remRuleNames.Contains(temp))
                {
                    var i = remRuleNames.FindIndex(s => s.Equals(temp));
                    KStr += " " + remLocNames[i] + ",";
                    remLocNames.RemoveAt(i);
                    remRuleNames.RemoveAt(i);
                }
                else RStr += " " + temp + ",";
            }
            foreach (string s in remLocNames)
                LStr += " " + s + ",";

            if (opt.rule.L.globalLabels.Count > 0)
            {
                if (opt.rule.OrderedGlobalLabels)
                    LStr += " gl(" + opt.globalLabelStartLoc + "):";
                else
                    LStr += " gl:";
                foreach (string g in opt.rule.L.globalLabels)
                    LStr += g + ",";
            }

            foreach (string g in opt.rule.R.globalLabels)
            {
                var temp = " gl:" + g + ",";
                if (LStr.Contains(temp))
                {
                    LStr = LStr.Replace(temp, "");
                    KStr += temp;
                }
                else RStr += temp;
            }
            LStr = LStr.Length == 0 ? " " : LStr.Remove(LStr.Length - 1);
            KStr = KStr.Length == 0 ? " " : KStr.Remove(KStr.Length - 1);
            RStr = RStr.Length == 0 ? " " : RStr.Remove(RStr.Length - 1);

            StrLocation = "<" + LStr + " [" + KStr + " >" + RStr + " ]";
            Inlines.Add(new Bold(new Run("<")));
            Inlines.Add(new Run(LStr));
            Inlines.Add(new Bold(new Run(" [")));
            Inlines.Add(new Run(KStr));
            Inlines.Add(new Bold(new Run(" >")));
            Inlines.Add(new Run(RStr));
            Inlines.Add(new Bold(new Run(" ]")));
        }
    }
}