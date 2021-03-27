using System.Collections.Generic;
using IxMilia.BCad.EventArguments;

namespace IxMilia.BCad.Services
{
    internal class UndoRedoService : IUndoRedoService
    {
        private IWorkspace _workspace;
        private Stack<Drawing> undoHistory = new Stack<Drawing>();
        private Stack<Drawing> redoHistory = new Stack<Drawing>();
        private bool ignoreDrawingChange = false;

        public UndoRedoService(IWorkspace workspace)
        {
            _workspace = workspace;
            _workspace.WorkspaceChanging += WorkspaceChanging;
        }

        private void WorkspaceChanging(object sender, WorkspaceChangeEventArgs e)
        {
            if (e.IsDrawingChange && ! ignoreDrawingChange && !e.IsOnlyDirtyChange())
            {
                // save the last snapshot
                undoHistory.Push(_workspace.Drawing);
                redoHistory.Clear();
            }
        }

        public void Undo()
        {
            if (UndoHistorySize == 0)
            {
                // nothing to undo
                return;
            }

            ignoreDrawingChange = true;
            redoHistory.Push(_workspace.Drawing);
            _workspace.Update(drawing: undoHistory.Pop());
            ignoreDrawingChange = false;
        }

        public void Redo()
        {
            if (RedoHistorySize == 0)
            {
                // nothing to redo
                return;
            }

            ignoreDrawingChange = true;
            undoHistory.Push(_workspace.Drawing);
            _workspace.Update(drawing: redoHistory.Pop());
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
