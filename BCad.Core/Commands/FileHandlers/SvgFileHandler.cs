using System.IO;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Xml;
using System.Xml.Linq;
using BCad.Entities;
using BCad.FileHandlers;

namespace BCad.Commands.FileHandlers
{
    [ExportFileReader(SvgFileHandler.DisplayName, SvgFileHandler.FileExtension)]
    [ExportFileWriter(SvgFileHandler.DisplayName, SvgFileHandler.FileExtension)]
    internal class SvgFileHandler : IFileReader, IFileWriter
    {
        public const string DisplayName = "SVF Files (" + FileExtension + ")";
        public const string FileExtension = ".svg";

        private static XNamespace Xmlns = "http://www.w3.org/2000/svg";

        public void ReadFile(string fileName, Stream stream, out Drawing drawing, out ViewPort activeViewPort)
        {
            var doc = XDocument.Load(stream);
            var root = doc.Root;
            var dwg = new Drawing();

            foreach (var node in root.Elements())
            {
                dwg = ReadEntities(node, dwg);
            }

            drawing = dwg;
            activeViewPort = new ViewPort(Point.Origin, Vector.ZAxis, Vector.YAxis, 10);
        }

        public void WriteFile(IWorkspace workspace, Stream stream)
        {
            // create transform
            // normalizing to 11x8.5
            var actualWidth = 11.0;
            var actualHeight = 8.5;
            var bottomLeft = workspace.ActiveViewPort.BottomLeft;
            var height = workspace.ActiveViewPort.ViewHeight;
            var width = height * actualWidth / actualHeight;
            var transform = Matrix3D.Identity
                * TranslationMatrix(-bottomLeft.X, -bottomLeft.Y, 0)
                * ScalingMatrix(1, -1, 1)
                * TranslationMatrix(0, height, 0);

            var root = new XElement(Xmlns + "svg",
                new XAttribute("width", string.Format("{0}in", actualWidth)),
                new XAttribute("height", string.Format("{0}in", actualHeight)),
                //new XAttribute("viewBox", string.Format("{0} {1} {2} {3}", 0, 0, actualWidth, actualHeight)),
                new XAttribute("version", "1.1"));
            foreach (var layer in from l in workspace.Drawing.Layers.Values
                                  where l.Entities.Count > 0
                                  orderby l.Name
                                  select l)
            {
                root.Add(new XComment(string.Format(" layer '{0}' ", layer.Name)));
                var g = new XElement(Xmlns + "g",
                    new XAttribute("stroke", layer.Color.MediaColor.ToColorString()));
                // TODO: stroke-width="0.5"
                foreach (var entity in layer.Entities.OrderBy(x => x.Id))
                {
                    XElement elem;
                    switch (entity.Kind)
                    {
                        case EntityKind.Line:
                            elem = ToXElement((Line)entity, transform);
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
                IndentChars = "  ",
                OmitXmlDeclaration = true
            };
            var writer = XmlWriter.Create(stream, settings);
            var doc = new XDocument(
                new XDocumentType("svg", "-//W3C//DTD SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", null),
                root);
            doc.WriteTo(writer);
            writer.Flush();
            writer.Close();
        }

        private static XElement ToXElement(Line line, Matrix3D transform)
        {
            var p1 = transform.Transform(line.P1.ToPoint3D());
            var p2 = transform.Transform(line.P2.ToPoint3D());
            return new XElement(Xmlns + "line",
                new XAttribute("x1", p1.X),
                new XAttribute("y1", p1.Y),
                new XAttribute("x2", p2.X),
                new XAttribute("y2", p2.Y),
                line.Color.IsAuto ? null : new XAttribute("stroke", line.Color.MediaColor.ToColorString()));
        }

        private static Drawing ReadEntities(XElement node, Drawing drawing)
        {
            switch (node.Name.LocalName)
            {
                case "line":
                    var x1 = double.Parse(node.Attribute("x1").Value);
                    var y1 = double.Parse(node.Attribute("y1").Value);
                    var x2 = double.Parse(node.Attribute("x2").Value);
                    var y2 = double.Parse(node.Attribute("y2").Value);
                    var color = node.Attribute("stroke") == null
                        ? Color.Auto
                        : Color.Auto; //: new Color(node.Attribute("stroke").Value.ParseColor());
                    return drawing.AddToCurrentLayer(new Line(new Point(x1, y1, 0), new Point(x2, y2, 0), color));
                case "g":
                    foreach (var sub in node.Elements())
                    {
                        drawing = ReadEntities(sub, drawing);
                    }
                    return drawing;
                default:
                    return drawing;
            }
        }

        private static Matrix3D TranslationMatrix(double dx, double dy, double dz)
        {
            var matrix = Matrix3D.Identity;
            matrix.OffsetX = dx;
            matrix.OffsetY = dy;
            matrix.OffsetZ = dz;
            return matrix;
        }

        private static Matrix3D ScalingMatrix(double xs, double ys, double zs)
        {
            var matrix = Matrix3D.Identity;
            matrix.M11 = xs;
            matrix.M22 = ys;
            matrix.M33 = zs;
            return matrix;
        }
    }
}
