using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

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
