using System;
using System.Collections.Generic;
using System.Windows;
using GraphSynth.Representation;
using GraphSynth.Search;

namespace GraphSynth.UserRandLindChoose
{
    public class UserChooseProcess : SearchProcess
    {
        public UserChooseProcess()
        {
            RequireSeed = true;
            RequiredNumRuleSets = 1;
            AutoPlay = true;
        }

        public override string text
        {
            get { return "Recognize-->Human Choose-->Apply"; }
        }

        protected override void Run()
        {
            var display = (MessageBoxResult.Yes == MessageBox.Show("Would you like to see the graph " +
                                                                   "after each rule application?",
                                                                   "Display Interim Graphs",
                                                                   MessageBoxButton.YesNo, MessageBoxImage.Information,
                                                                   MessageBoxResult.No));
            var userChoose = new UserChooseRCA(seedGraph, rulesets, settings, display);
            //userChoose.InParallel = false; // this is left here to help with debugging
            var cand = userChoose.GenerateOneCandidate();
            SearchIO.addAndShowGraphWindow(cand.graph, "After Rule Application");
            SaveResultDialog.Show(settings.filer, cand);
        }
    }

    public class UserChooseRCA : RecognizeChooseApply
    {
        private readonly GlobalSettings settings;

        public override int[] choose(List<option> options, candidate cand)
        {
            option.AssignOptionConfluence(options, cand, ConfluenceAnalysis.Full);
            SearchIO.output("There are " + options.Count + " recognized locations.", 2);
            if (options.Count == 0)
            {
                SearchIO.output("Sorry there are no rules recognized.");
                return null;
            }
            if (options.Count > settings.MaxRulesToDisplay)
            {
                SearchIO.output("Sorry there are too many rules to show.");
                return null;
            }
            SearchIO.output("Double-click on one to show the location.", 2);
            return UserChooseWindow.PromptUser(options, settings, (cand.recipe.Count == 0));
        }

        public override double[] choose(option opt, candidate cand)
        {
            return null;
        }

        #region Constructors

        public UserChooseRCA(designGraph seed, ruleSet[] rulesets, GlobalSettings settings, Boolean display)
            : base(seed, rulesets, null, display
                )
        {
            this.settings = settings;
        }

        #endregion
    }
}