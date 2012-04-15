using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.EventArguments;
using System.Windows.Input;

namespace BCad
{
    public delegate void DocumentChangingEventHandler(object sender, DocumentChangingEventArgs e);

    public delegate void DocumentChangedEventHandler(object sender, DocumentChangedEventArgs e);

    public delegate void CurrentLayerChangingEventHandler(object sender, LayerChangingEventArgs e);

    public delegate void CurrentLayerChangedEventHandler(object sender, LayerChangedEventArgs e);

    public delegate void CommandExecutingEventHandler(object sender, CommandExecutingEventArgs e);

    public delegate void CommandExecutedEventHandler(object sender, CommandExecutedEventArgs e);

    public enum UnsavedChangesResult
    {
        Saved,
        Discarded,
        Cancel
    }

    public interface IWorkspace
    {
        Document Document { get; set; }
        Layer CurrentLayer { get; set; }

        ISettingsManager SettingsManager { get; }
        void LoadSettings(string path);
        void SaveSettings(string path);
        bool ExecuteCommandSynchronous(string commandName, params object[] parameters);
        void ExecuteCommand(string commandName, params object[] parameters);
        bool CommandExists(string commandName);
        bool CanExecute();
        
        event DocumentChangingEventHandler DocumentChanging;
        event DocumentChangedEventHandler DocumentChanged;
        event CurrentLayerChangingEventHandler CurrentLayerChanging;
        event CurrentLayerChangedEventHandler CurrentLayerChanged;
        event CommandExecutingEventHandler CommandExecuting;
        event CommandExecutedEventHandler CommandExecuted;
        UnsavedChangesResult PromptForUnsavedChanges();
    }
}
