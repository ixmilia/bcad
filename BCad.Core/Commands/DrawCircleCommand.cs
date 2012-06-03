using System.ComponentModel.Composition;
using BCad.Entities;
using BCad.Primitives;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("Draw.Circle", "circle", "c", "cir")]
    public class DrawCircleCommand : ICommand
    {
        [Import]
        private IInputService InputService = null;

        [Import]
        private IWorkspace Workspace = null;

        public bool Execute(object arg)
        {
            Circle circle = null;

            var cen = InputService.GetPoint(new UserDirective("Select center, [ttr], or [3]-point", "ttr", "3"));
            if (cen.Cancel) return false;
            if (cen.HasValue)
            {
                bool getRadius = true;
                while (circle == null)
                {
                    if (getRadius)
                    {
                        var rad = InputService.GetPoint(new UserDirective("Enter radius or [d]iameter", "d"), (p) =>
                        {
                            return new IPrimitive[]
                            {
                                new PrimitiveLine(cen.Value, p, Color.Default),
                                new PrimitiveEllipse(cen.Value, (p - cen.Value).Length, Workspace.DrawingPlaneNormal(), Color.Default)
                            };
                        });
                        if (rad.Cancel) return false;
                        if (rad.HasValue)
                        {
                            circle = new Circle(cen.Value, (rad.Value - cen.Value).Length, Workspace.DrawingPlaneNormal(), Color.Default);
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
                        var diameter = InputService.GetPoint(new UserDirective("Enter diameter or [r]adius", "r"), (p) =>
                        {
                            return new IPrimitive[]
                            {
                                new PrimitiveLine(cen.Value, p, Color.Default),
                                new PrimitiveEllipse(cen.Value, (p - cen.Value).Length / 2.0, Workspace.DrawingPlaneNormal(), Color.Default)
                            };
                        });
                        if (diameter.Cancel) return false;
                        if (diameter.HasValue)
                        {
                            circle = new Circle(cen.Value, (diameter.Value - cen.Value).Length / 2.0, Workspace.DrawingPlaneNormal(), Color.Default);
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
                        break;
                    case "2":
                        break;
                    case "3":
                        var first = InputService.GetPoint(new UserDirective("First point"));
                        if (first.Cancel || !first.HasValue)
                            break;
                        var second = InputService.GetPoint(new UserDirective("Second point"), p =>
                            new[]
                            {
                                new PrimitiveLine(first.Value, p)
                            });
                        if (second.Cancel || !second.HasValue)
                            break;
                        var third = InputService.GetPoint(new UserDirective("Third point"), p =>
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
                            circle = new Circle(circ.Center, circ.MajorAxis.Length, circ.Normal, Color.Auto);
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

        public string DisplayName
        {
            get { return "CIRCLE"; }
        }
    }
}
