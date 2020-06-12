/*************************************************************************
 *     This MYIOPath file & class is part of the GraphSynth.BaseClasses 
 *     Project which is the foundation of the GraphSynth Application.
 *     GraphSynth.BaseClasses is protected and copyright under the MIT
 *     License.
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

using System.Globalization;
using System.IO;

namespace GraphSynth
{
    /// <summary>
    ///   A simple static class created within a single function - to create relative paths.
    /// </summary>
    public static class MyIOPath
    {
        private static readonly char DS =System.IO.Path.DirectorySeparatorChar;
        private static readonly string DSStr = DS.ToString(CultureInfo.InvariantCulture);
        /// <summary>
        ///   Gets the relative path to the target path starting at the "with Respect to" directory.
        ///   The method will add the necessary "..\" to get back to the common directory.
        /// </summary>
        /// <param name = "target">The target path.</param>
        /// <param name = "withRespectTo">The "with respect to" directory.</param>
        /// <returns></returns>
        public static string GetRelativePath(string target, string withRespectTo)
        {
            if (string.IsNullOrWhiteSpace(target)) target = "";
            var i = 0;
            var lastSlash = 0;
            if (string.IsNullOrWhiteSpace(withRespectTo)) withRespectTo = DSStr;
            if (!withRespectTo.EndsWith(DSStr)) withRespectTo += DS;
            /* this while loop is used to find the position of the last backslash that the two 
                 * directories have in common. */
            while ((i < target.Length) && (i < withRespectTo.Length) && (target[i].Equals(withRespectTo[i])))
            {
                if (target[i].Equals(DS)) lastSlash = i + 1;
                i++;
            }


            /* using what's left of the WRT path, we can find how many "..\" to add to the beginning of the
             * relative path. This is indicated by numSubDirs. */
            var numSubDirs = withRespectTo.Remove(0, lastSlash).Split(DS).GetLength(0) - 1;

            /* return the relativePath string, which starts as the back part of the substring of target, 
             * prepended with any number of "..\". */
            var relativePath = target.Substring(lastSlash);
            for (i = 0; i < numSubDirs; i++)
                relativePath = ".." + DSStr + relativePath;

            return relativePath;
        }
    }
}