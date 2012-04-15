using System.Collections.Generic;
using BCad.EventArguments;

namespace BCad
{
    public delegate void SettingsChangedEventHandler(object sender, SettingsChangedEventArgs e);

    public interface ISettingsManager
    {
        string LayerDialogId { get; set; }
        string ViewControlId { get; set; }
        string ConsoleControlId { get; set; }
        double SnapPointDistance { get; set; }
        double SnapPointSize { get; set; }
        bool AngleSnap { get; set; }
        double SnapAngleDistance { get; set; }
        double[] SnapAngles { get; set; }
        event SettingsChangedEventHandler SettingsChanged;
    }
}
