using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using BCad.Collections;
using BCad.Commands;
using BCad.Entities;
using BCad.EventArguments;
using BCad.Services;

namespace BCad
{
    internal class WorkspaceLogEntry : LogEntry
    {
        public string Event { get; private set; }

        public WorkspaceLogEntry(string @event)
        {
            Event = @event;
        }

        public override string ToString()
        {
            return string.Format("workspace: {0}", Event);
        }
    }

    [Export(typeof(IWorkspace)), Shared]
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

        public IViewControl ViewControl { get; private set; }

        public ObservableHashSet<Entity> SelectedEntities { get; private set; }

        #endregion

        #region Imports

        [Import]
        public IInputService InputService { get; set; }

        [Import]
        public IFileSystemService FileSystemService { get; set; }

        // required so that the service is created before the undo or redo commands are fired
        [Import]
        public IUndoRedoService UndoRedoService { get; set; }

        [ImportMany]
        public IEnumerable<Lazy<ICommand, CommandMetadata>> Commands { get; set; }

        [Import]
        public IDebugService DebugService { get; set; }

        #endregion

        #region IWorkspace implementation

        public ISettingsManager SettingsManager { get; private set; }

        public void Update(
            Drawing drawing = null,
            Plane drawingPlane = null,
            ViewPort activeViewPort = null,
            IViewControl viewControl = null,
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

        private async Task<bool> Execute(Tuple<ICommand, string> commandPair, object arg)
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
            DebugService.Add(new WorkspaceLogEntry(string.Format("execute {0}", commandName)));
            var commandPair = GetCommand(commandName);
            if (commandPair == null)
            {
                InputService.WriteLine("Command {0} not found", commandName);
                isExecuting = false;
                return false;
            }

            bool result;

            try
            {
                result = await Execute(commandPair, arg);
                lastCommand = commandName;
            }
            catch (Exception ex)
            {
                InputService.WriteLine("Error: {0} - {1}", ex.GetType().ToString(), ex.Message);
                result = false;
            }
            finally
            {
                lock (executeGate)
                {
                    isExecuting = false;
                }
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

        public Task<UnsavedChangesResult> PromptForUnsavedChanges()
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
                        var fileName = Drawing.Settings.FileName;
                        if (fileName == null)
                            fileName = FileSystemService.GetFileNameFromUserForSave();
                        if (fileName == null)
                            result = UnsavedChangesResult.Cancel;
                        else if (FileSystemService.TryWriteDrawing(fileName, Drawing, ActiveViewPort))
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

            return Task.FromResult<UnsavedChangesResult>(result);
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

        private Tuple<ICommand, string> GetCommand(string commandName)
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
