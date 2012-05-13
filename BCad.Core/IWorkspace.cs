using System.ComponentModel;
using BCad.Collections;
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
        Document Document { get; set; }
        Layer CurrentLayer { get; set; }
        DrawingPlane DrawingPlane { get; set; }
        double DrawingPlaneOffset { get; set; }
        ObservableHashSet<uint> SelectedEntities { get; }

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
