using BCad.Commands;
using System.Collections.Generic;

namespace BCad.Core.UI
{
    public interface IUIWorkspace : IWorkspace
    {
        IEnumerable<CommandSuppliment> SupplimentedCommands { get; }
    }
}
