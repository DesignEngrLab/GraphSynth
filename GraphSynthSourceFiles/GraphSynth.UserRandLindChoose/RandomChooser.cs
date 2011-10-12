﻿using System;
using System.Collections.Generic;
using GraphSynth.Representation;
using GraphSynth.Search;

namespace GraphSynth.UserRandLindChoose
{
    public class RandomChooseProcess : SearchProcess
    {
        public static int[] numOfCalls;
        public Boolean Cancel;
        public Boolean display;

        public RandomChooseProcess()
            : base()
        {
            RequireSeed = true;
            RequiredNumRuleSets = 1;
            AutoPlay = true;
        }
    
        public override string text
        {
            get { return "Recognize-->Random Choose-->Apply"; }
        }
        
        protected override void Run()
        {
            var setupWin = new RandomStartDialog(this);
            setupWin.ShowDialog();
            if (Cancel) return;
            var userChoose = new RandomChooseRCA(seedGraph, rulesets, GlobalSettings.ExecDir, settings.CompiledRuleFunctions,
                                                 numOfCalls,
                                                 display, settings.RecompileRuleConditions);
            var cand = userChoose.GenerateOneCandidate();
            SearchIO.addAndShowGraphWindow(cand.graph, "After Rule Application");
        }
    }
}