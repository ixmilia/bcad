using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IxMilia.BCad.Services
{
    public interface IFileSystemService : IWorkspaceService
    {
        Task<string> GetFileNameFromUserForSave();
        Task<string> GetFileNameFromUserForWrite(IEnumerable<FileSpecification> fileSpecifications);
        Task<string> GetFileNameFromUserForOpen();
        Task<Stream> GetStreamForWriting(string fileName);
        Task<Stream> GetStreamForReading(string fileName);
    }
}
