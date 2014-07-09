using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BCad.Collections;
using BCad.Entities;
using BCad.EventArguments;
using BCad.Primitives;

namespace BCad
{
    public delegate void CommandExecutingEventHandler(object sender, CommandExecutingEventArgs e);

    public delegate void CommandExecutedEventHandler(object sender, CommandExecutedEventArgs e);

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

        void Update(Optional<Drawing> drawing = default(Optional<Drawing>),
            Optional<Plane> drawingPlane = default(Optional<Plane>),
            Optional<ViewPort> activeViewPort = default(Optional<ViewPort>),
            Optional<IViewControl> viewControl = default(Optional<IViewControl>),
            bool isDirty = true);
        event WorkspaceChangingEventHandler WorkspaceChanging;
        event WorkspaceChangedEventHandler WorkspaceChanged;
        event EventHandler RubberBandGeneratorChanged;

        ObservableHashSet<Entity> SelectedEntities { get; }

        ISettingsManager SettingsManager { get; }
        void SaveSettings();
        Task<bool> ExecuteCommand(string commandName, object arg = null);
        bool CommandExists(string commandName);
        bool CanExecute();
        event CommandExecutingEventHandler CommandExecuting;
        event CommandExecutedEventHandler CommandExecuted;
        Task<UnsavedChangesResult> PromptForUnsavedChanges();
    }
}
