// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Input;
using IxMilia.BCad.Services;

namespace IxMilia.BCad
{
    public class ToggleSettingsCommand : ICommand
    {
        private ISettingsService settingsService;
        private string settingName;

        public ToggleSettingsCommand(ISettingsService settingsService, string settingName)
        {
            this.settingsService = settingsService;
            this.settingName = settingName;
        }

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var previous = settingsService.GetValue<bool>(settingName);
            settingsService.SetValue(settingName, !previous);
        }
    }
}
