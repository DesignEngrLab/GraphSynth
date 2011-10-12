using System.IO;
using System.Reflection;

namespace GraphSynth
{
    public static class GetShapeReourceString
    {
        public static string get(string filename)
        {
            StreamReader r = null;
            try
            {
                var _assembly = Assembly.GetExecutingAssembly();
                r = new StreamReader(_assembly.GetManifestResourceStream("GraphSynth.ShapeStringDataFiles."
                                                                             + filename + ".xml"));
                var result = r.ReadToEnd();
                return result;
            }
            catch
            {
                return "";
            }
            finally
            {
                if (r != null) r.Close();
            }
        }
    }
}
