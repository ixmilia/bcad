using System.Threading.Tasks;

namespace IxMilia.BCad.Rpc
{
    internal class RpcServerWorkspace : WorkspaceBase
    {
        public override async Task<UnsavedChangesResult> PromptForUnsavedChanges()
        {
            if (!IsDirty)
            {
                return UnsavedChangesResult.Saved;
            }

            var result = await DialogService.ShowDialog("saveChanges", null);
            if (result is UnsavedChangesResult unsavedChangesResult)
            {
                if (unsavedChangesResult == UnsavedChangesResult.Saved)
                {
                    var saveResult = await ExecuteCommand("File.Save", null);
                    if (!saveResult)
                    {
                        return UnsavedChangesResult.Cancel;
                    }
                }

                return unsavedChangesResult;
            }

            return UnsavedChangesResult.Cancel;
        }
    }
}
