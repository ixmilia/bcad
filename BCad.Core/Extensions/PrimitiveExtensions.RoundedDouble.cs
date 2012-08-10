using System;
using BCad.Helpers;

namespace BCad.Extensions
{
    public static partial class PrimitiveExtensions
    {
        /// <summary>
        /// Used to force rounding to a certain level of precision after each operation.
        /// </summary>
        private struct RoundedDouble
        {
            private const int Digits = 12;

            public double Value { get; private set; }

            public RoundedDouble(double value)
                : this()
            {
                this.Value = value;
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is RoundedDouble)
                {
                    var other = (RoundedDouble)obj;
                    return this.Value == other.Value;
                }

                return false;
            }

            public static implicit operator double(RoundedDouble rounded)
            {
                return rounded.Value;
            }

            public static implicit operator RoundedDouble(double d)
            {
                return new RoundedDouble(d);
            }

            private static double Round(double d)
            {
                return Math.Round(d, MathHelper.Precision);
            }

            public static RoundedDouble operator +(RoundedDouble a, RoundedDouble b)
            {
                return Round(a.Value + b.Value);
            }

            public static RoundedDouble operator -(RoundedDouble a, RoundedDouble b)
            {
                return Round(a.Value - b.Value);
            }

            public static RoundedDouble operator *(RoundedDouble a, RoundedDouble b)
            {
                return Round(a.Value * b.Value);
            }

            public static RoundedDouble operator /(RoundedDouble a, RoundedDouble b)
            {
                return Round(a.Value / b.Value);
            }

            public static bool operator ==(RoundedDouble a, RoundedDouble b)
            {
                return a.Value == b.Value;
            }

            public static bool operator !=(RoundedDouble a, RoundedDouble b)
            {
                return a.Value != b.Value;
            }
        }
    }
}
