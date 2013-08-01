using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using BCad.Entities;
using BCad.FilePlotters;

namespace BCad.Commands.FilePlotters
{
    [ExportFilePlotter(PngFilePlotter.DisplayName, PngFilePlotter.FileExtension)]
    internal class PngFilePlotter : IFilePlotter
    {
        public const string DisplayName = "PNG Files (" + FileExtension + ")";
        public const string FileExtension = ".png";

        private Dictionary<System.Drawing.Color, Brush> brushCache = new Dictionary<System.Drawing.Color, Brush>();
        private Dictionary<System.Drawing.Color, Pen> penCache = new Dictionary<System.Drawing.Color, Pen>();
        private System.Drawing.Color autoColor = System.Drawing.Color.Black;

        public void Plot(IEnumerable<ProjectedEntity> entities, double width, double height, Stream stream)
        {
            // set autocolor
            var bg = System.Windows.Media.Colors.Black; // TODO: choose color
            var backgroundColor = (bg.R << 16) | (bg.G << 8) | bg.B;
            var brightness = System.Drawing.Color.FromArgb(backgroundColor).GetBrightness();
            var color = brightness < 0.67 ? 0xFFFFFF : 0x000000;
            autoColor = System.Drawing.Color.FromArgb((0xFF << 24) | color);

            var image = new Bitmap((int)width, (int)height);
            using (var graphics = Graphics.FromImage(image))
            {
                graphics.FillRectangle(Brushes.Black, new Rectangle(0, 0, image.Width, image.Height));
                foreach (var groupedEntity in entities.GroupBy(p => p.OriginalLayer).OrderBy(x => x.Key.Name))
                {
                    var layer = groupedEntity.Key;
                    foreach (var entity in groupedEntity)
                    {
                        DrawEntity(graphics, entity, layer.Color);
                    }
                }
            }

            image.Save(stream, ImageFormat.Png);
        }

        private void DrawEntity(Graphics graphics, ProjectedEntity entity, Color layerColor)
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

        private Brush ColorToBrush(System.Drawing.Color color)
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

        private void DrawEntity(Graphics graphics, ProjectedLine line, Color layerColor)
        {
            graphics.DrawLine(ColorToPen(GetDisplayColor(layerColor, line.OriginalLine.Color)), line.P1, line.P2);
        }

        private void DrawEntity(Graphics graphics, ProjectedCircle circle, Color layerColor)
        {
            // TODO: handle rotation
            var width = circle.RadiusX * 2.0;
            var height = circle.RadiusY * 2.0;
            var topLeft = (Point)(circle.Center - new Point(circle.RadiusX, circle.RadiusX, 0.0));
            graphics.DrawEllipse(ColorToPen(GetDisplayColor(layerColor, circle.OriginalCircle.Color)), (float)topLeft.X, (float)topLeft.Y, (float)width, (float)height);
        }

        private void DrawEntity(Graphics graphics, ProjectedText text, Color layerColor)
        {
            // TODO: handle rotation
            var x = (float)text.Location.X;
            var y = (float)(text.Location.Y - text.Height);
            graphics.DrawString(text.OriginalText.Value, SystemFonts.DefaultFont, new SolidBrush(text.OriginalText.Color.DrawingColor), x, y);
        }

        private Pen ColorToPen(System.Drawing.Color color)
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

        private System.Drawing.Color GetDisplayColor(Color layerColor, Color primitiveColor)
        {
            System.Drawing.Color display;
            if (!primitiveColor.IsAuto)
                display = primitiveColor.DrawingColor;
            else if (!layerColor.IsAuto)
                display = layerColor.DrawingColor;
            else
                display = autoColor;

            return display;
        }
    }
}
