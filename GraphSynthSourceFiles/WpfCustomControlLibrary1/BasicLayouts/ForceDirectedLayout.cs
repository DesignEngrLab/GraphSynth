using System;
using System.Collections.Generic;
using GraphSynth.GraphLayout.Force;
using GraphSynth.Representation;
using System.Windows;
using System.Threading;

namespace GraphSynth.GraphLayout
{
    /// <summary>
    /// The idea of a force directed layout algorithm is to consider a force between any two nodes. 
    /// In this algorithm, the nodes are represented by steel rings and the edges are springs between them. 
    /// The attractive force is analogous to the spring force and the repulsive force is analogous to the electrical force. 
    /// The basic idea is to minimize the energy of the system by moving the nodes and changing the forces between them.
    /// Adapted from the GraphSynth1 project
    /// </summary>
    public class ForceDirectedLayout : GraphLayoutBaseClass
    {
        #region Global Declarations
        private ForceSimulator m_fsim;
        private long m_maxstep = 50L;
        private int m_iterations = 100;
        protected node referrer;
        protected String m_nodeGroup;
        protected String m_edgeGroup;
        private Dictionary<string, ForceItem> Pars;
        long defaultSpan;
        private int progress = 0;

        #endregion

        #region Layout declaration, Sliders
        public long MaxTimeStep
        {
            get { return m_maxstep; }
            set { m_maxstep = value; }
        }

        public ForceSimulator getForceSimulator
        {
            get { return m_fsim; }
            set { m_fsim = value; }
        }

        public int Iterations
        {
            get { return m_iterations; }
            set
            {
                if (value < 1)
                    throw new ArgumentException("The amount of iterations has to be bigger or equal to one.");
                m_iterations = value;
            }
        }

        public ForceDirectedLayout()
        {
            MakeSlider(SpacingProperty, "Spacing",
                      "Algorithm runs the specified time (in mseconds)",
                      1, 3, 1, 50.0, true, 0);
        }

        public override string text
        {
            get { return "Force Directed Layout"; }
        }
        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty SpacingProperty
            = DependencyProperty.Register("Spacing",
                                          typeof(double), typeof(ForceDirectedLayout),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));
       public double Spacing
        {
            get
            {
                double val = 0.0;
                Dispatcher.Invoke((ThreadStart)delegate { val = (double)GetValue(SpacingProperty); });
                return val;
            }
            set { SetValue(SpacingProperty, value); }
        }
        #endregion

        #region Layout Methods / Algorithms

        protected override bool RunLayout()
        {
            m_fsim = new ForceSimulator();
            m_fsim.AddForce(new NBodyForce());
            m_fsim.AddForce(new SpringForce());
            m_fsim.AddForce(new DragForce());
            Pars = new Dictionary<string, ForceItem>();
            foreach (node n in graph.nodes)
            {
                Pars.Add(n.name.ToString(), new ForceItem());
            }

            progress = 20;
            backgroundWorker.ReportProgress(progress);
            if (backgroundWorker.CancellationPending) return false;

            defaultSpan = 100 * graph.nodes.Count;
            bool success = Run(defaultSpan);
            return success;
        }

        /// <summary>
        /// Runs the specified time.
        /// </summary>
        /// <param name="time">The time.</param>
        public bool Run(long time)
        {
            int updateEvery = graph.nodes.Count;
            DateTime start = DateTime.Now;
            InitializeSimulator(m_fsim);
            while (DateTime.Now < start.AddMilliseconds(time))
            {
                if (updateEvery == 0)
                {
                    progress = progress + 4;
                    if (progress > 95)
                    {
                        progress = 95;
                    }
                    backgroundWorker.ReportProgress(progress);
                    if (backgroundWorker.CancellationPending) return false;
                    updateEvery = graph.nodes.Count;
                }
                m_fsim.RunSimulator(defaultSpan / 100);
                UpdateNodePositions();
                updateEvery--;
            }
            return true;
        }

