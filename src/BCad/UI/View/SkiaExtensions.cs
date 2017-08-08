// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using SkiaSharp;

namespace IxMilia.BCad.UI.View
{
    internal static class SkiaExtensions
    {
        public static SKColor ToSKColor(this CadColor color)
        {
            return new SKColor(color.R, color.G, color.B);
        }

        public static SKPoint ToSKPoint(this Point point)
        {
            return new SKPoint((float)point.X, (float)point.Y);
        }
    }
}
