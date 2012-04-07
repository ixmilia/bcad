using System;
using System.ComponentModel.Composition;

namespace BCad.UI
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportControlFactoryAttribute : ExportAttribute
    {
        /// <summary>
        /// The unique ID of the exported control.  Specified in the config file.
        /// </summary>
        public string ControlId { get; private set; }

        /// <summary>
        /// The title the window containing the control should have.
        /// </summary>
        public string Title { get; private set; }

        public ExportControlFactoryAttribute(string controlId, string title)
            : base(typeof(IControlFactory))
        {
            this.ControlId = controlId;
            this.Title = title;
        }
    }
}
