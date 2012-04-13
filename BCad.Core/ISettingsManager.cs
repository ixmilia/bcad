using BCad.EventArguments;

namespace BCad
{
    public delegate void SettingsChangedEventHandler(object sender, SettingsChangedEventArgs e);

    public interface ISettingsManager
    {
        string LayerDialogId { get; set; }
        string ViewControlId { get; set; }
        string ConsoleControlId { get; set; }
        bool OrthoganalLines { get; set; }
        event SettingsChangedEventHandler SettingsChanged;
    }
}
