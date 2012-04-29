using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using Shapes = System.Windows.Shapes;
using BCad.EventArguments;
using BCad.Extensions;
using BCad.Helpers;
using BCad.Objects;
using BCad.SnapPoints;
using System.Threading.Tasks;
using System.ComponentModel;

namespace BCad.UI.Views
{
    /// <summary>
    /// Interaction logic for WpfView.xaml
    /// </summary>
    [ExportViewControl("Default")]
    public partial class WpfViewControl : ViewControl, IPartImportsSatisfiedNotification
    {
        private class TransformedSnapPoint
        {
            public System.Windows.Point ControlPoint { get; set; }

            public Point WorldPoint { get; set; }

            public Image Icon { get; set; }

            public TransformedSnapPoint(System.Windows.Point controlPoint, Point worldPoint, Image icon = null)
            {
                ControlPoint = controlPoint;
                WorldPoint = worldPoint;
                Icon = icon;
            }

            public static TransformedSnapPoint FromView(IView view, double scale, SnapPoint snapPoint, Entity source)
            {
                var controlPoint = view.WorldToControl(snapPoint.Point).ToWindowsPoint();
                var geom = GetIcon(snapPoint.Kind);
                geom.Pen = new Pen(Brushes.Yellow, 0.2);
                var di = new DrawingImage(geom);
                var i = new Image();
                i.Source = di;
                i.Stretch = Stretch.None;
                i.LayoutTransform = new ScaleTransform(scale, scale);
                i.Tag = source;
                Canvas.SetLeft(i, controlPoint.X - geom.Bounds.Width * scale / 2.0);
                Canvas.SetTop(i, controlPoint.Y - geom.Bounds.Height * scale / 2.0);
                return new TransformedSnapPoint(controlPoint, snapPoint.Point, i);
            }

            private static GeometryDrawing GetIcon(SnapPointKind kind)
            {
                string name;
                switch (kind)
                {
                    case SnapPointKind.Center:
                        name = "CenterPointIcon";
                        break;
                    case SnapPointKind.EndPoint:
                        name = "EndPointIcon";
                        break;
                    case SnapPointKind.MidPoint:
                        name = "MidPointIcon";
                        break;
                    case SnapPointKind.Quadrant:
                        name = "QuadrantPointIcon";
                        break;
                    default:
                        throw new ArgumentException("kind");
                }

                return (GeometryDrawing)Resources[name];
            }

            private static ResourceDictionary _resources = null;
            private static ResourceDictionary Resources
            {
                get
                {
                    if (_resources == null)
                    {
                        _resources = new ResourceDictionary();
                        _resources.Source = new Uri("/BCad.Core;component/SnapPoints/SnapPointIcons.xaml", UriKind.Relative);
                    }
                    return _resources;
                }
            }
        }

        public WpfViewControl()
        {
            InitializeComponent();
        }

        public override Point GetCursorPoint()
        {
            var cursor = Mouse.GetPosition(this);
            var sp = GetActiveModelPoint(cursor);
            return sp.WorldPoint;
        }

        [Import]
        private IView View = null;

        [Import]
        private IInputService InputService = null;

        [Import]
        private IWorkspace Workspace = null;

        private bool panning = false;

        private List<TransformedSnapPoint> snapPoints = new List<TransformedSnapPoint>();

        private Dictionary<Entity, IEnumerable<UIElement>> objectsToPrimitivesMap = new Dictionary<Entity, IEnumerable<UIElement>>();

        private double Scale
        {
            get
            {
                return View.ViewWidth / this.ActualWidth;
            }
        }

        public void OnImportsSatisfied()
        {
            InputService.RubberBandGeneratorChanged += UserConsole_RubberBandGeneratorChanged;
            View.ViewPortChanged += TransformationMatrixChanged;
            Workspace.CommandExecuted += Workspace_CommandExecuted;
            Workspace.DocumentChanging += DocumentChanging;
            Workspace.DocumentChanged += DocumentChanged;
            Workspace.SettingsManager.PropertyChanged += SettingsManager_PropertyChanged;

            foreach (var setting in new[] { "BackgroundColor" })
                SettingsManager_PropertyChanged(null, new PropertyChangedEventArgs(setting));
        }

