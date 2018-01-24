// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using IxMilia.Pdf;

namespace IxMilia.BCad.Plotting.Pdf
{
    internal static class PdfExtensions
    {
        public static PdfPoint ToPdfPoint(this Point point, PdfMeasurementType measurementType)
        {
            return new PdfPoint(new PdfMeasurement(point.X, measurementType), new PdfMeasurement(point.Y, measurementType));
        }

        public static PdfColor ToPdfColor(this CadColor color)
        {
            return new PdfColor(color.R / 255.0, color.G / 255.0, color.B / 255.0);
        }
    }
}
