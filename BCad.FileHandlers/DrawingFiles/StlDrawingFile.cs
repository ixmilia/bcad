using System.IO;
using BCad.Core;
using BCad.Stl;

namespace BCad.FileHandlers.DrawingFiles
{
    internal class StlDrawingFile : IDrawingFile
    {
        public StlFile File { get; set; }

        public StlDrawingFile(StlFile file)
        {
            this.File = file;
        }

        public void Save(Stream stream)
        {
            this.File.Save(stream);
        }
    }
}
