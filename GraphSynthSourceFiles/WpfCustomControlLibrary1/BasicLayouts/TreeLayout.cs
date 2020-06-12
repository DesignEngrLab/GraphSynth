using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace GraphSynth.GraphLayout
{
    public class TreeLayout : GraphLayoutBaseClass
    {
        #region Layout Declaration, Sliders
        public override string text
        {
            get { return "Simple Tree Layout"; }
        }
        public TreeLayout()
        {
            MakeSlider(VerticalSpacingProperty, "Vertical Spacing",
                       "The vertical spacing between the center of the nodes",
                       1.0, 3, 1, 50, true, 0);
            MakeSlider(HorizontalSpacingProperty, "Horizontal Spacing",
                       "The horizontal spacing between the center of the nodes",
                       1.0, 3, 1, 50, true, 0);
        }
        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty VerticalSpacingProperty
            = DependencyProperty.Register("Vertical Spacing",
                                          typeof(double), typeof(TreeLayout),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty HorizontalSpacingProperty
            = DependencyProperty.Register("Horizontal Spacing",
                                          typeof(double), typeof(TreeLayout),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));
        public double VerticalSpacing
        {
            get
            {
                var val = 0.0;
                Dispatcher.Invoke((ThreadStart)delegate { val = (double)GetValue(VerticalSpacingProperty); });
                return val;
            }
            set { SetValue(VerticalSpacingProperty, value); }
        }

        public double HorizontalSpacing
        {
            get
            {
                var val = 0.0;
                Dispatcher.Invoke((ThreadStart)delegate { val = (double)GetValue(HorizontalSpacingProperty); });
                return val;
            }
            set { SetValue(HorizontalSpacingProperty, value); }
        }

        #endregion

        #region Layout Methods / Algorithm
        protected override bool RunLayout()
        {
            HashSet<Representation.node> visitedLeaves = new HashSet<Representation.node>();
            Representation.designGraph copiedgraph = graph.copy();
            bool istree = true;
           

            while (istree == true && copiedgraph.nodes.Count > 1)
            {
                backgroundWorker.ReportProgress(100 - copiedgraph.nodes.Count);
                if (backgroundWorker.CancellationPending) return false;
                istree = false;
                for (var i = 0; i < copiedgraph.nodes.Count; i++)
                {
                    if (copiedgraph.nodes[i].arcsFrom.Count == 0)
                    {
                        for (var j = 0; (i < copiedgraph.nodes.Count) && (j < copiedgraph.nodes[i].arcsTo.Count); j++)
                        {
                            string removedname = copiedgraph.nodes[i].arcsTo[j].To.name;
                            copiedgraph.removeNode(copiedgraph.nodes[i], false);

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
            {
                 backgroundWorker.ReportProgress(100);
                throw new Exception("The input graph is not a tree. This layout only works on tree structures.");
            }
                //return;

            var k = 0;
            var x_dist = 0;
            for (k = 0; k < copiedgraph.nodes.Count; k++)
            {
                for (var l = 0; l < graph.nodes.Count; l++)
                {
                    if (copiedgraph.nodes[k].name == graph.nodes[l].name)
                    {
                        graph.nodes[l].X = x_dist;
                        graph.nodes[l].Y = 0;
                        outputchild(graph.nodes[l], x_dist, 0);
                        x_dist = x_dist + (int)HorizontalSpacing;
                        break;
                    }
                }
            }
            return true;
        }

        protected void outputchild(Representation.node node, int x_val, int y_val)
        {
            node.X = x_val;
            node.Y = y_val;
            var x_offset = 0;

            for (int i = 0; i < node.arcsFrom.Count; i++)
            {
                if (i > 0)
                {
                    if ( num_descendents(node.arcsFrom[i - 1].To) > 0)
                        x_offset += (int)HorizontalSpacing * (num_descendents(node.arcsFrom[i - 1].To) - 1);
                }

                outputchild(node.arcsFrom[i].To,  (x_val + x_offset), (y_val + (int)VerticalSpacing));
                x_offset = x_offset + (int)HorizontalSpacing;
            }
        }

        protected int num_descendents(Representation.node node)
        {
            if (node.arcsFrom.Count == 0)
                return 0;
            else if (node.arcsFrom.Count == 1)
                return num_descendents(node.arcsFrom[0].To);
            else
            {
                var total_desc_count = 0;
                for (int i = 0; i < node.arcsFrom.Count; i++)
                {
                    total_desc_count += num_descendents(node.arcsFrom[i].To);
                }
                return node.arcsFrom.Count + total_desc_count;
            }
        }
        #endregion
    }
}

//////////////////////////////////////////////
//  This Graph Layout is a modification of  //
//  a graph layout within the GraphSharp    //
//  application. Portions of code within    //
//  this file were taken directly from      //
//  GraphSharp.                             //
//                                          //
//  Go to http://graphsharp.codeplex.com/   //
//  to find out more about GraphSharp.      //
//                                          //
//////////////////////////////////////////////