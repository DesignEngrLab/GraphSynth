using System;
using System.Windows;
using System.Windows.Shapes;
using GraphSynth.Representation;

namespace GraphSynth.GraphDisplay
{
    public class NodeIconBank : IconBank
    {
        public NodeIconBank(GraphGUI gd) : base(gd) { }

        public void Add(Shape displayShape, node n)
        {
            var icon = new NodeIconShape(n, displayShape, gd);
            base.Add(icon, n);
        }
    }
    public class NullNodeIconBank : IconBank
    {
        public NullNodeIconBank(GraphGUI gd) : base(gd) { }

        public NullNodeIconShape Add(Point p, graphElement e, Boolean attachedToHead)
        {
            var icon = new NullNodeIconShape(p, e, attachedToHead, gd);
            base.Add(icon);
            return icon;
        }
    }
    public class ArcIconBank : IconBank
    {
        public ArcIconBank(GraphGUI gd) : base(gd) { }

        public void Add(arc a, FrameworkElement arcshape)
        {
            var icon = new ArcIconShape(a, (ArcShape)arcshape, gd);
            base.Add(icon, a);
        }
    }
    public class HyperArcIconBank : IconBank
    {
        public HyperArcIconBank(GraphGUI gd) : base(gd) { }

        public void Add(hyperarc h, FrameworkElement hyperarcshape)
        {
            var icon = new HyperArcIconShape(h, (HyperArcShape)hyperarcshape, gd);
            base.Add(icon, h);
        }
    }
}