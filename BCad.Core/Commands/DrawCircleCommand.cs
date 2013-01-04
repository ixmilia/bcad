using System.ComponentModel.Composition;
using BCad.Entities;
using BCad.Extensions;
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
        private IEditService EditService = null;

        [Import]
        private IWorkspace Workspace = null;

        public bool Execute(object arg)
        {
            Circle circle = null;
            var drawingPlane = Workspace.DrawingPlane;

            var cen = InputService.GetPoint(new UserDirective("Select center, [ttr], or [th]ree-point", "ttr", "th"));
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
                                new PrimitiveEllipse(cen.Value, (p - cen.Value).Length, drawingPlane.Normal, Color.Default)
                            };
                        });
                        if (rad.Cancel) return false;
                        if (rad.HasValue)
                        {
                            circle = new Circle(cen.Value, (rad.Value - cen.Value).Length, drawingPlane.Normal, Color.Default);
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
                                new PrimitiveEllipse(cen.Value, (p - cen.Value).Length / 2.0, drawingPlane.Normal, Color.Default)
                            };
                        });
                        if (diameter.Cancel) return false;
                        if (diameter.HasValue)
                        {
                            circle = new Circle(cen.Value, (diameter.Value - cen.Value).Length / 2.0, drawingPlane.Normal, Color.Default);
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
                        var firstEntity = InputService.GetEntity(new UserDirective("First entity"));
                        if (firstEntity.Cancel || !firstEntity.HasValue)
                            break;
                        var secondEntity = InputService.GetEntity(new UserDirective("Second entity"));
                        if (secondEntity.Cancel || !secondEntity.HasValue)
                            break;
                        var radius = InputService.GetDistance();
                        var ellipse = EditService.Ttr(drawingPlane, firstEntity.Value, secondEntity.Value, radius.Value);
                        if (ellipse != null)
                        {
                            circle = (Circle)ellipse.ToEntity();
                        }
                        break;
                    case "2":
                        break;
                    case "th":
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
