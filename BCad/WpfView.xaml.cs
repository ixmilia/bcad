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
using BCad.Objects;
using BCad.SnapPoints;

namespace BCad
{
    /// <summary>
    /// Interaction logic for WpfView.xaml
    /// </summary>
    public partial class WpfView : UserControl, IPartImportsSatisfiedNotification
    {
        private class TransformedSnapPoint
        {
            public System.Windows.Point ControlPoint { get; set; }

            public Point WorldPoint { get; set; }

            public Image Icon { get; set; }

            public TransformedSnapPoint(System.Windows.Point controlPoint, Point worldPoint, Image icon)
            {
                ControlPoint = controlPoint;
                WorldPoint = worldPoint;
                Icon = icon;
            }

            public static TransformedSnapPoint FromView(IView view, double scale, SnapPoint snapPoint)
            {
                var controlPoint = view.WorldToControl(snapPoint.Point).ToWindowsPoint();
                var geom = snapPoint.Icon;
                geom.Pen = new Pen(Brushes.Yellow, 0.2);
                var di = new DrawingImage(geom);
                var i = new Image();
                i.Source = di;
                i.Stretch = Stretch.None;
                i.LayoutTransform = new ScaleTransform(scale, scale);
                Canvas.SetLeft(i, controlPoint.X - geom.Bounds.Width * scale / 2.0);
                Canvas.SetTop(i, controlPoint.Y - geom.Bounds.Height * scale / 2.0);
                return new TransformedSnapPoint(controlPoint, snapPoint.Point, i);
            }
        }

        public WpfView()
        {
            InitializeComponent();
        }

        [Import]
        public IView View { get; set; }

        [Import]
        public IUserConsole UserConsole { get; set; }

        [Import]
        public IWorkspace Workspace { get; set; }

        private bool panning = false;

        private List<TransformedSnapPoint> snapPoints = new List<TransformedSnapPoint>();

        private double snapPointSize = 15.0;

        private double snapPointDistSquared = 15.0 * 15.0;

        private Dictionary<IObject, IEnumerable<UIElement>> objectsToPrimitivesMap = new Dictionary<IObject, IEnumerable<UIElement>>();

        private double Scale
        {
            get
            {
                return View.ViewWidth / this.ActualWidth;
            }
        }

        public void OnImportsSatisfied()
        {
            UserConsole.RubberBandGeneratorChanged += UserConsole_RubberBandGeneratorChanged;
            View.ViewPortChanged += TransformationMatrixChanged;
            PopulateDocumentEvents(Workspace.Document);
            Workspace.DocumentChanging += DocumentChanging;
            Workspace.DocumentChanged += DocumentChanged;
        }

