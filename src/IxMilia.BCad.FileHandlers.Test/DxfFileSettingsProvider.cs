// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using IxMilia.BCad.Core.Test;

namespace IxMilia.BCad.FileHandlers.Test
{
    internal class DxfFileSettingsProvider : IDisposable
    {
        Action<object> _lastSettingsProvider;

        public DxfFileSettingsProvider(DxfFileSettings fileSettings)
        {
            _lastSettingsProvider = TestDialogService.ModifyTestFileSettingsTransform;
            TestDialogService.ModifyTestFileSettingsTransform = (existingFileSettings) =>
            {
                var dxf = (DxfFileSettings)existingFileSettings;
                dxf.FileVersion = fileSettings.FileVersion;
            };
        }

        public void Dispose()
        {
            TestDialogService.ModifyTestFileSettingsTransform = _lastSettingsProvider;
        }
    }
}
