using System;
using System.Collections.Generic;
using System.Composition;
using IxMilia.BCad.EventArguments;

namespace IxMilia.BCad.Services
{
    [ExportWorkspaceService, Shared]
    internal class UndoRedoService : IUndoRedoService
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        private Stack<Drawing> undoHistory = new Stack<Drawing>();
        private Stack<Drawing> redoHistory = new Stack<Drawing>();
        private bool ignoreDrawingChange = false;

        [OnImportsSatisfied]
        public void OnImportsSatisfied()
        {
            Workspace.WorkspaceChanging += WorkspaceChanging;
        }

        private void WorkspaceChanging(object sender, WorkspaceChangeEventArgs e)
        {
            if (e.IsDrawingChange && ! ignoreDrawingChange && !e.IsOnlyDirtyChange())
            {
                // save the last snapshot
                undoHistory.Push(Workspace.Drawing);
                redoHistory.Clear();
            }
        }

        public void Undo()
        {
            if (UndoHistorySize == 0)
                throw new NotSupportedException("There are no items to undo");

            ignoreDrawingChange = true;
            redoHistory.Push(Workspace.Drawing);
            Workspace.Update(drawing: undoHistory.Pop());
            ignoreDrawingChange = false;
        }

        public void Redo()
        {
            if (RedoHistorySize == 0)
                throw new NotSupportedException("There are no items to redo");

            ignoreDrawingChange = true;
            undoHistory.Push(Workspace.Drawing);
            Workspace.Update(drawing: redoHistory.Pop());
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
