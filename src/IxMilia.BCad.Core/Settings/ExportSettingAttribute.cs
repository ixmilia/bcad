using System;
using System.Composition;

namespace IxMilia.BCad.Settings
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ExportSettingAttribute : ExportAttribute, ISettingMetadata
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public object Value { get; set; }

        public ExportSettingAttribute(string name, Type type, object value)
            : base(typeof(object))
        {
            Name = name;
            Type = type;
            Value = value;
        }
    }
}
