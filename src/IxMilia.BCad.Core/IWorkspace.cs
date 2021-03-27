using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Commands;
using IxMilia.BCad.Entities;
using IxMilia.BCad.EventArguments;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.Services;

namespace IxMilia.BCad
{
    public delegate void CommandExecutingEventHandler(object sender, CadCommandExecutingEventArgs e);

    public delegate void CommandExecutedEventHandler(object sender, CadCommandExecutedEventArgs e);

    public delegate void WorkspaceChangingEventHandler(object sender, WorkspaceChangeEventArgs e);

    public delegate void WorkspaceChangedEventHandler(object sender, WorkspaceChangeEventArgs e);

    public delegate IEnumerable<IPrimitive> RubberBandGenerator(Point point);

    public enum UnsavedChangesResult
    {
        Saved,
        Discarded,
        Cancel
    }

    public interface IWorkspace
    {
        bool IsDirty { get; }
        Drawing Drawing { get; }
        Plane DrawingPlane { get; }
        ViewPort ActiveViewPort { get; }
        IViewControl ViewControl { get; }
        RubberBandGenerator RubberBandGenerator { get; set; }
        bool IsDrawing { get; }
        bool IsCommandExecuting { get; }

        TService GetService<TService>() where TService : class, IWorkspaceService;

        IDebugService DebugService { get; }
        IDialogService DialogService { get; }
        IInputService InputService { get; }
        IOutputService OutputService { get; }
        IReaderWriterService ReaderWriterService { get; }
        ISettingsService SettingsService { get; }
        IUndoRedoService UndoRedoService { get; }

        void Update(Optional<Drawing> drawing = default(Optional<Drawing>),
            Optional<Plane> drawingPlane = default(Optional<Plane>),
            Optional<ViewPort> activeViewPort = default(Optional<ViewPort>),
            Optional<IViewControl> viewControl = default(Optional<IViewControl>),
            bool isDirty = true);
        event WorkspaceChangingEventHandler WorkspaceChanging;
        event WorkspaceChangedEventHandler WorkspaceChanged;
        event EventHandler RubberBandGeneratorChanged;

        ObservableHashSet<Entity> SelectedEntities { get; }

        Task Initialize(params string[] args);
        void RegisterCommand(CadCommandInfo commandInfo);
        Task<bool> ExecuteCommand(string commandName, object arg = null);
        bool CommandExists(string commandName);
        Tuple<ICadCommand, string> GetCommand(string commandName);
        bool CanExecute();
        event CommandExecutingEventHandler CommandExecuting;
        event CommandExecutedEventHandler CommandExecuted;
        Task<UnsavedChangesResult> PromptForUnsavedChanges();
    }
}
