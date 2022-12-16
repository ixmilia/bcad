using System.Threading.Tasks;

namespace IxMilia.BCad
{
    internal class ConsoleWorkspace : WorkspaceBase
    {
        public override Task<UnsavedChangesResult> PromptForUnsavedChanges()
        {
            // no prompting
            return Task.FromResult(UnsavedChangesResult.Saved);
        }
    }
}
