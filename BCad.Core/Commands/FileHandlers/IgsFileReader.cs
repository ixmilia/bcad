using System.IO;
using BCad.Collections;
using BCad.Entities;
using BCad.FileHandlers;
using BCad.Igs;
using BCad.Igs.Entities;

namespace BCad.Commands.FileHandlers
{
    [ExportFileReader(IgsFileReader.DisplayName, IgsFileReader.FileExtension)]
    internal class IgsFileReader : IFileReader
    {
        public const string DisplayName = "IGES Files (" + FileExtension + ")";
        public const string FileExtension = ".igs";

        public void ReadFile(string fileName, Stream stream, out Drawing drawing, out ViewPort activeViewPort)
        {
            var file = IgsFile.Load(stream);
            var layer = new Layer("igs", Color.Auto);
            foreach (var entity in file.Entities)
            {
                var cadEntity = ToEntity(entity);
                if (cadEntity != null)
                {
                    layer = layer.Add(cadEntity);
                }
            }

            drawing = new Drawing(
                new DrawingSettings(fileName, UnitFormat.None, -1),
                new ReadOnlyTree<string, Layer>().Insert(layer.Name, layer));

            activeViewPort = new ViewPort(
                Point.Origin,
                Vector.ZAxis,
                Vector.YAxis,
                100.0);
        }

        private static Entity ToEntity(IgsEntity entity)
        {
            Entity result = null;
            switch (entity.Type)
            {
                case IgsEntityType.Line:
                    result = ToLine((IgsLine)entity);
                    break;
            }

            return result;
        }

        private static Line ToLine(IgsLine line)
        {
            return new Line(new Point(line.X1, line.Y1, line.Z1), new Point(line.X2, line.Y2, line.Z2), ToColor(line.Color));
        }

        private static Color ToColor(IgsColorNumber color)
        {
            return new Color((byte)color);
        }
    }
}
