using System.Threading.Tasks;

namespace IxMilia.BCad.Commands
{
    public interface ICadCommand
    {
        Task<bool> Execute(IWorkspace workspace, object arg = null);
    }
}
