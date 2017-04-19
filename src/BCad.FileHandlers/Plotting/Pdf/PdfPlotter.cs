// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using BCad.Entities;
using BCad.Helpers;
using IxMilia.Pdf;

namespace BCad.Plotting.Pdf
{
    internal class PdfPlotter : PlotterBase
    {
        public PdfPlotterViewModel ViewModel { get; }

        private static CadColor AutoColor = CadColor.Black;

        public PdfPlotter(PdfPlotterViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public override void Plot(IWorkspace workspace)
        {
            var file = new PdfFile();
            foreach (var pageViewModel in ViewModel.Pages)
            {
                var projectedEntities = ProjectionHelper.ProjectTo2D(
                    workspace.Drawing,
                    pageViewModel.ViewPort,
                    pageViewModel.PlotWidth,
                    pageViewModel.PlotHeight,
                    ProjectionStyle.OriginBottomLeft);
                var builder = new PdfPathBuilder();
                foreach (var group in projectedEntities.GroupBy(e => e.OriginalLayer).OrderBy(l => l.Key.Name))
                {
                    var layer = group.Key;
                    foreach (var entity in group)
                    {
                        var scale = 1.0;
                        switch (entity.Kind)
                        {
                            case EntityKind.Circle:
                                var circle = (ProjectedCircle)entity;
                                scale = circle.RadiusX / circle.OriginalCircle.Radius;
                                builder.Add(new PdfCircle(
                                    circle.Center.ToPdfPoint(),
                                    circle.RadiusX,
                                    state: new PdfStreamState(
                                        color: (circle.OriginalCircle.Color ?? layer.Color ?? AutoColor).ToPdfColor(),
                                        strokeWidth: circle.OriginalCircle.Thickness * scale)));
                                break;
                            case EntityKind.Line:
                                var line = (ProjectedLine)entity;
                                scale = (line.P2 - line.P1).Length / (line.OriginalLine.P2 - line.OriginalLine.P1).Length;
                                builder.Add(new PdfLine(
                                    line.P1.ToPdfPoint(),
                                    line.P2.ToPdfPoint(),
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

                var page = new PdfPage(pageViewModel.PlotWidth, pageViewModel.PlotHeight);
                page.Items.Add(builder.ToPath());
                file.Pages.Add(page);
            }

            file.Save(ViewModel.Stream);
        }
    }
}
