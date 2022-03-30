using System;
using IxMilia.BCad.Helpers;

namespace IxMilia.BCad.Primitives
{
    public class PrimitiveImage : IPrimitive
    {
        public Point Location { get; }
        public byte[] ImageData { get; }
        public string Path { get; }
        public double Width { get; }
        public double Height { get; }
        public double Rotation { get; }
        public CadColor? Color { get; private set; }

        public PrimitiveKind Kind => PrimitiveKind.Image;

        private PrimitiveLine[] _boundaryLines;
        private BoundingBox _boundingBox;

        public PrimitiveImage(Point location, byte[] imageData, string path, double width, double height, double rotation, CadColor? color = null)
        {
            Location = location;
            ImageData = imageData ?? throw new ArgumentNullException(nameof(imageData));
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Width = width;
            Height = height;
            Rotation = rotation;
            Color = color;

            var radians = Rotation * MathHelper.DegreesToRadians;
            var upVector = new Vector(-Math.Sin(radians) * Height, Math.Cos(radians) * Height, 0.0);
            var rightVector = new Vector(Math.Cos(radians) * Width, Math.Sin(radians) * Width, 0.0);
            var bottomLeft = Location;
            var bottomRight = bottomLeft + rightVector;
            var topLeft = bottomLeft + upVector;
            var topRight = bottomRight + upVector;
            _boundaryLines = new[]
            {
                new PrimitiveLine(bottomLeft, bottomRight, Color),
                new PrimitiveLine(bottomRight, topRight, Color),
                new PrimitiveLine(topRight, topLeft, Color),
                new PrimitiveLine(topLeft, bottomLeft, Color),
            };

            _boundingBox = BoundingBox.FromPoints(bottomLeft, bottomRight, topLeft, topRight);
        }

        internal BoundingBox GetBoundingBox() => _boundingBox;

        internal PrimitiveLine[] GetBoundaryLines() => _boundaryLines;
    }
}
