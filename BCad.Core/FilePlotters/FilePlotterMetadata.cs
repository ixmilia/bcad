using System.Collections.Generic;

namespace BCad.FilePlotters
{
    public class FilePlotterMetadata : IFilePlotterMetadata
    {
        public string DisplayName { get; set; }

        public IEnumerable<string> FileExtensions { get; set; }
    }
}
