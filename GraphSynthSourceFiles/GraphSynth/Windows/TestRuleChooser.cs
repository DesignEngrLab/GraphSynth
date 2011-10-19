using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GraphSynth.Representation;
using GraphSynth.Search;

namespace GraphSynth.UI
{
    public class TestRuleChooser : RecognizeChooseApply
    {
        private Boolean firstPass = true;
        private int maxConfluence;
        private int numOptions;
        private int numberWithConfluence;
        protected Random rnd = new Random();
        private int withMaxConfluence;

        public override int[] choose(List<option> options, candidate cand)
        {
            var continueTesting = false;
            if (firstPass)
            {
                firstPass = false;
                if (options.Count == 0)
                    MessageBox.Show("There were no recognized options. ", "Test Rule Status",
                                        MessageBoxButton.OK, MessageBoxImage.Asterisk);
                else continueTesting = true;
            }
            else
            {
                var status = "There ";
                switch (numOptions)
                {
                    case 0:
                        status += "were no recognized options.";
                        break;
                    case 1:
                        status += "was only one recognized option and it applied as shown.\n";
                        break;
                    default:
                        status += "were " + numOptions + " recognized locations.\n";
                        if (numberWithConfluence > 0)
                        {
                            status += "Confluence existed between " + numberWithConfluence + " of them";
                            if (maxConfluence > 1)
                                status += "; \nwith " + withMaxConfluence + " options having a confluence with "
                                          + maxConfluence + " other options.\n";
                            else status += ".\n";
                        }
                        status += "Option #" + choice[0] + " was randomly chosen, and invoked.\n";
                        break;
                }
                switch (options.Count)
                {
                    case 0:
                        status += "The rule is not recognized on the new graph.";
                        MessageBox.Show(status, "Test Rule Status", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        break;
                    case 1:
                        status += "There is one recognized location. Would you like to invoke it?";
                        continueTesting = (MessageBox.Show(status, "Continue Applying?", MessageBoxButton.YesNo,
                                                           MessageBoxImage.Asterisk, MessageBoxResult.No) ==
                                           MessageBoxResult.Yes);
                        break;
                    default:
                        status += "There are " + options.Count + " recognized locations. Would you "
                                  + "like to randomly invoke one?";
                        continueTesting = (MessageBox.Show(status, "Continue Applying?", MessageBoxButton.YesNo,
                                                           MessageBoxImage.Asterisk, MessageBoxResult.No) ==
                                           MessageBoxResult.Yes);
                        break;
                }
            }
            if (!continueTesting) return new[] { -2 };
            choice = new[] { rnd.Next(options.Count) };
            numOptions = options.Count;
            AssignOptionConfluence(options, cand);
            numberWithConfluence = options.Count(o => (o.confluence.Count > 0));
            maxConfluence = options.Max(o => o.confluence.Count);
            withMaxConfluence = options.Count(o => (o.confluence.Count == maxConfluence));
            return choice;
        }

        public override double[] choose(option opt, candidate cand)
        {
            return null;
        }

        #region Constructors

        public TestRuleChooser(designGraph seed, ruleSet[] rulesets)
            : base(seed, rulesets, null, true)
        {
        }

        public static void Run(designGraph seed, grammarRule rule)
        {
            try
            {
                if (!BasicFiler.checkRule(rule)) return;
                var rs = new ruleSet
                             {
                                 generationAfterChoice = nextGenerationSteps.Stop,
                                 generationAfterNoRules = nextGenerationSteps.Loop,
                                 generationAfterNormal = nextGenerationSteps.Loop
                             };
                rs.rules.Add(rule);
                var trc = new TestRuleChooser(seed, new[] { rs });
                //trc.InParallel = false; //this is left to help with some debugging.
                trc.RecognizeChooseApplyCycle(new candidate(seed.copy(), 1), 0, new[] { -1 });
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        #endregion
    }
}