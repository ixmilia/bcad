using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BCad.Ribbons
{
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.GetType() == typeof(RealColor))
            {
                var real = (RealColor)value;
                return Color.FromArgb(real.A, real.R, real.G, real.B);
            }

            return Color.FromArgb(255, 255, 0, 0); // red for error
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.GetType() == typeof(Color))
            {
                var color = (Color)value;
                return RealColor.FromArgb(color.A, color.R, color.G, color.B);
            }

            return RealColor.White; // error
        }
    }
}
