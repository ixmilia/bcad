using System;

namespace BCad.EventArguments
{
    public class WorkspaceChangeEventArgs : EventArgs
    {
        public bool IsDrawingChange { get; private set; }

        public bool IsDrawingPlaneChange { get; private set; }

        public bool IsActiveViewPortChange { get; private set; }

        public bool IsViewControlChange { get; private set; }

        public WorkspaceChangeEventArgs(bool isDrawingChange, bool isDrawingPlaneChange, bool isActiveViewPortChange, bool isViewControlChange)
        {
            this.IsDrawingChange = isDrawingChange;
            this.IsDrawingPlaneChange = isDrawingPlaneChange;
            this.IsActiveViewPortChange = isActiveViewPortChange;
            this.IsViewControlChange = isViewControlChange;
        }

        public static WorkspaceChangeEventArgs UpdateAll()
        {
            return new WorkspaceChangeEventArgs(true, true, true, true);
        }
    }
}
