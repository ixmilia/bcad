using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.EventArguments;

namespace BCad
{
    public delegate void DocumentChangingEventHandler(object sender, DocumentChangingEventArgs e);

    public delegate void DocumentChangedEventHandler(object sender, DocumentChangedEventArgs e);

    public delegate void CurrentLayerChangingEventHandler(object sender, LayerChangingEventArgs e);

    public delegate void CurrentLayerChangedEventHandler(object sender, LayerChangedEventArgs e);

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

        IUserConsole UserConsole { get; }
        IView View { get; }
        IUndoRedoService UndoRedoService { get; }
        IDialogFactory DialogFactory { get; }
        ICommandManager CommandManager { get; }
        IControlFactory ControlFactory { get; }
        ISettingsManager SettingsManager { get; }

        void LoadSettings(string path);
        
        event DocumentChangingEventHandler DocumentChanging;
        event DocumentChangedEventHandler DocumentChanged;
        event CurrentLayerChangingEventHandler CurrentLayerChanging;
        event CurrentLayerChangedEventHandler CurrentLayerChanged;
        UnsavedChangesResult PromptForUnsavedChanges();

        void Focus();
    }
}
