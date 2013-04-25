namespace BCad.Igs.Entities
{
    public class IegsLine : IegsEntity
    {
        public override IegsEntityType Type { get { return IegsEntityType.Line; } }

        public IegsBounding Bounding { get; set; }

        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double Z1 { get; set; }

        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double Z2 { get; set; }

        public IegsLine()
        {
            Bounding = IegsBounding.BoundOnBothSides;
        }
    }
}
