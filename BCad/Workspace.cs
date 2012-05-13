using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Xml.Serialization;
using BCad.Collections;
using BCad.Commands;
using BCad.EventArguments;

namespace BCad
{
    [Export(typeof(IWorkspace))]
    internal class Workspace : IWorkspace
    {
        public Workspace()
        {
            LoadSettings(ConfigFile);
        }

        #region Events

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

        public event PropertyChangingEventHandler PropertyChanging;

        protected void OnPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Properties

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
                OnPropertyChanging("Document");

                // ensure the same layer is selected after the change
                var currentLayerName = CurrentLayer.Name;

                // change the value and fire events
                UndoRedoService.SetSnapshot();
                document = value;
                OnPropertyChanged("Document");

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
                OnPropertyChanging("CurrentLayer");
                currentLayer = value;
                OnPropertyChanged("CurrentLayer");
            }
        }

        private DrawingPlane drawingPlane = DrawingPlane.XY;
        public DrawingPlane DrawingPlane
        {
            get { return drawingPlane; }
            set
            {
                if (this.drawingPlane == value)
                    return;
                OnPropertyChanging("DrawingPlane");
                this.drawingPlane = value;
                OnPropertyChanged("DrawingPlane");
            }
        }

        private double drawingPlaneOffset = 0.0;
        public double DrawingPlaneOffset
        {
            get { return this.drawingPlaneOffset; }
            set
            {
                if (this.drawingPlaneOffset == value)
                    return;
                OnPropertyChanging("DrawingPlaneOffset");
                this.drawingPlaneOffset = value;
                OnPropertyChanged("DrawingPlaneOffset");
            }
        }

        private ObservableHashSet<uint> selectedEntities = new ObservableHashSet<uint>();
        public ObservableHashSet<uint> SelectedEntities
        {
            get { return selectedEntities; }
        }

        #endregion

        #region Imports

        [Import]
        private IInputService InputService = null;

        [Import]
        private IUndoRedoService UndoRedoService = null;

        [ImportMany]
        private IEnumerable<Lazy<BCad.Commands.ICommand, ICommandMetadata>> Commands = null;

        #endregion

        #region IWorkspace implementation

        public ISettingsManager SettingsManager { get; private set; }

        private void LoadSettings(string path)
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

        public void SaveSettings()
        {
            var serializer = new XmlSerializer(typeof(SettingsManager));
            using (var stream = new FileStream(ConfigFile, FileMode.Create))
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
                isExecuting = false;
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

        public bool CanExecute()
        {
            return !this.isExecuting;
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

        #endregion

        #region Privates

        private bool isExecuting = false;

        private object executeGate = new object();

        private BCad.Commands.ICommand GetCommand(string commandName)
        {
            var command = (from c in Commands
                           let data = c.Metadata
                           where string.Compare(data.Name, commandName, StringComparison.OrdinalIgnoreCase) == 0
                              || data.CommandAliases.Contains(commandName, StringComparer.OrdinalIgnoreCase)
                           select c).SingleOrDefault();
            return command == null ? null : command.Value;
        }

        private const string ConfigFile = "BCad.config";

        #endregion

    }
}
