using System.Collections.Generic;

namespace BCad.FileHandlers
{
    public interface IFileReaderMetadata
    {
        string DisplayName { get; }

        IEnumerable<string> FileExtensions { get; }
    }
}
