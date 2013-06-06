using System.IO;
using BCad.Iges;

namespace BCad.DrawingFiles
{
    internal class IgesDrawingFile : IDrawingFile
    {
        public IgesFile File { get; set; }

        public IgesDrawingFile(IgesFile file)
        {
            this.File = file;
        }

        public void Save(Stream stream)
        {
            this.File.Save(stream);
        }
    }
}
