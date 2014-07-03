using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace BCad.UI
{
    public partial class ViewPane
    {
        private class BindingClass : INotifyPropertyChanged
        {
            private Brush hotPointBrush;

            public Brush HotPointBrush
            {
                get { return hotPointBrush; }
                set
                {
                    if (hotPointBrush == value)
                        return;
                    hotPointBrush = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                OnPropertyChangedDirect(propertyName);
            }

            private void OnPropertyChangedDirect(string propertyName)
            {
                var changed = PropertyChanged;
                if (changed != null)
                    changed(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
