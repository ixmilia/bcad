using System;

namespace IxMilia.BCad.Display
{
    [Flags]
    public enum CursorState
    {
        None = 0,
        Point = 1,
        Object = 2,
        Text = 4,
    }
}
