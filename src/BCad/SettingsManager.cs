// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows.Input;
using IxMilia.Config;

namespace BCad
{
    public enum SelectedEntityDrawStyle
    {
        Dashed,
        Glow,
    }

    public class SettingsManager : DefaultSettingsManager
    {
        private KeyboardShortcut angleSnapShortcut = null;
        private KeyboardShortcut pointSnapShortcut = null;
        private KeyboardShortcut orthoShortcut = null;
        private KeyboardShortcut debugShortcut = null;
        private SelectedEntityDrawStyle selectedEntityDrawStyle = SelectedEntityDrawStyle.Dashed;

        [ConfigPath("UI.AngleSnapShortcut")]
        public KeyboardShortcut AngleSnapShortcut
        {
            get { return this.angleSnapShortcut; }
            set
            {
                this.angleSnapShortcut = value;
                OnPropertyChanged(nameof(AngleSnapShortcut));
            }
        }

        [ConfigPath("UI.PointSnapShortcut")]
        public KeyboardShortcut PointSnapShortcut
        {
            get { return this.pointSnapShortcut; }
            set
            {
                this.pointSnapShortcut = value;
                OnPropertyChanged(nameof(PointSnapShortcut));
            }
        }

        [ConfigPath("UI.OrthoShortcut")]
        public KeyboardShortcut OrthoShortcut
        {
            get { return this.orthoShortcut; }
            set
            {
                this.orthoShortcut = value;
                OnPropertyChanged(nameof(OrthoShortcut));
            }
        }

        [ConfigPath("UI.DebugShortcut")]
        public KeyboardShortcut DebugShortcut
        {
            get { return this.debugShortcut; }
            set
            {
                this.debugShortcut = value;
                OnPropertyChanged(nameof(DebugShortcut));
            }
        }

        [ConfigPath("UI.SelectedEntityDrawStyle")]
        public SelectedEntityDrawStyle SelectedEntityDrawStyle
        {
            get { return this.selectedEntityDrawStyle; }
            set
            {
                this.selectedEntityDrawStyle = value;
                OnPropertyChanged(nameof(SelectedEntityDrawStyle));
            }
        }

        public override void LoadDefaults()
        {
            base.LoadDefaults();
            AngleSnapShortcut = new KeyboardShortcut(ModifierKeys.None, Key.F7);
            PointSnapShortcut = new KeyboardShortcut(ModifierKeys.None, Key.F3);
            OrthoShortcut = new KeyboardShortcut(ModifierKeys.None, Key.F8);
            DebugShortcut = new KeyboardShortcut(ModifierKeys.None, Key.F12);
            SelectedEntityDrawStyle = SelectedEntityDrawStyle.Dashed;
        }
    }
}
