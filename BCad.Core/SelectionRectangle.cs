namespace BCad
{
    public class SelectionRectangle
    {
        public Point TopLeft { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public Point WorldPoint { get; private set; }

        public SelectionRectangle(Point topLeft, double width, double height, Point worldPoint)
        {
            TopLeft = topLeft;
            Width = width;
            Height = height;
            WorldPoint = worldPoint;
        }
    }
}
