using System.Windows;

namespace GraphSynth.UI
{
    public enum WindowType
    {
        Invalid,
        Graph,
        Rule,
        RuleSet,
        GlobalSetting,
        SearchProcessController,
        UserChooser
    } ;

    public class WinData
    {
        public WinData(Window Win, WindowType wt, string filename)
        {
            this.Win = Win;
            WinName = Win.Title;
            WinType = wt;
            WinPath = filename;
        }

        public Window Win { get; private set; }
        public string WinName { get; private set; }
        public string WinPath { get; private set; }
        public WindowType WinType { get; private set; }
    }
}