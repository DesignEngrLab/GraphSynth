using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using GraphSynth.Representation;
using GraphSynth.Search;

namespace GraphSynth.UI
{
    public class TestRuleChooser
    {
        public static void Run(designGraph seed, grammarRule rule, Relaxation RelaxationTemplate = null)
        {
            try
            {
                if (RelaxationTemplate == null) RelaxationTemplate = new Relaxation(0);
                var rnd = new Random();
                int k = 0;
                var continueTesting = true;

                SearchIO.output("begin recognizing rule: " + rule.name + "on graph :" + seed.name, 2);
                var dummyRS = new ruleSet();
                dummyRS.Add(rule);
                if (SearchIO.GetTerminateRequest(Thread.CurrentThread.Name)) return;
                var options = dummyRS.recognize(seed, true, RelaxationTemplate.copy());
                if (SearchIO.GetTerminateRequest(Thread.CurrentThread.Name)) return;
                var numOptions = options.Count;
                if (numOptions == 0)
                {
                    if (MessageBox.Show("There were no recognized options. Should the rule be relaxed?", "Test Rule Status",
                                      MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.No) ==
                                                 MessageBoxResult.Yes)
                        Run(seed, rule, new Relaxation(RelaxationTemplate.NumberAllowable + 1));
                    return;
                }
                do
                {
                    var status = "There ";
                    int choice = -1;
                    switch (numOptions)
                    {
                        case 0: throw new Exception("Should not be able to reach here. (Test Rule Chooser, zero options.)");
                        case 1:
                            status += "was only one recognized option and it applied as shown.\n";
                            choice = 0;
                            status += options[choice].Relaxations.RelaxationSummary;
                            break;
                        default:
                            status += "were " + numOptions + " recognized locations.\n";
                            choice = rnd.Next(options.Count);
                            status += options[choice].Relaxations.RelaxationSummary;
                            option.AssignOptionConfluence(options, new candidate(seed, 0), ConfluenceAnalysis.Full);
                            var numberWithConfluence = options.Count(o => (o.confluence.Count > 0));
                            var maxConfluence = options.Max(o => o.confluence.Count);
                            var withMaxConfluence = options.Count(o => (o.confluence.Count == maxConfluence));
                            if (numberWithConfluence > 0)
                            {
                                status += "Confluence existed between " + numberWithConfluence + " of them";
                                if (maxConfluence > 1)
                                    status += "; \nwith " + withMaxConfluence + " options having a confluence with "
                                              + maxConfluence + " other options.\n";
                                else status += ".\n";
                            }
                            status += "Option #" + choice + " was randomly chosen, and invoked.\n";
                            break;
                    }
                    if (!continueTesting) continue;
                    if (SearchIO.GetTerminateRequest(Thread.CurrentThread.Name)) return;
                    var seedCopy = seed.copy();
                    options[choice].apply(seedCopy, null);
                    SearchIO.output("Rule sucessfully applied", 4);
                    SearchIO.addAndShowGraphWindow(seedCopy, "After calling " + ++k + " rules");
                    if (SearchIO.GetTerminateRequest(Thread.CurrentThread.Name)) return;
                    options = dummyRS.recognize(seedCopy, true, RelaxationTemplate.copy());
                    if (SearchIO.GetTerminateRequest(Thread.CurrentThread.Name)) return;
                    numOptions = options.Count;
                    switch (numOptions)
                    {
                        case 0:
                            status += "There are no recognized locations on the new graph";
                            continueTesting = false;
                            MessageBox.Show(status, "Test Rule Status", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                            break;
                        case 1:
                            status += "There is one recognized location on the new graph. Would you like to invoke it?";
                            continueTesting = (MessageBox.Show(status, "Continue Applying?", MessageBoxButton.YesNo,
                                                               MessageBoxImage.Asterisk, MessageBoxResult.No) ==
                                               MessageBoxResult.Yes);
                            break;
                        default:
                            status += "There are " + options.Count + " recognized locations on the new graph. Would you "
                                      + "like to randomly invoke one?";
                            continueTesting = (MessageBox.Show(status, "Continue Applying?", MessageBoxButton.YesNo,
                                                               MessageBoxImage.Asterisk, MessageBoxResult.No) ==
                                               MessageBoxResult.Yes);
                            break;
                    }
                } while (continueTesting);
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }
    }
}