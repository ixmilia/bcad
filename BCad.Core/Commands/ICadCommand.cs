using System.Threading.Tasks;

namespace BCad.Commands
{
    public interface ICadCommand
    {
        Task<bool> Execute(IWorkspace workspace, object arg = null);
    }
}
