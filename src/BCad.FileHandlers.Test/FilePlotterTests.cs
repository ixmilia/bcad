// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using BCad.Entities;
using BCad.FilePlotters;
using BCad.Helpers;
using Xunit;

namespace BCad.FileHandlers.Test
{
    public class FilePlotterTests
    {
        private static string PlotAsString(Drawing drawing, IFilePlotter plotter)
        {
            var width = 100;
            var height = 100;
            var projectedEntities = ProjectionHelper.ProjectTo2D(drawing, ViewPort.CreateDefaultViewPort(), width, height);
            using (var ms = new MemoryStream())
            {
                plotter.Plot(projectedEntities, width, height, ms);
                ms.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(ms))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static string PlotAsString(Entity entity, IFilePlotter plotter)
        {
            var layer = new Layer("layer").Add(entity);
            var drawing = new Drawing().Add(layer);
            return PlotAsString(drawing, plotter);
        }

        private static string NormalizeToCrLf(string s)
        {
            return s.Trim('\r', '\n').Replace("\r", "").Replace("\n", "\r\n");
        }

        [Fact]
        public void PdfYAxisTest()
        {
            // ensure the y-axis is inverted when plotting PDFs
            var actual = PlotAsString(new Line(new Point(0.0, 0.0, 0.0), new Point(1.0, 1.0, 0.0)), new PdfFilePlotter());
            Assert.Contains(NormalizeToCrLf(@"
0.00 0.00 m
1.00 1.00 l
"), actual);
        }
    }
}
