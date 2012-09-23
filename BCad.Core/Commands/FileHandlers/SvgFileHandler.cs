using System.IO;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Xml;
using System.Xml.Linq;
using BCad.Entities;
using BCad.FileHandlers;

namespace BCad.Commands.FileHandlers
{
    [ExportFileWriter(SvgFileHandler.DisplayName, SvgFileHandler.FileExtension)]
    internal class SvgFileHandler : IFileWriter
    {
        public const string DisplayName = "SVF Files (" + FileExtension + ")";
        public const string FileExtension = ".svg";

        private static XNamespace Xmlns = "http://www.w3.org/2000/svg";

        public void WriteFile(IWorkspace workspace, Stream stream)
        {
            // create transform
            // normalizing to 640x480
            var actualWidth = 640.0;
            var actualHeight = 480.0;
            var bottomLeft = workspace.ActiveViewPort.BottomLeft;
            var height = workspace.ActiveViewPort.ViewHeight;
            var width = height * actualWidth / actualHeight;
            var transform = Matrix3D.Identity
                * TranslationMatrix(-bottomLeft.X, -bottomLeft.Y, 0)
                * ScalingMatrix(1, 1, 1);
                //* TranslationMatrix(-width / 2.0, -height / 2.0, 0);
                //* ScalingMatrix(2.0 / width, 2.0 / height, 1.0);

            // TODO: use XDocument with <!DOCTYPE svg PUBLIC "-//W3C//DTD SVG 1.1//EN" "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd">
            var root = new XElement(Xmlns + "svg",
                new XAttribute("version", "1.1"));
            // TODO: width="15cm" height="15cm" viewBox="0 0 640 480"
            foreach (var layer in from l in workspace.Drawing.Layers.Values
                                  where l.Entities.Count > 0
                                  orderby l.Name
                                  select l)
            {
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

                root.Add(new XComment(string.Format(" layer '{0}' ", layer.Name)));
                root.Add(g);
            }

            var settings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "  ",
                OmitXmlDeclaration = true
            };
            var writer = XmlWriter.Create(stream, settings);
            root.WriteTo(writer);
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

        private static Matrix3D TranslationMatrix(double x, double y, double z)
        {
            var matrix = Matrix3D.Identity;
            matrix.OffsetX = x;
            matrix.OffsetY = y;
            matrix.OffsetZ = z;
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
