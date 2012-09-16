using System;

namespace BCad
{
    public class ViewPort
    {
        private readonly Point bottomLeft;
        private readonly Vector sight;
        private readonly Vector up;
        private readonly double viewHeight;

        public Point BottomLeft { get { return bottomLeft; } }

        public Vector Sight { get { return sight; } }

        public Vector Up { get { return up; } }

        public double ViewHeight { get { return viewHeight; } }

        public ViewPort(Point bottomLeft, Vector sight, Vector up, double viewHeight)
        {
            if (bottomLeft == null)
                throw new ArgumentNullException("bottomLeft");
            if (sight == null)
                throw new ArgumentNullException("sight");
            if (up == null)
                throw new ArgumentNullException("up");
            if (sight == Vector.Zero)
                throw new ArgumentOutOfRangeException("Sight vector cannot be zero.");
            if (up == Vector.Zero)
                throw new ArgumentOutOfRangeException("Up vector cannot be zero.");
            if (double.IsInfinity(viewHeight) || double.IsNaN(viewHeight) || viewHeight == 0.0)
                throw new ArgumentOutOfRangeException("ViewHeight must have a non-zero, real value.");
            this.bottomLeft = bottomLeft;
            this.sight = sight;
            this.up = up;
            this.viewHeight = viewHeight;
        }

        public ViewPort Update(Point bottomLeft = null, Vector sight = null, Vector up = null, double? viewHeight = null)
        {
            return new ViewPort(
                bottomLeft ?? this.bottomLeft,
                sight ?? this.sight,
                up ?? this.up,
                viewHeight ?? this.viewHeight);
        }
    }
}
