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
using BCad.Commands;
using System.Collections.Generic;
using System.Windows.Input;

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
                UndoRedoService.SetSnapshot();
                document = value;
                OnDocumentChanged(new DocumentChangedEventArgs(document));

                // reset the current layer
                if (document.Layers.ContainsKey(currentLayerName))
                    this.CurrentLayer = document.Layers[currentLayerName];
                else if (document.Layers.ContainsKey("0"))
                    this.CurrentLayer = document.Layers["0"];
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
        private IInputService InputService = null;

        [Import]
        private IUndoRedoService UndoRedoService = null;

        [ImportMany]
        private IEnumerable<Lazy<BCad.Commands.ICommand, ICommandMetadata>> Commands = null;

        public ISettingsManager SettingsManager { get; private set; }

        private bool isExecuting = false;

        private object executeGate = new object();

        public Workspace()
        {
        }

        public void LoadSettings(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(SettingsManager));
                    using (var stream = new FileStream(path, FileMode.Open))
                    {
                        var manager = (SettingsManager)serializer.Deserialize(stream);
                        manager.InputService = this.InputService;
                        this.SettingsManager = manager;
                    }
                }
                catch
                {
                    this.SettingsManager = new SettingsManager();
                }
            }
            else
            {
                this.SettingsManager = new SettingsManager();
            }
        }

        public void SaveSettings(string path)
        {
            var serializer = new XmlSerializer(typeof(SettingsManager));
            using (var stream = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(stream, this.SettingsManager);
            }
        }

        private bool Execute(BCad.Commands.ICommand command, object arg)
        {
            OnCommandExecuting(new CommandExecutingEventArgs(command));
            InputService.WriteLine(command.DisplayName);
            bool result = command.Execute(arg);
            OnCommandExecuted(new CommandExecutedEventArgs(command));
            return result;
        }

        public bool ExecuteCommandSynchronous(string commandName, object arg)
        {
            Debug.Assert(commandName != null, "Null command not supported");
            lock (executeGate)
            {
                if (isExecuting)
                    return false;
                isExecuting = true;
            }

            var command = GetCommand(commandName);
            if (command == null)
            {
                InputService.WriteLine("Command {0} not found", commandName);
                return false;
            }

            bool result = Execute(command, arg);
            lock (executeGate)
            {
                isExecuting = false;
            }

            return result;
        }

        public void ExecuteCommand(string commandName, object arg)
        {
            ThreadPool.QueueUserWorkItem(_ => ExecuteCommandSynchronous(commandName, arg));
        }

        public bool CommandExists(string commandName)
        {
            return GetCommand(commandName) != null;
        }

        private BCad.Commands.ICommand GetCommand(string commandName)
        {
            var command = (from c in Commands
                           let data = c.Metadata
                           where string.Compare(data.Name, commandName, StringComparison.OrdinalIgnoreCase) == 0
                              || data.CommandAliases.Contains(commandName, StringComparer.OrdinalIgnoreCase)
                           select c).SingleOrDefault();
            return command == null ? null : command.Value;
        }

        public bool CanExecute()
        {
            return !this.isExecuting;
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
                        // TODO: can't execute another command
                        if (ExecuteCommandSynchronous("File.Save", Document.FileName))
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
