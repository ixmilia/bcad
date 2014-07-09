using System.Collections.Generic;
using System.Windows.Input;

namespace BCad.Commands
{
    public class UICommandMetadata : IUICommandMetadata
    {
        public IEnumerable<string> CommandAliases { get; set; }

        public string DisplayName { get; set; }

        public Key Key { get; set; }

        public ModifierKeys Modifier { get; set; }

        public string Name { get; set; }
    }
}
