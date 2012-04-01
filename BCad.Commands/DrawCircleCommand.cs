using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using BCad;
using BCad.Objects;

namespace BCad.Commands
{
    [ExportCommand("Draw.Circle", "circle", "c", "cir")]
    internal class DrawCircleCommand : ICommand
    {
        [Import]
        public IUserConsole UserConsole { get; set; }

        [Import]
        public IWorkspace Workspace { get; set; }

        [Import]
        public IUndoRedoService UndoRedoService { get; set; }

        public bool Execute(params object[] parameters)
        {
            Point center = Point.Origin;
            double radius = 0.0;

            var cen = UserConsole.GetPoint(new UserDirective("Select center, [ttr], or [3]-point", "ttr", "3"));
            if (cen.Cancel) return false;
            if (cen.HasValue)
            {
                center = cen.Value;
                bool getRadius = true;
                while (radius == 0.0)
                {
                    if (getRadius)
                    {
                        var rad = UserConsole.GetPoint(new UserDirective("Enter radius or [d]iameter", "d"), (p) =>
                        {
                            return new IPrimitive[]
                            {
                                new Line(center, p, Color.Default),
                                new Circle(center, (p - center).Length, -Workspace.View.Sight, Color.Default)
                            };
                        });
                        if (rad.Cancel) return false;
                        if (rad.HasValue)
                        {
                            radius = (rad.Value - center).Length;
                        }
                        else // switch modes
                        {
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
                        var diameter = UserConsole.GetPoint(new UserDirective("Enter diameter or [r]adius", "r"), (p) =>
                        {
                            return new IPrimitive[]
                            {
                                new Line(center, p, Color.Default),
                                new Circle(center, (p - center).Length / 2.0, -Workspace.View.Sight, Color.Default)
                            };
                        });
                        if (diameter.Cancel) return false;
                        if (diameter.HasValue)
                        {
                            radius = (diameter.Value - center).Length / 2.0f;
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
                        break;
                }
            }

            UndoRedoService.SetSnapshot();
            Workspace.AddToCurrentLayer(new Circle(center, radius, -Workspace.View.Sight, Color.Default));
            return true;
        }

        public string DisplayName
        {
            get { return "CIRCLE"; }
        }
    }
}
