using System.ComponentModel;
using BCad.Collections;
using BCad.Entities;
using BCad.EventArguments;

namespace BCad
{
    public delegate void CommandExecutingEventHandler(object sender, CommandExecutingEventArgs e);

    public delegate void CommandExecutedEventHandler(object sender, CommandExecutedEventArgs e);

    public enum UnsavedChangesResult
    {
        Saved,
        Discarded,
        Cancel
    }

    public interface IWorkspace : INotifyPropertyChanging, INotifyPropertyChanged
    {
        Drawing Drawing { get; }
        Plane DrawingPlane { get; }
        ObservableHashSet<Entity> SelectedEntities { get; }

        void Update(Drawing drawing = null, Plane drawingPlane = null);

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
