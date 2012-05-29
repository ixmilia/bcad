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
        private Stack<Drawing> undoHistory = new Stack<Drawing>();
        private Stack<Drawing> redoHistory = new Stack<Drawing>();
        private bool ignoreDrawingChange = false;

        public void OnImportsSatisfied()
        {
            this.workspace.PropertyChanging += WorkspacePropertyChanging;
        }

        void WorkspacePropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case Constants.DrawingString:
                    if (!ignoreDrawingChange)
                    {
                        // save the last snapshot
                        undoHistory.Push(workspace.Drawing);
                        redoHistory.Clear();
                    }
                    break;
            }
        }

        public void Undo()
        {
            if (UndoHistorySize == 0)
                throw new NotSupportedException("There are no items to undo");

            ignoreDrawingChange = true;
            redoHistory.Push(workspace.Drawing);
            workspace.Drawing = undoHistory.Pop();
            ignoreDrawingChange = false;
        }

        public void Redo()
        {
            if (RedoHistorySize == 0)
                throw new NotSupportedException("There are no items to redo");

            ignoreDrawingChange = true;
            undoHistory.Push(workspace.Drawing);
            workspace.Drawing = redoHistory.Pop();
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
