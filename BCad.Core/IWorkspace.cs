using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.EventArguments;
using System.Windows.Input;
using System.ComponentModel;

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

        ISettingsManager SettingsManager { get; }
        void LoadSettings(string path);
        void SaveSettings(string path);
        bool ExecuteCommandSynchronous(string commandName, object arg = null);
        void ExecuteCommand(string commandName, object arg = null);
        bool CommandExists(string commandName);
        bool CanExecute();
        event CommandExecutingEventHandler CommandExecuting;
        event CommandExecutedEventHandler CommandExecuted;
        UnsavedChangesResult PromptForUnsavedChanges();
    }
}
