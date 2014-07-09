using System.Collections.Generic;
using System.Windows.Input;

namespace BCad.Commands
{
    public interface IUICommandMetadata : ICommandMetadata
    {
        IEnumerable<string> CommandAliases { get; }
        ModifierKeys Modifier { get; }
        Key Key { get; }
    }
}
