// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using BCad.Entities;

namespace BCad.Test
{
    public static class Entities
    {
        public static Line Line()
        {
            return new Line(Point.Origin, Point.Origin, null);
        }
    }
}
