using System;
using System.Collections.Generic;
using GraphSynth.Representation;
using GraphSynth.Search;

namespace GraphSynth.UserRandLindChoose
{
    public class LindenmayerChooseProcess : SearchProcess
    {
        public static int[] numOfCalls;
        public Boolean Cancel;
        public Boolean display;

        public LindenmayerChooseProcess()
            : base()
        {
            RequireSeed = true;
            RequiredNumRuleSets = 1;
            AutoPlay = true;
        }


        public override string text
        {
            get { return "Recognize-->Lindenmayer Choose-->Apply"; }
        }

        protected override void Run()
        {
            var setupWin = new LindenmayerStartDialog(this);
            setupWin.ShowDialog();
            if (Cancel) return;
            var userChoose = new LindenmayerChooseRCA(seedGraph, rulesets, numOfCalls, display);
            var cand = userChoose.GenerateOneCandidate();
            SearchIO.addAndShowGraphWindow(cand.graph, "After Rule Application");
        }
    }
}

