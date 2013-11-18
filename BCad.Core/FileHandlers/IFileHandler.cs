using System.IO;
using BCad.Core;

namespace BCad.FileHandlers
{
    public interface IFileHandler
    {
        IDrawingConverter GetConverter();
        IDrawingFile Load(Stream stream);
    }
}
