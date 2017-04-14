// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace BCad.Settings
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
