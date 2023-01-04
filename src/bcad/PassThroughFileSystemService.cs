using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IxMilia.BCad.Services;

namespace bcad
{
    public class PassThroughFileSystemService : IFileSystemService
    {
        private readonly IInputService _inputService;

        public PassThroughFileSystemService(IInputService inputService)
        {
            _inputService = inputService;
        }

        public Task<string> GetFileNameFromUserForOpen(IEnumerable<FileSpecification> fileSpecifications) => GetStringAsync();

        public Task<string> GetFileNameFromUserForSave(IEnumerable<FileSpecification> fileSpecifications) => GetStringAsync();

        public Task<byte[]> ReadAllBytesAsync(string path) => File.ReadAllBytesAsync(path);

        private async Task<string> GetStringAsync()
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
