using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using BCad.Extensions;

namespace BCad.UI
{
    public class ColorDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value.GetType() == typeof(CadColor))
                {
                    return ((CadColor)value).ToColorString();
                }
                else
                {
                    Debug.Fail("unsupported color conversion from type " + value.GetType().ToString());
                }
            }

            return "(Auto)";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
