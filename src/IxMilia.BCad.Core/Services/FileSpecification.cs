using System;
using System.Collections.Generic;
using System.Linq;

namespace IxMilia.BCad.Services
{
    public class FileSpecification
    {
        public string DisplayName { get; private set; }

        public IEnumerable<string> FileExtensions { get; private set; }

        public FileSpecification(string displayName, IEnumerable<string> fileExtensions)
        {
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            FileExtensions = fileExtensions ?? throw new ArgumentNullException(nameof(fileExtensions));
            if (!FileExtensions.All(ext => ext.StartsWith(".")))
            {
                throw new ArgumentException("All file extensions must start with a '.'", nameof(fileExtensions));
            }
        }
    }
}
