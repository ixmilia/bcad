using System.IO;
using BCad.Core;
using BCad.Iges;

namespace BCad.FileHandlers.DrawingFiles
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
