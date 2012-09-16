namespace BCad
{
    public class ViewPort
    {
        private readonly Point bottomLeft;
        private readonly Vector sight;
        private readonly Vector up;
        private readonly double viewWidth;

        public Point BottomLeft { get { return bottomLeft; } }

        public Vector Sight { get { return sight; } }

        public Vector Up { get { return up; } }

        public double ViewWidth { get { return viewWidth; } }

        public ViewPort(Point bottomLeft, Vector sight, Vector up, double viewWidth)
        {
            this.bottomLeft = bottomLeft;
            this.sight = sight;
            this.up = up;
            this.viewWidth = viewWidth;
        }

        public ViewPort Update(Point bottomLeft = null, Vector sight = null, Vector up = null, double? viewWidth = null)
        {
            return new ViewPort(
                bottomLeft ?? this.bottomLeft,
                sight ?? this.sight,
                up ?? this.up,
                viewWidth ?? this.viewWidth);
        }
    }
}
