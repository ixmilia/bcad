using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;

namespace BCad.Commands
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportCommandAttribute : ExportAttribute, ICommandMetadata
    {
        public ExportCommandAttribute(string name, string displayName, params string[] aliases)
            : this(name, displayName, ModifierKeys.None, Key.None, aliases)
        {
        }

        public ExportCommandAttribute(string name, string displayName, ModifierKeys modifier, Key key, params string[] aliases)
            : base(typeof(ICommand))
        {
            this.Name = name;
            this.DisplayName = displayName;
            this.Modifier = modifier;
            this.Key = key;
            this.CommandAliases = aliases ?? new string[0];
        }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public IEnumerable<string> CommandAliases { get; set; }

        public ModifierKeys Modifier { get; set; }

        public Key Key { get; set; }
    }
}
