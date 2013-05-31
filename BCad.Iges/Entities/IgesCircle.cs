namespace BCad.Iges.Entities
{
    public class IgesCircle : IgesEntity
    {
        public override IgesEntityType Type { get { return IgesEntityType.Circle; } }

        public override int LineCount { get { return 1; } }

        public double PlaneDisplacement { get; set; }

        public IgesPoint Center { get; set; }

        public IgesPoint StartPoint { get; set; }

        public IgesPoint EndPoint { get; set; }

        public IgesCircle()
        {
            PlaneDisplacement = 0.0;
            Center = IgesPoint.Origin;
            StartPoint = IgesPoint.Origin;
            EndPoint = IgesPoint.Origin;
        }
    }
}
