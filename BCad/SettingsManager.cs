using System;
using System.Windows.Input;

namespace BCad
{
    [Serializable]
    public class SettingsManager : DefaultSettingsManager
    {
        private KeyboardShortcut angleSnapShortcut = null;
        private KeyboardShortcut pointSnapShortcut = null;
        private KeyboardShortcut orthoShortcut = null;
        private KeyboardShortcut debugShortcut = null;

        public KeyboardShortcut AngleSnapShortcut
        {
            get { return this.angleSnapShortcut; }
            set
            {
                this.angleSnapShortcut = value;
                OnPropertyChanged("AngleSnapShortcut");
            }
        }

        public KeyboardShortcut PointSnapShortcut
        {
            get { return this.pointSnapShortcut; }
            set
            {
                this.pointSnapShortcut = value;
                OnPropertyChanged("PointSnapShortcut");
            }
        }

        public KeyboardShortcut OrthoShortcut
        {
            get { return this.orthoShortcut; }
            set
            {
                this.orthoShortcut = value;
                OnPropertyChanged("OrthoShortcut");
            }
        }

        public KeyboardShortcut DebugShortcut
        {
            get { return this.debugShortcut; }
            set
            {
                this.debugShortcut = value;
                OnPropertyChanged("DebugShortcut");
            }
        }

        public override void LoadDefaults()
        {
            base.LoadDefaults();
            AngleSnapShortcut = new KeyboardShortcut(ModifierKeys.None, Key.F7);
            PointSnapShortcut = new KeyboardShortcut(ModifierKeys.None, Key.F3);
            OrthoShortcut = new KeyboardShortcut(ModifierKeys.None, Key.F8);
            DebugShortcut = new KeyboardShortcut(ModifierKeys.None, Key.F12);
        }
    }
}
