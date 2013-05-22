namespace BCad.Iegs.Entities
{
    public class IegsCircle : IegsEntity
    {
        public override IegsEntityType Type { get { return IegsEntityType.Circle; } }

        public double PlaneDisplacement { get; set; }

        public IegsPoint Center { get; set; }

        public IegsPoint StartPoint { get; set; }

        public IegsPoint EndPoint { get; set; }

        public IegsCircle()
        {
            PlaneDisplacement = 0.0;
            Center = IegsPoint.Origin;
            StartPoint = IegsPoint.Origin;
            EndPoint = IegsPoint.Origin;
        }
    }
}
