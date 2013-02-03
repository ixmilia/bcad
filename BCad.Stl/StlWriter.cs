using System.IO;

namespace BCad.Stl
{
    internal class StlWriter
    {
        private const string FloatFormat = "e6";

        public void Write(StlFile file, Stream stream, bool asAscii)
        {
            if (asAscii)
                WriteAscii(file, stream);
        }

        private void WriteAscii(StlFile file, Stream stream)
        {
            var writer = new StreamWriter(stream);
            writer.WriteLine(string.Format("solid {0}", file.SolidName));
            foreach (var triangle in file.Triangles)
            {
                writer.WriteLine(string.Format("  facet normal {0}", NormalToString(triangle.Normal)));
                writer.WriteLine("    outer loop");
                writer.WriteLine(string.Format("      vertex {0}", VertexToString(triangle.Vertex1)));
                writer.WriteLine(string.Format("      vertex {0}", VertexToString(triangle.Vertex2)));
                writer.WriteLine(string.Format("      vertex {0}", VertexToString(triangle.Vertex3)));
                writer.WriteLine("    endloop");
                writer.WriteLine("  endfacet");
            }

            writer.WriteLine(string.Format("endsolid {0}", file.SolidName));
            writer.Flush();
        }

        private static string NormalToString(StlNormal normal)
        {
            return string.Format("{0} {1} {2}", normal.X.ToString(FloatFormat), normal.Y.ToString(FloatFormat), normal.Z.ToString(FloatFormat));
        }

        private static string VertexToString(StlVertex vertex)
        {
            return string.Format("{0} {1} {2}", vertex.X.ToString(FloatFormat), vertex.Y.ToString(FloatFormat), vertex.Z.ToString(FloatFormat));
        }
    }
}
