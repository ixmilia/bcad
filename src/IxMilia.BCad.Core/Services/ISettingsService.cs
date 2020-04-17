using System.Collections.Generic;
using IxMilia.BCad.Settings;

namespace IxMilia.BCad.Services
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
