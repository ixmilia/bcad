// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace BCad.Settings
{
    [ExportSetting(Debug, typeof(bool), false)]
    public class DefaultSettingsProvider
    {
        public const string Debug = nameof(Debug);
    }
}
