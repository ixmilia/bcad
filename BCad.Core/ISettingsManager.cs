using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.EventArguments;

namespace BCad
{
    public delegate void SettingsChangedEventHandler(object sender, SettingsChangedEventArgs e);

    public interface ISettingsManager
    {
        string LayerDialogId { get; }
        event SettingsChangedEventHandler SettingsChanged;
    }
}
