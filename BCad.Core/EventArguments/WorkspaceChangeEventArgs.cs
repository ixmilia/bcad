using System;

namespace BCad.EventArguments
{
    public class WorkspaceChangeEventArgs : EventArgs
    {
        public bool IsDrawingChange { get; private set; }

        public bool IsDrawingPlaneChange { get; private set; }

        public WorkspaceChangeEventArgs(bool isDrawingChange, bool isDrawingPlaneChange)
        {
            this.IsDrawingChange = isDrawingChange;
            this.IsDrawingPlaneChange = isDrawingPlaneChange;
        }
    }
}
