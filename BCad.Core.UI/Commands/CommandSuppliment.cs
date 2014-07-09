using System.Collections.Generic;
using System.Windows.Input;

namespace BCad.Commands
{
    public class CommandSuppliment
    {
        public string Name { get; private set; }
        public IEnumerable<string> CommandAliases { get; private set; }
        public ModifierKeys Modifier { get; private set; }
        public Key Key { get; private set; }

        public CommandSuppliment(string name, ModifierKeys modifier, Key key, params string[] aliases)
        {
            Name = name;
            Modifier = modifier;
            Key = key;
            CommandAliases = aliases ?? new string[0];
        }
    }
}
