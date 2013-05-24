using System.Collections.Generic;
using System.IO;
using GraphSynth.Representation;

namespace GraphSynth
{
    public partial class ConsoleFiler : BasicFiler
    {
        protected override List<grammarRule> LoadRulesFromFileNames(string ruleDir, List<string> ruleFileNames,
                                                                    out int numLoaded)
        {
            var rules = new List<grammarRule>();
            numLoaded = 0;
            while (numLoaded < ruleFileNames.Count)
            {
                var rulePath = ruleDir + ruleFileNames[numLoaded];
                if (File.Exists(rulePath))
                {
                    SearchIO.output("Loading " + ruleFileNames[numLoaded]);
                    object ruleObj = OpenRuleAndCanvas(rulePath);
                    if (ruleObj is grammarRule)
                        rules.Add((grammarRule)ruleObj);
                    else if (ruleObj is object[])
                        foreach (object o in (object[])ruleObj)
                            if (o is grammarRule)
                                rules.Add((grammarRule)o);
                    numLoaded++;
                }
                else
                {
                    SearchIO.output("Rule Not Found: " + ruleFileNames[numLoaded]);
                    ruleFileNames.RemoveAt(numLoaded);
                }
            }
            return rules;
        }

        public override void ReloadSpecificRule(ruleSet rs, int i)
        {
            lock (fileTransfer)
            {
                var rulePath = rs.rulesDir + rs.ruleFileNames[i];
                SearchIO.output("Loading " + rs.ruleFileNames[i]);
                object ruleObj = Open(rulePath);
                if (ruleObj is grammarRule)
                    rs.rules[i] = (grammarRule)ruleObj;
                else if (ruleObj is object[] &&
                         ((object[])ruleObj)[0] is grammarRule)
                    rs.rules[i] = ((grammarRule)((object[])ruleObj)[0]);

            }
        }
    }
}