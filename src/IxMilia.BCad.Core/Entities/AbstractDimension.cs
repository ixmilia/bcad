namespace IxMilia.BCad.Entities
{
    public abstract class AbstractDimension : Entity
    {
        public string DimensionStyleName { get; }
        public CadColor? TextColor { get; }

        public double ActualMeasurement { get; protected set; }

        protected AbstractDimension(string dimensionStyleName, CadColor? textColor, CadColor? color, LineTypeSpecification lineTypeSpecification, object tag)
            : base(color, lineTypeSpecification, tag)
        {
            TextColor = textColor;
            DimensionStyleName = dimensionStyleName;
        }
    }
}
