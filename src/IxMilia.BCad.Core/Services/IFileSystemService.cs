using System.Collections.Generic;
using System.Threading.Tasks;

namespace IxMilia.BCad.Services
{
    public interface IFileSystemService : IWorkspaceService
    {
        Task<string> GetFileNameFromUserForSave(string extensionHint = null);
        Task<string> GetFileNameFromUserForOpen();
    }
}
