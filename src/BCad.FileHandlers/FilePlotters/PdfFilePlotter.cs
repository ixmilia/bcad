// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using BCad.Entities;
using IxMilia.Pdf;

namespace BCad.FilePlotters
{
    [ExportFilePlotter(DisplayName, FileExtension)]
    public class PdfFilePlotter : IFilePlotter
    {
        public const string DisplayName = "PDF Files (" + FileExtension + ")";
        public const string FileExtension = ".pdf";

        private static CadColor AutoColor = CadColor.Black;

        public void Plot(IEnumerable<ProjectedEntity> entities, double width, double height, Stream stream)
        {
            var builder = new PdfPathBuilder();
            foreach (var group in entities.GroupBy(e => e.OriginalLayer).OrderBy(l => l.Key.Name))
            {
                var layer = group.Key;
                foreach (var entity in group)
                {
                    switch (entity.Kind)
                    {
                        case EntityKind.Line:
                            var line = (ProjectedLine)entity;
                            var scale = (line.P2 - line.P1).Length / (line.OriginalLine.P2 - line.OriginalLine.P1).Length;
                            builder.Add(new PdfLine(
                                line.P1.ToPdfPoint(height),
                                line.P2.ToPdfPoint(height),
                                state: new PdfStreamState(
                                    color: (line.OriginalLine.Color ?? layer.Color ?? AutoColor).ToPdfColor(),
                                    strokeWidth: line.OriginalLine.Thickness * scale)));
                            break;
                        default:
                            // TODO:
                            break;
                    }
                }
            }

            var page = new PdfPage(width, height);
            page.Items.Add(builder.ToPath());
            var file = new PdfFile();
            file.Pages.Add(page);
            file.Save(stream);
        }
    }

    internal static class PdfExtensions
    {
        public static PdfPoint ToPdfPoint(this Point point, double height)
        {
            return new PdfPoint(point.X, height - point.Y);
        }

        public static PdfColor ToPdfColor(this CadColor color)
        {
            return new PdfColor(color.R / 255.0, color.G / 255.0, color.B / 255.0);
        }
    }
}
