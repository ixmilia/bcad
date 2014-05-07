using System.ComponentModel;

#if BCAD_METRO
using Windows.UI;
using Windows.UI.Xaml.Media;
#endif

#if BCAD_WPF
using System.Windows.Media;
#endif

namespace BCad.UI.View
{
    public partial class RenderCanvas
    {
        private class BindingClass : INotifyPropertyChanged
        {
            private double thickness = 0.0;
            public double Thickness
            {
                get { return thickness; }
                set
                {
                    if (thickness == value)
                        return;
                    thickness = value;
                    OnPropertyChanged("Thickness");
                }
            }

            private ScaleTransform scale = new ScaleTransform() { ScaleX = 1.0, ScaleY = 1.0 };
            public ScaleTransform Scale
            {
                get { return scale; }
                set
                {
                    if (scale == value)
                        return;
                    scale = value;
                    OnPropertyChanged("Scale");
                }
            }

            private Brush autoBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            public Brush AutoBrush
            {
                get { return autoBrush; }
                set
                {
                    if (autoBrush == value)
                        return;
                    autoBrush = value;
                    OnPropertyChanged("AutoBrush");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string property)
            {
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(property));
                }
            }
        }
    }
}
