using System.Collections.Generic;

namespace BCad.FileHandlers
{
    public interface IFileWriterMetadata
    {
        string DisplayName { get; }

        IEnumerable<string> FileExtensions { get; }
    }
}
