using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BCad
{
    [Export(typeof(IUndoRedoService))]
    internal class UndoRedoService : IUndoRedoService, IPartImportsSatisfiedNotification
    {
        [Import]
        private IWorkspace workspace = null;
        private Stack<Document> undoHistory = new Stack<Document>();
        private Stack<Document> redoHistory = new Stack<Document>();
        private bool ignoreDocumentChange = false;

        public void OnImportsSatisfied()
        {
            this.workspace.PropertyChanging += WorkspacePropertyChanging;
        }

        void WorkspacePropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Document":
                    if (!ignoreDocumentChange)
                    {
                        // save the last snapshot
                        undoHistory.Push(workspace.Document);
                        redoHistory.Clear();
                    }
                    break;
            }
        }

        public void Undo()
        {
            if (UndoHistorySize == 0)
                throw new NotSupportedException("There are no items to undo");

            ignoreDocumentChange = true;
            redoHistory.Push(workspace.Document);
            workspace.Document = undoHistory.Pop();
            ignoreDocumentChange = false;
        }

        public void Redo()
        {
            if (RedoHistorySize == 0)
                throw new NotSupportedException("There are no items to redo");

            ignoreDocumentChange = true;
            undoHistory.Push(workspace.Document);
            workspace.Document = redoHistory.Pop();
            ignoreDocumentChange = false;
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
