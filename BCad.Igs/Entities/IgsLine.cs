namespace BCad.Igs.Entities
{
    public class IgsLine : IgsEntity
    {
        public override IgsEntityType Type { get { return IgsEntityType.Line; } }

        public IgsBounding Bounding { get; set; }

        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double Z1 { get; set; }

        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double Z2 { get; set; }

        public IgsLine()
        {
            Bounding = IgsBounding.BoundOnBothSides;
        }
    }
}
