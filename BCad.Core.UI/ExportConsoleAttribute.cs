using System;
using System.Composition;

namespace BCad.UI
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportConsoleAttribute : ExportAttribute, IConsoleMetadata
    {
        public string ControlId { get; set; }

        public ExportConsoleAttribute(string controlId)
            : base(typeof(ConsoleControl))
        {
            this.ControlId = controlId;
        }
    }
}
