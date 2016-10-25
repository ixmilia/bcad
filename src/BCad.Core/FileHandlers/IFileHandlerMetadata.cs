using System.Collections.Generic;

namespace BCad.FileHandlers
{
    public interface IFileHandlerMetadata
    {
        string DisplayName { get; }

        IEnumerable<string> FileExtensions { get; }

        bool CanRead { get; }

        bool CanWrite { get; }
    }
}
