using System.ComponentModel;
using BCad.Collections;
using BCad.Entities;
using BCad.EventArguments;
using BCad.UI;

namespace BCad
{
    public delegate void CommandExecutingEventHandler(object sender, CommandExecutingEventArgs e);

    public delegate void CommandExecutedEventHandler(object sender, CommandExecutedEventArgs e);

    public delegate void WorkspaceChangingEventHandler(object sender, WorkspaceChangeEventArgs e);

    public delegate void WorkspaceChangedEventHandler(object sender, WorkspaceChangeEventArgs e);

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
        ViewControl ViewControl { get; }

        void Update(Drawing drawing = null, Plane drawingPlane = null, ViewPort activeViewPort = null, ViewControl viewControl = null, bool? isDirty = true);
        event WorkspaceChangingEventHandler WorkspaceChanging;
        event WorkspaceChangedEventHandler WorkspaceChanged;

        ObservableHashSet<Entity> SelectedEntities { get; }

        ISettingsManager SettingsManager { get; }
        void SaveSettings();
        bool ExecuteCommandSynchronous(string commandName, object arg = null);
        void ExecuteCommand(string commandName, object arg = null);
        bool CommandExists(string commandName);
        bool CanExecute();
        event CommandExecutingEventHandler CommandExecuting;
        event CommandExecutedEventHandler CommandExecuted;
        UnsavedChangesResult PromptForUnsavedChanges();
    }
}
