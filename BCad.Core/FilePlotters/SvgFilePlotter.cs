using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using BCad.Entities;
using BCad.Extensions;
using BCad.FilePlotters;
using BCad.Helpers;

namespace BCad.Commands.FilePlotters
{
    [ExportFilePlotter(SvgFilePlotter.DisplayName, SvgFilePlotter.FileExtension)]
    internal class SvgFilePlotter : IFilePlotter
    {
        public const string DisplayName = "SVG Files (" + FileExtension + ")";
        public const string FileExtension = ".svg";

        private static XNamespace Xmlns = "http://www.w3.org/2000/svg";

        public void Plot(IEnumerable<ProjectedEntity> entities, double width, double height, Stream stream)
        {
            var root = new XElement(Xmlns + "svg",
                //new XAttribute("width", string.Format("{0}in", 6)),
                //new XAttribute("height", string.Format("{0}in", 6)),
                new XAttribute("viewBox", string.Format("{0} {1} {2} {3}", 0, 0, width, height)),
                new XAttribute("version", "1.1"));
            foreach (var groupedEntity in entities.GroupBy(p => p.OriginalLayer).OrderBy(x => x.Key.Name))
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
                case EntityKind.Arc:
                    return ToXElement((ProjectedArc)entity);
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
            AddStrokeIfNotDefault(xml, line.OriginalLine.Color);
            return xml;
        }

        private static XElement ToXElement(ProjectedText text)
        {
            var xml = new XElement(Xmlns + "text",
                new XAttribute("x", text.Location.X),
                new XAttribute("y", text.Location.Y),
                new XAttribute("font-size", string.Format("{0}px", text.Height)),
                text.OriginalText.Value);
            AddRotationTransform(xml, text.Rotation, text.Location);
            AddStrokeIfNotDefault(xml, text.OriginalText.Color);
            AddFillIfNotDefault(xml, text.OriginalText.Color);
            return xml;
        }

        private static XElement ToXElement(ProjectedCircle circle)
        {
            var xml = new XElement(Xmlns + "ellipse",
                new XAttribute("cx", circle.Center.X),
                new XAttribute("cy", circle.Center.Y),
                new XAttribute("rx", circle.RadiusX),
                new XAttribute("ry", circle.RadiusY),
                new XAttribute("fill-opacity", 0));
            AddRotationTransform(xml, circle.Rotation, circle.Center);
            AddStrokeIfNotDefault(xml, circle.OriginalCircle.Color);
            return xml;
        }

        private static XElement ToXElement(ProjectedArc arc)
        {
            var pathData = string.Format("M {0} {1} a {2} {3} {4} {5} {6} {7} {8}",
                arc.StartPoint.X,
                arc.StartPoint.Y,
                arc.RadiusX,
                arc.RadiusY,
                0, // x axis rotation
                0, // flag
                0, // flag
                arc.EndPoint.X - arc.StartPoint.X,
                arc.EndPoint.Y - arc.StartPoint.Y);
            var lineData = string.Format("M {0} {1} L {2} {3}",
                arc.StartPoint.X,
                arc.StartPoint.Y,
                arc.EndPoint.X,
                arc.EndPoint.Y);
            var xml = new XElement(Xmlns + "path",
                new XAttribute("d", pathData),
                new XAttribute("fill-opacity", 0));
            AddRotationTransform(xml, arc.Rotation, arc.Center);
            AddStrokeIfNotDefault(xml, arc.OriginalArc.Color);
            return xml;
        }

        private static void AddRotationTransform(XElement xml, double angle, Point location)
        {
            if (!MathHelper.CloseTo(0, angle) && !MathHelper.CloseTo(360, angle))
            {
                var rotateText = string.Format("rotate({0} {1} {2})", angle * -1.0, location.X, location.Y);
                var transform = xml.Attribute("transform");
                if (transform == null)
                {
                    // add new attribute
                    xml.Add(new XAttribute("transform", rotateText));
                }
                else
                {
                    // append a space and the rotation
                    transform.Value += " " + rotateText;
                }
            }
        }

        private static void AddStrokeIfNotDefault(XElement xml, Color color)
        {
            if (!color.IsAuto)
            {
                var stroke = xml.Attribute("stroke");
                var colorString = color.MediaColor.ToColorString();
                if (stroke == null)
                {
                    // add new attribute
                    xml.Add(new XAttribute("stroke", colorString));
                }
                else
                {
                    // replace attribute
                    stroke.Value = colorString;
                }
            }
        }

        private static void AddFillIfNotDefault(XElement xml, Color color)
        {
            if (!color.IsAuto)
            {
                var stroke = xml.Attribute("fill");
                var colorString = color.MediaColor.ToColorString();
                if (stroke == null)
                {
                    // add new attribute
                    xml.Add(new XAttribute("fill", colorString));
                }
                else
                {
                    // replace attribute
                    stroke.Value = colorString;
                }
            }
        }
    }
}
