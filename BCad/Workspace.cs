using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Xml.Serialization;
using BCad.Collections;
using BCad.Commands;
using BCad.Entities;
using BCad.EventArguments;
using BCad.FileHandlers;
using BCad.Services;

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

        private Drawing drawing = new Drawing();
        public Drawing Drawing
        {
            get { return drawing; }
        }

        private Plane drawingPlane = new Plane(Point.Origin, Vector.ZAxis);
        public Plane DrawingPlane
        {
            get { return drawingPlane; }
        }

        private ObservableHashSet<Entity> selectedEntities = new ObservableHashSet<Entity>();
        public ObservableHashSet<Entity> SelectedEntities
        {
            get { return selectedEntities; }
        }

        #endregion

        #region Imports

        [Import]
        private IInputService InputService = null;

        // required so that the service is created before the undo or redo commands are fired
        [Import]
        private IUndoRedoService UndoRedoService { get; set; }

        [ImportMany]
        private IEnumerable<Lazy<BCad.Commands.ICommand, ICommandMetadata>> Commands = null;

        [ImportMany]
        private IEnumerable<IFileWriter> FileWriters = null;

        #endregion

        #region IWorkspace implementation

        public ISettingsManager SettingsManager { get; private set; }

        public void Update(Drawing drawing = null, Plane drawingPlane = null)
        {
            // fire changing events
            if (drawing != null)
                OnPropertyChanging(Constants.DrawingString);
            if (drawingPlane != null)
                OnPropertyChanging(Constants.DrawingPlaneString);

            // update values
            if (drawing != null)
                this.drawing = drawing;
            if (drawingPlane != null)
                this.drawingPlane = drawingPlane;

            // fire changed events
            if (drawing != null)
                OnPropertyChanged(Constants.DrawingString);
            if (drawingPlane != null)
                OnPropertyChanged(Constants.DrawingPlaneString);
        }

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
            if (commandName == null && lastCommand == null)
            {
                return false;
            }

            lock (executeGate)
            {
                if (isExecuting)
                    return false;
                isExecuting = true;
            }

            commandName = commandName ?? lastCommand;
            var command = GetCommand(commandName);
            if (command == null)
            {
                InputService.WriteLine("Command {0} not found", commandName);
                isExecuting = false;
                return false;
            }

            bool result = Execute(command, arg);
            lastCommand = commandName;
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
            if (Drawing.Settings.IsDirty)
            {
                string filename = Drawing.Settings.FileName ?? "(Untitled)";
                var dialog = MessageBox.Show(string.Format("Save changes to '{0}'?", filename),
                    "Unsaved changes",
                    MessageBoxButton.YesNoCancel);
                switch (dialog)
                {
                    case MessageBoxResult.Yes:
                        // TODO: can't execute another command
                        if (SaveAsCommand.Execute(this, FileWriters, Drawing.Settings.FileName))
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
        private string lastCommand = null;
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
