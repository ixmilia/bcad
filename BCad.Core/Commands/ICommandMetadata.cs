using System.Collections.Generic;

namespace BCad.Commands
{
    public interface ICommandMetadata
    {
        string Name { get; }
        string DisplayName { get; }
        IEnumerable<string> CommandAliases { get; }
        ModifierKeys Modifier { get; }
        Key Key { get; }
    }
}
