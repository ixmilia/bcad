using System;
using System.Composition;
using System.Threading.Tasks;
using BCad.Entities;
using BCad.Extensions;
using BCad.Primitives;
using BCad.Services;
using BCad.Utilities;
using BCad.Helpers;

namespace BCad.Commands
{
    [ExportCommand("Draw.Circle", "CIRCLE", "circle", "c", "cir")]
    public class DrawCircleCommand : ICommand
    {
        [Import]
        public IInputService InputService { get; set; }

        [Import]
        public IWorkspace Workspace { get; set; }

        private static readonly double IsoMinorRatio = Math.Sqrt(1.5) / Math.Sqrt(2.0) * 2.0 / 3.0;

        private enum CircleMode
        {
            Radius,
            Diameter,
            Isometric
        }

        public async Task<bool> Execute(object arg)
        {
            Entity circle = null;
            var drawingPlane = Workspace.DrawingPlane;

            var cen = await InputService.GetPoint(new UserDirective("Select center, [ttr], or [th]ree-point", "ttr", "th"));
            if (cen.Cancel) return false;
            if (cen.HasValue)
            {
                var mode = CircleMode.Radius;
                while (circle == null)
                {
                    switch (mode)
                    {
                        case CircleMode.Radius:
                            {
                                var rad = await InputService.GetPoint(new UserDirective("Enter radius or [d]iameter/[i]sometric", "d", "i"), (p) =>
                                {
                                    return new IPrimitive[]
                                    {
                                        new PrimitiveLine(cen.Value, p, IndexedColor.Default),
                                        new PrimitiveEllipse(cen.Value, (p - cen.Value).Length, drawingPlane.Normal, IndexedColor.Default)
                                    };
                                });
                                if (rad.Cancel) return false;
                                if (rad.HasValue)
                                {
                                    circle = new Circle(cen.Value, (rad.Value - cen.Value).Length, drawingPlane.Normal, IndexedColor.Default);
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
                                var dia = await InputService.GetPoint(new UserDirective("Enter diameter or [r]adius/[i]sometric", "r", "i"), (p) =>
                                {
                                    return new IPrimitive[]
                                    {
                                        new PrimitiveLine(cen.Value, p, IndexedColor.Default),
                                        new PrimitiveEllipse(cen.Value, (p - cen.Value).Length, drawingPlane.Normal, IndexedColor.Default)
                                    };
                                });
                                if (dia.Cancel) return false;
                                if (dia.HasValue)
                                {
                                    circle = new Circle(cen.Value, (dia.Value - cen.Value).Length * 0.5, drawingPlane.Normal, IndexedColor.Default);
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
                                var isoRad = await InputService.GetPoint(new UserDirective("Enter isometric-radius or [r]adius/[d]iameter", "r", "d"), (p) =>
                                {
                                    return new IPrimitive[]
                                    {
                                        new PrimitiveLine(cen.Value, p, IndexedColor.Default),
                                        new PrimitiveEllipse(cen.Value,
                                            Vector.SixtyDegrees * (p - cen.Value).Length * MathHelper.SqrtThreeHalves,
                                            drawingPlane.Normal,
                                            IsoMinorRatio,
                                            0.0,
                                            360.0,
                                            IndexedColor.Default)
                                    };
                                });
                                if (isoRad.Cancel) return false;
                                if (isoRad.HasValue)
                                {
                                    circle = new Ellipse(cen.Value,
                                        Vector.SixtyDegrees * (isoRad.Value - cen.Value).Length * MathHelper.SqrtThreeHalves,
                                        IsoMinorRatio,
                                        0.0,
                                        360.0,
                                        drawingPlane.Normal,
                                        IndexedColor.Default);
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
                switch (cen.Directive)
                {
                    case "ttr":
                        var firstEntity = await InputService.GetEntity(new UserDirective("First entity"));
                        if (firstEntity.Cancel || !firstEntity.HasValue)
                            break;
                        var secondEntity = await InputService.GetEntity(new UserDirective("Second entity"));
                        if (secondEntity.Cancel || !secondEntity.HasValue)
                            break;
                        var radius = await InputService.GetDistance();
                        var ellipse = EditUtilities.Ttr(drawingPlane, firstEntity.Value, secondEntity.Value, radius.Value);
                        if (ellipse != null)
                        {
                            circle = ellipse.ToEntity();
                        }
                        break;
                    case "2":
                        break;
                    case "th":
                        var first = await InputService.GetPoint(new UserDirective("First point"));
                        if (first.Cancel || !first.HasValue)
                            break;
                        var second = await InputService.GetPoint(new UserDirective("Second point"), p =>
                            new[]
                            {
                                new PrimitiveLine(first.Value, p)
                            });
                        if (second.Cancel || !second.HasValue)
                            break;
                        var third = await InputService.GetPoint(new UserDirective("Third point"), p =>
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
                            circle = new Circle(circ.Center, circ.MajorAxis.Length, circ.Normal, IndexedColor.Auto);
                        }
                        break;
                }
            }

            if (circle != null)
            {
                Workspace.AddToCurrentLayer(circle);
            }

            return true;
        }
    }
}
