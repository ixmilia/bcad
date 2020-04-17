using System;

namespace IxMilia.BCad.Settings
{
    public interface ISettingMetadata
    {
        string Name { get; }
        Type Type { get; }
        object Value { get; }
    }
}
