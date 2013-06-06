using System.IO;

namespace BCad.DrawingFiles
{
    public interface IDrawingFile
    {
        void Save(Stream stream);
    }
}
