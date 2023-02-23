using System;
using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.Utilities;
using IxMilia.BCad.Helpers;

namespace IxMilia.BCad.Commands
{
    public class DrawCircleCommand : ICadCommand
    {
        private static readonly double IsoMinorRatio = Math.Sqrt(1.5) / Math.Sqrt(2.0) * 2.0 / 3.0;

        private enum CircleMode
        {
            Radius,
            Diameter,
            Isometric
        }

        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            Entity circle = null;
            var drawingPlane = workspace.DrawingPlane;
            var lineTypeSpecification = workspace.Drawing.Settings.CurrentLineTypeSpecification;

            var initial = await workspace.InputService.GetPoint(new UserDirective("Select center, [ttr], or [th]ree-point", "ttr", "th"));
            if (initial.Cancel) return false;
            if (initial.HasValue)
            {
                var mode = CircleMode.Radius;
                while (circle == null)
                {
                    switch (mode)
                    {
                        case CircleMode.Radius:
                            {
                                var rad = await workspace.InputService.GetDistanceFromPoint(initial.Value, new UserDirective("Enter radius or [d]iameter/[i]sometric", "d", "i"), p =>
                                {
                                    return new IPrimitive[]
                                    {
                                        new PrimitiveLine(initial.Value, p),
                                        new PrimitiveEllipse(initial.Value, (p - initial.Value).Length, drawingPlane.Normal)
                                    };
                                });
                                if (rad.Cancel) return false;
                                if (rad.HasValue)
                                {
                                    circle = new Circle(initial.Value, rad.Value, drawingPlane.Normal, lineTypeSpecification: lineTypeSpecification);
                                }
                                else // switch modes
                                {
                                    if (rad.Directive == null)
                                    {
                                        return false;
                                    }

                                    switch (rad.Directive)
                                    {
                                        case "d":
                                            mode = CircleMode.Diameter;
                                            break;
                                        case "i":
                                            mode = CircleMode.Isometric;
                                            break;
                                    }
                                }

                                break;
                            }
                        case CircleMode.Diameter:
                            {
                                var dia = await workspace.InputService.GetPoint(new UserDirective("Enter diameter or [r]adius/[i]sometric", "r", "i"), (p) =>
                                {
                                    var diameterLine = new PrimitiveLine(initial.Value, p);
                                    return new IPrimitive[]
                                    {
                                        diameterLine,
                                        new PrimitiveEllipse(diameterLine.MidPoint(), (p - initial.Value).Length * 0.5, drawingPlane.Normal)
                                    };
                                });
                                if (dia.Cancel) return false;
                                if (dia.HasValue)
                                {
                                    var diameterLine = new PrimitiveLine(initial.Value, dia.Value);
                                    circle = new Circle(diameterLine.MidPoint(), (dia.Value - initial.Value).Length * 0.5, drawingPlane.Normal, lineTypeSpecification: lineTypeSpecification);
                                }
                                else // switch modes
                                {
                                    if (dia.Directive == null)
                                    {
                                        return false;
                                    }

                                    switch (dia.Directive)
                                    {
                                        case "r":
                                            mode = CircleMode.Radius;
                                            break;
                                        case "i":
                                            mode = CircleMode.Isometric;
                                            break;
                                    }
                                }

                                break;
                            }
                        case CircleMode.Isometric:
                            {
                                var isoRad = await workspace.InputService.GetPoint(new UserDirective("Enter isometric-radius or [r]adius/[d]iameter", "r", "d"), (p) =>
                                {
                                    return new IPrimitive[]
                                    {
                                        new PrimitiveLine(initial.Value, p),
                                        new PrimitiveEllipse(initial.Value,
                                            Vector.SixtyDegrees * (p - initial.Value).Length * MathHelper.SqrtThreeHalves,
                                            drawingPlane.Normal,
                                            IsoMinorRatio,
                                            0.0,
                                            360.0)
                                    };
                                });
                                if (isoRad.Cancel) return false;
                                if (isoRad.HasValue)
                                {
                                    circle = new Ellipse(initial.Value,
                                        Vector.SixtyDegrees * (isoRad.Value - initial.Value).Length * MathHelper.SqrtThreeHalves,
                                        IsoMinorRatio,
                                        0.0,
                                        360.0,
                                        drawingPlane.Normal,
                                        lineTypeSpecification: lineTypeSpecification);
                                }
                                else // switch modes
                                {
                                    if (isoRad.Directive == null)
                                    {
                                        return false;
                                    }

                                    switch (isoRad.Directive)
                                    {
                                        case "r":
                                            mode = CircleMode.Radius;
                                            break;
                                        case "d":
                                            mode = CircleMode.Diameter;
                                            break;
                                    }
                                }

                                break;
                            }
                    }
                }
            }
            else
            {
                switch (initial.Directive)
                {
                    case "ttr":
                        var firstEntity = await workspace.InputService.GetEntity(new UserDirective("First entity"));
                        if (firstEntity.Cancel || !firstEntity.HasValue)
                            break;
                        var secondEntity = await workspace.InputService.GetEntity(new UserDirective("Second entity"));
                        if (secondEntity.Cancel || !secondEntity.HasValue)
                            break;
                        var radius = await workspace.InputService.GetDistance();
                        var ellipse = EditUtilities.Ttr(drawingPlane, firstEntity.Value, secondEntity.Value, radius.Value);
                        if (ellipse != null)
                        {
                            circle = ellipse.ToEntity(lineTypeSpecification);
                        }
                        break;
                    case "2":
                        break;
                    case "th":
                        var first = await workspace.InputService.GetPoint(new UserDirective("First point"));
                        if (first.Cancel || !first.HasValue)
                            break;
                        var second = await workspace.InputService.GetPoint(new UserDirective("Second point"), p =>
                            new[]
                            {
                                new PrimitiveLine(first.Value, p)
                            });
                        if (second.Cancel || !second.HasValue)
                            break;
                        var third = await workspace.InputService.GetPoint(new UserDirective("Third point"), p =>
                            {
                                var c = PrimitiveEllipse.ThreePointCircle(first.Value, second.Value, p);
                                if (c == null)
                                    return new IPrimitive[]
                                    {
                                        new PrimitiveLine(first.Value, second.Value),
                                        new PrimitiveLine(second.Value, p),
                                        new PrimitiveLine(p, first.Value)
                                    };
                                else
                                    return new IPrimitive[]
                                    {
                                        new PrimitiveLine(first.Value, second.Value),
                                        new PrimitiveLine(second.Value, p),
                                        new PrimitiveLine(p, first.Value),
                                        c
                                    };
                            });
                        if (third.Cancel || !third.HasValue)
                            break;
                        var circ = PrimitiveEllipse.ThreePointCircle(first.Value, second.Value, third.Value);
                        if (circ != null)
                        {
                            circle = new Circle(circ.Center, circ.MajorAxis.Length, circ.Normal, lineTypeSpecification: lineTypeSpecification);
                        }
                        break;
                }
            }

            if (circle != null)
            {
                workspace.AddToCurrentLayer(circle);
            }

            return true;
        }
    }
}
