/*************************************************************************
 *     This IntCollectionConverter file & interface is part of the 
 *     GraphSynth.BaseClasses Project which is the foundation of the 
 *     GraphSynth Application.
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace GraphSynth
{
    /// <summary>
    ///   A converter class for changing a collection of ints into a string and vice-versa.
    /// </summary>
    public class IntCollectionConverter
    {

        /// <summary>
        ///   Converts the for a string of comma-separated-values to a IEnumerable of ints.
        /// </summary>
        /// <param name = "value">The value.</param>
        /// <returns></returns>
        public static List<int> Convert(string value)
        {
            var items = new List<int>();
            var charSeparators = new[] { ',', '(', ')', ' ', ':', ';', '/', '\\', '\'', '\"' };
            int temp;
            var results = value.Split(charSeparators);

            for (var i = 0; i < results.GetLength(0); i++)
                if ((results[i] != "") && (int.TryParse(results[i].Trim(), out temp)))
                    items.Add(temp);

            return items;
        }

        /// <summary>
        ///   Converts the specified values from a IEnumerable of ints to a comma-separated string.
        /// </summary>
        /// <param name = "values">The values.</param>
        /// <returns></returns>
        public static string Convert(IEnumerable<int> values)
        {
            var text = "";
            foreach (var value in values)
            {
                text += ", " + value.ToString(CultureInfo.InvariantCulture);
            }
            return text.Length < 2 ? text : text.Remove(0, 2);
        }
    }
}