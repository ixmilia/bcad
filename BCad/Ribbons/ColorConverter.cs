using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BCad.Extensions;

namespace BCad.Ribbons
{
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RealColor real;
            if (value != null)
            {
                if (value.GetType() == typeof(RealColor))
                {
                    real = (RealColor)value;
                    return Color.FromArgb(real.A, real.R, real.G, real.B);
                }
                else if (value.GetType() == typeof(IndexedColor))
                {
                    real = ((IndexedColor)value).RealColor;
                }
                else if (value.GetType() == typeof(string))
                {
                    real = ((string)value).ParseColor();
                }
                else
                {
                    Debug.Fail("unsupported color conversion from type " + value.GetType().ToString());
                }
            }

            return Color.FromArgb(real.A, real.R, real.G, real.B);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.GetType() == typeof(Color))
            {
                var color = (Color)value;
                if (targetType == typeof(RealColor))
                {
                    return RealColor.FromArgb(color.A, color.R, color.G, color.B);
                }
                else
                {
                    // TODO: how to turn back into an indexed color?  is this even necessary?
                }
            }

            Debug.Fail("unable to convert color");
            return RealColor.White; // error
        }
    }
}
