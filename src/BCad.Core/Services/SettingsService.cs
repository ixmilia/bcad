// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Reflection;
using IxMilia.BCad.Settings;
using IxMilia.Config;

namespace IxMilia.BCad.Services
{
    [ExportWorkspaceService, Shared]
    internal class SettingsService : ISettingsService
    {
        private Dictionary<string, Tuple<Type, object>> _settings = new Dictionary<string, Tuple<Type, object>>();

        public event SettingChangedEventHandler SettingChanged;

        [Import]
        public IWorkspace Workspace { get; set; }

        [ImportMany]
        public IEnumerable<Lazy<object, SettingMetadata>> Settings { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied()
        {
            foreach (var setting in Settings)
            {
                var settingName = setting.Metadata.Name;
                var value = setting.Metadata.Value;
                if (_settings.ContainsKey(settingName))
                {
                    Workspace.OutputService.WriteLine($"The setting '{settingName}' has already been exported by another component and will not be used again.");
                }
                else
                {
                    _settings[settingName] = Tuple.Create(setting.Metadata.Type, (object)null);
                    SetValue(settingName, value, ignoreTypeCheck: true);
                }
            }
        }

        public T GetValue<T>(string settingName)
        {
            if (!_settings.TryGetValue(settingName, out var pair))
            {
                //return default(T);
                throw new InvalidOperationException($"The requested setting '{settingName}' does not exist.");
            }

            if (pair.Item1 != typeof(T))
            {
                throw new InvalidOperationException($"The setting '{settingName}' is of type '{pair.Item1.Name}' but type '{typeof(T).Name}' was requested.");
            }

            return (T)pair.Item2;
        }

        public void SetValue<T>(string settingName, T value)
        {
            SetValue(settingName, value, ignoreTypeCheck: false);
        }

        private void SetValue<T>(string settingName, T value, bool ignoreTypeCheck = false)
        {
            if (!_settings.TryGetValue(settingName, out var pair))
            {
                return;
            }

            var type = pair.Item1;
            var oldValue = pair.Item2;
            object valueToSet = value;
            if (value is string && type != typeof(string))
            {
                // a terrible hack to get the appropriate parse method
                var tryParseFunction = typeof(ConfigExtensions).GetRuntimeMethods()
                    .Single(m => m.Name == nameof(ConfigExtensions.TryParseValue) && m.GetParameters().Length == 2);
                tryParseFunction = tryParseFunction.MakeGenericMethod(type);
                var parameters = new object[] { value, null };
                if (!(bool)tryParseFunction.Invoke(null, parameters))
                {
                    return;
                }

                valueToSet = parameters[1];
            }
            else if (!ignoreTypeCheck && type != typeof(T))
            {
                return;
            }

            if (Equals(pair.Item2, valueToSet))
            {
                return;
            }

            _settings[settingName] = Tuple.Create(type, valueToSet);
            SettingChanged?.Invoke(this, new SettingChangedEventArgs(settingName, type, oldValue, value));
        }

        public void LoadFromLines(string[] lines)
        {
            var fileValues = new Dictionary<string, string>();
            fileValues.ParseConfig(lines);
            foreach (var kvp in fileValues)
            {
                SetValue(kvp.Key, kvp.Value);
            }
        }

        public string WriteWithLines(string[] existingLines)
        {
            var values = new Dictionary<string, string>();
            foreach (var kvp in _settings)
            {
                values[kvp.Key] = kvp.Value.Item2.ToConfigString();
            }

            var newContent = values.WriteConfig(existingLines);
            return newContent;
        }
    }
}
