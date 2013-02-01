using System.Collections.Generic;

namespace BCad.FileHandlers
{
    public interface IFilePlotterMetadata
    {
        string DisplayName { get; }
        IEnumerable<string> FileExtensions { get; }
    }
}
