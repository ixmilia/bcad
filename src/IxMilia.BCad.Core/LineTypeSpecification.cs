using System;
using System.Collections.Generic;

namespace IxMilia.BCad
{
    public class LineTypeSpecification : IEquatable<LineTypeSpecification>
    {
        public string Name { get; }
        public double Scale { get; }

        public LineTypeSpecification(string name, double scale)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Scale = scale;
        }

        public LineTypeSpecification Update(
            string name = null,
            Optional<double> scale = default)
        {
            var newName = name ?? Name;
            var newScale = scale.HasValue ? scale.Value : Scale;

            if (newName == Name &&
                newScale == Scale)
            {
                return this;
            }

            return new LineTypeSpecification(newName, newScale);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LineTypeSpecification);
        }

        public bool Equals(LineTypeSpecification other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            int hashCode = 1215559687;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Scale.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(LineTypeSpecification a, LineTypeSpecification b)
        {
            if (a is null && b is null)
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a.Name == b.Name
                && a.Scale == b.Scale; ;
        }

        public static bool operator !=(LineTypeSpecification a, LineTypeSpecification b)
        {
            return !(a == b);
        }
    }
}
