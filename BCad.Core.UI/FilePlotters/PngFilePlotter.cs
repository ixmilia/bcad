using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using BCad.Core.UI.Extensions;
using BCad.Entities;
using BCad.FilePlotters;

namespace BCad.Commands.FilePlotters
{
    [ExportFilePlotter(DisplayName, FileExtension)]
    internal class PngFilePlotter : IFilePlotter
    {
        public const string DisplayName = "PNG Files (" + FileExtension + ")";
        public const string FileExtension = ".png";

        private Dictionary<Color, Brush> brushCache = new Dictionary<Color, Brush>();
        private Dictionary<Color, Pen> penCache = new Dictionary<Color, Pen>();
        private Color bgColor;
        private Color autoColor;

        public PngFilePlotter()
        {
            // TODO: set autocolor
            bgColor = Color.White;
            autoColor = Color.Black;
        }

        public void Plot(IEnumerable<ProjectedEntity> entities, double width, double height, Stream stream)
        {
            using (var image = new Bitmap((int)width, (int)height))
            {
                using (var graphics = Graphics.FromImage(image))
                {
                    graphics.FillRectangle(new SolidBrush(bgColor), new Rectangle(0, 0, image.Width, image.Height));
                    PlotGraphics(entities, graphics);
                }

                image.Save(stream, ImageFormat.Png);
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

        private void DrawEntity(Graphics graphics, ProjectedEntity entity, IndexedColor layerColor)
        {
            switch (entity.Kind)
            {
                case EntityKind.Line:
                    DrawEntity(graphics, (ProjectedLine)entity, layerColor);
                    break;
                case EntityKind.Circle:
                case EntityKind.Ellipse:
                    DrawEntity(graphics, (ProjectedCircle)entity, layerColor);
                    break;
                case EntityKind.Text:
                    DrawEntity(graphics, (ProjectedText)entity, layerColor);
                    break;
                case EntityKind.Aggregate:
                    foreach (var child in ((ProjectedAggregate)entity).Children)
                    {
                        DrawEntity(graphics, child, layerColor);
                    }
                    break;
                default:
                    break;
            }
        }

        private Brush ColorToBrush(Color color)
        {
            if (brushCache.ContainsKey(color))
            {
                return brushCache[color];
            }
            else
            {
                var brush = new SolidBrush(color);
                brushCache.Add(color, brush);
                return brush;
            }
        }

        private void DrawEntity(Graphics graphics, ProjectedLine line, IndexedColor layerColor)
        {
            graphics.DrawLine(ColorToPen(GetDisplayColor(layerColor, line.OriginalLine.Color)), line.P1.ToPointF(), line.P2.ToPointF());
        }

        private void DrawEntity(Graphics graphics, ProjectedCircle circle, IndexedColor layerColor)
        {
            // TODO: handle rotation
            var width = circle.RadiusX * 2.0;
            var height = circle.RadiusY * 2.0;
            var topLeft = (Point)(circle.Center - new Point(circle.RadiusX, circle.RadiusX, 0.0));
            graphics.DrawEllipse(ColorToPen(GetDisplayColor(layerColor, circle.OriginalCircle.Color)), (float)topLeft.X, (float)topLeft.Y, (float)width, (float)height);
        }

        private void DrawEntity(Graphics graphics, ProjectedText text, IndexedColor layerColor)
        {
            // TODO: handle rotation
            var x = (float)text.Location.X;
            var y = (float)(text.Location.Y - text.Height);
            graphics.DrawString(text.OriginalText.Value, SystemFonts.DefaultFont, new SolidBrush(text.OriginalText.Color.ToDrawingColor()), x, y);
        }

        private Pen ColorToPen(Color color)
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

        private Color GetDisplayColor(IndexedColor layerColor, IndexedColor primitiveColor)
        {
            Color display;
            if (!primitiveColor.IsAuto)
                display = primitiveColor.ToDrawingColor();
            else if (!layerColor.IsAuto)
                display = layerColor.ToDrawingColor();
            else
                display = autoColor;

            return display;
        }
    }
}
