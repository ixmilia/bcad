using System.Composition;

namespace BCad.Services
{
    public class ExportWorkspaceServiceAttribute : ExportAttribute
    {
        public ExportWorkspaceServiceAttribute()
            : base(typeof(IWorkspaceService))
        {
        }
    }
}
