using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.EventArguments;

namespace BCad
{
    public delegate void DocumentChangingEventHandler(object sender, DocumentChangingEventArgs e);

    public delegate void DocumentChangedEventHandler(object sender, DocumentChangedEventArgs e);

    public enum UnsavedChangesResult
    {
        Saved,
        Discarded,
        Cancel
    }

    public interface IWorkspace
    {
        Document Document { get; set; }
        IView View { get; }
        event DocumentChangingEventHandler DocumentChanging;
        event DocumentChangedEventHandler DocumentChanged;
        UnsavedChangesResult PromptForUnsavedChanges();
    }
}
