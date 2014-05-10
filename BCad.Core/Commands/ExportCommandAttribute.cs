using System;
using System.Composition;

namespace BCad.Commands
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportCommandAttribute : ExportAttribute, ICommandMetadata
    {
        public ExportCommandAttribute(string name, string displayName)
            : base(typeof(ICommand))
        {
            Name = name;
            DisplayName = displayName;
        }        

        public string Name { get; set; }

        public string DisplayName { get; set; }
    }
}
