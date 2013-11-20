using System.Collections.Generic;

namespace BCad.Commands
{
    public class CommandMetadata : ICommandMetadata
    {
        public IEnumerable<string> CommandAliases { get; set; }

        public string DisplayName { get; set; }

        public Key Key { get; set; }

        public ModifierKeys Modifier { get; set; }

        public string Name { get; set; }
    }
}
