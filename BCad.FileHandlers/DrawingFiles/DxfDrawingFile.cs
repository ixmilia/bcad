using System.IO;
using BCad.Core;
using BCad.Dxf;

namespace BCad.FileHandlers.DrawingFiles
{
    public class DxfDrawingFile : IDrawingFile
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
