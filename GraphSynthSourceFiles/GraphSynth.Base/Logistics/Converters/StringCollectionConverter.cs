/*************************************************************************
 *     This StringCollectionConverter file & interface is part of the 
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
    ///   Used to convert a single string into a list of strings and vice-versa.
    /// </summary>
    public class StringCollectionConverter
    {    
        /// <summary>
        ///   Converts the comma-separated-values into a IEnumerable of strings.
        /// </summary>
        /// <param name = "value">The value.</param>
        /// <returns></returns>
        public static List<string> Convert(string value)
        {
            var items = new List<string>();
            var charSeparators = new[] { ',', '(', ')', ' ', ';', '\'', '\"', '*', '?', '<', '>', '|' };

            var results = value.Split(charSeparators);

            for (var i = 0; i < results.GetLength(0); i++)
            {
                if (results[i] != "")
                    items.Add(results[i].Trim());
            }
            return items;
        }

        /// <summary>
        ///   Converts the specified IEnumerable of strings into a comma separated single string.
        /// </summary>
        /// <param name = "values">The values.</param>
        /// <returns></returns>
        public static string Convert(IEnumerable<string> values)
        {
            var text = "";
         
            foreach (var value in values)
            {
                text += ", " + value;
            }
            return text.Length < 2 ? text : text.Remove(0, 2);
        }
    }
}