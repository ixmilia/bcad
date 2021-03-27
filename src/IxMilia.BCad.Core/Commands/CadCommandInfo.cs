using System;
using System.Collections.Generic;
using System.Linq;

namespace IxMilia.BCad.Commands
{
    public class CadCommandInfo
    {
        public string Name { get; }

        public string DisplayName { get; }

        public IReadOnlyList<string> Aliases { get; }

        public Key Key { get; }

        public ModifierKeys Modifier { get; }

        public ICadCommand Command { get; }

        public CadCommandInfo(string name, string displayName, ICadCommand command, params string[] aliases)
            : this(name, displayName, command, ModifierKeys.None, Key.None, aliases)
        {
        }

        public CadCommandInfo(string name, string displayName, ICadCommand command, ModifierKeys modifier, Key key, params string[] aliases)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Modifier = modifier;
            Key = key;
            Aliases = aliases.ToList();
        }
    }
}
