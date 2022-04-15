using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IxMilia.BCad.Services;

namespace IxMilia.BCad.Core.Test
{
    public class TestFileSystemService : IFileSystemService
    {
        public Task<string> GetFileNameFromUserForOpen(IEnumerable<FileSpecification> fileSpecifications) => throw new System.NotImplementedException();
        public Task<string> GetFileNameFromUserForSave(IEnumerable<FileSpecification> fileSpecifications) => throw new System.NotImplementedException();
        public Task<byte[]> ReadAllBytesAsync(string path) => File.ReadAllBytesAsync(path);
    }
}