        void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "BackgroundColor":
                    this.objects.Background = new SolidColorBrush(Workspace.SettingsManager.BackgroundColor);
                    break;
                default:
                    break;
            }
        }

        void Workspace_CommandExecuted(object sender, CommandExecutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke((Action)(() => this.snap.Children.Clear()));
        }

        void UserConsole_RubberBandGeneratorChanged(object sender, RubberBandGeneratorChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => DrawRubberBandObjects(View.RegisteredControl.GetCursorPoint())));
        }

        private void TransformationMatrixChanged(object sender, ViewPortChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => DrawObjects()));
        }

        private void DocumentChanging(object sender, DocumentChangingEventArgs e)
        {
        }

        private void DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => DrawObjects()));
        }

        private void AddObjectSnapPoints(Entity obj)
        {
            foreach (var sp in obj.GetSnapPoints())
            {
                snapPoints.Add(TransformedSnapPoint.FromView(View, Workspace.SettingsManager.SnapPointSize, sp, obj));
            }
        }

        private void DrawObjects()
        {
            objects.Children.Clear();
            snapPoints.Clear();

            foreach (var layer in Workspace.Document.Layers.Values.Where(l => l.IsVisible))
            {
                foreach (var obj in layer.Objects)
                {
                    DrawObject(obj, layer.Color);
                    AddObjectSnapPoints(obj);
                }
            }
        }

        private void DrawObject(Entity obj, Color layerColor)
        {
            foreach (var p in obj.GetPrimitives())
            {
                DrawPrimitive(objects, p, layerColor, true);
            }
        }

        private void DrawPrimitive(Canvas canvas, IPrimitive primitive, Color layerColor, bool saveObject = false)
        {
            if (primitive is Line)
                Draw(canvas, (Line)primitive, layerColor, saveObject);
            else if (primitive is Circle)
                Draw(canvas, (Circle)primitive, layerColor, saveObject);
            else if (primitive is Arc)
                Draw(canvas, (Arc)primitive, layerColor, saveObject);
            else
                Debug.Fail("Unsupported primitive " + primitive.GetType().FullName);
        }

        private System.Windows.Media.Color GetAutoColor(Color itemColor, Color layerColor)
        {
            if (!itemColor.IsAuto)
                return itemColor.MediaColor;
            if (!layerColor.IsAuto)
                return layerColor.MediaColor;

            var bg = Workspace.SettingsManager.BackgroundColor;
            var color = System.Drawing.Color.FromArgb(bg.R, bg.G, bg.B);
            return color.GetBrightness() < 0.67
                ? Colors.White
                : Colors.Black;
        }

        private void Draw(Canvas canvas, Line l, Color layerColor, bool saveObject)
        {
            var p1 = View.WorldToControl(l.P1).ToPoint3D();
            var p2 = View.WorldToControl(l.P2).ToPoint3D();
            var line = new Shapes.Line()
            {
                X1 = p1.X,
                Y1 = p1.Y,
                X2 = p2.X,
                Y2 = p2.Y,
                Stroke = new SolidColorBrush(GetAutoColor(l.Color, layerColor)),
                StrokeThickness = 1.0
            };
            if (saveObject)
                line.Tag = l;
            canvas.Children.Add(line);
        }

        private void Draw(Canvas canvas, Circle c, Color layerColor, bool saveObject)
        {
            // TODO: proper rendering
            var topLeft = (c.Center + new Vector(-c.Radius, c.Radius, 0)).ToPoint();
            var botRight = (topLeft + new Vector(c.Radius, -c.Radius, 0) * 2.0).ToPoint();

            var tlScreen = View.WorldToControl(topLeft).ToPoint3D();
            var brScreen = View.WorldToControl(botRight).ToPoint3D();
            var size = Math.Abs(tlScreen.X - brScreen.X);

            var e = new Shapes.Ellipse()
            {
                Width = size,
                Height = size,
                Stroke = new SolidColorBrush(GetAutoColor(c.Color, layerColor)),
                StrokeThickness = 1.0
            };
            if (saveObject)
                e.Tag = c;
            Canvas.SetLeft(e, tlScreen.X);
            Canvas.SetTop(e, tlScreen.Y);
            canvas.Children.Add(e);
        }

        private void Draw(Canvas canvas, Arc a, Color layerColor, bool saveObject)
        {
            var angleBetween = a.EndAngle - a.StartAngle;
            if (angleBetween < 0.0) angleBetween += 360.0;
            var largeArc = angleBetween > 180.0;
            var radius = a.Radius / Scale;
            var arc = new Shapes.Path()
            {
                Data = new PathGeometry(new PathFigure[]
                {
                    new PathFigure(View.WorldToControl(a.EndPoint1).ToWindowsPoint(), new PathSegment[]
                    {
                        new ArcSegment(View.WorldToControl(a.EndPoint2).ToWindowsPoint(), new Size(radius, radius), 0.0, largeArc, SweepDirection.Counterclockwise, true)
                    }, false)
                }),
                Stroke = new SolidColorBrush(GetAutoColor(a.Color, layerColor)),
                StrokeThickness = 1.0
            };
            if (saveObject)
                arc.Tag = a;
            canvas.Children.Add(arc);
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var cursor = e.GetPosition(this);
            var p = new Point(cursor);
            var sp = GetActiveModelPoint(cursor);
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    switch (InputService.DesiredInputType)
                    {
                        case InputType.Point:
                            InputService.PushValue(sp.WorldPoint);
                            break;
                        case InputType.Object:
                            Entity foundObject = null;
                            var selectionRadius = Workspace.SettingsManager.ObjectSelectionRadius;
                            foreach (var shape in objects.Children.OfType<Shapes.Shape>())
                            {
                                if (shape is Shapes.Ellipse) // circle
                                {
                                    // if dist between centers <= sum of radii
                                    var circle = (Shapes.Ellipse)shape;
                                    var separation = (circle.Center() - p).LengthSquared;
                                    var maxAllowed =
                                        (selectionRadius + circle.Radius()) *
                                        (selectionRadius + circle.Radius());
                                    if (separation <= maxAllowed)
                                    {
                                        if (shape.Tag as Entity != null)
                                        {
                                            foundObject = (Entity)shape.Tag;
                                            break;
                                        }
                                        else
                                        {
                                            Debug.Fail("Shape's tag wasn't valid IObject");
                                        }
                                    }
                                }
                                else if (shape is Shapes.Line)
                                {
                                    // http://mathworld.wolfram.com/Circle-LineIntersection.html
                                    var line = (Shapes.Line)shape;
                                    var x1 = line.X1 - p.X;
                                    var x2 = line.X2 - p.X;
                                    var y1 = line.Y1 - p.Y;
                                    var y2 = line.Y2 - p.Y;
                                    var dx = x2 - x1;
                                    var dy = y2 - y1;
                                    var dr2 = dx * dx + dy * dy;
                                    var D = x1 * y2 - x2 * y1;
                                    var det = (selectionRadius * selectionRadius * dr2) - (D * D);
                                    if (det >= 0)// &&
                                        //NumberHelper.Between(p.X, line.X1, line.X2) &&
                                        //NumberHelper.Between(p.Y, line.Y1, line.Y2))
                                    {
                                        if (shape.Tag as Entity != null)
                                        {
                                            foundObject = (Entity)shape.Tag;
                                            break;
                                        }
                                        else
                                        {
                                            Debug.Fail("Shape's tag wasn't valid IObject");
                                        }
                                    }
                                }
                                else
                                {
                                    Debug.Fail("Unknown object: " + shape.GetType().Name);
                                }
                            }

                            if (foundObject != null)
                            {
                                InputService.PushValue(foundObject);
                            }

                            break;
                    }
                    break;
                case MouseButton.Middle:
                    panning = true;
                    lastPoint = p;
                    break;
                case MouseButton.Right:
                    InputService.PushValue(null);
                    break;
            }
            DrawSnapPoints(sp);
            DrawRubberBandObjects(sp.WorldPoint);
        }

        private void DrawRubberBandObjects(Point modelPoint)
        {
            rubber.Children.Clear();
            var gen = InputService.PrimitiveGenerator;
            if (gen != null)
            {
                foreach (var prim in gen(modelPoint))
                    DrawPrimitive(rubber, prim, Color.Auto);
            }
        }

        private void DrawSnapPoints(TransformedSnapPoint snapPoint)
        {
            snap.Children.Clear();
            if (snapPoint == null || snapPoint.Icon == null || InputService.DesiredInputType != InputType.Point)
                return;
            snap.Children.Add(snapPoint.Icon);
        }

        private TransformedSnapPoint GetActiveModelPoint(System.Windows.Point cursor)
        {
            return ActiveObjectSnapPoint(cursor)
                ?? ActiveOrthoPoint(cursor)
                ?? ActiveAngleSnapPoint(cursor)
                ?? new TransformedSnapPoint(cursor, View.ControlToWorld(new Point(cursor)));
        }

        private TransformedSnapPoint ActiveObjectSnapPoint(System.Windows.Point cursor)
        {
            if (Workspace.SettingsManager.PointSnap)
            {
                var points = from sp in snapPoints
                             let dist = (sp.ControlPoint - cursor).Length
                             where dist <= Workspace.SettingsManager.SnapPointDistance
                             orderby dist
                             select sp;
                return points.FirstOrDefault();
            }

            return null;
        }

        private TransformedSnapPoint ActiveOrthoPoint(System.Windows.Point cursor)
        {
            if (InputService.IsDrawing && Workspace.SettingsManager.Ortho)
            {
                var last = View.WorldToControl(InputService.LastPoint);
                var delta = new Point(cursor) - last;
                Point control;
                if (Math.Abs(delta.X) > Math.Abs(delta.Y))
                {
                    // horizontal ortho
                    control = (last + new Vector(delta.X, 0, 0)).ToPoint();
                }
                else
                {
                    // vertical ortho
                    control = (last + new Vector(0, delta.Y, 0)).ToPoint();
                }

                return new TransformedSnapPoint(control.ToWindowsPoint(), View.ControlToWorld(control));
            }

            return null;
        }

        private TransformedSnapPoint ActiveAngleSnapPoint(System.Windows.Point cursor)
        {
            if (InputService.IsDrawing && Workspace.SettingsManager.AngleSnap)
            {
                // get control distance to last point
                var last = View.WorldToControl(InputService.LastPoint);
                var current = new Point(cursor);
                var vector = current - last;
                var dist = vector.Length;

                // for each snap angle, find the point `dist` out on the angle vector
                var points = from sa in Workspace.SettingsManager.SnapAngles
                             let rad = -sa * Math.PI / 180.0
                             let radVector = new Vector(Math.Cos(rad), Math.Sin(rad), 0.0).Normalize() * dist
                             let controlPoint = (last + radVector).ToPoint()
                             let di = (current - controlPoint).Length
                             where di <= Workspace.SettingsManager.SnapAngleDistance
                             orderby di
                             select new TransformedSnapPoint(controlPoint.ToWindowsPoint(), View.ControlToWorld(controlPoint));

                // return the closest one
                return points.FirstOrDefault();
            }

            return null;
        }

        private Point lastPoint;

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            //var controlPoint = e.GetPosition(this);

            CursorMove(e.GetPosition(this));
        }

        private void CursorMove(System.Windows.Point cursor)
        {
            var point = new Point(cursor);
            if (panning)
            {
                var delta = lastPoint - point;
                var scale = Scale;
                var dx = View.BottomLeft.X + delta.X * scale;
                var dy = View.BottomLeft.Y - delta.Y * scale;
                View.UpdateView(bottomLeft: new Point(dx, dy, View.BottomLeft.Z));
                lastPoint = point;
            }

            if (InputService.DesiredInputType == InputType.Point)
            {
                var sp = GetActiveModelPoint(cursor);
                DrawSnapPoints(sp);
                DrawRubberBandObjects(sp.WorldPoint);
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Middle:
                    panning = false;
                    break;
                default:
                    break;
            }

            DrawRubberBandObjects(GetActiveModelPoint(e.GetPosition(this)).WorldPoint);
        }

        private void canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // scale everything
            var scale = 1.25;
            if (e.Delta > 0) scale = 1.0 / scale;

            // center zoom operation on mouse
            var cursorPoint = e.GetPosition(this);
            var controlPoint = new Point(cursorPoint);
            var cursorPos = View.ControlToWorld(controlPoint);
            var botLeft = View.BottomLeft;

            // find relative scales
            var viewHeight = this.ActualHeight / this.ActualWidth * View.ViewWidth;
            var relHoriz = controlPoint.X / this.ActualWidth;
            var relVert = controlPoint.Y / this.ActualHeight;
            var viewWidthDelta = View.ViewWidth * (scale - 1.0);
            var viewHeightDelta = viewHeight * (scale - 1.0);

            // set values
            View.UpdateView(viewWidth: View.ViewWidth * scale, bottomLeft: (botLeft - new Vector(viewWidthDelta * relHoriz, viewHeightDelta * relVert, 0.0)).ToPoint());

            var sp = GetActiveModelPoint(cursorPoint);
            DrawRubberBandObjects(sp.WorldPoint);
            DrawSnapPoints(sp);
        }
    }
}
