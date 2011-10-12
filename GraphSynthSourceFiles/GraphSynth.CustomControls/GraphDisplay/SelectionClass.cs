using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using System.Xml.Serialization;
using GraphSynth.Representation;

namespace GraphSynth.GraphDisplay
{
    public class SelectionClass
    {
        #region Fields and Properties
        [XmlIgnore]
        public GraphGUI gd { get; private set; }

        public Point ReferencePoint { get; set; }

        public node SelectedNode
        {
            get
            {
                if ((selectedNodes == null) || (selectedNodes.Count < 1))
                    return null;
                return selectedNodes[0];
            }
        }

        public List<node> selectedNodes { get; set; }

        public arc SelectedArc
        {
            get
            {
                if ((selectedArcs == null) || (selectedArcs.Count < 1))
                    return null;
                return selectedArcs[0];
            }
        }

        public List<arc> selectedArcs { get; set; }

        public hyperarc SelectedHyperArc
        {
            get
            {
                if ((selectedHyperArcs == null) || (selectedHyperArcs.Count < 1))
                    return null;
                return selectedHyperArcs[0];
            }
        }

        public List<hyperarc> selectedHyperArcs { get; set; }


        [XmlIgnore]
        public List<UIElement> SelectedShapes { get; set; }

        [XmlIgnore]
        public XElement ReadInXmlShapes { get; private set; }

        public XElement XmlOfShapes
        {
            get
            {
                var xShapes = new XElement("Shapes");

                foreach (var n in selectedNodes)
                    xShapes.Add(XElement.Parse(((DisplayShape)n.DisplayShape).String));

                foreach (var a in selectedArcs)
                    xShapes.Add(XElement.Parse(((DisplayShape)a.DisplayShape).String));

                foreach (var h in selectedHyperArcs)
                    xShapes.Add(XElement.Parse(((DisplayShape)h.DisplayShape).String));
                return xShapes;
            }
            set { ReadInXmlShapes = value; }
        }

        #endregion

        #region Constructor

        public SelectionClass(GraphGUI gd)
            : this()
        {
            this.gd = gd;
        }

        public SelectionClass()
        {
            SelectedShapes = new List<UIElement>();
            selectedNodes = new List<node>();
            selectedArcs = new List<arc>();
            selectedHyperArcs = new List<hyperarc>();
        }

        #endregion

