using System;
using System.IO;
using System.Threading.Tasks;
using IxMilia.BCad.FileHandlers;

namespace IxMilia.BCad.Services
{
    public interface IReaderWriterService : IWorkspaceService
    {
        void RegisterFileHandler(IFileHandler fileHandler, bool canRead, bool canWrite, params string[] fileExtensions);
        Task<ReadDrawingResult> ReadDrawing(string fileName, Stream stream, Func<string, Task<byte[]>> contentResolver);
        Task<bool> TryWriteDrawing(string fileName, Drawing drawing, ViewPort viewPort, Stream stream, bool preserveSettings = true);
    }
}
