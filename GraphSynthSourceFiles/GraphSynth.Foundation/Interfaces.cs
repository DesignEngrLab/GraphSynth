using System;
using System.Collections.Generic;

namespace GraphSynth.Foundation.Interfaces
{
    interface IGraphElement
    {
        IGraphElement Copy(Boolean MakeDeepCopy = true);
        IDisplayShape DisplayShape { get; }
        string Name { get; set; }
        (string Label, double Variable) Data { get; }
    }
    interface IGraph : IGraphElement
    {
        IList<IArc> Arcs { get; }
        IList<INode> Nodes { get; }
    }

    interface IHyperGraph : IGraph
    {
        IList<IHyperArc> HyperArcs { get; }
    }
    interface IDisplayShape
    {
        float Height { get; set; }
        float Width { get; set; }
        float X { get; set; }
        float Y { get; set; }
        float StrokeThickness { get; set; }
        float RotationAngle { get; set; }
    }
    interface IArc
    {
        IList<IArc> Arcs { get; }
        INode From { get; set; }
        INode To { get; set; }
        Boolean IsDirected { get; set; }
    }
    interface INode
    {
        IList<IArc> Arcs { get; }
        double X { get; }
        double Y { get; }
        double Z { get; }
    }
    interface IHyperArc
    {
        IList<INode> Nodes { get; }
        IList<IArc> IntraArcs { get; }
    }

    interface IGrammarRule
    { }

    struct LHSRuleArc : IArc
    {
        Boolean ContainsAllLabels { get; set; }
        Boolean DirectionIsEqual { get; set; }
        Boolean MustNotExist { get; set; }
        Boolean DanglingIsRequired { get; set; }
        Boolean ContainsAllLocalLabels { get; set; }
        IDictionary<string, double> NegativeData { get; }
        Type TargetType { get; set; }
    }
    interface IRuleNode : INode { }
}
