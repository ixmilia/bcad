using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCad.Entities;
using BCad.Primitives;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("Draw.Arc", "ARC", "arc", "a")]
    internal class DrawArcCommand : ICommand
    {
        [Import]
        private IInputService InputService = null;

        [Import]
        private IWorkspace Workspace = null;

        public async Task<bool> Execute(object arg)
        {
            var first = await InputService.GetPoint(new UserDirective("First point"));
            if (!first.Cancel && first.HasValue)
            {
                var second = await InputService.GetPoint(new UserDirective("Second point"), (p) =>
                    {
                        return new[]
                        {
                            new PrimitiveLine(first.Value, p)
                        };
                    });
                if (!second.Cancel && second.HasValue)
                {
                    var third = await InputService.GetPoint(new UserDirective("Third point"), (p) =>
                        {
                            var a = PrimitiveEllipse.ThreePointArc(first.Value, second.Value, p);
                            if (a == null)
                            {
                                return new IPrimitive[0];
                            }
                            else
                            {
                                return new[] { a };
                            }
                        });
                    if (!third.Cancel && third.HasValue)
                    {
                        var primitiveArc = PrimitiveEllipse.ThreePointArc(first.Value, second.Value, third.Value, Workspace.DrawingPlane.Normal);
                        if (primitiveArc != null)
                        {
                            var arc = new Arc(
                                primitiveArc.Center,
                                primitiveArc.MajorAxis.Length,
                                primitiveArc.StartAngle,
                                primitiveArc.EndAngle,
                                primitiveArc.Normal,
                                IndexedColor.Auto);
                            Workspace.AddToCurrentLayer(arc);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
