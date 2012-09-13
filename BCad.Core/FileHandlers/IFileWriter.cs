using System.IO;

namespace BCad.FileHandlers
{
    public interface IFileWriter
    {
        void WriteFile(Drawing drawing, Stream stream);
    }
}
