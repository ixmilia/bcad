namespace IxMilia.BCad.Entities
{
    public class ProjectedText : ProjectedEntity
    {
        public override EntityKind Kind
        {
            get { return EntityKind.Text; }
        }

        public Text OriginalText { get; private set; }

        public Point Location { get; private set; }

        public double Height { get; private set; }

        public double Rotation { get; private set; }

        public ProjectedText(Text text, Layer layer, Point location, double height, double rotation)
            : base(layer)
        {
            OriginalText = text;
            Location = location;
            Height = height;
            Rotation = rotation;
        }
    }
}
