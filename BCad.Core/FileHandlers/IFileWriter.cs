using System.IO;

namespace BCad.FileHandlers
{
    public interface IFileWriter
    {
        void WriteFile(string fileName, Stream stream, Drawing drawing, ViewPort activeViewPort);
    }
}
