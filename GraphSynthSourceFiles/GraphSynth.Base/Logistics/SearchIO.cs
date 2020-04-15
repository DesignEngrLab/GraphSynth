/*************************************************************************
 *     This SearchIO file & class is part of the GraphSynth.BaseClasses 
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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
#if WPF
using System.Windows;
#endif

namespace GraphSynth
{
    /// <summary>
    ///   The static class that handles input and output statements from a
    ///   Search Process.
    /// </summary>
    public static class SearchIO
    {
        #region Iteration Handling

        private const int defaultIteration = 0;
        private static readonly Dictionary<int, int> iterations = new Dictionary<int, int>();

        /// <summary>
        ///   Gets or sets the iteration.
        /// </summary>
        /// <value>The iteration.</value>
        public static int iteration
        {
            set
            {
                var searchThreadName = Thread.CurrentThread.ManagedThreadId;
                if (iterations.ContainsKey(searchThreadName))
                    iterations[searchThreadName] = value;
                else iterations.Add(searchThreadName, value);
            }
            get
            {
                return getIteration(Thread.CurrentThread.ManagedThreadId);
            }
        }

        /// <summary>
        /// Gets the iteration.
        /// </summary>
        /// <param name="threadID">The thread identifier.</param>
        /// <returns></returns>
        public static int getIteration(int threadID)
        {
            if (iterations.ContainsKey(threadID))
                return (int)iterations[threadID];
            return defaultIteration;
        }

        #endregion

        #region miscObject Handling

        private const string defaultMiscObject = "misc";    
        private static readonly Dictionary<int, object> miscHash = new Dictionary<int, object>();

        /// <summary>
        ///   Gets or sets the misc object.
        /// </summary>
        /// <value>The misc object.</value>
        public static object miscObject
        {
            set
            {
                var searchThreadName = Thread.CurrentThread.ManagedThreadId;
                if (miscHash.ContainsKey(searchThreadName))
                    miscHash[searchThreadName] = value;
                else miscHash.Add(searchThreadName, value);
            }
            get
            {
                return getMiscObject(Thread.CurrentThread.ManagedThreadId);
            }
        }

        /// <summary>
        ///   Gets the misc object.
        /// </summary>
        /// <param name = "threadName">Name of the thread.</param>
        /// <returns></returns>
        public static string getMiscObject(int threadName)
        {
            if (miscHash.ContainsKey(threadName))
                return miscHash[threadName].ToString();
            return defaultMiscObject;
        }

        #endregion

        #region Termination Request Handling

        private static readonly Dictionary<int, Boolean> termRequests = new Dictionary<int, Boolean>();

        /// <summary>
        ///   Gets a value indicating whether [terminate request].
        /// </summary>
        /// <value><c>true</c> if [terminate request]; otherwise, <c>false</c>.</value>
        public static Boolean terminateRequest
        {
            get { return GetTerminateRequest(Thread.CurrentThread.ManagedThreadId); }
        }

        /// <summary>
        /// Gets the Boolean indicating whether a termination request has been sent.
        /// </summary>
        /// <param name="searchThreadName">Name of the search thread.</param>
        /// <returns></returns>
        public static Boolean GetTerminateRequest(int searchThreadName)
        {
            if (termRequests.ContainsKey(searchThreadName))
                return (Boolean)termRequests[searchThreadName];
            return false;
        }

        /// <summary>
        ///   Sets the termination request.
        /// </summary>
        /// <param name = "threadName">Name of the thread.</param>
        public static void setTerminationRequest(int threadName)
        {
            if (termRequests.ContainsKey(threadName))
                termRequests[threadName] = true;
            else termRequests.Add(threadName, true);
        }

        #endregion

        #region Time Interval Handling

        private static readonly Dictionary<int, TimeSpan> timeIntervals = new Dictionary<int, TimeSpan>();
        private static readonly TimeSpan zeroTimeInterval = new TimeSpan(0);

        /// <summary>
        ///   Gets the time interval.
        /// </summary>
        /// <value>The time interval.</value>
        public static TimeSpan timeInterval
        {
            get
            {
                return getTimeInterval(Thread.CurrentThread.ManagedThreadId);
            }
        }

        /// <summary>
        ///   Sets the time interval.
        /// </summary>
        /// <param name = "threadName">Name of the thread.</param>
        /// <param name = "value">The value.</param>
        public static void setTimeInterval(int threadName, TimeSpan value)
        {
            if (timeIntervals.ContainsKey(threadName))
                timeIntervals[threadName] = value;
            else timeIntervals.Add(threadName, value);
        }

        /// <summary>
        ///   Gets the time interval.
        /// </summary>
        /// <param name = "threadName">Name of the thread.</param>
        /// <returns></returns>
        public static TimeSpan getTimeInterval(int threadName)
        {
            if ( timeIntervals.ContainsKey(threadName))
                return (TimeSpan)timeIntervals[threadName];
            return zeroTimeInterval;
        }

        #endregion

        #region Verbosity Handling

        /// <summary>
        ///   Defines the default verbosity of all search threads.
        /// </summary>
        public static int defaultVerbosity;


        private static readonly Dictionary<int, int> verbosities = new Dictionary<int, int>();

        /// <summary>
        ///   Gets the verbosity.
        /// </summary>
        /// <value>The verbosity.</value>
        private static int verbosity
        {
            get
            {
                return getVerbosity(Thread.CurrentThread.ManagedThreadId);
            }
        }

        /// <summary>
        ///   Sets the verbosity.
        /// </summary>
        /// <param name = "threadName">Name of the thread.</param>
        /// <param name = "value">The value.</param>
        public static void setVerbosity(int threadName, int value)
        {
            if (verbosities.ContainsKey(threadName))
                verbosities[threadName] = value;
            else verbosities.Add(threadName, value);
        }

        /// <summary>
        ///   Gets the verbosity.
        /// </summary>
        /// <param name = "threadName">Name of the thread.</param>
        /// <returns></returns>
        public static int getVerbosity(int threadName)
        {
            if ( verbosities.ContainsKey(threadName))
                return (int)verbosities[threadName];
            return defaultVerbosity;
        }

        #endregion

        #region Outputting to sidebar Console
        //private static readonly TimeSpan[] verbosityInterval = new[]
        //                                                           {
        //                                                               new TimeSpan(0, 0, 0,0, 1),
        //                                                               new TimeSpan(0, 0, 0, 0,3),
        //                                                               new TimeSpan(0, 0, 0,0, 10),
        //                                                               new TimeSpan(0, 0, 0, 0, 30),
        //                                                               new TimeSpan(0, 0, 0, 0, 100),
        //                                                               new TimeSpan(0, 0, 0, 0, 300),
        //                                                               new TimeSpan(0, 0, 0, 1, 0),
        //                                                               new TimeSpan(0, 0, 0, 3, 0),
        //                                                               new TimeSpan(0, 0, 0, 10, 0),
        //                                                               new TimeSpan(0, 0, 0, 30, 0)
        //                                                           };

        //private static readonly Stopwatch timer = Stopwatch.StartNew();


        /// <summary>
        ///  Calling SearchIO.output will output the string, message, to the 
        ///  text display on the right of GraphSynth, but ONLY if the verbosity (see
        ///  below) is greater than or equal to your specified limit for this message.
        ///  the verbosity limit must be 0, 1, 2, 3, or 4.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="verbosityLimit">The verbosity limit.</param>
        public static Boolean output(object message, int verbosityLimit = 0)
        {
            if ((verbosityLimit > verbosity)
                || (string.IsNullOrWhiteSpace(message.ToString())))
                return false;
            Console.WriteLine(message);
            return true;
        }
        /// <summary>
        /// Outputs the one item of the specified list corresponding to the particular verbosity.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        public static Boolean output(params object[] list)
        {
            if ((verbosity >= list.Length)
                || (string.IsNullOrWhiteSpace(list[verbosity].ToString())))
                return false;
            Console.WriteLine(list[verbosity]);
            return true;
        }

        ///// <summary>
        /////   This was a new and better idea but users didn't like it. It was motivated 
        /////   by an issue of sending too much to the buffer and having the program lock
        /////   up, but the problem is, people want messages to appear reliably not 
        /////   "randomly". You would see it for one iteration and not the next - and that
        /////   was frustrating.
        ///// </summary>
        ///// <param name = "message">The message.</param>
        ///// <param name = "verbosityLimit">The verbosity limit.</param>
        //public static Boolean output(object message, int verbosityLimit = 0)
        //{
        //    if ((message == null) || (message.ToString() == "")) return true;
        //    if (verbosityLimit == 0)
        //    {
        //        Console.WriteLine(message.ToString());
        //        timer.Reset();
        //        timer.Start();
        //        return true;
        //    }
        //    var index = verbosityLimit - verbosity + 2;
        //    if ((index < 0) || ((index < 7) && (verbosityInterval[index] <= timer.Elapsed)))
        //    {
        //        Console.WriteLine(message.ToString());
        //        timer.Reset();
        //        timer.Start();
        //        return true;
        //    }
        //    return false;
        //}

        ///// <summary>
        /////   Outputs the specified message to the output textbox -
        /////   one for each verbosity level.
        ///// </summary>
        ///// <param name = "list">The list.</param>
        //public static void output(params object[] list)
        //{
        //    var index = list.Length;
        //    while (index-- > 0 && !output(list[index], index))
        //    {
        //    }
        //}
        #endregion

#if WPF
        #region Showing Message Boxes, Dialogs

        /// <summary>
        /// Shows a messagebox (pop-up or dialog) - redirects to System.Windows.MessageBox.Show().
        /// </summary>
        /// <param name="messageBoxText">The message box text.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="iconStr">The icon STR.</param>
        /// <param name="buttonStr">The buttons to show.</param>
        /// <param name="defaultResultStr">The default result STR.</param>
        /// <param name="optionsStr">The options STR.</param>
        /// <returns></returns>
        public static bool MessageBoxShow(string messageBoxText, string caption = "Message", string iconStr = "Information", string buttonStr = "OK", string defaultResultStr = "OK", string optionsStr = "None")
        {
            MessageBoxButton button;
            if (!Enum.TryParse(buttonStr, true, out button)) button = MessageBoxButton.OK;
            MessageBoxImage icon;
            if (!Enum.TryParse(iconStr, true, out icon)) icon = MessageBoxImage.Information;
            MessageBoxResult defaultResult;
            if (!Enum.TryParse(defaultResultStr, true, out defaultResult)) defaultResult = MessageBoxResult.OK;
            MessageBoxOptions options;
            if (!Enum.TryParse(optionsStr, true, out options)) options = MessageBoxOptions.None;

            var result = MessageBoxResult.None;
            if ((main == null) || main.Dispatcher.CheckAccess())
                result = MessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options);
            else
                main.Dispatcher.Invoke(
                    (ThreadStart)
                    delegate { result = MessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options); }
                    );
            return ((result == MessageBoxResult.OK)
                    || (result == MessageBoxResult.Yes));
            /*********************************************************************************/
        }

        #endregion

        #region Showing the Graph in Main

        /// <summary>
        ///   A reference to the main window
        /// </summary>
        public static IMainWindow main;
             
        /// <summary>
        ///   Adds and shows a graph window.
        /// </summary>
        /// <param name = "graphObjects">The graph objects.</param>
        /// <param name = "title">The title.</param>
        public static void addAndShowGraphWindow(object graphObjects, string title = "")
        {
            if (main == null)
                output("Cannot show graph, {0}, without GUI loaded.", title);
            else if (main.Dispatcher.CheckAccess())
                main.addAndShowGraphWindow(graphObjects, title);
            else
                main.Dispatcher.Invoke(
                    (ThreadStart)(() => main.addAndShowGraphWindow(graphObjects, title))
                    );
        }

        /// <summary>
        ///   Adds and shows a rule window.
        /// </summary>
        /// <param name = "ruleObjects">The rule objects.</param>
        /// <param name = "title">The title.</param>
        public static void addAndShowRuleWindow(object ruleObjects, string title)
        {
            if (main == null)
                output("Cannot show rule, {0}, without GUI loaded.", title);
            else if (main.Dispatcher.CheckAccess())
                main.addAndShowRuleWindow(ruleObjects, title);
            else
                main.Dispatcher.Invoke(
                    (ThreadStart)(() => main.addAndShowRuleWindow(ruleObjects, title))
                    );
        }


        /// <summary>
        /// Adds and shows a ruleset window.
        /// </summary>
        /// <param name="ruleSetObjects">The rule set objects.</param>
        /// <param name="title">The title.</param>
        public static void addAndShowRuleSetWindow(object ruleSetObjects, string title)
        {
            if (main == null)
                output("Cannot show ruleset, {0}, without GUI loaded.", title);
            else if (main.Dispatcher.CheckAccess())
                main.addAndShowRuleSetWindow(ruleSetObjects, title);
            else
                main.Dispatcher.Invoke(
                    (ThreadStart)(() => main.addAndShowRuleSetWindow(ruleSetObjects, title))
                    );
        }

        #endregion
