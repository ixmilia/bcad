using System.IO;
using BCad.Core;
using BCad.Dxf;

namespace BCad.FileHandlers.DrawingFiles
{
    internal class DxfDrawingFile : IDrawingFile
    {
        public DxfFile File { get; set; }

        public DxfDrawingFile(DxfFile file)
        {
            this.File = file;
        }

        public void Save(Stream stream)
        {
            this.File.Save(stream);
        }
    }
}
