using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace BCad
{
    [Export(typeof(IUndoRedoService))]
    internal class UndoRedoService : IUndoRedoService
    {
        [Import]
        private IWorkspace Workspace = null;

        private Stack<Document> undoHistory = new Stack<Document>();

        private Stack<Document> redoHistory = new Stack<Document>();

        public void SetSnapshot()
        {
            undoHistory.Push(Workspace.Document);
            redoHistory.Clear();
        }

        public void Undo()
        {
            if (UndoHistorySize == 0)
                throw new NotSupportedException("There are no items to undo");
            redoHistory.Push(Workspace.Document);
            Workspace.Document = undoHistory.Pop();
        }

        public void Redo()
        {
            if (RedoHistorySize == 0)
                throw new NotSupportedException("There are no items to redo");
            undoHistory.Push(Workspace.Document);
            Workspace.Document = redoHistory.Pop();
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
