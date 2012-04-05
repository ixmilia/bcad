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
using System.Xml.Serialization;
using System.IO;

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
                if (document == value)
                    return;
                var args = new DocumentChangingEventArgs(document, value);
                OnDocumentChanging(args);
                if (args.Cancel)
                    return;
                document = value;
                OnDocumentChanged(new DocumentChangedEventArgs(document));
                if (document.Layers.ContainsKey("Default"))
                    CurrentLayer = document.Layers["Default"];
                else
                    CurrentLayer = document.Layers.Values.First();
            }
        }

        private Layer currentLayer;

        public Layer CurrentLayer
        {
            get { return currentLayer; }
            set
            {
                if (currentLayer == value)
                    return;
                var args = new LayerChangingEventArgs(currentLayer, value);
                OnCurrentLayerChanging(args);
                if (args.Cancel)
                    return;
                currentLayer = value;
                OnCurrentLayerChanged(new LayerChangedEventArgs(currentLayer));
            }
        }

        [Import]
        public IUserConsole UserConsole { get; private set; }

        [Import]
        public ICommandManager CommandManager { get; private set; }

        [Import]
        public IView View { get; private set; }

        [Import]
        public IUndoRedoService UndoRedoService { get; private set; }

        [Import]
        public IDialogFactory DialogFactory { get; private set; }

        [Import]
        public IControlFactory ControlFactory { get; private set; }

        public ISettingsManager SettingsManager { get; private set; }

        public Workspace()
        {
        }

        public void LoadSettings(string path)
        {
            var serializer = new XmlSerializer(typeof(SettingsManager));
            var stream = new FileStream(path, FileMode.Open);
            this.SettingsManager = (SettingsManager)serializer.Deserialize(stream);
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

        public event CurrentLayerChangingEventHandler CurrentLayerChanging;

        protected virtual void OnCurrentLayerChanging(LayerChangingEventArgs e)
        {
            if (CurrentLayerChanging != null)
                CurrentLayerChanging(this, e);
        }

        public event CurrentLayerChangedEventHandler CurrentLayerChanged;

        protected virtual void OnCurrentLayerChanged(LayerChangedEventArgs e)
        {
            if (CurrentLayerChanged != null)
                CurrentLayerChanged(this, e);
        }

        public UnsavedChangesResult PromptForUnsavedChanges()
        {
            var result = UnsavedChangesResult.Discarded;
            if (Document.IsDirty)
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

        public void Focus()
        {
            Application.Current.Dispatcher.Invoke((Action)(() => Application.Current.MainWindow.Focus()));
        }
    }
}
