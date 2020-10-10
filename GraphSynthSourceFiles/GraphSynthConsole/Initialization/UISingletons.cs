using System;

namespace GraphSynth
{
    public class PopUpDialogger : IPopUpDialogger
    {
        public bool MessageBoxShow(string messageBoxText, string caption = "Message", string iconStr = "Information", string buttonStr = "OK",
            string defaultResultStr = "OK", string optionsStr = "None")
        {
            SearchIO.output(messageBoxText);
            var text = Console.ReadLine();
            return text.Contains('y', StringComparison.OrdinalIgnoreCase) || text.Contains("ok", StringComparison.OrdinalIgnoreCase);
            //if (!Enum.TryParse(buttonStr, true, out button)) button = MessageBoxButton.OK;
            //MessageBoxImage icon;
            //if (!Enum.TryParse(iconStr, true, out icon)) icon = MessageBoxImage.Information;
            //MessageBoxResult defaultResult;
            //if (!Enum.TryParse(defaultResultStr, true, out defaultResult)) defaultResult = MessageBoxResult.OK;
            //MessageBoxOptions options;
            //if (!Enum.TryParse(optionsStr, true, out options)) options = MessageBoxOptions.None;

            //var result = MessageBoxResult.None;
            //if ((main == null) || main.Dispatcher.CheckAccess())
            //    result = MessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options);
            //else
            //    main.Dispatcher.Invoke(
            //        (ThreadStart)
            //        delegate { result = MessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options); }
            //        );
            //return ((result == MessageBoxResult.OK)
            //        || (result == MessageBoxResult.Yes));
            /*********************************************************************************/
        }

        public bool? Query(string message, string title, string trueBtnText = "Ok", PopUpIconType popUpIconType = PopUpIconType.None, string falseBtnText = "", string nullBtnText = "")
        {
            throw new NotImplementedException();
        }

        public bool? Query(string commonMessageKey)
        {
            throw new NotImplementedException();
        }
    }
    public class GraphPresenter : IGraphPresenter
    {

        /// <summary>
        ///   Adds and shows a graph window.
        /// </summary>
        /// <param name = "graphObjects">The graph objects.</param>
        /// <param name = "title">The title.</param>
        public void addAndShowGraphWindow(object graphObjects, string title = "")
        {
            //if (main == null)
            //    SearchIO.output("Cannot show graph, {0}, without GUI loaded.", title);
            //else if (main.Dispatcher.CheckAccess())
            //    main.addAndShowGraphWindow(graphObjects, title);
            //else
            //    main.Dispatcher.Invoke(
            //        (ThreadStart)(() => main.addAndShowGraphWindow(graphObjects, title))
            //        );
        }

        /// <summary>
        ///   Adds and shows a rule window.
        /// </summary>
        /// <param name = "ruleObjects">The rule objects.</param>
        /// <param name = "title">The title.</param>
        public void addAndShowRuleWindow(object ruleObjects, string title)
        {
            //if (main == null)
            //    SearchIO.output("Cannot show rule, {0}, without GUI loaded.", title);
            //else if (main.Dispatcher.CheckAccess())
            //    main.addAndShowRuleWindow(ruleObjects, title);
            //else
            //    main.Dispatcher.Invoke(
            //        (ThreadStart)(() => main.addAndShowRuleWindow(ruleObjects, title))
            //        );
        }


        /// <summary>
        /// Adds and shows a ruleset window.
        /// </summary>
        /// <param name="ruleSetObjects">The rule set objects.</param>
        /// <param name="title">The title.</param>
        public void addAndShowRuleSetWindow(object ruleSetObjects, string title)
        {
            //if (main == null)
            //    SearchIO.output("Cannot show ruleset, {0}, without GUI loaded.", title);
            //else if (main.Dispatcher.CheckAccess())
            //    main.addAndShowRuleSetWindow(ruleSetObjects, title);
            //else
            //    main.Dispatcher.Invoke(
            //        (ThreadStart)(() => main.addAndShowRuleSetWindow(ruleSetObjects, title))
            //        );
        }
    }
}
