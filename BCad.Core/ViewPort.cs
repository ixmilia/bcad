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
            if (double.IsInfinity(viewHeight) || double.IsNaN(viewHeight) || viewHeight <= 0.0)
                throw new ArgumentOutOfRangeException("ViewHeight must have a positive, real value.");
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

        /// <summary>
        /// Origin is top-left of viewport with x increasing to the right and y increasing to the bottom.  x = [0, viewPortWidth], y = [0, viewPortHeight].
        /// </summary>
        /// <param name="viewPortWidth"></param>
        /// <param name="viewPortHeight"></param>
        /// <returns></returns>
        public Matrix4 GetTransformationMatrixWindowsStyle(double viewPortWidth, double viewPortHeight)
        {
            var scale = viewPortHeight / ViewHeight;
            var projectionMatrix = Matrix4.Identity
                * Matrix4.CreateTranslate(0, viewPortHeight, 0)
                * Matrix4.CreateScale(scale, -scale, 1.0)
                * Matrix4.CreateTranslate(-BottomLeft.X, -BottomLeft.Y, 0);
            return projectionMatrix;
        }

        /// <summary>
        /// Origin is in the center of the view port with x increasing to the right and y increasing to the top.  x = [-1, 1], y = [-1, 1].
        /// </summary>
        /// <param name="viewPortWidth"></param>
        /// <param name="viewPortHeight"></param>
        /// <returns></returns>
        public Matrix4 GetTransformationMatrixDirect3DStyle(double viewPortWidth, double viewPortHeight)
        {
            var viewWidth = ViewHeight * viewPortWidth / viewPortHeight;
            var projectionMatrix = Matrix4.Identity
                * Matrix4.CreateScale(2.0f / viewWidth, 2.0f / ViewHeight, 1.0f)
                * Matrix4.CreateTranslate(-BottomLeft.X, -BottomLeft.Y, 0)
                * Matrix4.CreateTranslate(-viewWidth / 2.0f, -ViewHeight / 2.0f, 0);
            return projectionMatrix;
        }

        public static ViewPort CreateDefaultViewPort()
        {
            return new ViewPort(Point.Origin, Vector.ZAxis, Vector.YAxis, 100.0);
        }
    }
}
