using System.IO;

namespace BCad.FileHandlers
{
    public interface IFileWriter
    {
        void WriteFile(IWorkspace workspace, Stream stream);
    }
}
