﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Objects;

namespace BCad.Extensions
{
    public static class ObjectExtensions
    {
        public static bool EquivalentTo(this Arc arc, Entity obj)
        {
            var other = obj as Arc;
            if (other != null)
            {
                return arc.Center == other.Center
                    && arc.Color == other.Color
                    && arc.EndAngle == other.EndAngle
                    && arc.Normal == other.Normal
                    && arc.Radius == other.Radius
                    && arc.StartAngle == other.StartAngle;
            }

            return false;
        }

        public static bool EquivalentTo(this Circle circle, Entity obj)
        {
            var other = obj as Circle;
            if (other != null)
            {
                return circle.Center == other.Center
                    && circle.Color == other.Color
                    && circle.Normal == other.Normal
                    && circle.Radius == other.Radius;
            }

            return false;
        }

        public static bool EquivalentTo(this Line line, Entity obj)
        {
            var other = obj as Line;
            if (other != null)
            {
                return line.Color == other.Color
                    && line.P1 == other.P1
                    && line.P2 == other.P2;
            }

            return false;
        }

        public static bool EquivalentTo(this Entity a, Entity b)
        {
            if (a is Arc)
                return ((Arc)a).EquivalentTo(b);
            else if (a is Circle)
                return ((Circle)a).EquivalentTo(b);
            else if (a is Line)
                return ((Line)a).EquivalentTo(b);
            else
                throw new NotSupportedException("Unsupported object type");
        }
    }
}
