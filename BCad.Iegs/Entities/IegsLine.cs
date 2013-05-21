namespace BCad.Iegs.Entities
{
    public class IegsLine : IegsEntity
    {
        public override IegsEntityType Type { get { return IegsEntityType.Line; } }

        public IegsBounding Bounding { get; set; }

        public IegsPoint P1 { get; set; }
        public IegsPoint P2 { get; set; }

        public IegsLine()
        {
            Bounding = IegsBounding.BoundOnBothSides;
        }
    }
}
