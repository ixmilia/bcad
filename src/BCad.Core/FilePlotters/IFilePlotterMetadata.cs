using System.Collections.Generic;

namespace BCad.FilePlotters
{
    public interface IFilePlotterMetadata
    {
        string DisplayName { get; }
        IEnumerable<string> FileExtensions { get; }
    }
}
