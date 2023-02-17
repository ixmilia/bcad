using System.Threading.Tasks;

namespace IxMilia.BCad.Lisp.Test
{
    public class TestLispWorkspace : LispWorkspace
    {
        public override Task<UnsavedChangesResult> PromptForUnsavedChanges()
        {
            return Task.FromResult(UnsavedChangesResult.Saved);
        }
    }
}
