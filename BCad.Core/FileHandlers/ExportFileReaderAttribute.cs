using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace BCad.FileHandlers
{
    public class ExportFileReaderAttribute : ExportAttribute
    {
        public string DisplayName { get; private set; }

        public IEnumerable<string> FileExtensions { get; private set; }

        public ExportFileReaderAttribute(string displayName, params string[] fileExtensions)
            : base(typeof(IFileReader))
        {
            DisplayName = displayName;
            FileExtensions = fileExtensions;
        }
    }
}