        void UserConsole_RubberBandGeneratorChanged(object sender, RubberBandGeneratorChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => DrawRubberBandObjects(View.GetCursorPoint())));
        }

        private void TransformationMatrixChanged(object sender, ViewPortChangedEventArgs e)
        {
            DrawObjects();
        }

        private void ObjectAdded(object sender, ObjectAddedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
                {
                    DrawObject(e.Object);
                    AddObjectSnapPoints(e.Object);
                }));
        }

        private void ObjectRemoved(object sender, ObjectRemovedEventArgs e)
        {
            // TODO: redraw world
        }

        private void LayerUpdated(object sender, LayerUpdatedEventArgs e)
        {
            // TODO: redraw objects if layer changed
        }

        private void LayerRemoved(object sender, LayerRemovedEventArgs e)
        {
            // TODO: remove layer objects
        }

        private void DocumentChanging(object sender, DocumentChangingEventArgs e)
        {
            ClearDocumentEvents(e.OldDocument);
        }

        private void DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
                {
                    PopulateDocumentEvents(e.Document);
                    DrawObjects();
                }));
        }

        private void ClearDocumentEvents(Document document)
        {
            document.ObjectAdded -= ObjectAdded;
            document.ObjectRemoved -= ObjectRemoved;
            document.LayerUpdated -= LayerUpdated;
            document.LayerRemoved -= LayerRemoved;
            // don't care about layer added
        }

        public void PopulateDocumentEvents(Document document)
        {
            document.ObjectAdded += ObjectAdded;
            document.ObjectRemoved += ObjectRemoved;
            document.LayerUpdated += LayerUpdated;
            document.LayerRemoved += LayerRemoved;
            // don't care about layer added
        }

        private void AddObjectSnapPoints(IObject obj)
        {
            foreach (var sp in obj.GetSnapPoints())
            {
                snapPoints.Add(TransformedSnapPoint.FromView(View, snapPointSize, sp));
            }
        }

        private void DrawObjects()
        {
            objects.Children.Clear();
            snapPoints.Clear();
            foreach (var layer in Workspace.Document.Layers)
            {
                foreach (var obj in layer.Objects)
                {
                    DrawObject(obj);
                    AddObjectSnapPoints(obj);
                }
            }
        }

        private void DrawObject(IObject obj)
        {
            foreach (var p in obj.GetPrimitives())
            {
                DrawPrimitive(objects, p, true);
            }
        }

        private void DrawPrimitive(Canvas canvas, IPrimitive primitive, bool saveObject = false)
        {
            if (primitive is Line)
                Draw(canvas, (Line)primitive, saveObject);
            else if (primitive is Circle)
                Draw(canvas, (Circle)primitive, saveObject);
            else if (primitive is Arc)
                Draw(canvas, (Arc)primitive, saveObject);
            else
                Debug.Fail("Unsupported primitive " + primitive.GetType().FullName);
        }

        private System.Windows.Media.Color GetAutoColor(Color color)
        {
            // TODO: find better method than inverting color; grays will look the same
            if (!color.IsAuto)
                return color.ToMediaColor();
            var mc = color.ToMediaColor();
            var r = mc.R;
            var g = mc.G;
            var b = mc.B;
            return System.Windows.Media.Color.FromRgb((byte)(255 - r), (byte)(255 - g), (byte)(255 - b));
        }

        private void Draw(Canvas canvas, Line l, bool saveObject)
        {
            var p1 = View.WorldToControl(l.P1).ToPoint3D();
            var p2 = View.WorldToControl(l.P2).ToPoint3D();
            var line = new Shapes.Line()
            {
                X1 = p1.X,
                Y1 = p1.Y,
                X2 = p2.X,
                Y2 = p2.Y,
                Stroke = new SolidColorBrush(GetAutoColor(l.Color)),
                StrokeThickness = 1.0
            };
            if (saveObject)
                line.Tag = l;
            canvas.Children.Add(line);
        }

        private void Draw(Canvas canvas, Circle c, bool saveObject)
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
                Stroke = new SolidColorBrush(GetAutoColor(c.Color)),
                StrokeThickness = 1.0
            };
            if (saveObject)
                e.Tag = c;
            Canvas.SetLeft(e, tlScreen.X);
            Canvas.SetTop(e, tlScreen.Y);
            canvas.Children.Add(e);
        }

        private void Draw(Canvas canvas, Arc a, bool saveObject)
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
                Stroke = new SolidColorBrush(GetAutoColor(a.Color)),
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
            var sp = ActiveSnapPoint(cursor);
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    switch (UserConsole.DesiredInputType)
                    {
                        case InputType.Point:
                            if (sp != null)
                            {
                                UserConsole.PushValue(sp.WorldPoint);
                            }
                            else
                            {
                                var worldPoint = View.ControlToWorld(p);
                                UserConsole.PushValue(worldPoint);
                            }
                            break;
                        case InputType.Object:
                            var obj = (from shape in objects.Children.OfType<Shapes.Shape>()
                                       let result = VisualTreeHelper.HitTest(shape, cursor)
                                       where result != null
                                       let o = shape.Tag as IObject
                                       where o != null
                                       select o).FirstOrDefault();
                            if (obj != null)
                            {
                                UserConsole.PushValue(obj);
                            }
                            break;
                    }
                    break;
                case MouseButton.Middle:
                    panning = true;
                    lastPoint = p;
                    break;
                case MouseButton.Right:
                    if (UserConsole.DesiredInputType == InputType.Command)
                        UserConsole.PushValue(null);
                    else
                        UserConsole.Cancel();
                    break;
            }
            DrawSnapPoints(sp);
            DrawRubberBandObjects(GetModelPoint(e));
        }

        private void DrawRubberBandObjects(Point modelPoint)
        {
            rubber.Children.Clear();
            var gen = UserConsole.PrimitiveGenerator;
            if (gen != null)
            {
                foreach (var prim in gen(modelPoint))
                    DrawPrimitive(rubber, prim);
            }
        }

        private void DrawSnapPoints(TransformedSnapPoint snapPoint)
        {
            snap.Children.Clear();
            if (UserConsole.DesiredInputType != InputType.Point)
                return;
            if (snapPoint == null)
                return;
            snap.Children.Add(snapPoint.Icon);
        }

        private TransformedSnapPoint ActiveSnapPoint(System.Windows.Point cursor)
        {
            var points = from sp in snapPoints
                         let dist = (sp.ControlPoint - cursor).LengthSquared
                         where dist <= snapPointDistSquared
                         orderby dist
                         select sp;
            return points.FirstOrDefault();
        }

        private Point lastPoint;

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            //var controlPoint = e.GetPosition(this);

            CursorMove(e.GetPosition(this));
        }

        private void CursorMove(System.Windows.Point point)
        {
            if (panning)
            {
                var p = new Point(point);
                var delta = lastPoint - p;
                var scale = Scale;
                var dx = View.BottomLeft.X + delta.X * scale;
                var dy = View.BottomLeft.Y - delta.Y * scale;
                View.UpdateView(bottomLeft: new Point(dx, dy, View.BottomLeft.Z));
                lastPoint = p;
            }

            var sp = ActiveSnapPoint(point);
            DrawSnapPoints(sp);
            Point wp = (sp != null) ? sp.WorldPoint : View.ControlToWorld(new Point(point));
            //coords.Content = worldPoint.ToString();
            DrawRubberBandObjects(wp);
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

            DrawRubberBandObjects(GetModelPoint(e));
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

            DrawRubberBandObjects(GetModelPoint(e));
            DrawSnapPoints(ActiveSnapPoint(cursorPoint));
        }

        private Point GetModelPoint(MouseEventArgs e)
        {
            var controlPoint = e.GetPosition(this);
            return View.ControlToWorld(new Point(controlPoint));
        }
    }
}
