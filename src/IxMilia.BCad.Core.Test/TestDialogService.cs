using System;
using System.Composition;
using System.Threading.Tasks;
using IxMilia.BCad.Services;

namespace IxMilia.BCad.Core.Test
{
    [ExportWorkspaceService, Shared]
    public class TestDialogService : IDialogService
    {
        public static Action<object> ModifyTestFileSettingsTransform = null;

        public Task<object> ShowDialog(string id, object parameter)
        {
            if (id == "FileSettings")
            {
                ModifyTestFileSettingsTransform?.Invoke(parameter);
                return Task.FromResult<object>(true);
            }

            throw new NotImplementedException();
        }
    }
}
