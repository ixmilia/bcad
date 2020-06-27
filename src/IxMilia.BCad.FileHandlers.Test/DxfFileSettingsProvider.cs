using System;
using IxMilia.BCad.Core.Test;
using IxMilia.BCad.Services;

namespace IxMilia.BCad.FileHandlers.Test
{
    internal class DxfFileSettingsProvider : IDisposable
    {
        Func<object, object> _lastSettingsProvider;

        public DxfFileSettingsProvider(DxfFileSettings fileSettings)
        {
            _lastSettingsProvider = TestDialogService.ModifyTestFileSettingsTransform;
            TestDialogService.ModifyTestFileSettingsTransform = (existingFileSettings) =>
            {
                var existingSettings = (FileSettings)existingFileSettings;
                var dxf = (DxfFileSettings)existingSettings.Settings;
                dxf.FileVersion = fileSettings.FileVersion;
                return dxf;
            };
        }

        public void Dispose()
        {
            TestDialogService.ModifyTestFileSettingsTransform = _lastSettingsProvider;
        }
    }
}
