using System.Composition;

namespace IxMilia.BCad.Services
{
    public class ExportWorkspaceServiceAttribute : ExportAttribute
    {
        public ExportWorkspaceServiceAttribute()
            : base(typeof(IWorkspaceService))
        {
        }
    }
}
