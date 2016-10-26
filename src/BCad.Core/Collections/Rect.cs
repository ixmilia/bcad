// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace BCad.Collections
{
    public struct Rect
    {
        public double Left;
        public double Top;
        public double Width;
        public double Height;

        public Point TopLeft => new Point(Left, Top, 0.0);
        public Point TopRight => new Point(Left + Width, Top, 0.0);
        public Point BottomLeft => new Point(Left, Top + Height, 0.0);
        public Point BottomRight => new Point(Left + Width, Top + Height, 0.0);

        public Rect(double left, double top, double width, double height)
            : this()
        {
            if (double.IsNaN(left))
            {
                throw new ArgumentOutOfRangeException(nameof(left));
            }

            if (double.IsNaN(top))
            {
                throw new ArgumentOutOfRangeException(nameof(top));
            }

            if (width < 0.0 || double.IsNaN(width))
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height < 0.0 || double.IsNaN(height))
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }

        public bool Contains(Rect other)
        {
            return other.Left >= this.Left && (other.Left + other.Width) <= (this.Left + this.Width)
                && other.Top >= this.Top && (other.Top + other.Height) <= (this.Top + this.Height);
        }

        public bool Contains(Point point)
        {
            return point.X >= Left && point.X <= (Left + Width)
                && point.Y >= Top && point.Y <= (Top + Height);
        }

        public bool Intersects(Rect other)
        {
            var left1 = Math.Max(this.Left, other.Left);
            var left2 = Math.Min(this.Left + this.Width, other.Left + other.Width);
            var top1 = Math.Max(this.Top, other.Top);
            var top2 = Math.Min(this.Top + this.Height, other.Top + other.Height);
            return left2 >= left1 && top2 >= top1;
        }
    }
}
