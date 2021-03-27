using System.IO;
using System.Threading.Tasks;
using IxMilia.BCad.FileHandlers;

namespace IxMilia.BCad.Services
{
    public interface IReaderWriterService : IWorkspaceService
    {
        void RegisterFileHandler(IFileHandler fileHandler, bool canRead, bool canWrite, params string[] fileExtensions);
        Task<bool> TryReadDrawing(string fileName, Stream stream, out Drawing drawing, out ViewPort viewPort);
        Task<bool> TryWriteDrawing(string fileName, Drawing drawing, ViewPort viewPort, Stream stream, bool preserveSettings = true);
    }
}
