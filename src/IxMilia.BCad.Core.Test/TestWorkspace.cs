using System.Composition;
using System.Threading.Tasks;

namespace IxMilia.BCad.Core.Test
{
    [Export(typeof(IWorkspace)), Shared]
    public class TestWorkspace : WorkspaceBase
    {
        public override Task<UnsavedChangesResult> PromptForUnsavedChanges()
        {
            return Task.FromResult(UnsavedChangesResult.Saved);
        }
    }
}
