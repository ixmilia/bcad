using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace BCad.FileHandlers
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportFileHandlerAttribute : ExportAttribute, IFileHandlerMetadata
    {
        public string DisplayName { get; private set; }

        public IEnumerable<string> FileExtensions { get; private set; }

        public bool CanRead { get; private set; }

        public bool CanWrite { get; private set; }

        public ExportFileHandlerAttribute(string displayName, bool canRead, bool canWrite, params string[] fileExtensions)
            : base(typeof(IFileHandler))
        {
            DisplayName = displayName;
            CanRead = canRead;
            CanWrite = canWrite;
            FileExtensions = fileExtensions;
        }
    }
}
