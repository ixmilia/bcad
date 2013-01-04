using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using BCad.Entities;
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

        private Dictionary<Color, Brush> brushCache = new Dictionary<Color, Brush>();
        private Dictionary<Color, Pen> penCache = new Dictionary<Color, Pen>();

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
                        DrawEntity(graphics, entity);
                    }
                }
            }

            image.Save(stream, ImageFormat.Png);
        }

        private void DrawEntity(Graphics graphics, ProjectedEntity entity)
        {
            switch (entity.Kind)
            {
                case EntityKind.Line:
                    DrawEntity(graphics, (ProjectedLine)entity);
                    break;
                case EntityKind.Circle:
                case EntityKind.Ellipse:
                    DrawEntity(graphics, (ProjectedCircle)entity);
                    break;
                case EntityKind.Text:
                    DrawEntity(graphics, (ProjectedText)entity);
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
                var brush = new SolidBrush(color.DrawingColor);
                brushCache.Add(color, brush);
                return brush;
            }
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

        private void DrawEntity(Graphics graphics, ProjectedLine line)
        {
            graphics.DrawLine(ColorToPen(line.OriginalLine.Color), line.P1.ToDrawingPoint(), line.P2.ToDrawingPoint());
        }

        private void DrawEntity(Graphics graphics, ProjectedCircle circle)
        {
            // TODO: handle rotation
        }

        private void DrawEntity(Graphics graphics, ProjectedText text)
        {
            // TODO: handle rotation
            var x = (float)text.Location.X;
            var y = (float)(text.Location.Y - text.Height);
            graphics.DrawString(text.OriginalText.Value, SystemFonts.DefaultFont, new SolidBrush(text.OriginalText.Color.DrawingColor), x, y);
        }
    }
}
