using System.IO;

namespace IxMilia.BCad
{
    public class FileReadResult
    {
        public Stream Stream { get; private set; }

        public string FileName { get; private set; }

        public FileReadResult(Stream stream, string fileName)
        {
            Stream = stream;
            FileName = fileName;
        }
    }
}
