using System.Threading.Tasks;
using IxMilia.BCad;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Services;

namespace bcad
{
    internal class ConsoleWorkspace : WorkspaceBase
    {
        public ConsoleWorkspace()
        {
            var passThroughFileSystemService = new PassThroughFileSystemService(InputService);
            RegisterService<IFileSystemService>(passThroughFileSystemService);
            this.RegisterFileHandlers();
        }

        public override Task<UnsavedChangesResult> PromptForUnsavedChanges()
        {
            // no prompting
            return Task.FromResult(UnsavedChangesResult.Saved);
        }
    }
}
