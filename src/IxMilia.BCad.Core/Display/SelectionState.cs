using IxMilia.BCad.Collections;

namespace IxMilia.BCad.Display
{
    public struct SelectionState
    {
        public Rect Rectangle { get; }
        public SelectionMode Mode { get; }

        public SelectionState(Rect rect, SelectionMode mode)
        {
            Rectangle = rect;
            Mode = mode;
        }
    }
}
