using System;
using System.Composition;

namespace BCad.UI
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportViewControlAttribute : ExportAttribute
    {
        /// <summary>
        /// The unique ID of the exported control.  Specified in the config file.
        /// </summary>
        public string ControlId { get; private set; }

        public ExportViewControlAttribute(string controlId)
            : base(typeof(IViewControl))
        {
            this.ControlId = controlId;
        }
    }
}
