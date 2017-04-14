// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace BCad.Settings
{
    public interface ISettingMetadata
    {
        string Name { get; }
        Type Type { get; }
        object Value { get; }
    }
}
