using System;

namespace BCad.EventArguments
{
    public class SettingsChangedEventArgs : EventArgs
    {
        public ISettingsManager SettingsManager { get; private set; }

        public SettingsChangedEventArgs(ISettingsManager settingsManager)
        {
            this.SettingsManager = settingsManager;
        }
    }
}
