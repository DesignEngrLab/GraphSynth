using System;
using System.ComponentModel;
using System.Threading;
using GraphSynth.Representation;

namespace GraphSynth
{
    /// <summary>
    ///   Graph Layout Base Class
    /// </summary>
    public abstract class GraphLayoutBaseClass 
    {
        public BackgroundWorker backgroundWorker;
        private Boolean completed;
        private string eMessage;
        protected int numNodes;
        private double[,] origNodeXYZs;
        private EventWaitHandle progressWait;
        private bool success;

        protected GraphLayoutBaseClass()
        {
        }

        public abstract string text { get; }
        public double[] Origin { get; set; }

        public designGraph graph
        {
            get
            {
                if (SelectedGraphGUI == null) return null;
                return SelectedGraphGUI.graph;
            }
        }


        protected virtual bool RunLayout()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether [the specified type] is inherited from GraphLayoutAlgorithm.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static Boolean IsInheritedType(Type t)
        {
            while (t != typeof(object))
            {
                if (t == typeof(GraphLayoutBaseClass)) return true;
                t = t.BaseType;
            }
            return false;
        }

        public static GraphLayoutBaseClass Make(Type lt)
        {
            try
            {
                var constructor = lt.GetConstructor(new Type[] { });
                return (GraphLayoutBaseClass)constructor.Invoke(new object[] { });
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
                return null;
            }
        }
    }
}