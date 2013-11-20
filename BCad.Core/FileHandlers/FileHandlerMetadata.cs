using System.Collections.Generic;

namespace BCad.FileHandlers
{
    public class FileHandlerMetadata : IFileHandlerMetadata
    {
        public bool CanRead { get; set; }

        public bool CanWrite { get; set; }

        public string DisplayName { get; set; }

        public IEnumerable<string> FileExtensions { get; set; }
    }
}
