using GraphSynth.GraphDisplay;
using GraphSynth.Representation;

namespace GraphSynth.UI
{
    public interface IRuleWindow
    {
        RuleDisplay graphGUIL { get; }
        RuleDisplay graphGUIK { get; }
        RuleDisplay graphGUIR { get; }
        grammarRule rule { get; }
    }
}