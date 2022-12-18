using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IxMilia.BCad.Services;

namespace IxMilia.BCad
{
    public class PassThroughFileSystemService : IFileSystemService
    {
        private readonly IInputService _inputService;

        public PassThroughFileSystemService(IInputService inputService)
        {
            _inputService = inputService;
        }

        public Task<string> GetFileNameFromUserForOpen(IEnumerable<FileSpecification> fileSpecifications) => GetString();

        public Task<string> GetFileNameFromUserForSave(IEnumerable<FileSpecification> fileSpecifications) => GetString();

        public Task<byte[]> ReadAllBytesAsync(string path) => File.ReadAllBytesAsync(path);

        private async Task<string> GetString()
        {
            var result = await _inputService.GetText();
            if (result.HasValue)
            {
                return result.Value;
            }

            return null;
        }
    }
}
