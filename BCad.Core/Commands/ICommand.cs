using System.Threading.Tasks;

namespace BCad.Commands
{
    public interface ICommand
    {
        Task<bool> Execute(object arg = null);
    }
}
