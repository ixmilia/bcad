// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using IxMilia.BCad.Extensions;

namespace IxMilia.BCad.UI
{
    public class ColorDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value.GetType() == typeof(CadColor))
                {
                    return ((CadColor)value).ToARGBString();
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
