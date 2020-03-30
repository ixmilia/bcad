namespace IxMilia.BCad
{
    public struct DisplayTransform
    {
        public Matrix4 Transform { get; }
        public double DisplayXScale { get; }
        public double DisplayYScale { get; }

        public DisplayTransform(Matrix4 transform, double displayXScale, double displayYScale)
        {
            Transform = transform;
            DisplayXScale = displayXScale;
            DisplayYScale = displayYScale;
        }
    }
}
