using System.Threading.Tasks;

namespace BCad.Services
{
    public interface IDialogFactoryService : IWorkspaceService
    {
        Task<bool?> ShowDialog(string type, string id);
    }
}
