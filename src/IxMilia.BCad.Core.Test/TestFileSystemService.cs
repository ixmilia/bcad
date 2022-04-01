using System.IO;
using System.Threading.Tasks;
using IxMilia.BCad.Services;

namespace IxMilia.BCad.Core.Test
{
    public class TestFileSystemService : IFileSystemService
    {
        public Task<string> GetFileNameFromUserForOpen() => throw new System.NotImplementedException();
        public Task<string> GetFileNameFromUserForSave(string extensionHint = null) => throw new System.NotImplementedException();
        public Task<byte[]> ReadAllBytesAsync(string path) => File.ReadAllBytesAsync(path);
    }
}
