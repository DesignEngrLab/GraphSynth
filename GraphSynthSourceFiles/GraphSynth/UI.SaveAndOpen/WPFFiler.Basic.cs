using System;
using System.Threading;
using System.Windows.Threading;
using System.Xml;
using GraphSynth.Representation;
using GraphSynth.UI;

namespace GraphSynth
{
    public partial class WPFFiler : BasicFiler
    {
        /* pass through constructor - same as base */

        /// <summary>
        ///   The fileTransfer object is used only in lock statements to prevent
        ///   accessing the same file in two separate instances such as opening it
        ///   while trying to save it. The impetus for this is the need to re-load
        ///   rules within a ruleset.
        /// </summary>
        private readonly object fileTransfer = new object();

        private int _p;
        private int progressEnd = 100;
        /// <summary>
        ///   Sets a value indicating whether [suppress warnings].
        /// </summary>
        /// <value><c>true</c> if [suppress warnings]; otherwise, <c>false</c>.</value>
        private Boolean suppressWarnings { get; set; }

        public WPFFiler(string iDir, string oDir, string rDir)
            : base(iDir, oDir, rDir)
        {
        }

        private MainWindow main
        {
            get { return GSApp.main; }
        }

        public FilerProgressWindow progWindow { get; set; }

        private int progress
        {
            get { return _p; }
            set
            {
                if (value > 0) _p = value;
                if (progWindow != null) progWindow.backgroundWorker.ReportProgress(value);
            }
        }

        private Boolean UserCancelled
        {
            get
            {
                if (progWindow != null) return (progWindow.backgroundWorker.CancellationPending);
                else return false;
            }
        }

        private Dispatcher dispatch
        {
            get
            {
                if (progWindow != null) return progWindow.Dispatcher;
                else return GSApp.main.Dispatcher;
            }
        }

        /// <summary>
        ///   Saves the specified filename.
        /// </summary>
        /// <param name = "filename">The filename.</param>
        /// <param name = "o">The o.</param>
        /// <param name = "SuppressWarnings">if set to <c>true</c> [suppress warnings].</param>
        public override void Save(string filename, object o, Boolean suppressWarnings = false)
        {
            this.suppressWarnings = suppressWarnings;
            if (typeof(ruleWindow).IsInstanceOfType(o) ||
                typeof(grammarRule).IsInstanceOfType(o) ||
                (typeof(object[]).IsInstanceOfType(o) &&
                 typeof(grammarRule).IsInstanceOfType(((object[])o)[0])))
            {
                if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                    FilerProgressWindow.SaveRule(filename, false, this, o);
                else if (typeof(object[]).IsInstanceOfType(o))
                    SaveRule(filename, (object[])o);
                else SaveRule(filename, new[] { o });
            }
            else if (typeof(graphWindow).IsInstanceOfType(o) ||
                     typeof(designGraph).IsInstanceOfType(o) ||
                     (typeof(object[]).IsInstanceOfType(o) &&
                      typeof(designGraph).IsInstanceOfType(((object[])o)[0])))
            {
                if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                    FilerProgressWindow.SaveGraph(filename, false, this, o);
                else if (typeof(object[]).IsInstanceOfType(o))
                    SaveGraph(filename, (object[])o);
                else SaveGraph(filename, new[] { o });
            }
            else base.Save(filename, o);
        }

        public override object[] Open(string filename, Boolean suppressWarnings = false)
        {
            this.suppressWarnings = suppressWarnings;
            XmlReader xR;
            try
            {
                xR = XmlReader.Create(filename);
                /* Load the file. */
                var doc = new XmlDocument();
                doc.Load(xR);
                xR.Close();
                /* create prefix<->namespace mappings (if any)  */
                var nsMgr = new XmlNamespaceManager(doc.NameTable);
                /* Query the document */
                if (main.windowsMgr.FindAndFocusFileInCollection(doc, nsMgr, filename))
                {
                    if (!suppressWarnings && (progWindow != null))
                        progWindow.QueryUser("That file is already open, or there is another file open with the" +
                                             " same name. If this is another file, please rename one of the names.",
                                             5000, "OK", "", false);
                    if (typeof(graphWindow).IsInstanceOfType(main.windowsMgr.activeWindow))
                    {
                        var gWin = (graphWindow)main.windowsMgr.activeWindow;
                        return new object[] { gWin.graph, gWin.canvasProps, gWin.filename };
                    }
                    else  if (typeof(ruleWindow).IsInstanceOfType(main.windowsMgr.activeWindow))
                    {
                        var rWin = (ruleWindow)main.windowsMgr.activeWindow;
                        return new object[] { rWin.rule, rWin.canvasProps, rWin.filename };
                    }
                    else if (typeof(ruleSetWindow).IsInstanceOfType(main.windowsMgr.activeWindow))
                    {
                        var rsWin = (ruleSetWindow)main.windowsMgr.activeWindow;
                        return new object[] { rsWin.ruleset };
                    }
                }
                else if (doc.SelectNodes("/designGraph", nsMgr).Count > 0)
                    return new object[] { OpenGraph(filename) };
                else if (doc.SelectNodes("/grammarRule", nsMgr).Count > 0)
                    return new object[] { OpenRule(filename) };
                else if (doc.SelectNodes("/candidate", nsMgr).Count > 0)
                    return new object[] { OpenCandidate(filename) };
                else if (doc.SelectNodes("/ruleSet", nsMgr).Count > 0)
                    return FilerProgressWindow.OpenRuleSet(filename, suppressWarnings, this);
                else if (doc.DocumentElement.Attributes["Tag"].Value == "Graph")
                {
                    if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                        return FilerProgressWindow.OpenGraph(filename, suppressWarnings, this);
                    else return OpenGraphAndCanvas(filename);
                }
                else if (doc.DocumentElement.Attributes["Tag"].Value == "Rule")
                {
                    if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                        return FilerProgressWindow.OpenRule(filename, suppressWarnings, this);
                    else return OpenRuleAndCanvas(filename);
                }
                else throw new Exception();
            }
            catch (Exception e)
            {
                if (!suppressWarnings && (progWindow != null))
                {
                    progWindow.QueryUser("The XML files that you have attempted to open contains an unknown " +
                                         "type (not designGraph, grammarRule, ruleSet, or candidate).", 10000, "",
                                         "Cancel", false);
                }
                SearchIO.output(e.ToString());
            }
            return null;
        }

        #region Xml String Corrections

        protected string RemoveIgnorablePrefix(string x)
        {
            return x.Replace(IgnorablePrefix, "");
        }

        protected string AddIgnorablePrefix(string x)
        {
            x = x.Insert(x.IndexOf('<') + 1, IgnorablePrefix);
            return x.Insert(x.LastIndexOf("</") + 2, IgnorablePrefix);
        }

        protected static string RemoveXAMLns(string s)
        {
            //get rid of all the xaml related namespace stuff 
            // how to do this without hardcoding?
            // -- k spent a lot of time to know that it had to be removed for successful deserialization oofff!
            s = s.Replace("xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"", "");

            return s;
        }

        #endregion
    }
}