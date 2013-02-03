using System.Collections.Generic;
using System.IO;

namespace BCad.Stl
{
    public class StlFile
    {
        public string SolidName { get; set; }

        public List<StlTriangle> Triangles { get; private set; }

        public StlFile()
        {
            Triangles = new List<StlTriangle>();
        }

        public void Save(Stream stream, bool asAscii = true)
        {
            var writer = new StlWriter();
            writer.Write(this, stream, asAscii);
        }

        public static StlFile Load(Stream stream)
        {
            var file = new StlFile();
            var reader = new StlReader(stream);
            file.SolidName = reader.ReadSolidName();
            file.Triangles = reader.ReadTriangles();
            return file;
        }
    }
}
