/*************************************************************************
 *     This IMainWindow file & interface is part of the GraphSynth.BaseClasses 
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

namespace GraphSynth
{
    /// <summary>
    /// Interface for the main window of GraphSynth.
    /// </summary>
    public interface IMainWindow
    {
        /// <summary>
        /// Gets the selected add item.
        /// </summary>
        /// <value>The selected add item.</value>
        string SelectedAddItem { get; }
        /// <summary>
        /// Sets the selected add item.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        Boolean SetSelectedAddItem(int i);
        /// <summary>
        /// Gets a value indicating whether the selected item should [stay on].
        /// </summary>
        /// <value><c>true</c> if [stay on]; otherwise, <c>false</c>.</value>
        Boolean stayOn { get; }

        /// <summary>
        /// Focuses on the label field for easy entry.
        /// </summary>
        /// <param name="o">The o.</param>
        void FocusOnLabelEntry(object o);
        /// <summary>
        /// Properties the update.
        /// </summary>
        /// <param name="o">The o.</param>
        void propertyUpdate(object o = null);
        /// <summary>
        /// Sets the canvas property scale factor. This is needed since the
        /// GraphGUI needs to inform the main window what its scale is. The
        /// reason it is defined here, is because GraphGUI is defined in the
        /// CustomControls.dll and MainWindow is in the main EXE. Thus, we
        /// can make a call up from a dependent library to the main EXE.
        /// </summary>
        /// <param name="scale">The scale.</param>
        /// <param name="zoomToFit">The zoom to fit.</param>
        void SetCanvasPropertyScaleFactor(double scale, Boolean? zoomToFit);
        /// <summary>
        /// Adds and shows a graph window.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="title">The title.</param>
        void addAndShowGraphWindow(object obj, string title);
        /// <summary>
        /// Adds and shows a rule window.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="title">The title.</param>
        void addAndShowRuleWindow(object obj, string title);
        /// <summary>
        /// Adds and shows a rule window.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="title">The title.</param>
        void addAndShowRuleSetWindow(object obj, string title);

#if WPF
        /********************* WPF Specific References *********************/
        /// <summary>
        /// Gets the dispatcher.
        /// </summary>
        /// <value>The dispatcher.</value>
        System.Windows.Threading.Dispatcher Dispatcher { get; }
        /// <summary>
        /// Gets the short cut keys.
        /// </summary>
        /// <value>The short cut keys.</value>
        System.Windows.Input.Key[] shortCutKeys { get; }
        /*********************************************************************************/
#endif
    }

}
