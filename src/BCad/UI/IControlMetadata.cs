// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace IxMilia.BCad.UI
{
    public interface IControlMetadata
    {
        string ControlType { get; }
        string ControlId { get; }
        string Title { get; }
    }
}
