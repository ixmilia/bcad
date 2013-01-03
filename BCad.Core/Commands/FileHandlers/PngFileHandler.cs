using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
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

        private static void DrawEntity(Graphics graphics, ProjectedEntity entity)
        {
            switch (entity.Kind)
            {
                case EntityKind.Line:
                    DrawEntity(graphics, (ProjectedLine)entity);
                    break;
                case EntityKind.Circle:
                case EntityKind.Ellipse:
                    //DrawEntity((ProjectedCircle)entity);
                case EntityKind.Text:
                    //DrawEntity((ProjectedText)entity);
                default:
                    break;
            }
        }

        private static void DrawEntity(Graphics graphics, ProjectedLine line)
        {
            graphics.DrawLine(new Pen(new SolidBrush(line.OriginalLine.Color.DrawingColor)), line.P1.ToDrawingPoint(), line.P2.ToDrawingPoint());
        }
    }
}
