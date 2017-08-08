// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using IxMilia.Pdf;

namespace IxMilia.BCad.Plotting.Pdf
{
    internal static class PdfExtensions
    {
        public static PdfPoint ToPdfPoint(this Point point)
        {
            return new PdfPoint(point.X, point.Y);
        }

        public static PdfColor ToPdfColor(this CadColor color)
        {
            return new PdfColor(color.R / 255.0, color.G / 255.0, color.B / 255.0);
        }
    }
}
