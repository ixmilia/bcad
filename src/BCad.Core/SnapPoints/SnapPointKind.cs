// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace BCad.SnapPoints
{
    [Flags]
    public enum SnapPointKind
    {
        None = 0x00,
        Center = 0x01,
        EndPoint = 0x02,
        MidPoint = 0x04,
        Quadrant = 0x08,
        Focus = 0x10,
        All = Center | EndPoint | MidPoint | Quadrant | Focus
    }
}
