using System;
using System.Collections.Generic;
using System.Linq;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.SnapPoints;

namespace IxMilia.BCad.Entities
{
    public class Image : Entity
    {
        public override EntityKind Kind => EntityKind.Image;

        private PrimitiveImage _image;

        public Point Location => _image.Location;
        public byte[] ImageData => _image.ImageData;
        public string Path => _image.Path;
        public double Width => _image.Width;
        public double Height => _image.Height;
        public double Rotation => _image.Rotation;

        private IPrimitive[] _primitives;
        private SnapPoint[] _snapPoints;

        public override BoundingBox BoundingBox { get; }

        public Image(Point location, string path, byte[] imageData, double width, double height, double rotation, CadColor? color = null, object tag = null)
            : this(new PrimitiveImage(location, imageData, path, width, height, rotation, color), tag)
        {
        }

        public Image(PrimitiveImage image, object tag = null)
            : base(image.Color, tag)
        {
            _image = image ?? throw new ArgumentNullException(nameof(image));

            var boundaryLines = _image.GetBoundaryLines();
            _primitives = new IPrimitive[] { _image }.Concat(boundaryLines).ToArray();
            _snapPoints = boundaryLines.SelectMany(l =>
                new SnapPoint[]
                {
                    new EndPoint(l.P1),
                    new MidPoint(l.MidPoint()),
                }).ToArray();
            BoundingBox = _image.GetBoundingBox();
        }

        public override IEnumerable<IPrimitive> GetPrimitives() => _primitives;

        public override IEnumerable<SnapPoint> GetSnapPoints() => _snapPoints;

        public Image Update(
            Optional<Point> location = default(Optional<Point>),
            byte[] imageData = null,
            string path = null,
            Optional<double> width = default(Optional<double>),
            Optional<double> height = default(Optional<double>),
            Optional<double> rotation = default(Optional<double>),
            Optional<CadColor?> color = default(Optional<CadColor?>),
            Optional<object> tag = default(Optional<object>))
        {
            var newLocation = location.HasValue ? location.Value : Location;
            var newPath = path ?? Path;
            var newImageData = imageData ?? ImageData;
            var newWidth = width.HasValue ? width.Value : Width;
            var newHeight = height.HasValue ? height.Value : Height;
            var newRotation = rotation.HasValue ? rotation.Value : Rotation;
            var newColor = color.HasValue ? color.Value : Color;
            var newTag = tag.HasValue ? tag.Value : Tag;

            if (newLocation == Location &&
                newPath == Path &&
                newWidth == Width &&
                newHeight == Height &&
                newRotation == Rotation &&
                newColor == Color &&
                newTag == Tag)
            {
                return this;
            }

            return new Image(newLocation, newPath, newImageData, newWidth, newHeight, newRotation, newColor, newTag);
        }
    }
}
