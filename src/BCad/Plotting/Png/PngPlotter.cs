// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Helpers;

namespace IxMilia.BCad.Plotting.Png
{
    internal class PngPlotter : PlotterBase
    {
        public PngPlotterViewModel ViewModel { get; }

        private Dictionary<CadColor, Brush> brushCache = new Dictionary<CadColor, Brush>();
        private Dictionary<CadColor, Pen> penCache = new Dictionary<CadColor, Pen>();
        private Color bgColor;
        private CadColor autoColor;

        public PngPlotter(PngPlotterViewModel viewModel)
        {
            ViewModel = viewModel;
            bgColor = Color.White;
            autoColor = CadColor.Black;
        }

        public override void Plot(IWorkspace workspace)
        {
            var projectedEntities = ProjectionHelper.ProjectTo2D(
                workspace.Drawing,
                ViewModel.ViewPort,
                ViewModel.Width,
                ViewModel.Height,
                ProjectionStyle.OriginTopLeft);
            using (var image = new Bitmap((int)ViewModel.Width, (int)ViewModel.Height))
            {
                using (var graphics = Graphics.FromImage(image))
                {
                    graphics.FillRectangle(new SolidBrush(bgColor), new Rectangle(0, 0, image.Width, image.Height));
                    PlotGraphics(projectedEntities, graphics);
                }

                image.Save(ViewModel.Stream, ImageFormat.Png);
            }
        }

        private void PlotGraphics(IEnumerable<ProjectedEntity> entities, Graphics graphics)
        {
            foreach (var groupedEntity in entities.GroupBy(p => p.OriginalLayer).OrderBy(x => x.Key.Name))
            {
                var layer = groupedEntity.Key;
                foreach (var entity in groupedEntity)
                {
                    DrawEntity(graphics, entity, layer.Color);
                }
            }
        }

        private void DrawEntity(Graphics graphics, ProjectedEntity entity, CadColor? layerColor, Vector offset = default(Vector))
        {
            switch (entity.Kind)
            {
                case EntityKind.Line:
                    DrawEntity(graphics, (ProjectedLine)entity, layerColor, offset);
                    break;
                case EntityKind.Circle:
                case EntityKind.Ellipse:
                    DrawEntity(graphics, (ProjectedCircle)entity, layerColor, offset);
                    break;
                case EntityKind.Text:
                    DrawEntity(graphics, (ProjectedText)entity, layerColor, offset);
                    break;
                case EntityKind.Aggregate:
                    var ag = (ProjectedAggregate)entity;
                    foreach (var child in ag.Children)
                    {
                        DrawEntity(graphics, child, layerColor, offset + ag.Location);
                    }
                    break;
                default:
                    break;
            }
        }

        private Brush ColorToBrush(CadColor color)
        {
            if (brushCache.ContainsKey(color))
            {
                return brushCache[color];
            }
            else
            {
                var brush = new SolidBrush(color.ToDrawingColor());
                brushCache.Add(color, brush);
                return brush;
            }
        }

        private void DrawEntity(Graphics graphics, ProjectedLine line, CadColor? layerColor, Vector offset)
        {
            graphics.DrawLine(ColorToPen(layerColor ?? line.OriginalLine.Color ?? autoColor), (line.P1 + offset).ToPointF(), (line.P2 + offset).ToPointF());
        }

        private void DrawEntity(Graphics graphics, ProjectedCircle circle, CadColor? layerColor, Vector offset)
        {
            // TODO: handle rotation
            var width = circle.RadiusX * 2.0;
            var height = circle.RadiusY * 2.0;
            var topLeft = (Point)(circle.Center - new Point(circle.RadiusX, circle.RadiusX, 0.0) + offset);
            graphics.DrawEllipse(ColorToPen(GetDisplayColor(layerColor, circle.OriginalCircle.Color)), (float)topLeft.X, (float)topLeft.Y, (float)width, (float)height);
        }

        private void DrawEntity(Graphics graphics, ProjectedText text, CadColor? layerColor, Vector offset)
        {
            // TODO: handle rotation
            var x = (float)(text.Location.X + offset.X);
            var y = (float)(text.Location.Y - text.Height + offset.Y);
            graphics.DrawString(text.OriginalText.Value, SystemFonts.DefaultFont, ColorToBrush(text.OriginalText.Color ?? layerColor ?? autoColor), x, y);
        }

        private Pen ColorToPen(CadColor color)
        {
            if (penCache.ContainsKey(color))
            {
                return penCache[color];
            }
            else
            {
                var pen = new Pen(ColorToBrush(color));
                penCache.Add(color, pen);
                return pen;
            }
        }

        private CadColor GetDisplayColor(CadColor? layerColor, CadColor? primitiveColor)
        {
            return primitiveColor ?? layerColor ?? autoColor;
        }
    }
}
