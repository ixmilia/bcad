using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace BCad
{
    public class ToggleSettingsCommand : ICommand
    {
        private ISettingsManager settingsManager = null;

        private IInputService inputService = null;

        private Action toggle = null;

        public ToggleSettingsCommand(IInputService inputService, ISettingsManager settingsManager, string settingName)
        {
            this.settingsManager = settingsManager;
            this.inputService = inputService;
            var propInfo = typeof(ISettingsManager).GetProperty(settingName);
            this.toggle = () =>
                {
                    bool previous = (bool)propInfo.GetValue(this.settingsManager, null);
                    propInfo.SetValue(this.settingsManager, !previous, null);
                    inputService.WriteLine("{0} is {1}", settingName, (!previous) ? "on" : "off");
                };
        }

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            toggle();
        }
    }
}
