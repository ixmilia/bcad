using System;

namespace IxMilia.BCad.SnapPoints
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
