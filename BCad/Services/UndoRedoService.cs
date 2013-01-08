using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using BCad.EventArguments;

namespace BCad.Services
{
    [Export(typeof(IUndoRedoService))]
    internal class UndoRedoService : IUndoRedoService, IPartImportsSatisfiedNotification
    {
        [Import]
        private IWorkspace workspace = null;
        private Stack<Drawing> undoHistory = new Stack<Drawing>();
        private Stack<Drawing> redoHistory = new Stack<Drawing>();
        private bool ignoreDrawingChange = false;

        public void OnImportsSatisfied()
        {
            this.workspace.WorkspaceChanging += WorkspaceChanging;
        }

        private void WorkspaceChanging(object sender, WorkspaceChangeEventArgs e)
        {
            if (e.IsDrawingChange && ! ignoreDrawingChange && !e.IsOnlyDirtyChange())
            {
                // save the last snapshot
                undoHistory.Push(workspace.Drawing);
                redoHistory.Clear();
            }
        }

        public void Undo()
        {
            if (UndoHistorySize == 0)
                throw new NotSupportedException("There are no items to undo");

            ignoreDrawingChange = true;
            redoHistory.Push(workspace.Drawing);
            workspace.Update(drawing: undoHistory.Pop());
            ignoreDrawingChange = false;
        }

        public void Redo()
        {
            if (RedoHistorySize == 0)
                throw new NotSupportedException("There are no items to redo");

            ignoreDrawingChange = true;
            undoHistory.Push(workspace.Drawing);
            workspace.Update(drawing: redoHistory.Pop());
            ignoreDrawingChange = false;
        }

        public void ClearHistory()
        {
            undoHistory.Clear();
            redoHistory.Clear();
        }

        public int UndoHistorySize
        {
            get { return undoHistory.Count; }
        }

        public int RedoHistorySize
        {
            get { return redoHistory.Count; }
        }
    }
}
