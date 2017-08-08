// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Helpers;
using IxMilia.Pdf;

namespace IxMilia.BCad.Plotting.Pdf
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
            var font = new PdfFontType1(PdfFontType1Type.Helvetica);
            foreach (var pageViewModel in ViewModel.Pages)
            {
                var projectedEntities = ProjectionHelper.ProjectTo2D(
                    workspace.Drawing,
                    pageViewModel.ViewPort,
                    pageViewModel.PlotWidth,
                    pageViewModel.PlotHeight,
                    ProjectionStyle.OriginBottomLeft);
                var page = new PdfPage(pageViewModel.PlotWidth, pageViewModel.PlotHeight);
                file.Pages.Add(page);
                var builder = new PdfPathBuilder();
                void AddPathItemToPage(IPdfPathItem pathItem)
                {
                    builder.Add(pathItem);
                }
                void AddStreamItemToPage(PdfStreamItem streamItem)
                {
                    if (builder.Items.Count > 0)
                    {
                        page.Items.Add(builder.ToPath());
                        builder = new PdfPathBuilder();
                    }

                    page.Items.Add(streamItem);
                }
                foreach (var group in projectedEntities.GroupBy(e => e.OriginalLayer).OrderBy(l => l.Key.Name))
                {
                    var layer = group.Key;
                    foreach (var entity in group)
                    {
                        var scale = 1.0;
                        switch (entity.Kind)
                        {
                            case EntityKind.Arc:
                                var arc = (ProjectedArc)entity;
                                scale = arc.RadiusX / arc.OriginalArc.Radius;
                                AddPathItemToPage(new PdfArc(
                                    arc.Center.ToPdfPoint(),
                                    arc.RadiusX,
                                    arc.StartAngle * MathHelper.DegreesToRadians,
                                    arc.EndAngle * MathHelper.DegreesToRadians,
                                    state: new PdfStreamState(
                                        strokeColor: (arc.OriginalArc.Color ?? layer.Color ?? AutoColor).ToPdfColor(),
                                        strokeWidth: arc.OriginalArc.Thickness * scale)));
                                break;
                            case EntityKind.Circle:
                                var circle = (ProjectedCircle)entity;
                                scale = circle.RadiusX / circle.OriginalCircle.Radius;
                                AddPathItemToPage(new PdfCircle(
                                    circle.Center.ToPdfPoint(),
                                    circle.RadiusX,
                                    state: new PdfStreamState(
                                        strokeColor: (circle.OriginalCircle.Color ?? layer.Color ?? AutoColor).ToPdfColor(),
                                        strokeWidth: circle.OriginalCircle.Thickness * scale)));
                                break;
                            case EntityKind.Line:
                                var line = (ProjectedLine)entity;
                                scale = (line.P2 - line.P1).Length / (line.OriginalLine.P2 - line.OriginalLine.P1).Length;
                                AddPathItemToPage(new PdfLine(
                                    line.P1.ToPdfPoint(),
                                    line.P2.ToPdfPoint(),
                                    state: new PdfStreamState(
                                        strokeColor: (line.OriginalLine.Color ?? layer.Color ?? AutoColor).ToPdfColor(),
                                        strokeWidth: line.OriginalLine.Thickness * scale)));
                                break;
                            case EntityKind.Text:
                                var text = (ProjectedText)entity;
                                AddStreamItemToPage(
                                    new PdfText(
                                        text.OriginalText.Value,
                                        font,
                                        text.Height,
                                        text.Location.ToPdfPoint(),
                                        state: new PdfStreamState(
                                            nonStrokeColor: (text.OriginalText.Color ?? layer.Color ?? AutoColor).ToPdfColor())));
                                break;
                            default:
                                // TODO:
                                break;
                        }
                    }
                }

                if (builder.Items.Count > 0)
                {
                    page.Items.Add(builder.ToPath());
                }
            }

            file.Save(ViewModel.Stream);
        }
    }
}