#else
        /// <summary>
        /// Messages the box show.
        /// </summary>
        /// <param name="messageBoxText">The message box text.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="iconStr">The icon string.</param>
        /// <param name="buttonStr">The button string.</param>
        /// <param name="defaultResultStr">The default result string.</param>
        /// <param name="optionsStr">The options string.</param>
        /// <returns></returns>
        public static bool MessageBoxShow(string messageBoxText, string caption = "", string iconStr = "", string buttonStr = "OK", string defaultResultStr = "", string optionsStr = "")
        {
            if (!string.IsNullOrWhiteSpace(iconStr)) iconStr = " " + iconStr + ":";
            if (!string.IsNullOrWhiteSpace(caption)) caption = " " + caption.Trim() + " ";
            else iconStr.Replace(':', ' ');

            output("**" + iconStr + caption + "**\n" + messageBoxText + "\n");
            if (buttonStr.StartsWith("OK"))
            {
                output("Hit any key to continue.");
                Console.ReadKey();
                return true;
            }
            ConsoleKey response;
            do
            {
                output("Please Respond Y/N:");
                response = Console.ReadKey().Key;
            } while (response != ConsoleKey.N && response != ConsoleKey.Y);
            return (response == ConsoleKey.Y);
        }

        /// <summary>
        /// Adds the and show graph window.
        /// </summary>
        /// <param name="graphObjects">The graph objects.</param>
        /// <param name="title">The title.</param>
        public static void addAndShowGraphWindow(object graphObjects, string title = "")
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Adds the and show rule window.
        /// </summary>
        /// <param name="ruleObjects">The rule objects.</param>
        /// <param name="title">The title.</param>
        public static void addAndShowRuleWindow(object ruleObjects, string title)
        {
            //throw new NotImplementedException();
        }
        /// <summary>
        /// Adds the and show rule set window.
        /// </summary>
        /// <param name="ruleObjects">The rule objects.</param>
        /// <param name="title">The title.</param>
        public static void addAndShowRuleSetWindow(object ruleObjects, string title)
        {
            //throw new NotImplementedException();
        }
#endif

    }
}