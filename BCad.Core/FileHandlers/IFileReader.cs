using System.IO;

namespace BCad.FileHandlers
{
    public interface IFileReader
    {
        Drawing ReadFile(string fileName, Stream stream);
    }
}
