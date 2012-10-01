using System.ComponentModel.Composition;
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
                    new XAttribute("stroke", layer.Color.MediaColor.ToColorString()),
                    new XAttribute("fill", layer.Color.MediaColor.ToColorString()));
                // TODO: stroke-width="0.5"
                foreach (var entity in groupedEntity)
                {
                    var elem = ToXElement(entity);
                    if (elem != null)
                        g.Add(elem);
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

        private static XElement ToXElement(ProjectedEntity entity)
        {
            switch (entity.Kind)
            {
                case EntityKind.Line:
                    return ToXElement((ProjectedLine)entity);
                case EntityKind.Circle:
                    return ToXElement((ProjectedCircle)entity);
                case EntityKind.Text:
                    return ToXElement((ProjectedText)entity);
                default:
                    return null;
            }
        }

        private static XElement ToXElement(ProjectedLine line)
        {
            var xml = new XElement(Xmlns + "line",
                new XAttribute("x1", line.P1.X),
                new XAttribute("y1", line.P1.Y),
                new XAttribute("x2", line.P2.X),
                new XAttribute("y2", line.P2.Y));
            if (!line.OriginalLine.Color.IsAuto)
                xml.Add(new XAttribute("stroke", line.OriginalLine.Color.MediaColor.ToColorString()));
            return xml;
        }

        private static XElement ToXElement(ProjectedText text)
        {
            var xml = new XElement(Xmlns + "text",
                new XAttribute("x", text.Location.X),
                new XAttribute("y", text.Location.Y),
                new XAttribute("font-size", string.Format("{0}px", text.Height)),
                text.OriginalText.Value);
            if (!MathHelper.CloseTo(0, text.Rotation) && !MathHelper.CloseTo(360, text.Rotation))
                xml.Add(new XAttribute("transform", string.Format("rotate({0} {1} {2})", text.Rotation * -1.0, text.Location.X, text.Location.Y)));
            if (!text.OriginalText.Color.IsAuto)
            {
                xml.Add(new XAttribute("stroke", text.OriginalText.Color.MediaColor.ToColorString()));
                xml.Add(new XAttribute("fill", text.OriginalText.Color.MediaColor.ToColorString()));
            }
            return xml;
        }

        private static XElement ToXElement(ProjectedCircle ellipse)
        {
            var xml = new XElement(Xmlns + "ellipse",
                new XAttribute("cx", ellipse.Center.X),
                new XAttribute("cy", ellipse.Center.Y),
                new XAttribute("rx", ellipse.RadiusX),
                new XAttribute("ry", ellipse.RadiusY),
                new XAttribute("fill-opacity", 0));
            if (!ellipse.OriginalCircle.Color.IsAuto)
                xml.Add(new XAttribute("stroke", ellipse.OriginalCircle.Color.MediaColor.ToColorString()));
            return xml;
        }
    }
}
