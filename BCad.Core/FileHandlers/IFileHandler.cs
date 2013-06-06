using System.IO;
using BCad.Converters;
using BCad.DrawingFiles;

namespace BCad.FileHandlers
{
    public interface IFileHandler
    {
        IDrawingConverter GetConverter();
        IDrawingFile Load(Stream stream);
    }
}
