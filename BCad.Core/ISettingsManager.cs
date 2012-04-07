using BCad.EventArguments;

namespace BCad
{
    public delegate void SettingsChangedEventHandler(object sender, SettingsChangedEventArgs e);

    public interface ISettingsManager
    {
        string LayerDialogId { get; }
        string ViewControlId { get; }
        string ConsoleControlId { get; }
        event SettingsChangedEventHandler SettingsChanged;
    }
}
