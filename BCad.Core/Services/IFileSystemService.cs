using System.Collections.Generic;
using System.Threading.Tasks;

namespace BCad.Services
{
    public interface IFileSystemService
    {
        Task<string> GetFileNameFromUserForSave();
        Task<string> GetFileNameFromUserForWrite(IEnumerable<FileSpecification> fileSpecifications);
        Task<string> GetFileNameFromUserForOpen();
        Task<bool> TryWriteDrawing(string fileName, Drawing drawing, ViewPort viewPort, Dictionary<string, object> propertyBag);
        Task<bool> TryReadDrawing(string fileName, out Drawing drawing, out ViewPort viewPort, out Dictionary<string, object> propertyBag);
    }
}
