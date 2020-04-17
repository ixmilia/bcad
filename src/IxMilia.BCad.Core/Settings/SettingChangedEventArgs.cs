using System;

namespace IxMilia.BCad.Settings
{
    public class SettingChangedEventArgs : EventArgs
    {
        public string SettingName { get; }
        public Type Type { get; }
        public object OldValue { get; }
        public object NewValue { get; }

        public SettingChangedEventArgs(string settingName, Type type, object oldValue, object newValue)
        {
            SettingName = settingName;
            Type = type;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
