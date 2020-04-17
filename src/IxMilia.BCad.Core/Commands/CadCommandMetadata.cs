using System.Collections.Generic;

namespace IxMilia.BCad.Commands
{
    public class CadCommandMetadata
    {
        public string DisplayName { get; set; }

        public string Name { get; set; }

        public IEnumerable<string> CommandAliases { get; set; }

        public Key Key { get; set; }

        public ModifierKeys Modifier { get; set; }
    }
}
