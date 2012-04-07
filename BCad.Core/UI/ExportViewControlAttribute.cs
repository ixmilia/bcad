using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;

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
            : base(typeof(ViewControl))
        {
            this.ControlId = controlId;
        }
    }
}
