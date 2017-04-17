// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using BCad.Core.Test;

namespace BCad.FileHandlers.Test
{
    internal class DxfFileSettingsProvider : IDisposable
    {
        Action<INotifyPropertyChanged> _lastSettingsProvider;

        public DxfFileSettingsProvider(DxfFileSettings fileSettings)
        {
            _lastSettingsProvider = TestDialogFactoryService.ModifyTestFileSettingsTransform;
            TestDialogFactoryService.ModifyTestFileSettingsTransform = (existingFileSettings) =>
            {
                var dxf = (DxfFileSettings)existingFileSettings;
                dxf.FileVersion = fileSettings.FileVersion;
            };
        }

        public void Dispose()
        {
            TestDialogFactoryService.ModifyTestFileSettingsTransform = _lastSettingsProvider;
        }
    }
}
