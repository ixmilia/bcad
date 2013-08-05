using System;
using System.ComponentModel.Composition;
using Microsoft.Windows.Controls.Ribbon;

namespace BCad.Ribbons
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportRibbonTabAttribute : ExportAttribute, IRibbonTabMetadata
    {
        public ExportRibbonTabAttribute(string id)
            : base(typeof(RibbonTab))
        {
            Id = id;
        }

        public string Id { get; private set; }
    }
}
