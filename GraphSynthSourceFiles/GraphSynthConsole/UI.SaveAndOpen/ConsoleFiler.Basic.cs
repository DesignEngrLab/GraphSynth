using System;
using System.Xml;
using GraphSynth.Representation;

namespace GraphSynth
{
    public partial class ConsoleFiler : BasicFiler
    {
        /* pass through constructor - same as base */

        /// <summary>
        ///   The fileTransfer object is used only in lock statements to prevent
        ///   accessing the same file in two separate instances such as opening it
        ///   while trying to save it. The impetus for this is the need to re-load
        ///   rules within a ruleset.
        /// </summary>
        private readonly object fileTransfer = new object();


        public ConsoleFiler(string iDir, string oDir, string rDir)
            :base(iDir,oDir,rDir)
        {
        }


        /// <summary>
        ///   Saves the specified filename.
        /// </summary>
        /// <param name = "filename">The filename.</param>
        /// <param name = "o">The o.</param>
        public override void Save(string filename, object o, Boolean suppressWarnings = false)
        {
           if (o is grammarRule ||
                (o is object[] &&
                 ((object[])o)[0] is grammarRule))
            {
                if (o is object[])
                    SaveRule(filename, (object[])o);
                else SaveRule(filename, new[] { o });
            }
            else if (o is designGraph ||
                     (o is object[] &&
                      ((object[])o)[0] is designGraph))
            {
                if (o is object[])
                    SaveGraph(filename, (object[])o);
                else SaveGraph(filename, new[] { o });
            }
            else base.Save(filename, o);
        }

        public override object[] Open(string filename, Boolean suppressWarnings = false)
        {
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
                if (doc.SelectNodes("/designGraph", nsMgr).Count > 0)
                    return new object[] { OpenGraph(filename) };
                else if (doc.SelectNodes("/grammarRule", nsMgr).Count > 0)
                    return new object[] { OpenRule(filename) };
                else if (doc.SelectNodes("/candidate", nsMgr).Count > 0)
                    return new object[] { OpenCandidate(filename) };
                else if (doc.SelectNodes("/ruleSet", nsMgr).Count > 0)
                    return new object[] { OpenRuleSet(filename) };
                else if (doc.DocumentElement.Attributes["Tag"].Value == "Graph")
                    return OpenGraphAndCanvas(filename);
                else if (doc.DocumentElement.Attributes["Tag"].Value == "Rule")
                    return OpenRuleAndCanvas(filename);

                else throw new Exception();
            }
            catch (Exception e)
            {
                SearchIO.output(e.ToString());
            }
            return null;
        }

        #region Xml String Corrections

        protected string RemoveIgnorablePrefix(string x)
        {
            return x.Replace(IgnorablePrefix, "").Replace("xmlns=\"ignorableUri\"", "");
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