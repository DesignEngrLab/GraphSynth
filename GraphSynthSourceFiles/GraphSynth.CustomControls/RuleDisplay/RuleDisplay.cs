using System.Windows;
using GraphSynth.Representation;

namespace GraphSynth.GraphDisplay
{
    public partial class RuleDisplay : GraphGUI
    {
        /// <summary>
        ///   Adds the K node to L and R.
        /// </summary>
        /// <param name = "n">The node, n.</param>
        public void AddKNodeToLandR(node n)
        {
            // then need to add the node to both L and R
            // the nodes are added in ths same position as it was in K graph
            var tempNode = (ruleNode)(n).copy();
            //add it to L graph                        
            rW.graphGUIL.graph.addNode(tempNode);
            rW.graphGUIL.addNodeShape(tempNode);

            tempNode = (ruleNode)(n).copy();
            //add it to R graph                        
            rW.graphGUIR.graph.addNode(tempNode);
            rW.graphGUIR.addNodeShape(tempNode);

            mainObject.propertyUpdate(this);
        }

        /// <summary>
        /// Adds the K arc to L and R.
        /// </summary>
        /// <param name="a">arc, a.</param>
        public void AddKArcToLandR(arc a)
        {
            var Larc = (ruleArc)(a).copy();
            var Rarc = (ruleArc)(a).copy();

            if (a.From != null)
            {
                Larc.From = rW.graphGUIL.graph.nodes.Find(b => (b.name == a.From.name));
                Rarc.From = rW.graphGUIR.graph.nodes.Find(b => (b.name == a.From.name));
            }
            if (a.To != null)
            {
                Larc.To = rW.graphGUIL.graph.nodes.Find(b => (b.name == a.To.name));
                Rarc.To = rW.graphGUIR.graph.nodes.Find(b => (b.name == a.To.name));
            }
            rW.graphGUIL.graph.arcs.Add(Larc);
            rW.graphGUIL.AddArcShape(Larc);
            rW.graphGUIL.SetUpNewArcShape(Larc);
            rW.graphGUIR.graph.arcs.Add(Rarc);
            rW.graphGUIR.AddArcShape(Rarc);
            rW.graphGUIR.SetUpNewArcShape(Rarc);
        }

        public void AddKHyperToLandR(hyperarc h)
        {
            var Larc = (ruleHyperarc)(h).copy();
            var Rarc = (ruleHyperarc)(h).copy();
            rW.graphGUIL.graph.addHyperArc(Larc);
            rW.graphGUIR.graph.addHyperArc(Rarc);
            rW.graphGUIL.AddHyperArcShape(Larc);
            rW.graphGUIR.AddHyperArcShape(Rarc);

            foreach (var n in h.nodes)
            {
                Larc.ConnectTo(rW.graphGUIL.graph.nodes.Find(b => (b.name == n.name)));
                Rarc.ConnectTo(rW.graphGUIR.graph.nodes.Find(b => (b.name == n.name)));
            }
            rW.graphGUIL.BindHyperArcToNodeShapes(Larc);
            rW.graphGUIR.BindHyperArcToNodeShapes(Rarc);
        }
    }
}