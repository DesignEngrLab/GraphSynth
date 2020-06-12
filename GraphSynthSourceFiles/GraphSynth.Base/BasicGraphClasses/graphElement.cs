/*************************************************************************
 *     This graphElement file & class is part of the GraphSynth.BaseClasses 
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
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;


namespace GraphSynth.Representation
{
    /// <summary>
    ///   The base class of node, arc, and hyperarc. It simply captures the basic
    ///   qualities that all includes - mainly a list of labels.
    /// </summary>
    public abstract class graphElement
    {
        #region Fields & Properties

        /// <summary>
        ///   Gets or sets the name of the node or arc. All names must be distinct 
        ///   within a given graph in order to correctly serialize and de-
        ///   serialize the graph from the XML (*.gxml) file.
        /// </summary>
        /// <value>The name string.</value>
        public string name { get; set; }

        /// <summary>
        ///   Gets or sets the old data. In order to be compatible with previous versions,
        ///   this oldData object will be used to catch old files that use screenX and 
        ///   screenY instead of the new format.
        /// </summary>
        /// <value>The old data.</value>
        [XmlAnyElement]
        public XElement[] extraData { get; set; }

        //private Shape _displayShape;
        /// <summary>
        ///   Gets or sets the display shape.
        /// </summary>
        /// <value>The display shape.</value>
        [XmlIgnore]
        public ShapeData DisplayShape { get; set; }



        #region Labels and Variables

        /* a node or arc contains both characterizing strings, known as 
         * localLabels and numbers, stored as localVariables. */

        /// <summary />
        protected List<string> _localLabels;

        /// <summary />
        protected List<double> _localVariables;

        /// <summary>
        ///   Gets the local labels.
        /// </summary>
        /// <value>The local labels.</value>
        public List<string> localLabels
        {
            get { return _localLabels ?? (_localLabels = new List<string>()); }
        }

        /// <summary>
        ///   Gets the local variables.
        /// </summary>
        /// <value>The local variables.</value>
        public List<double> localVariables
        {
            get { return _localVariables ?? (_localVariables = new List<double>()); }
        }

        #endregion

        #region Copy Method

        /// <summary>
        ///   Copies this graphElement data into the copyOfElt.
        /// </summary>
        /// <param name = "copyOfElt">The copy of elt.</param>
        protected virtual void copy(graphElement copyOfElt)
        {
            copyOfElt.name = name;
            foreach (var label in localLabels)
                copyOfElt.localLabels.Add(label);
            foreach (var d in localVariables)
                copyOfElt.localVariables.Add(d);
            if (DisplayShape != null)
                copyOfElt.DisplayShape = DisplayShape.Copy(copyOfElt);
        }

        #endregion

        #endregion

        #region Property-like Functions

        /// <summary>
        ///   Sets the label. The following two functions are not properties, but work on 
        ///   a similar philosophy. These are like the f0, f1, etc. properties in candidate.
        ///   Here we want to set a label but we are unsure whether the list of local labels
        ///   is long enough. The while loop insures that there are enough items of list, 
        ///   before adding the label. */
        /// </summary>
        /// <param name = "index">The index.</param>
        /// <param name = "label">The label.</param>
        public void setLabel(int index, string label)
        {
            while (localLabels.Count <= index)
                localLabels.Add("");
            localLabels[index] = label;
        }

        /// <summary>
        ///   Sets the variable. Like the function above, here we want to set a variable but 
        ///   we are unsure whether the list of local labels is long enough. The while loop 
        ///   insures that there are enough items of list, before adding the label.
        /// </summary>
        /// <param name = "index">The index.</param>
        /// <param name = "var">The var.</param>
        public void setVariable(int index, double var)
        {
            while (localVariables.Count <= index)
                localVariables.Add(0.0);
            localVariables[index] = var;
        }

        #endregion
    }
}