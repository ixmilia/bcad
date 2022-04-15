using System.Collections.Generic;
using System.Threading.Tasks;

namespace IxMilia.BCad.Services
{
    public interface IFileSystemService : IWorkspaceService
    {
        Task<string> GetFileNameFromUserForSave(IEnumerable<FileSpecification> fileSpecifications);
        Task<string> GetFileNameFromUserForOpen(IEnumerable<FileSpecification> fileSpecifications);
        Task<byte[]> ReadAllBytesAsync(string path);
    }
}
