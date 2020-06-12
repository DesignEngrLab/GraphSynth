using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace GraphSynth.GraphLayout
{
    /// <summary>
    /// TreeLayout instance that computes a radial layout, laying out subsequent
    /// The algorithm used is that of Ka-Ping Yee, Danyel Fisher, Rachna Dhamija,
    ///  and Marti Hearst in their research paper
    ///  <a href="http://citeseer.ist.psu.edu/448292.html">Animated Exploration of
    ///  Dynamic Graphs with Radial Layout</a>, InfoVis 2001. This algorithm computes
    ///  a radial layout which factors in possible variation in sizes, and maintains
    ///  both orientation and ordering constraints to facilitate smooth and
    ///  understandable transitions between layout configurations.
    ///  Adapted from the GraphSynth1 project
    /// </summary>
    public class RadialTreeLayout : GraphLayoutBaseClass
    {
        #region Global Declarations
        protected int m_maxDepth = 0;
        protected double m_radiusInc =25;
        protected double m_theta1, m_theta2;
        protected bool m_setTheta = false;
        protected Point m_origin;
        protected Representation.node m_prevRoot;
        private Dictionary<string, Params> Pars = new Dictionary<string,Params>();
        private Representation.node root;
        private int RootIndex;
        #endregion

        #region Layout declaration, Sliders

        public RadialTreeLayout()
        {
            MakeSlider(SpacingProperty, "Radial Spacing", "Spacing between nodes ",
           1.0, 3.0, 2, 50, true, 0);
        }

        public override string text
        {
            get { return "Radial Tree Layout"; }
        }

        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty SpacingProperty
    = DependencyProperty.Register("Spacing",
                                  typeof(double), typeof(RadialTreeLayout),
                                  new FrameworkPropertyMetadata(20.0,
                                                                FrameworkPropertyMetadataOptions.AffectsRender));
        public double RadialInc
        {
            get
            {
                var val = 0.0;
                Dispatcher.Invoke((ThreadStart)delegate { val = (double)GetValue(SpacingProperty); });
                return val;
            }
            set { SetValue(SpacingProperty, value); }
        }

        public double RadiusIncrement
        {
            get { return m_radiusInc; }
            set { m_radiusInc = value; }
        }
        #endregion

        #region Layout Methods / Algorithm
        public void setAngularBounds(double theta, double width)
        {
            m_theta1 = theta;
            m_theta2 = theta + width;
            m_setTheta = true;
        }

        protected override bool RunLayout()
        {
            bool istree = FindRoot();
            if (istree == false)
            {
                backgroundWorker.ReportProgress(100);
                throw new Exception("The input graph is not a tree. This layout only works on tree structures.");
            }

            
            backgroundWorker.ReportProgress(25);
            if (backgroundWorker.CancellationPending) return false;

            m_origin.X = 0;
            m_origin.Y = 0;
            int minNumArcsFrom = int.MaxValue;
            int rootIndex = 0;
            int[] numArcsFrom = new int[graph.nodes.Count];
            for (int i = 0; i != graph.nodes.Count; i++)
            {
                numArcsFrom[i] = graph.nodes[i].arcsFrom.Count;
                if (numArcsFrom[i] < minNumArcsFrom)
                {
                    minNumArcsFrom = numArcsFrom[i];
                    rootIndex = i;
                }
            }

            backgroundWorker.ReportProgress(40);
            if (backgroundWorker.CancellationPending) return false;
            root = graph.nodes[rootIndex];

            m_radiusInc = RadialInc;        //Make default radius the value of spacing slider
            m_prevRoot = null;
            m_theta1 = 0;
            m_theta2 = m_theta1 + Math.PI * 2;
            Pars = new Dictionary<string, Params>();
            Params par;
            foreach (Representation.node node in graph.nodes)
            {
                par = new Params();
                Pars.Add(node.name, par);
            }
            foreach (Params p in Pars.Values)
                p.visited = false;
            Representation.node n = root;
            Params np = Pars[n.name];
            // calc relative widths and maximum tree depth
            // performs one pass over the tree
            m_maxDepth = 0;
            calcAngularWidth(n, 0);

            backgroundWorker.ReportProgress(50);
            if (backgroundWorker.CancellationPending) return false;

            foreach (Params p in Pars.Values)
                p.visited = false;

            if (!m_setTheta) calcAngularBounds(n, null);

            // perform the layout
            if (m_maxDepth > 0)
            {
                layout(n, m_radiusInc, m_theta1, m_theta2, null);
            }

            // update properties of the root node
            for (int i = 0; i <graph.nodes.Count; i++)
            {
                if (graph.nodes[i].name == n.name)
                {
                    graph.nodes[i].X = m_origin.X;
                    graph.nodes[i].Y = m_origin.Y;
                }
            }

            backgroundWorker.ReportProgress(90);
            if (backgroundWorker.CancellationPending) return false;
            np.angle = m_theta2 - m_theta1;
            return true;
        }

        private bool FindRoot()
        {
            HashSet<Representation.node> visitedLeaves = new HashSet<Representation.node>();
            Representation.designGraph copiedgraph = graph.copy(true);
            bool istree = true;

            while (istree == true && copiedgraph.nodes.Count > 1)
            {
                istree = false;
                for (var i = 0; i < copiedgraph.nodes.Count; i++)
                {
                    if (copiedgraph.nodes[i].arcsFrom.Count == 0)
                    {
                        for (var j = 0; (i < copiedgraph.nodes.Count) && (j < copiedgraph.nodes[i].arcsTo.Count); j++)
                        {
                            string removedname = copiedgraph.nodes[i].arcsTo[j].To.name;
                            copiedgraph.removeNode(copiedgraph.nodes[i].arcsTo[j].To, false);

                            for (var l = 0; l < copiedgraph.arcs.Count; l++)
                            {
                                if (copiedgraph.arcs[l].To != null)
                                {
                                    if (copiedgraph.arcs[l].To.name == removedname)
                                    {
                                        copiedgraph.removeArc(copiedgraph.arcs[l]);
                                    }
                                }
                            }
                            istree = true;
                        }
                        break;
                    }
                }
            }

            if (istree == false)
                return false;
            var k = 0;
            for (k = 0; copiedgraph.nodes[0].name != graph.nodes[k].name; k++)
            { }
            RootIndex = k;

            return true;
        }

        private void calcAngularBounds(Representation.node r, Representation.node ParentNode)
        {
            if (m_prevRoot == null || r == m_prevRoot)           
            {
                m_prevRoot = r;
                return;
            }

            // try to find previous parent of root
            Representation.node p = m_prevRoot;
            while (true)
            {
                Representation.node pp = ParentNode;
                if (pp == r)
                {
                    break;
                }
                else if (pp == null)
                {
                    m_prevRoot = r;
                    return;
                }
                p = pp;
            }

            // compute offset due to children's angular width
            double dt = 0;
            List<Representation.node> iter = sortedChildren(r, ParentNode);
            foreach (Representation.node n in iter)
            {
                if (n == p) break;
                dt += Pars[n.name].width;

            }
            double rw = Pars[r.name].width;
            double pw = Pars[p.name].width;
            dt = -Math.PI * 2 * (dt + pw / 2) / rw;

            // set angular bounds
            m_theta1 = dt + Math.Atan2(p.Y - r.Y, p.X - r.X);
            m_theta2 = m_theta1 + Math.PI * 2;
            m_prevRoot = r;
        }

        private double calcAngularWidth(Representation.node n, int d)
        {
            Pars[n.name].visited = true;
            if (d > m_maxDepth) m_maxDepth = d;
            double aw = 0;
            double w = n.DisplayShape.Width, h = n.DisplayShape.Height;
            double diameter = d == 0 ? 0 : Math.Sqrt(w * w + h * h) / d;

            if ((n.arcs.Count > 1) || (n == root))
            {

                foreach (Representation.arc a in n.arcs)
                {
                    if (!Pars[a.otherNode(n).name].visited)
                        aw += calcAngularWidth(a.otherNode(n), d + 1);
                }
                aw = Math.Max(diameter, aw);
            }
            else
            {
                aw = diameter;
            }
            Pars[n.name].width = aw;
            return aw;
        }

        private List<Representation.node> sortedChildren(Representation.node n, Representation.node p)
        {
            double basevalue = 0;
            int cc;
            // update basevalue angle for node ordering

            if (p != null)
            {
                cc = n.arcs.Count - 1;
                basevalue = normalize(Math.Atan2(p.Y - n.Y, p.X - n.X));
            }
            else cc = n.arcs.Count;

            if ((cc == 0) && (n != root))
            {
                return null;
            }

            Representation.arc arc0 = (Representation.arc) n.arcs[0];
            Representation.node c = arc0.otherNode(n);

            if (c == p)
            {
                Representation.arc arc1 = (Representation.arc)n.arcs[1];
                c = arc1.otherNode(n);
            }

            double[] angle = new double[n.arcs.Count];
            int[] idx = new int[n.arcs.Count];
            for (int i = 0; i < n.arcs.Count; ++i)
            {
                Representation.arc arc = (Representation.arc)n.arcs[i];
                c = arc.otherNode(n);
                idx[i] = i;
                if (c != p)
                    angle[i] = normalize(-basevalue + Math.Atan2(c.Y - n.Y, c.X - n.X));
                    
                else angle[i] = Double.MaxValue;
            }
            Array.Sort(angle, idx); //or is it the other way around?
            List<Representation.node> col = new List<Representation.node>();
            //List<node> children = n.Children;
            for (int i = 0; i < cc; ++i)
            {
                Representation.arc arc = (Representation.arc) n.arcs[idx[i]];
                col.Add(arc.otherNode(n));
            }
            return col;
        }

        private static double normalize(double angle)
        {
            while (angle > Math.PI * 2)
            {
                angle -= Math.PI * 2;
            }
            while (angle < 0)
            {
                angle += Math.PI * 2;
            }
            return angle;
        }

        protected void layout(Representation.node n, double r, double theta1, double theta2, Representation.node ParentNode)
        {
            Pars[n.name].visited = true;
            double dtheta = (theta2 - theta1);
            double dtheta2 = dtheta / 2.0;
            double width = Pars[n.name].width;
            double cfrac, nfrac = 0.0;
            foreach (Representation.node c in sortedChildren(n, ParentNode))
            {
                Params cp = Pars[c.name];
                cfrac = cp.width / width;
                if (!Pars[c.name].visited && ((c.arcs.Count > 1) || (c == root)))
                {
                    layout(c, r + m_radiusInc,
                        theta1 + nfrac * dtheta, theta1 + (nfrac + cfrac) * dtheta,
                        n);
                }
                setPolarLocation(c, n, r, theta1 + nfrac * dtheta + cfrac * dtheta2);
                cp.angle = cfrac * dtheta;
                nfrac += cfrac;
            }
        }

        protected void setPolarLocation(Representation.node n, Representation.node p, double r, double t)
        {
            for (int i = 0; i <graph.nodes.Count; i++)
            {
                if (graph.nodes[i].name == n.name)
                {
                    graph.nodes[i].X = (float)(m_origin.X + r * Math.Cos(t));
                    graph.nodes[i].Y = (float)(m_origin.Y + r * Math.Sin(t));
                }
            }
        }

        #endregion
    }

    #region Helper Parameter Class
    /// <summary>
    /// Paramter blob to temporarily keep working data of one node.
    /// </summary>
    public class Params
    {
        public int d;
        public int r;
        public float rx;
        public float ry;
        public float a;
        public float c;
        public float f;
        public double width;
        public double angle;
        public Boolean visited;
        public double[] loc;
        public double[] disp;

        public Object clone()
        {
            Params p = new Params();
            p.width = this.width;
            p.angle = this.angle;
            return p;
        }

        public Params(double[] loc, double[] disp)
        {
            this.loc = loc;
            this.disp = disp;
            visited = false;
            width = 0.0;
            angle = 0.0;
        }

        public Params()
        {
            visited = false;
            width = 0.0;
            angle = 0.0;
        }
    }
    #endregion
}
