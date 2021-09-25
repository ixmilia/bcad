using System.Threading.Tasks;

namespace IxMilia.BCad.Rpc
{
    internal class RpcServerWorkspace : WorkspaceBase
    {
        public override Task<UnsavedChangesResult> PromptForUnsavedChanges()
        {
            return Task.FromResult(UnsavedChangesResult.Saved);
        }
    }
}
