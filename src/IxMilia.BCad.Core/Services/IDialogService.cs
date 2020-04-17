using System.Threading.Tasks;

namespace IxMilia.BCad.Services
{
    public interface IDialogService : IWorkspaceService
    {
        Task<object> ShowDialog(string id, object parameter);
    }
}
