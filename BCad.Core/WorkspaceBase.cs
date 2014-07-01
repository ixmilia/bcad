using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using BCad.Collections;
using BCad.Commands;
using BCad.Entities;
using BCad.EventArguments;
using BCad.Services;

namespace BCad
{
    public abstract class WorkspaceBase : IWorkspace
    {
        private RubberBandGenerator rubberBandGenerator;

        public WorkspaceBase()
        {
            Drawing = new Drawing();
            DrawingPlane = new Plane(Point.Origin, Vector.ZAxis);
            ActiveViewPort = ViewPort.CreateDefaultViewPort();
            SelectedEntities = new ObservableHashSet<Entity>();
            ViewControl = null;
            RubberBandGenerator = null;

            SettingsManager = LoadSettings();
        }

        #region Events

        public event CommandExecutingEventHandler CommandExecuting;

        protected virtual void OnCommandExecuting(CommandExecutingEventArgs e)
        {
            var executing = CommandExecuting;
            if (executing != null)
                executing(this, e);
        }

        public event CommandExecutedEventHandler CommandExecuted;

        protected virtual void OnCommandExecuted(CommandExecutedEventArgs e)
        {
            var executed = CommandExecuted;
            if (executed != null)
                executed(this, e);
        }

        public event EventHandler RubberBandGeneratorChanged;

        protected virtual void OnRubberBandGeneratorChanged(EventArgs e)
        {
            var changed = RubberBandGeneratorChanged;
            if (changed != null)
                changed(this, e);
        }

        #endregion

        #region Properties

        public bool IsDirty { get; private set; }

        public Drawing Drawing { get; private set; }

        public Plane DrawingPlane { get; private set; }

        public ViewPort ActiveViewPort { get; private set; }

        public IViewControl ViewControl { get; private set; }

        public ObservableHashSet<Entity> SelectedEntities { get; private set; }

        public RubberBandGenerator RubberBandGenerator
        {
            get { return rubberBandGenerator; }
            set
            {
                if (rubberBandGenerator == value)
                    return;
                rubberBandGenerator = value;
                OnRubberBandGeneratorChanged(new EventArgs());
            }
        }

        public bool IsDrawing { get { return RubberBandGenerator != null; } }

        #endregion

        #region Imports

        [Import]
        public IOutputService OutputService { get; set; }

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

        public virtual void Update(
            Optional<Drawing> drawing = default(Optional<Drawing>),
            Optional<Plane> drawingPlane = default(Optional<Plane>),
            Optional<ViewPort> activeViewPort = default(Optional<ViewPort>),
            Optional<IViewControl> viewControl = default(Optional<IViewControl>),
            bool isDirty = true)
        {
            var e = new WorkspaceChangeEventArgs(
                drawing.HasValue,
                drawingPlane.HasValue,
                activeViewPort.HasValue,
                viewControl.HasValue,
                this.IsDirty != isDirty);

            OnWorkspaceChanging(e);
            if (drawing.HasValue)
                this.Drawing = drawing.Value;
            if (drawingPlane.HasValue)
                this.DrawingPlane = drawingPlane.Value;
            if (activeViewPort.HasValue)
                this.ActiveViewPort = activeViewPort.Value;
            if (viewControl.HasValue)
                this.ViewControl = viewControl.Value;
            this.IsDirty = isDirty;
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

        protected abstract ISettingsManager LoadSettings();
        public abstract void SaveSettings();

        private async Task<bool> Execute(Tuple<ICommand, string> commandPair, object arg)
        {
            var command = commandPair.Item1;
            var display = commandPair.Item2;
            OnCommandExecuting(new CommandExecutingEventArgs(command));
            OutputService.WriteLine(display);
            bool result;
            try
            {
                result = await command.Execute(arg);
            }
            catch (Exception ex)
            {
                OutputService.WriteLine("Error: {0} - {1}", ex.GetType().ToString(), ex.Message);
                result = false;
            }

            RubberBandGenerator = null;
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
                OutputService.WriteLine("Command {0} not found", commandName);
                isExecuting = false;
                return false;
            }

            var selectedStart = SelectedEntities;
            var result = await Execute(commandPair, arg);
            lastCommand = commandName;
            lock (executeGate)
            {
                isExecuting = false;
                SelectedEntities = selectedStart;
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

        public abstract Task<UnsavedChangesResult> PromptForUnsavedChanges();

        #endregion

        #region Privates

        private bool isExecuting = false;
        private string lastCommand = null;
        private object executeGate = new object();

        protected virtual Tuple<ICommand, string> GetCommand(string commandName)
        {
            var command = (from c in Commands
                           let data = c.Metadata
                           where string.Compare(data.Name, commandName, StringComparison.OrdinalIgnoreCase) == 0
                           select c).SingleOrDefault();
            return command == null ? null : Tuple.Create(command.Value, command.Metadata.DisplayName);
        }

        #endregion
    }
}
