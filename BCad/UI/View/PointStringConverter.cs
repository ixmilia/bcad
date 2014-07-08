using System;
using System.Windows.Data;

namespace BCad.UI.View
{
    public class PointStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var point = (Point)value;
            return string.Format("{0},{1},{2}", point.X, point.Y, point.Z);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Point newPoint;
            if (Point.TryParse(value as string, out newPoint))
            {
                return newPoint;
            }

            return null;
        }
    }
}
