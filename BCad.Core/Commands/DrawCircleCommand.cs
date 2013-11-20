using System.Composition;
using System.Threading.Tasks;
using BCad.Entities;
using BCad.Extensions;
using BCad.Primitives;
using BCad.Services;
using BCad.Utilities;

namespace BCad.Commands
{
    [ExportCommand("Draw.Circle", "CIRCLE", "circle", "c", "cir")]
    public class DrawCircleCommand : ICommand
    {
        [Import]
        public IInputService InputService { get; set; }

        [Import]
        public IWorkspace Workspace { get; set; }

        public async Task<bool> Execute(object arg)
        {
            Circle circle = null;
            var drawingPlane = Workspace.DrawingPlane;

            var cen = await InputService.GetPoint(new UserDirective("Select center, [ttr], or [th]ree-point", "ttr", "th"));
            if (cen.Cancel) return false;
            if (cen.HasValue)
            {
                bool getRadius = true;
                while (circle == null)
                {
                    if (getRadius)
                    {
                        var rad = await InputService.GetPoint(new UserDirective("Enter radius or [d]iameter", "d"), (p) =>
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
                                    getRadius = false;
                                    break;
                            }
                        }
                    }
                    else // get diameter
                    {
                        var diameter = await InputService.GetPoint(new UserDirective("Enter diameter or [r]adius", "r"), (p) =>
                        {
                            return new IPrimitive[]
                            {
                                new PrimitiveLine(cen.Value, p, IndexedColor.Default),
                                new PrimitiveEllipse(cen.Value, (p - cen.Value).Length / 2.0, drawingPlane.Normal, IndexedColor.Default)
                            };
                        });
                        if (diameter.Cancel) return false;
                        if (diameter.HasValue)
                        {
                            circle = new Circle(cen.Value, (diameter.Value - cen.Value).Length / 2.0, drawingPlane.Normal, IndexedColor.Default);
                        }
                        else // switch modes
                        {
                            switch (diameter.Directive)
                            {
                                case "r":
                                    getRadius = true;
                                    break;
                            }
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
                            circle = (Circle)ellipse.ToEntity();
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
