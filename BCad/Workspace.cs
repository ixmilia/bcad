using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using BCad.Collections;
using BCad.Commands;
using BCad.Entities;
using BCad.EventArguments;
using BCad.FileHandlers;
using BCad.Services;
using BCad.UI;

namespace BCad
{
    [Export(typeof(IWorkspace))]
    internal class Workspace : IWorkspace
    {
        public Workspace()
        {
            Drawing = new Drawing();
            DrawingPlane = new Plane(Point.Origin, Vector.ZAxis);
            ActiveViewPort = new ViewPort(Point.Origin, Vector.ZAxis, Vector.YAxis, 100.0);
            SelectedEntities = new ObservableHashSet<Entity>();
            ViewControl = null;

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

        #endregion

        #region Properties

        public bool IsDirty { get; private set; }

        public Drawing Drawing { get; private set; }

        public Plane DrawingPlane { get; private set; }

        public ViewPort ActiveViewPort { get; private set; }

        public ViewControl ViewControl { get; private set; }

        public ObservableHashSet<Entity> SelectedEntities { get; private set; }

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
        private IEnumerable<Lazy<IFileWriter, IFileWriterMetadata>> FileWriters = null;

        #endregion

        #region IWorkspace implementation

        public ISettingsManager SettingsManager { get; private set; }

        public void Update(
            Drawing drawing = null,
            Plane drawingPlane = null,
            ViewPort activeViewPort = null,
            ViewControl viewControl = null,
            bool? isDirty = true)
        {
            var e = new WorkspaceChangeEventArgs(
                drawing != null,
                drawingPlane != null,
                activeViewPort != null,
                viewControl != null,
                isDirty != null);

            OnWorkspaceChanging(e);
            if (drawing != null)
                this.Drawing = drawing;
            if (drawingPlane != null)
                this.DrawingPlane = drawingPlane;
            if (activeViewPort != null)
                this.ActiveViewPort = activeViewPort;
            if (viewControl != null)
                this.ViewControl = viewControl;
            if (isDirty != null)
                this.IsDirty = isDirty.Value;
            OnWorkspaceChanged(e);
        }

        public event WorkspaceChangingEventHandler WorkspaceChanging;

        protected void OnWorkspaceChanging(WorkspaceChangeEventArgs e)
        {
            var handler = WorkspaceChanging;
            if (handler != null)
                handler(this, e);
        }

        public event WorkspaceChangedEventHandler WorkspaceChanged;

        protected void OnWorkspaceChanged(WorkspaceChangeEventArgs e)
        {
            var handler = WorkspaceChanged;
            if (handler != null)
                handler(this, e);
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

        private async Task<bool> Execute(Tuple<BCad.Commands.ICommand, string> commandPair, object arg)
        {
            var command = commandPair.Item1;
            var display = commandPair.Item2;
            OnCommandExecuting(new CommandExecutingEventArgs(command));
            InputService.WriteLine(display);
            bool result = await command.Execute(arg);
            OnCommandExecuted(new CommandExecutedEventArgs(command));
            return result;
        }

        public async Task<bool> ExecuteCommand(string commandName, object arg)
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
            var commandPair = GetCommand(commandName);
            if (commandPair == null)
            {
                InputService.WriteLine("Command {0} not found", commandName);
                isExecuting = false;
                return false;
            }

            bool result = await Execute(commandPair, arg);
            lastCommand = commandName;
            lock (executeGate)
            {
                isExecuting = false;
            }

            return result;
        }

        public bool CommandExists(string commandName)
        {
            return GetCommand(commandName) != null;
        }

        public bool CanExecute()
        {
            return !this.isExecuting;
        }

        public async Task<UnsavedChangesResult> PromptForUnsavedChanges()
        {
            var result = UnsavedChangesResult.Discarded;
            if (this.IsDirty)
            {
                string filename = Drawing.Settings.FileName ?? "(Untitled)";
                var dialog = MessageBox.Show(string.Format("Save changes to '{0}'?", filename),
                    "Unsaved changes",
                    MessageBoxButton.YesNoCancel);
                switch (dialog)
                {
                    case MessageBoxResult.Yes:
                        // TODO: can't execute another command
                        if (await SaveAsCommand.Execute(this, FileWriters, Drawing.Settings.FileName))
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

        private Tuple<BCad.Commands.ICommand, string> GetCommand(string commandName)
        {
            var command = (from c in Commands
                           let data = c.Metadata
                           where string.Compare(data.Name, commandName, StringComparison.OrdinalIgnoreCase) == 0
                              || data.CommandAliases.Contains(commandName, StringComparer.OrdinalIgnoreCase)
                           select c).SingleOrDefault();
            return command == null ? null : Tuple.Create(command.Value, command.Metadata.DisplayName);
        }

        private const string ConfigFile = "BCad.config";

        #endregion

    }
}
