using System;
using System.Windows.Data;

namespace BCad.UI.View
{
    public class PointVectorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var vector = (Vector)value;
            return (Point)vector;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var point = (Point)value;
            return (Vector)point;
        }
    }
}
