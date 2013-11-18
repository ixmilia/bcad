using System;
using System.ComponentModel.Composition;

namespace BCad.UI
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportControlAttribute : ExportAttribute, IControlMetadata
    {
        public string ControlType { get; private set; }

        public string ControlId { get; private set; }

        public string Title { get; private set; }

        public ExportControlAttribute(string controlType, string controlId, string title)
            : base(typeof(BCadControl))
        {
            ControlType = controlType;
            ControlId = controlId;
            Title = title;
        }
    }
}
