/*************************************************************************
 *     This RandomChooseRCA file & class is part of the GraphSynth.
 *     BaseClasses Project which is the foundation of the GraphSynth Ap-
 *     plication. GraphSynth.BaseClasses is protected and copyright under 
 *     the MIT License.
 *     Copyright (c) 2011 Matthew Ira Campbell, PhD.
 *
 *     Permission is hereby granted, free of charge, to any person obtain-
 *     ing a copy of this software and associated documentation files 
 *     (the "Software"), to deal in the Software without restriction, incl-
 *     uding without limitation the rights to use, copy, modify, merge, 
 *     publish, distribute, sublicense, and/or sell copies of the Software, 
 *     and to permit persons to whom the Software is furnished to do so, 
 *     subject to the following conditions:
 *     
 *     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 *     EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
 *     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGE-
 *     MENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
 *     FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 *     CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
 *     WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *     
 *     Please find further details and contact information on GraphSynth
 *     at http://www.GraphSynth.com.
 *************************************************************************/
using System;
using System.Collections.Generic;
using GraphSynth.Representation;

namespace GraphSynth.Search
{
    /// <summary>
    /// An overload for the RCA class that randomly chooses options. 
    /// 
    /// </summary>
    public class RandomChooseRCA : RecognizeChooseApply
    {
        /// <summary>
        /// a random number generator to be used in choose.
        /// </summary>
        protected Random rnd = new Random();

        /// <summary>
        /// Chooses the specified options. Given the list of options and the candidate,
        /// determine what option to invoke. Return the integer index of this option from the list.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cand">The cand.</param>
        /// <returns></returns>
        public override int[] choose(List<option> options, candidate cand)
        {
            return new[] { rnd.Next(options.Count) };
        }


        /// <summary>
        /// Chooses the specified option. Given that the rule has now been chosen, determine
        /// the values needed by the rule to properly apply it to the candidate, cand. The
        /// array of double is to be determined by parametric apply rules written in
        /// complement C# files for the ruleSet being used.
        /// </summary>
        /// <param name="opt">The opt.</param>
        /// <param name="cand">The cand.</param>
        /// <returns></returns>
        public override double[] choose(option opt, candidate cand)
        {
            return null;
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomChooseRCA"/> class.
        /// </summary>
        /// <param name="seed">The seed.</param>
        /// <param name="rulesets">The rulesets.</param>
        /// <param name="numCalls">The num calls.</param>
        /// <param name="display">if set to <c>true</c> [display].</param>
        public RandomChooseRCA(designGraph seed, ruleSet[] rulesets,
                               int[] numCalls,
                               Boolean display)
            : base(seed, rulesets, numCalls, display)
        {
        }

        #endregion
    }
}
