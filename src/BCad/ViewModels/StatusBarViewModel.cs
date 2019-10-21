﻿// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using IxMilia.BCad.Display;
using IxMilia.BCad.Services;
using IxMilia.BCad.Settings;

namespace IxMilia.BCad.ViewModels
{
    internal class StatusBarViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool Ortho
        {
            get => GetValue(DisplaySettingsProvider.Ortho);
            set => SetValue(DisplaySettingsProvider.Ortho, value);
        }

        public bool PointSnap
        {
            get => GetValue(DisplaySettingsProvider.PointSnap);
            set => SetValue(DisplaySettingsProvider.PointSnap, value);
        }

        public bool AngleSnap
        {
            get => GetValue(DisplaySettingsProvider.AngleSnap);
            set => SetValue(DisplaySettingsProvider.AngleSnap, value);
        }

        public bool Debug
        {
            get => GetValue(DefaultSettingsProvider.Debug);
            set => SetValue(DefaultSettingsProvider.Debug, value);
        }

        private ISettingsService _settingsService;

        public StatusBarViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _settingsService.SettingChanged += SettingChanged;
        }

        private void SettingChanged(object sender, SettingChangedEventArgs e)
        {
            switch (e.SettingName)
            {
                case DisplaySettingsProvider.AngleSnap:
                    OnPropertyChanged(nameof(AngleSnap));
                    break;
                case DefaultSettingsProvider.Debug:
                    OnPropertyChanged(nameof(Debug));
                    break;
                case DisplaySettingsProvider.Ortho:
                    OnPropertyChanged(nameof(Ortho));
                    break;
                case DisplaySettingsProvider.PointSnap:
                    OnPropertyChanged(nameof(PointSnap));
                    break;
                default:
                    break;
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool GetValue(string settingName)
        {
            return _settingsService.GetValue<bool>(settingName);
        }

        private void SetValue(string settingName, bool value, [CallerMemberName] string propertyName = null)
        {
            _settingsService.SetValue(settingName, value);
            OnPropertyChanged(propertyName);
        }
    }
}
