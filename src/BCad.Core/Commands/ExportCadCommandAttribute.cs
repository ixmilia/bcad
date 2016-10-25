using System;
using System.Collections.Generic;
using System.Composition;

namespace BCad.Commands
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportCadCommandAttribute : ExportAttribute
    {
        public ExportCadCommandAttribute(string name, string displayName, params string[] aliases)
            : this(name, displayName, ModifierKeys.None, Key.None, aliases)
        {
        }

        public ExportCadCommandAttribute(string name, string displayName, ModifierKeys modifier, Key key, params string[] aliases)
            : base(typeof(ICadCommand))
        {
            Name = name;
            DisplayName = displayName;
            CommandAliases = aliases;
            Modifier = modifier;
            Key = key;
        }        

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public IEnumerable<string> CommandAliases { get; set; }

        public ModifierKeys Modifier { get; set; }

        public Key Key { get; set; }
    }
}
