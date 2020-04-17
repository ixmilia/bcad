using System.IO;
using System.Threading.Tasks;

namespace IxMilia.BCad.Services
{
    public interface IReaderWriterService : IWorkspaceService
    {
        Task<bool> TryReadDrawing(string fileName, Stream stream, out Drawing drawing, out ViewPort viewPort);
        Task<bool> TryWriteDrawing(string fileName, Drawing drawing, ViewPort viewPort, Stream stream, bool preserveSettings = true);
    }
}
