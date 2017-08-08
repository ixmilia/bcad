// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows.Media;

namespace IxMilia.BCad.Extensions
{
    public static class ColorExtensions
    {
        public static System.Drawing.Color ToDrawingColor(this CadColor color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
