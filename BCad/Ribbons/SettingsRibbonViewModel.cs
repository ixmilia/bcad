using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BCad.Ribbons
{
    public class SettingsRibbonViewModel : INotifyPropertyChanged
    {
        private ISettingsManager settingsManager;
        private double[] oldSnapAngles = new double[0];
        private double[] isoSnapAngles = new double[] { 30.0, 90.0, 150.0, 210.0, 270.0, 330.0 };

        private bool useIsoSettings;
        public bool UseIsoSettings
        {
            get { return useIsoSettings; }
            set
            {
                if (useIsoSettings == value)
                    return;
                useIsoSettings = value;
                if (useIsoSettings)
                {
                    oldSnapAngles = settingsManager.SnapAngles;
                    settingsManager.SnapAngles = isoSnapAngles;
                }
                else
                {
                    settingsManager.SnapAngles = oldSnapAngles;
                }

                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsRibbonViewModel(ISettingsManager settingsManager)
        {
            this.settingsManager = settingsManager;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed != null)
                changed(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
