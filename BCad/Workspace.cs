using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Xml.Serialization;
using BCad.EventArguments;

namespace BCad
{
    [Export(typeof(IWorkspace))]
    internal class Workspace : IWorkspace
    {
        private Document document = new Document();

        public Document Document
        {
            get { return document; }
            set
            {
                if (value == null)
                    throw new NotSupportedException("Null document not allowed.");
                if (document == value)
                    return;
                var args = new DocumentChangingEventArgs(document, value);
                OnDocumentChanging(args);
                if (args.Cancel)
                    return;

                // ensure the same layer is selected after the change
                var currentLayerName = CurrentLayer.Name;

                // change the value and fire events
                document = value;
                OnDocumentChanged(new DocumentChangedEventArgs(document));

                // reset the current layer
                if (document.Layers.ContainsKey(currentLayerName))
                    this.CurrentLayer = document.Layers[currentLayerName];
                else if (document.Layers.ContainsKey("Default"))
                    this.CurrentLayer = document.Layers["Default"];
                else
                    this.CurrentLayer = document.Layers.Values.First();
            }
        }

        private Layer currentLayer;

        public Layer CurrentLayer
        {
            get
            {
                if (currentLayer == null)
                    currentLayer = document.Layers.First().Value;
                return currentLayer;
            }
            set
            {
                if (value == null)
                    throw new NotSupportedException("Null layer not allowed.");
                if (!document.Layers.ContainsValue(value))
                    throw new NotSupportedException("Specified layer is not part of the current document.");
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
        private IUserConsole UserConsole = null;

        [Import]
        private ICommandManager CommandManager = null;

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

        public void ExecuteCommand(string commandName, params object[] args)
        {
            Debug.Assert(commandName != null, "Null command not supported");
            ThreadPool.QueueUserWorkItem((_) =>
                {
                    var command = CommandManager.GetCommand(commandName);
                    if (command == null)
                    {
                        UserConsole.WriteLine("Command {0} not found", commandName);
                    }
                    else
                    {
                        UserConsole.WriteLine(command.DisplayName);
                        OnCommandExecuting(new CommandExecutingEventArgs(command));
                        command.Execute();
                        OnCommandExecuted(new CommandExecutedEventArgs(command));
                    }
                });
        }

        public event CommandExecutingEventHandler CommandExecuting;

        protected virtual void OnCommandExecuting(CommandExecutingEventArgs e)
        {
            if (CommandExecuting != null)
                CommandExecuting(this, e);
        }

        public event CommandExecutedEventHandler CommandExecuted;

        protected virtual void OnCommandExecuted(CommandExecutedEventArgs e)
        {
            if (CommandExecuted != null)
                CommandExecuted(this, e);
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
