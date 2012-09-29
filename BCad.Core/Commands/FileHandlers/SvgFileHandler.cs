using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Xml;
using System.Xml.Linq;
using BCad.Entities;
using BCad.Extensions;
using BCad.FileHandlers;
using BCad.Services;

namespace BCad.Commands.FileHandlers
{
    [ExportFileWriter(SvgFileHandler.DisplayName, SvgFileHandler.FileExtension)]
    internal class SvgFileHandler : IFileWriter
    {
        public const string DisplayName = "SVF Files (" + FileExtension + ")";
        public const string FileExtension = ".svg";

        private static XNamespace Xmlns = "http://www.w3.org/2000/svg";

        [Import]
        private IExportService ExportService = null;

        public void WriteFile(IWorkspace workspace, Stream stream)
        {
            var projected = ExportService.ProjectTo2D(workspace.Drawing, workspace.ActiveViewPort);

            var root = new XElement(Xmlns + "svg",
                //new XAttribute("width", string.Format("{0}in", 6)),
                //new XAttribute("height", string.Format("{0}in", 6)),
                //new XAttribute("viewBox", string.Format("{0} {1} {2} {3}", -500, -500, 500, 500)),
                new XAttribute("version", "1.1"));
            foreach (var groupedEntity in projected.GroupBy(p => p.OriginalLayer).OrderBy(x => x.Key.Name))
            {
                var layer = groupedEntity.Key;
                root.Add(new XComment(string.Format(" layer '{0}' ", layer.Name)));
                var g = new XElement(Xmlns + "g",
                    new XAttribute("stroke", layer.Color.MediaColor.ToColorString()));
                // TODO: stroke-width="0.5"
                foreach (var entity in groupedEntity)
                {
                    XElement elem;
                    switch (entity.Kind)
                    {
                        case EntityKind.Line:
                            elem = ToXElement((ProjectedLine)entity);
                            break;
                        default:
                            elem = null;
                            break;
                    }

                    if (elem != null)
                    {
                        g.Add(elem);
                    }
                }

                root.Add(g);
            }

            var settings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "  "
            };
            var writer = XmlWriter.Create(stream, settings);
            var doc = new XDocument(
                new XDocumentType("svg", "-//W3C//DTD SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", null),
                root);
            doc.WriteTo(writer);
            writer.Flush();
            writer.Close();
        }

        private static XElement ToXElement(ProjectedLine line)
        {
            return new XElement(Xmlns + "line",
                new XAttribute("x1", line.P1.X),
                new XAttribute("y1", line.P1.Y),
                new XAttribute("x2", line.P2.X),
                new XAttribute("y2", line.P2.Y),
                line.OriginalLine.Color.IsAuto
                    ? null
                    : new XAttribute("stroke", line.OriginalLine.Color.MediaColor.ToColorString()));
        }
    }
}
