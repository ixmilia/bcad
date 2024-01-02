namespace IxMilia.BCad.Utilities
{
    public struct QuantizeSettings
    {
        public double DistanceQuantum { get; }
        public double AngleQuantum { get; }

        public QuantizeSettings(double distanceQuantum, double angleQuantum)
        {
            DistanceQuantum = distanceQuantum;
            AngleQuantum = angleQuantum;
        }
    }
}
