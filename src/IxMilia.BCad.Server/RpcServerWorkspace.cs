using System;
using System.Collections.Generic;
using System.Composition;
using System.Text;
using System.Threading.Tasks;

namespace IxMilia.BCad.Server
{
    [Export(typeof(IWorkspace)), Shared]
    internal class RpcServerWorkspace : WorkspaceBase
    {
        public override Task<UnsavedChangesResult> PromptForUnsavedChanges()
        {
            return Task.FromResult(UnsavedChangesResult.Saved);
        }
    }
}
