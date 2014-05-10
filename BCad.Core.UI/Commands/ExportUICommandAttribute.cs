using System;
using System.Collections.Generic;
using System.Composition;
using System.Windows.Input;

namespace BCad.Commands
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportUICommandAttribute : ExportAttribute, IUICommandMetadata
    {
        public ExportUICommandAttribute(string name, string displayName, params string[] aliases)
            : this(name, displayName, ModifierKeys.None, Key.None, aliases)
        {
        }

        public ExportUICommandAttribute(string name, string displayName, ModifierKeys modifier, Key key, params string[] aliases)
            : base(typeof(IUICommand))
        {
            Name = name;
            DisplayName = displayName;
            Modifier = modifier;
            Key = key;
            CommandAliases = aliases ?? new string[0];
        }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public IEnumerable<string> CommandAliases { get; set; }

        public ModifierKeys Modifier { get; set; }

        public Key Key { get; set; }
    }
}
