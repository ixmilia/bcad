using System;
using System.ComponentModel.Composition;

namespace BCad.UI
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportConsoleAttribute : ExportAttribute
    {
        public string ControlId { get; set; }

        public ExportConsoleAttribute(string controlId)
            : base(typeof(ConsoleControl))
        {
            this.ControlId = controlId;
        }
    }
}
