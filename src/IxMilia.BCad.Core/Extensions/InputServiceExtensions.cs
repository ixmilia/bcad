using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.Services;

namespace IxMilia.BCad
{
    public static class InputServiceExtensions
    {
        public static async Task<ValueOrDirective<double>> GetAngleInDegrees(this IInputService inputService, string prompt, Func<double, IEnumerable<IPrimitive>> onCursorMove = null)
        {
            onCursorMove ??= (_) => Array.Empty<IPrimitive>();
            prompt += " or [r]eference";
            var angleCandidateValue = await inputService.GetText(prompt);
            if (angleCandidateValue.Cancel || !angleCandidateValue.HasValue)
            {
                return ValueOrDirective<double>.GetCancel();
            }

            if (double.TryParse(angleCandidateValue.Value, out var angle))
            {
                return ValueOrDirective<double>.GetValue(angle);
            }
            else if (angleCandidateValue.Value == "r")
            {
                var pivotPoint = await inputService.GetPoint(new UserDirective("Pivot point"));
                if (pivotPoint.Cancel || !pivotPoint.HasValue)
                {
                    return ValueOrDirective<double>.GetCancel();
                }

                var firstReferencePoint = await inputService.GetPoint(new UserDirective("Reference point"), onCursorMove: cursor =>
                {
                    return new IPrimitive[]
                    {
                        new PrimitiveLine(pivotPoint.Value, cursor)
                    };
                });

                if (firstReferencePoint.Cancel || !firstReferencePoint.HasValue)
                {
                    return ValueOrDirective<double>.GetCancel();
                }

                var v1 = firstReferencePoint.Value - pivotPoint.Value;
                var secondReferencePoint = await inputService.GetPoint(new UserDirective("Second reference point"), onCursorMove: cursor =>
                {
                    var v2Temp = cursor - pivotPoint.Value;
                    var angleInRadians = Vector.AngleBetweenInRadians(v1, v2Temp);
                    var angleInDegrees = angleInRadians * MathHelper.RadiansToDegrees;
                    return new IPrimitive[]
                    {
                        new PrimitiveLine(pivotPoint.Value, firstReferencePoint.Value),
                        new PrimitiveLine(pivotPoint.Value, cursor)
                    }.Concat(onCursorMove(angleInDegrees));
                });

                if (secondReferencePoint.Cancel || !secondReferencePoint.HasValue)
                {
                    return ValueOrDirective<double>.GetCancel();
                }

                var v2 = secondReferencePoint.Value - pivotPoint.Value;
                var angleInRadians = Vector.AngleBetweenInRadians(v1, v2);
                var angleInDegrees = angleInRadians * MathHelper.RadiansToDegrees;
                return ValueOrDirective<double>.GetValue(angleInDegrees);
            }
            else
            {
                return ValueOrDirective<double>.GetCancel();
            }
        }
    }
}
