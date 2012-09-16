using System.IO;

namespace BCad.FileHandlers
{
    public interface IFileReader
    {
        void ReadFile(string fileName, Stream stream, out Drawing drawing, out ViewPort activeViewPort);
    }
}
