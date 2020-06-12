/*************************************************************************
 *     This ErrorLogger file & interface is part of the GraphSynth.BaseClasses 
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
using System;
using System.IO;
using System.Net;

namespace GraphSynth
{
    /// <summary>
    ///   A class that allows us to dump errors to a txt file in the executable directory.
    /// </summary>
    public static class ErrorLogger
    {
        /// <summary>
        ///   The contents of the Error log
        /// </summary>
        public static string ErrorLogFile = "ErrorLog.Temp.Startup.txt";

        private static int counter;

        private static readonly string[] ErrorWelcomeCaptions = new[]
                                                                    {
                                                                        "Error found.", "Whoops!",
                                                                        "What did you do, now?!?",
                                                                        "You're good at this!",
                                                                        "Are you just doing this to annoy me?",
                                                                        "Again with the...errors.", "Having a good day?"
                                                                        ,
                                                                        "Maybe it's time to cut our losses and restart, eh?"
                                                                    };

        /// <summary>
        ///   Catches the specified exception.
        /// </summary>
        /// <param name = "Exc">The exc.</param>
        public static void Catch(Exception Exc)
        {
            try
            {
                if (!File.Exists(ErrorLogFile))
                {
                    var fs = new FileStream(ErrorLogFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    fs.Close();
                }
                var sw = new StreamWriter(ErrorLogFile, true);
                sw.Write(MakeErrorString(Exc, true));
                sw.Flush();
                sw.Close();
                var r = new Random();
                string erCaption = counter == ErrorWelcomeCaptions.GetLength(0) 
                                       ? ErrorWelcomeCaptions[r.Next(ErrorWelcomeCaptions.GetLength(0))] 
                                       : ErrorWelcomeCaptions[counter++];
                SearchIO.MessageBoxShow("Error found in " + Exc.Source +
                                        "\nError contents can be found in " + ErrorLogFile, erCaption, "Error");
            }
            catch (Exception e)
            {
                SearchIO.output("Error in ErrorLogger (how did this happen?!)"
                                + e);
            }
        }


        /// <summary>
        ///   Makes the error string.
        /// </summary>
        /// <param name = "Exc">The exc.</param>
        /// <param name = "includeComputerData">if set to <c>true</c> [include computer data].</param>
        /// <returns></returns>
        public static string MakeErrorString(Exception Exc, Boolean includeComputerData)
        {
            var sw = new StringWriter();
            try
            {
                sw.WriteLine("Source    : " + Exc.Source.Trim());
                sw.WriteLine("Method	: " + Exc.TargetSite.Name);
                if (includeComputerData)
                {
                    sw.WriteLine("Date		: " + DateTime.Now.ToLongTimeString());
                    sw.WriteLine("Time		: " + DateTime.Now.ToShortDateString());
                    sw.WriteLine("Computer	: " + Dns.GetHostName());
                }
                sw.WriteLine("Error		: " + Exc.Message.Trim());
                sw.WriteLine("Stack Trace	: " + Exc.StackTrace.Trim());
                var tabString = "";
                while (Exc.InnerException != null)
                {
                    tabString += "\t";
                    Exc = Exc.InnerException;
                    sw.WriteLine("\n" + tabString + "Inner Exception in : " + Exc.TargetSite.Name);
                    sw.WriteLine(tabString + "Error              : " + Exc.Message.Trim());
                    sw.WriteLine(tabString + "Stack Trace     	: " + Exc.StackTrace.Trim());
                }
                sw.WriteLine("-------------------------------------------------------------------");
            }
            catch (Exception e)
            {
                SearchIO.output("Error in ErrorLogger (how did this happen?!)"
                                + e);
            }
            return sw.ToString();
        }
    }
}