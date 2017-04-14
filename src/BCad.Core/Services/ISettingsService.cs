// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using BCad.Settings;

namespace BCad.Services
{
    public delegate void SettingChangedEventHandler(object sender, SettingChangedEventArgs e);

    public interface ISettingsService : IWorkspaceService
    {
        event SettingChangedEventHandler SettingChanged;
        T GetValue<T>(string settingName);
        void SetValue<T>(string settingName, T value);
        void LoadFromLines(string[] lines);
        string WriteWithLines(string[] existingLines);
    }
}
