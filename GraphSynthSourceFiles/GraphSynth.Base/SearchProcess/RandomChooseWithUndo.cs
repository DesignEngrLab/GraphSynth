/*************************************************************************
 *     This RandomChooseWithUndoRCA file & class is part of the GraphSynth.
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
    /// An overload for the RCA class that randomly chooses options. In this 
    /// version, it can also randomly choose to undo the last option (by choosing
    /// -1).
    /// </summary>
    public class RandomChooseWithUndoRCA : RecognizeChooseApply
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
            return new[] { rnd.Next(-1, options.Count) };
        }

        #region Constructors

        /* a constructor like these are needed to invoke the main constructor in RecognizeChooseApply.cs */

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomChooseWithUndoRCA"/> class.
        /// </summary>
        /// <param name="_seed">The _seed.</param>
        /// <param name="_rulesets">The _rulesets.</param>
        /// <param name="_maxNumOfCalls">The _max num of calls.</param>
        /// <param name="_display">if set to <c>true</c> [_display].</param>
        public RandomChooseWithUndoRCA(designGraph _seed, ruleSet[] _rulesets,
                                    int[] _maxNumOfCalls = null, Boolean _display = false)
            : base(_seed, _rulesets, _maxNumOfCalls, _display)
        {
        }

        #endregion
    }
}