        ///<summary>
        /// Get the mass value associated with the given node. Subclasses should
        /// override this method to perform custom mass assignment.
        /// @param n the node for which to compute the mass value
        /// @return the mass value for the node. By default, all items are given
        /// a mass value of 1.0.
        ///</summary>
        protected float getMassValue(node n)
        {
            return 1.0f;
        }

        ///<summary>
        /// Get the spring length for the given edge. Subclasses should
        /// override this method to perform custom spring length assignment.
        /// @param e the edge for which to compute the spring length
        /// @return the spring length for the edge. A return value of
        /// -1 means to ignore this method and use the global default.
        ///</summary>
        protected float getSpringLength(arc e)
        {
            return -1.0F;
        }

        ///<summary>
        /// Get the spring coefficient for the given edge, which controls the
        /// tension or strength of the spring. Subclasses should
        /// override this method to perform custom spring tension assignment.
        /// @param e the edge for which to compute the spring coefficient.
        /// @return the spring coefficient for the edge. A return value of
        /// -1 means to ignore this method and use the global default.
        ///</summary>
        protected float getSpringCoefficient(arc e)
        {
            return -1.0F;
        }


        /// <summary>
        /// Updates the node positions.
        /// </summary>
        private void UpdateNodePositions()
        {
            // update node positions
            for (int i = 0; i <graph.nodes.Count; i++)
            {
                node item = graph.nodes[i];
                ForceItem fitem = Pars[item.name.ToString()];

                fitem.Force[0] = 0.0f;
                fitem.Force[1] = 0.0f;
                fitem.Velocity[0] = 0.0f;
                fitem.Velocity[1] = 0.0f;
                if (Double.IsNaN(item.X))
                {
                    graph.nodes[i].X = 0.0f;
                    graph.nodes[i].Y = 0.0f;
                }
 
                graph.nodes[i].X = fitem.Location[0] * (Spacing/50);
                graph.nodes[i].Y = fitem.Location[1] * (Spacing/50);
            }
        }

        ///<summary>
        /// Reset the force simulation state for all nodes processed by this layout.
        ///</summary>
        public void Reset()
        {
            foreach (node item in graph.nodes)
            {
                ForceItem fitem = Pars[item.name];
                if (fitem != null)
                {
                    fitem.Location[0] = (float)item.X;
                    fitem.Location[1] = (float)item.Y;
                    fitem.Force[0] = fitem.Force[1] = 0;
                    fitem.Velocity[0] = fitem.Velocity[1] = 0;
                }
            }
        }

        /// <summary>
        /// Loads the simulator with all relevant force items and springs.
        /// </summary>
        /// <param name="fsim"> the force simulator driving this layout.</param>
        protected void InitializeSimulator(ForceSimulator fsim)
        {
            //TODO: some checks here...?

            float startX = (referrer == null ? 0f : (float)referrer.X);
            float startY = (referrer == null ? 0f : (float)referrer.Y);
            startX = float.IsNaN(startX) ? 0f : startX;
            startY = float.IsNaN(startY) ? 0f : startY;
            foreach (node item in graph.nodes)
            {
                ForceItem fitem = Pars[item.name];
                fitem.Mass = getMassValue(item);
                double x = item.X;
                double y = item.Y;
                fitem.Location[0] = (Double.IsNaN(x) ? startX : (float)x);
                fitem.Location[1] = (Double.IsNaN(y) ? startY : (float)y);
                fsim.addItem(fitem);
            }
            foreach (arc e in graph.arcs)
            {
                node n1 = e.From;
                if (n1 == null) continue;
                ForceItem f1 = Pars[n1.name];
                node n2 = e.To;
                if (n2 == null) continue;
                ForceItem f2 = Pars[n2.name];
                float coeff = getSpringCoefficient(e);
                float slen = getSpringLength(e);
                fsim.addSpring(f1, f2, (coeff >= 0 ? coeff : -1.0F), (slen >= 0 ? slen : -1.0F));
            }
        }

        #endregion

    }

}

