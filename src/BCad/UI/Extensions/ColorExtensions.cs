// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows.Media;

namespace IxMilia.BCad.UI.Extensions
{
    public static class ColorExtensions
    {
        public static Color ToUIColor(this CadColor color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static CadColor ToColor(this Color color)
        {
            return CadColor.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
