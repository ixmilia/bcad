using System;

namespace IxMilia.BCad.Settings
{
    public class SettingMetadata : ISettingMetadata
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public object Value { get; set; }
    }
}
