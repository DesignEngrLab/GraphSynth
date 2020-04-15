/*************************************************************************
 *     This SearchIOToTextWriter file & class is part of the GraphSynth.
 *     BaseClasses Project which is the foundation of the GraphSynth 
 *     Application.
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

using System.IO;
using System.Text;
using System.Threading;

namespace GraphSynth
{
#if WPF
    /// <summary>
    ///   this class simply helps SearchIO get its output to the sidebar in GraphSynth.
    ///   It does this by inheriting from the TextWriter class. Borrowed from ? on CodeProject.
    /// </summary>
    public class SearchIOToTextWriter : TextWriter
    {
        /********************* .NET 4.0 (i.e. WPF) Specific Reference *********************/

        /// <summary>
        ///   The output box where text is presented to the user.
        /// </summary>
        public System.Windows.Controls.TextBox outputBox;

        /*********************************************************************************/

        /// <summary>
        ///   When overridden in a derived class, returns the <see cref = "T:System.Text.Encoding" /> in which the output is written.
        /// </summary>
        /// <value></value>
        /// <returns>The Encoding in which the output is written.</returns>
        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        /// <summary>
        ///   Writes the line.
        /// </summary>
        /// <param name = "str">The STR.</param>
        public override void WriteLine(string str)
        {
            /********************* .NET 4.0 (i.e. WPF) Specific Reference *********************/
            if (outputBox.Dispatcher.CheckAccess())
            {
                outputBox.AppendText(Thread.CurrentThread.Name + "•" + str + "\n");
                outputBox.ScrollToEnd();
            }
            else
            {
                outputBox.Dispatcher.Invoke(
                    (ThreadStart)delegate
                    {
                        /*********************************************************************************/
                        outputBox.AppendText(Thread.CurrentThread.Name + "•" + str + "\n");
                        outputBox.ScrollToEnd();
                    }
                    );
            }
        }
    }
#endif
}