        /* this function was rewritten in early May 2011 to handle hyperarcs, but also in pursuit of
         * more efficient code as it seemed to be a bottleneck for large graphs. The following is thus
         * "optimized" code which is not necessarily easy to follow, and could be more succinct. However,
         * the goal to remove looping overall all the graph elements has been eliminated. This was the
         * more "elegant" approach in the old method, but takes a long time for large graphs. */
        public void UpdateSelection()
        {
            try
            {
                Clear();
                var elements = gd.GetSelectedElements().Cast<FrameworkElement>().ToList();

                /* the following if is figure out when dragging an empty arc endpoint */
                if ((elements.Count == 1) && (typeof(NullNodeIconShape).IsInstanceOfType(elements[0])))
                {
                    gd.activeNullNode = (NullNodeIconShape)elements[0];
                    gd.Select(SelectedShapes);
                    if (typeof(arc).IsInstanceOfType(gd.activeNullNode.GraphElement))
                    {
                        ((DisplayShape)gd.activeNullNode.GraphElement.DisplayShape).icon.Selected = true;
                        selectedArcs.Add((arc)gd.activeNullNode.GraphElement);
                    }
                    else
                    {
                        ((DisplayShape)gd.activeNullNode.GraphElement.DisplayShape).icon.Selected = true;
                        selectedHyperArcs.Add((hyperarc)gd.activeNullNode.GraphElement);
                    }
                    return;
                }
                /* else, you are not dragging an arc and have selected a set of nodes and arcs. */
                int i = 0;
                while (i < elements.Count)
                {
                    if (typeof(NodeIconShape).IsInstanceOfType(elements[i]))
                    {
                        /*select node*/
                        var icon = (NodeIconShape)elements[i];
                        var n = icon.GraphElement;
                        var s = (FrameworkElement)n.DisplayShape.Shape;
                        selectedNodes.Add((node)n);

                        if ((icon.Width > s.Width) && (icon.Height > s.Height))
                            SelectedShapes.Add(icon);

                        elements.RemoveAt(i);
                        /*select node shape*/
                        SelectedShapes.Add(s);
                        var index = elements.IndexOf(s);
                        if (index >= 0)
                        {
                            elements.RemoveAt(index);
                            if (index <= i) i--;
                        }
                    }
                    else if (typeof(ArcIconShape).IsInstanceOfType(elements[i]))
                    {
                        /*select arc*/
                        var a = ((ArcIconShape)elements[i]).GraphElement;
                        selectedArcs.Add((arc)a);
                        /*select icon*/
                        // SelectedShapes.Add(elements[i]);
                        ((ArcIconShape)elements[i]).Selected = true;
                        elements.RemoveAt(i);
                        /*select arc shape*/
                        //SelectedShapes.Add((FrameworkElement)a.DisplayShape.Shape);
                        elements.Remove((FrameworkElement)a.DisplayShape.Shape);
                    }
                    else if (typeof(ArcShape).IsInstanceOfType(elements[i]))
                    {
                        /*select arc*/
                        var icon = ((ArcShape)elements[i]).icon;
                        var a = icon.GraphElement;
                        selectedArcs.Add((arc)a);
                        /*select icon*/
                        //  SelectedShapes.Add(icon);
                        icon.Selected = true;
                        elements.Remove(icon);
                        /*select arc shape*/
                        //SelectedShapes.Add(elements[i]);
                        elements.RemoveAt(i);
                    }
                    else if (typeof(HyperArcIconShape).IsInstanceOfType(elements[i]))
                    {
                        /*select node*/
                        var h = ((HyperArcIconShape)elements[i]).GraphElement;
                        selectedHyperArcs.Add((hyperarc)h);
                        /*select icon*/
                        ((HyperArcIconShape)elements[i]).Selected = true;
                        elements.RemoveAt(i);
                        /*select node shape*/
                        elements.Remove((FrameworkElement)h.DisplayShape.Shape);
                    }
                    else if (typeof(HyperArcShape).IsInstanceOfType(elements[i]))
                    {
                        /*select node*/
                        var icon = ((HyperArcShape)elements[i]).icon;
                        var h = icon.GraphElement;
                        selectedHyperArcs.Add((hyperarc)h);
                        /*select icon*/
                        icon.Selected = true;
                        elements.Remove(icon);
                        /*select node shape*/
                        elements.RemoveAt(i);
                    }
                    else i++;
                }
                /* now anything that's left may be a node shape */
                foreach (var e in elements)
                {
                    if (!gd.nodeShapes.Contains(e)) continue;
                    var n = gd.getNodeFromShape(e);
                    if (n == null) continue;
                    var icon = ((DisplayShape)n.DisplayShape).icon;
                    // what if e is a null shape?
                    /*select node*/
                    selectedNodes.Add(n);
                    /*select icon*/
                    if ((icon.Width > e.Width) && (icon.Height > e.Height))
                        SelectedShapes.Add(icon);
                    /*select node shape*/
                    SelectedShapes.Add(e);
                }
                /* now we need to add any arcs that have both their nodes (To, From) in the selection. */
                if (selectedNodes.Count > 0)
                {
                    /* three actions are performed in this condition. First we find the reference point 
                         * (else it's at the origin). Second, we select and implicitly selected arcs - arcs
                         * which have both of their nodes selected. And finally, we select all the implicitly
                         * selected hyperarcs. */
                    /* First */
                    ReferencePoint = new Point(selectedNodes.Min(n => n.X), selectedNodes.Min(n => n.Y));
                    /* Second */
                    var implicitSelectArcs = selectedNodes.SelectMany(n => n.arcsFrom);
                    implicitSelectArcs = implicitSelectArcs.Intersect(selectedNodes.SelectMany(n => n.arcsTo));
                    foreach (var a in implicitSelectArcs) ((DisplayShape)a.DisplayShape).icon.Selected = true;
                    selectedArcs = selectedArcs.Union(implicitSelectArcs).ToList();
                    /* Third */
                    var implicitHyperArcs = selectedNodes.SelectMany(n => n.arcs)
                        .Where(h => typeof(hyperarc).IsInstanceOfType(h)).Cast<hyperarc>().ToList();
                    implicitHyperArcs.RemoveAll(h => (selectedHyperArcs.Contains(h)
                        || (h.nodes.Intersect(selectedNodes).Count() < h.nodes.Count)));
                    selectedHyperArcs = selectedHyperArcs.Union(implicitHyperArcs).ToList();
                    foreach (hyperarc h in implicitHyperArcs)
                        ((DisplayShape)h.DisplayShape).icon.Selected = true;
                }
                else ReferencePoint = new Point();

                gd.Select(SelectedShapes);
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        public string SerializeToXml()
        {
            var sb = new StringBuilder();
            TextWriter tw = new StringWriter(sb);
            var Serializer = new XmlSerializer(typeof(SelectionClass));
            Serializer.Serialize(tw, this);

            return (sb.ToString());
        }

        public static SelectionClass DeSerializeClipboardFormatFromXML(string xmlString)
        {
            var stringReader = new StringReader(xmlString);
            var Deserializer = new XmlSerializer(typeof(SelectionClass));
            return (SelectionClass)Deserializer.Deserialize(stringReader);
        }

        internal SelectionClass FindCommonSelection(RuleDisplay ruleDisplay)
        {
            var sc = new SelectionClass(ruleDisplay);
            sc.selectedNodes.AddRange(ruleDisplay.graph.nodes.Where(r => selectedNodes.Select(n => n.name).Contains(r.name)));
            sc.selectedArcs.AddRange(ruleDisplay.graph.arcs.Where(r => selectedArcs.Select(a => a.name).Contains(r.name)));
            sc.selectedHyperArcs.AddRange(ruleDisplay.graph.hyperarcs.Where(r => selectedHyperArcs.Select(h => h.name).Contains(r.name)));

            return sc;
        }

        internal void Clear()
        {
            /* initialize local variables */
            ReferencePoint = new Point();
            /* reset object variables */
            selectedNodes.Clear();
            selectedArcs.Clear();
            foreach (var a in gd.arcIcons) a.Selected = false;
            foreach (var a in gd.hyperarcIcons) a.Selected = false;
            selectedHyperArcs.Clear();
            SelectedShapes.Clear();
        }
    }
}