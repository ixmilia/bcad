using System;

namespace IxMilia.BCad.Display
{
    [Flags]
    public enum CursorState
    {
        None = 0,
        Point = 1 << 0,
        Object = 1 << 1,
        Text = 1 << 2,
        Pan = 1 << 3,
    }
}
