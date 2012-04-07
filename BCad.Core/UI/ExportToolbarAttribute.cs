using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace BCad.UI
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportToolbarAttribute : ExportAttribute
    {
        public ExportToolbarAttribute()
            : base(typeof(ToolBar))
        {
        }
    }
}
