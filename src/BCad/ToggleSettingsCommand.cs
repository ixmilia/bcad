// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Input;

namespace BCad
{
    public class ToggleSettingsCommand : ICommand
    {
        private ISettingsManager settingsManager = null;
        private Action toggle = null;

        public ToggleSettingsCommand(ISettingsManager settingsManager, string settingName)
        {
            this.settingsManager = settingsManager;
            var propInfo = typeof(ISettingsManager).GetProperty(settingName, typeof(bool));
            if (propInfo == null)
                throw new NotSupportedException("Unable to find appropriate setting");
            this.toggle = () =>
                {
                    bool previous = (bool)propInfo.GetValue(this.settingsManager, null);
                    propInfo.SetValue(this.settingsManager, !previous, null);
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
