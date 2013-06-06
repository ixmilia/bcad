using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace BCad.FilePlotters
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportFilePlotterAttribute : ExportAttribute, IFilePlotterMetadata
    {
        public string DisplayName { get; private set; }

        public IEnumerable<string> FileExtensions { get; private set; }

        public ExportFilePlotterAttribute(string displayName, params string[] fileExtensions)
            : base(typeof(IFilePlotter))
        {
            DisplayName = displayName;
            FileExtensions = fileExtensions;
        }
    }
}
