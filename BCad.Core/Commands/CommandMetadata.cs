using System.Collections.Generic;

namespace BCad.Commands
{
    public class CommandMetadata
    {
        public string DisplayName { get; set; }

        public string Name { get; set; }

        public IEnumerable<string> CommandAliases { get; set; }

        public Key Key { get; set; }

        public ModifierKeys Modifier { get; set; }
    }
}
