using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using BCad.Commands;
using BCad.EventArguments;

namespace BCad
{
    [Export(typeof(IWorkspace))]
    internal class Workspace : IWorkspace
    {
        private Document document;

        public Document Document
        {
            get { return document; }
            set
            {
                var args = new DocumentChangingEventArgs(document, value);
                OnDocumentChanging(args);
                if (args.Cancel)
                    return;
                document = value;
                OnDocumentChanged(new DocumentChangedEventArgs(document));
            }
        }

        [Import]
        public IUserConsole UserConsole { get; private set; }

        [Import]
        public ICommandManager CommandManager { get; private set; }

        [Import]
        public IView View { get; private set; }

        public Workspace()
        {
        }

        public event DocumentChangingEventHandler DocumentChanging;

        protected virtual void OnDocumentChanging(DocumentChangingEventArgs e)
        {
            if (DocumentChanging != null)
                DocumentChanging(this, e);
        }

        public event DocumentChangedEventHandler DocumentChanged;

        protected virtual void OnDocumentChanged(DocumentChangedEventArgs e)
        {
            if (DocumentChanged != null)
                DocumentChanged(this, e);
        }

        public UnsavedChangesResult PromptForUnsavedChanges()
        {
            var result = UnsavedChangesResult.Discarded;
            if (Document.Dirty)
            {
                string filename = Document.FileName ?? "(Untitled)";
                var dialog = MessageBox.Show(string.Format("Save changes to '{0}'?", filename),
                    "Unsaved changes",
                    MessageBoxButton.YesNoCancel);
                switch (dialog)
                {
                    case MessageBoxResult.Yes:
                        if (CommandManager.ExecuteCommand("File.Save", Document.FileName))
                            result = UnsavedChangesResult.Saved;
                        else
                            result = UnsavedChangesResult.Cancel;
                        break;
                    case MessageBoxResult.No:
                        result = UnsavedChangesResult.Discarded;
                        break;
                    case MessageBoxResult.Cancel:
                        result = UnsavedChangesResult.Cancel;
                        break;
                }
            }
            else
            {
                result = UnsavedChangesResult.Saved;
            }

            return result;
        }
    }
}
