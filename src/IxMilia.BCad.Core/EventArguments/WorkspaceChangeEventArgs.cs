// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace IxMilia.BCad.EventArguments
{
    public class WorkspaceChangeEventArgs : EventArgs
    {
        public bool IsDrawingChange { get; private set; }

        public bool IsDrawingPlaneChange { get; private set; }

        public bool IsActiveViewPortChange { get; private set; }

        public bool IsViewControlChange { get; private set; }

        public bool IsDirtyChange { get; private set; }

        public WorkspaceChangeEventArgs(bool isDrawingChange, bool isDrawingPlaneChange, bool isActiveViewPortChange, bool isViewControlChange, bool isDirtyChange)
        {
            this.IsDrawingChange = isDrawingChange;
            this.IsDrawingPlaneChange = isDrawingPlaneChange;
            this.IsActiveViewPortChange = isActiveViewPortChange;
            this.IsViewControlChange = isViewControlChange;
            this.IsDirtyChange = isDirtyChange;
        }

        public bool IsOnlyDirtyChange()
        {
            return this.IsDirtyChange && !(this.IsDrawingChange || this.IsDrawingPlaneChange || this.IsActiveViewPortChange || this.IsViewControlChange);
        }

        public static WorkspaceChangeEventArgs Reset()
        {
            return new WorkspaceChangeEventArgs(true, true, true, true, true);
        }
    }
}
