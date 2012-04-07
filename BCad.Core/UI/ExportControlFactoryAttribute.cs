using System;
using System.ComponentModel.Composition;

namespace BCad.UI
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportControlFactoryAttribute : ExportAttribute
    {
        public string ControlId { get; private set; }

        public string Title { get; private set; }

        public ExportControlFactoryAttribute(string controlId, string title)
            : base(typeof(IControlFactory))
        {
            this.ControlId = controlId;
            this.Title = title;
        }
    }
}
