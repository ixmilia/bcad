using System;

namespace BCad.EventArguments
{
    public class WorkspaceChangeEventArgs : EventArgs
    {
        public bool IsDrawingChange { get; private set; }

        public bool IsDrawingPlaneChange { get; private set; }

        public bool IsActiveViewPortChange { get; private set; }

        public bool IsViewControlChange { get; private set; }

        public bool IsDirtyChange { get; private set; }

        public bool IsRubberBandGeneratorChange { get; private set; }

        public WorkspaceChangeEventArgs(bool isDrawingChange, bool isDrawingPlaneChange, bool isActiveViewPortChange, bool isViewControlChange, bool isRubberBandGeneratorChange, bool isDirtyChange)
        {
            this.IsDrawingChange = isDrawingChange;
            this.IsDrawingPlaneChange = isDrawingPlaneChange;
            this.IsActiveViewPortChange = isActiveViewPortChange;
            this.IsViewControlChange = isViewControlChange;
            this.IsRubberBandGeneratorChange = isRubberBandGeneratorChange;
            this.IsDirtyChange = isDirtyChange;

        }

        public bool IsOnlyDirtyChange()
        {
            return this.IsDirtyChange && !(this.IsDrawingChange || this.IsDrawingPlaneChange || this.IsActiveViewPortChange || this.IsViewControlChange || this.IsRubberBandGeneratorChange);
        }

        public static WorkspaceChangeEventArgs Reset()
        {
            return new WorkspaceChangeEventArgs(true, true, true, true, true, false);
        }
    }
}
