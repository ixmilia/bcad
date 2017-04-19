// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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

        public ViewPort Update(
            Optional<Point> bottomLeft = default(Optional<Point>),
            Optional<Vector> sight = default(Optional<Vector>),
            Optional<Vector> up = default(Optional<Vector>),
            Optional<double> viewHeight = default(Optional<double>))
        {
            var newBottomLeft = bottomLeft.HasValue ? bottomLeft.Value : this.bottomLeft;
            var newSight = sight.HasValue ? sight.Value : this.sight;
            var newUp = up.HasValue ? up.Value : this.up;
            var newViewHeight = viewHeight.HasValue ? viewHeight.Value : this.viewHeight;

            if (newBottomLeft == this.bottomLeft &&
                newSight == this.sight &&
                newUp == this.up &&
                newViewHeight == this.viewHeight)
            {
                return this;
            }

            return new ViewPort(newBottomLeft, newSight, newUp, newViewHeight);
        }

        /// <summary>
        /// Origin is bottom-left of viewport with x increasing to the right and y increasing to the top.  x = [0, viewPortWidth], y = [0, viewPortHeight].
        /// </summary>
        public Matrix4 GetTransformationMatrixCartesianStyle(double viewPortWidth, double viewPortHeight)
        {
            var scale = viewPortHeight / ViewHeight;
            var projectionMatrix = Matrix4.Identity
                * Matrix4.CreateScale(scale, scale, 1.0)
                * Matrix4.CreateTranslate(-BottomLeft.X, -BottomLeft.Y, 0);
            return projectionMatrix;
        }

        /// <summary>
        /// Origin is top-left of viewport with x increasing to the right and y increasing to the bottom.  x = [0, viewPortWidth], y = [0, viewPortHeight].
        /// </summary>
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
