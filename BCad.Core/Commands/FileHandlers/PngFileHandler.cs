using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using BCad.Entities;
using BCad.Extensions;
using BCad.FileHandlers;
using BCad.Helpers;
using BCad.Services;

namespace BCad.Commands.FileHandlers
{
    [ExportFileWriter(PngFileHandler.DisplayName, PngFileHandler.FileExtension)]
    internal class PngFileHandler : IFileWriter
    {
        public const string DisplayName = "PNG Files (" + FileExtension + ")";
        public const string FileExtension = ".png";

        [Import]
        private IExportService ExportService = null;

        private Dictionary<System.Drawing.Color, Brush> brushCache = new Dictionary<System.Drawing.Color, Brush>();
        private Dictionary<System.Drawing.Color, Pen> penCache = new Dictionary<System.Drawing.Color, Pen>();
        private System.Drawing.Color autoColor = System.Drawing.Color.Black;

        public void WriteFile(IWorkspace workspace, Stream stream)
        {
            var projected = ExportService.ProjectTo2D(workspace.Drawing, workspace.ActiveViewPort);

            var image = new Bitmap(500, 500);
            using (var graphics = Graphics.FromImage(image))
            {
                graphics.FillRectangle(Brushes.Black, new Rectangle(0, 0, image.Width, image.Height));
                foreach (var groupedEntity in projected.GroupBy(p => p.OriginalLayer).OrderBy(x => x.Key.Name))
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

        private void DrawEntity(Graphics graphics, ProjectedLine line, Color layerColor)
        {
            graphics.DrawLine(ColorToPen(GetDisplayColor(layerColor, line.OriginalLine.Color)), line.P1, line.P2);
        }

        private void DrawEntity(Graphics graphics, ProjectedCircle circle, Color layerColor)
        {
            // TODO: handle rotation
        }

        private void DrawEntity(Graphics graphics, ProjectedText text, Color layerColor)
        {
            // TODO: handle rotation
            var x = (float)text.Location.X;
            var y = (float)(text.Location.Y - text.Height);
            graphics.DrawString(text.OriginalText.Value, SystemFonts.DefaultFont, new SolidBrush(text.OriginalText.Color.DrawingColor), x, y);
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
