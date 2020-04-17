using System.Collections.Generic;

namespace IxMilia.BCad.Services
{
    public class FileSpecification
    {
        public string DisplayName { get; private set; }

        public IEnumerable<string> FileExtensions { get; private set; }

        public FileSpecification(string displayName, IEnumerable<string> fileExtensions)
        {
            DisplayName = displayName;
            FileExtensions = fileExtensions;
        }
    }
}
