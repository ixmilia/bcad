// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace IxMilia.BCad.Primitives
{
    public class PrimitivePoint : IPrimitive
    {
        public Point Location { get; private set; }
        public CadColor? Color { get; private set; }
        public PrimitiveKind Kind { get { return PrimitiveKind.Point; } }

        public PrimitivePoint(Point location, CadColor? color = null)
        {
            Location = location;
            Color = color;
        }
    }
}
