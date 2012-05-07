using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using BCad.EventArguments;
using BCad.Objects;
using BCad.SnapPoints;
using SlimDX;
using SlimDX.Direct3D9;
using Media = System.Windows.Media;
using System.Threading.Tasks;
using BCad.Helpers;

namespace BCad.UI.Views
{
    /// <summary>
    /// Interaction logic for SlimDXViewControl.xaml
    /// </summary>
    [ExportViewControl("SlimDX")]
    public partial class SlimDXViewControl : ViewControl, IPartImportsSatisfiedNotification
    {
        private class TransformedSnapPoint
        {
            public Point WorldPoint;
            public Vector3 ControlPoint;
            public SnapPointKind Kind;

            public TransformedSnapPoint(Point worldPoint, Vector3 controlPoint, SnapPointKind kind)
            {
                this.WorldPoint = worldPoint;
                this.ControlPoint = controlPoint;
                this.Kind = kind;
            }
        }

        public SlimDXViewControl()
        {
            InitializeComponent();
            Loaded += WindowLoaded;
            MouseWheel += SlimDXViewControl_MouseWheel;
            mainImage.MouseWheel += SlimDXViewControl_MouseWheel;
            this.SizeChanged += new SizeChangedEventHandler(SlimDXViewControl_SizeChanged);
        }

        private PresentParameters pp = null;

        void SlimDXViewControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //pp.BackBufferWidth = (int)ActualWidth;
            //pp.BackBufferHeight = (int)ActualHeight;
            //device.Reset(pp);
        }

        void SlimDXViewControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // scale everything
            var scale = 1.25f;
            if (e.Delta > 0.0f) scale = 0.8f; // 1.0f / 1.25f

            // center zoom operation on mouse
            var cursorPoint = e.GetPosition(this);
            var controlPoint = new Point(cursorPoint);
            var cursorPos = Unproject(controlPoint.ToVector3()).ToPoint();
            var botLeft = View.BottomLeft;

            // find relative scales
            var viewHeight = this.ActualHeight / this.ActualWidth * View.ViewWidth;
            var relHoriz = controlPoint.X / this.ActualWidth;
            var relVert = controlPoint.Y / this.ActualHeight;
            var viewWidthDelta = View.ViewWidth * (scale - 1.0);
            var viewHeightDelta = viewHeight * (scale - 1.0);

            // set values
            View.UpdateView(viewWidth: View.ViewWidth * scale, bottomLeft: (botLeft - new Vector(viewWidthDelta * relHoriz, viewHeightDelta * relVert, 0.0)).ToPoint());
            var cursor = GetActiveModelPoint(e.GetPosition(this).ToVector3());
            DrawSnapPoint(cursor);
            GenerateRubberBandLines(cursor.WorldPoint);
        }

        private Device device;

        private void WindowLoaded(object sender, RoutedEventArgs e)
        //protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            var hwnd = new HwndSource(0, 0, 0, 0, 0, "test", IntPtr.Zero);
            var direct3d = new Direct3D();
            pp = new PresentParameters()
            {
                SwapEffect = SwapEffect.Discard,
                DeviceWindowHandle = hwnd.Handle,
                Windowed = true,
                BackBufferWidth = (int)ActualWidth,
                BackBufferHeight = (int)ActualHeight,
                BackBufferFormat = Format.X8R8G8B8
            };
            device = new Device(direct3d, 0, DeviceType.Hardware, hwnd.Handle, CreateFlags.HardwareVertexProcessing, pp);

            device.SetTransform(TransformState.Projection, Matrix.Identity);
            device.SetTransform(TransformState.View, Matrix.Identity);
            device.SetTransform(TransformState.World, Matrix.Identity);

            d3dimage.Lock();
            d3dimage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, device.GetBackBuffer(0, 0).ComPointer);
            d3dimage.Unlock();

            device.VertexDeclaration = new VertexDeclaration(device, new[] {
                    new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
                    new VertexElement(0, 12, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0) });
            device.SetRenderState(RenderState.Lighting, false);

            Media.CompositionTarget.Rendering += OnRendering;
        }

        void OnRendering(object sender, EventArgs e)
        {
            try
            {
                d3dimage.Lock();
                d3dimage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, device.GetBackBuffer(0, 0).ComPointer);
                Render();
                d3dimage.AddDirtyRect(new Int32Rect(0, 0, d3dimage.PixelWidth, d3dimage.PixelHeight));
                d3dimage.Unlock();
            }
            catch (Direct3D9Exception)
            {
            }
        }

        struct LineVertex
        {
            public Vector3 Position;
            public int Color;
        }

        void Render()
        {
            if (d3dimage.IsFrontBufferAvailable && !processingDocument)
            {
                device.Clear(ClearFlags.Target, backgroundColor, 0, 0);
                device.BeginScene();
                foreach (var lineSet in lines.Values)
                {
                    if (lineSet.Length > 0)
                        device.DrawUserPrimitives(PrimitiveType.LineStrip, lineSet.Length - 1, lineSet);
                }

                if (rubberBandLines != null)
                {
                    foreach (var prim in rubberBandLines)
                    {
                        if (prim != null && prim.Length > 0)
                        {
                            device.DrawUserPrimitives(PrimitiveType.LineStrip, prim.Length - 1, prim);
                        }
                    }
                }

                device.EndScene();
                device.Present();
            }
        }

        public override Point GetCursorPoint()
        {
            var cursor = Mouse.GetPosition(this);
            var sp = GetActiveModelPoint(cursor.ToVector3());
            return sp.WorldPoint;
        }

        [Import]
        private IWorkspace Workspace = null;

        [Import]
        private IView View = null;

        [Import]
        private IInputService InputService = null;

        public void OnImportsSatisfied()
        {
            View.ViewPortChanged += TransformationMatrixChanged;
            Workspace.CommandExecuted += Workspace_CommandExecuted;
            Workspace.PropertyChanged += Workspace_PropertyChanged;
            Workspace.SettingsManager.PropertyChanged += SettingsManager_PropertyChanged;

            // load settings
            foreach (var setting in new[] { "BackgroundColor" })
                SettingsManager_PropertyChanged(null, new PropertyChangedEventArgs(setting));
        }

        void Workspace_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Document":
                    DocumentChanged(Workspace.Document);
                    break;
                default:
                    break;
            }
        }

        void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            bool redraw = false;
            switch (e.PropertyName)
            {
                case "BackgroundColor":
                    var c = Workspace.SettingsManager.BackgroundColor;
                    backgroundColor = (c.R << 16) | (c.G << 8) | c.B;
                    var brightness = System.Drawing.Color.FromArgb(backgroundColor).GetBrightness();
                    autoColor = brightness < 0.67 ? 0xFFFFFF : 0x000000;
                    break;
                case "AngleSnap":
                case "Ortho":
                case "PointSnap":
                    redraw = true;
                    break;
                default:
                    break;
            }

            if (redraw)
            {
                var cursor = Mouse.GetPosition(this);
                var sp = GetActiveModelPoint(cursor.ToVector3());
                GenerateRubberBandLines(sp.WorldPoint);
                DrawSnapPoint(sp);
            }
        }

        void Workspace_CommandExecuted(object sender, CommandExecutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke((Action)(() => this.snapLayer.Children.Clear()));
            rubberBandLines = null;
        }

        private Dictionary<uint, LineVertex[]> lines = new Dictionary<uint, LineVertex[]>();
        private IEnumerable<LineVertex[]> rubberBandLines = null;
        private bool processingDocument = false;
        private TransformedSnapPoint[] snapPoints = new TransformedSnapPoint[0];
        private Document document = null;

        private void DocumentChanged(Document document)
        {
            processingDocument = true;
            this.document = document;
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            lines.Clear();
            var red = System.Drawing.Color.Red.ToArgb();
            foreach (var layer in document.Layers.Values)
            {
                // TODO: parallelize this.  requires `lines` to be concurrent dictionary
                var start = DateTime.UtcNow;
                foreach (var entity in layer.Objects)
                {
                    lines[entity.Id] = GenerateEntitySegments(entity, layer.Color).ToArray();
                }
                var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
            }

            snapPoints = document.Layers.Values.SelectMany(l => l.Objects.SelectMany(o => o.GetSnapPoints()))
                .Select(sp => new TransformedSnapPoint(sp.Point, sp.Point.ToVector3(), sp.Kind)).ToArray();
            UpdateSnapPoints(device.GetTransform(TransformState.Projection));
            rubberBandLines = null;
            sw.Stop();
            var loadTime = sw.ElapsedMilliseconds;
            processingDocument = false;
        }

        private const int FullCircleDrawingSegments = 101;
        private const double DegreesToRadians = Math.PI / 180.0;
        private const double TwoPI = Math.PI * 2.0;

        private void GenerateRubberBandLines(Point worldPoint)
        {
            var generator = InputService.PrimitiveGenerator;
            rubberBandLines = generator == null
                ? null
                : generator(worldPoint).Select(p => GeneratePrimitiveSegments(p, autoColor).ToArray());
        }

        private int GetDisplayColor(Color layerColor, Color primitiveColor)
        {
            if (!primitiveColor.IsAuto)
                return primitiveColor.ToInt();
            if (!layerColor.IsAuto)
                return layerColor.ToInt();
            return autoColor;
        }

        private int backgroundColor = 0x000000;
        private int autoColor = 0xFFFFFF;

        private IEnumerable<LineVertex> GenerateEntitySegments(Entity entity, Color layerColor)
        {
            return entity.GetPrimitives().SelectMany(p => GeneratePrimitiveSegments(p, GetDisplayColor(layerColor, p.Color)));
        }

        private IEnumerable<LineVertex> GeneratePrimitiveSegments(IPrimitive primitive, int color)
        {
            LineVertex[] segments;
            switch (primitive.Kind)
            {
                case PrimitiveKind.Line:
                    var line = (BCad.Objects.Line)primitive;
                    segments = new[] {
                        new LineVertex() { Position = line.P1.ToVector3(), Color = color },
                        new LineVertex() { Position = line.P2.ToVector3(), Color = color }
                    };
                    break;
                case PrimitiveKind.Arc:
                case PrimitiveKind.Circle:
                    double startAngle, endAngle, radius;
                    Point center;
                    Vector normal;
                    if (primitive.Kind == PrimitiveKind.Arc)
                    {
                        var arc = (Arc)primitive;
                        startAngle = arc.StartAngle;
                        endAngle = arc.EndAngle;
                        radius = arc.Radius;
                        center = arc.Center;
                        normal = arc.Normal;
                    }
                    else
                    {
                        var circle = (Circle)primitive;
                        startAngle = 0.0;
                        endAngle = 360.0;
                        radius = circle.Radius;
                        center = circle.Center;
                        normal = circle.Normal;
                    }
                    startAngle *= DegreesToRadians;
                    endAngle *= DegreesToRadians;
                    var coveringAngle = endAngle - startAngle;
                    if (coveringAngle < 0.0) coveringAngle += TwoPI;
                    var segCount = Math.Max(3, (int)(coveringAngle / TwoPI * (double)FullCircleDrawingSegments));
                    segments = new LineVertex[segCount];
                    var angleDelta = coveringAngle / (double)(segCount - 1);
                    var angle = startAngle;
                    var transformation =
                        Matrix.Scaling(new Vector3((float)radius))
                        * Matrix.RotationZ(-(float)Math.Atan2(normal.Y, normal.X))
                        * Matrix.RotationX(-(float)Math.Atan2(normal.Y, normal.Z))
                        * Matrix.RotationY((float)Math.Atan2(normal.X, normal.Z))
                        * Matrix.Translation(center.ToVector3());
                    var start = DateTime.UtcNow;
                    for (int i = 0; i < segCount; i++, angle += angleDelta)
                    {
                        var result = Vector3.Transform(
                            new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0.0f),
                            transformation);
                        segments[i] = new LineVertex()
                        {
                            Position = new Vector3(result.X / result.W, result.Y / result.W, result.Z / result.W),
                            Color = color
                        };
                    }
                    var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
                    break;
                default:
                    throw new ArgumentException("entity.Kind");
            }

            return segments;
        }

        private void TransformationMatrixChanged(object sender, ViewPortChangedEventArgs e)
        {
            if (device == null)
                return;
            
            var width = (float)View.ViewWidth;
            var height = (float)(View.ViewWidth * mainImage.ActualHeight / mainImage.ActualWidth);
            var matrix = Matrix.Identity
                * Matrix.Translation((float)-View.BottomLeft.X, (float)-View.BottomLeft.Y, 0)
                * Matrix.Translation(-width / 2.0f, -height / 2.0f, 0)
                * Matrix.Scaling(2.0f / width, 2.0f / height, 1.0f);

            device.SetTransform(TransformState.Projection, matrix);
            device.SetTransform(TransformState.View, Matrix.Scaling(1, 1, 0));

            UpdateSnapPoints(matrix);
        }

        private void UpdateSnapPoints(Matrix matrix)
        {
            var start = DateTime.UtcNow;
            if (snapPoints.Length > 0)
            {
                Parallel.For(0, snapPoints.Length,
                    (i) =>
                    {
                        var wp = snapPoints[i].WorldPoint.ToVector3();
                        Vector3.Project(
                            ref wp, // input
                            device.Viewport.X, // x
                            device.Viewport.Y, // y
                            device.Viewport.Width, // viewport width
                            device.Viewport.Height, // viewport height
                            device.Viewport.MinZ, // z-min
                            device.Viewport.MaxZ, // z-max
                            ref matrix, // transformation matrix
                            out snapPoints[i].ControlPoint); // output
                        snapPoints[i].ControlPoint.Z = 0.0f;
                    });
            }
            var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
        }

        private Image GetSnapIcon(TransformedSnapPoint snapPoint)
        {
            string name;
            switch (snapPoint.Kind)
            {
                case SnapPointKind.None:
                    name = null;
                    break;
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
                    throw new ArgumentException("snapPoint.Kind");
            }

            if (name == null)
                return null;

            var geometry = (Media.GeometryDrawing)SnapPointResources[name];
            var scale = Workspace.SettingsManager.SnapPointSize;
            geometry.Pen = new Media.Pen(new Media.SolidColorBrush(Workspace.SettingsManager.SnapPointColor), 0.2);
            var di = new Media.DrawingImage(geometry);
            var icon = new Image();
            icon.Source = di;
            icon.Stretch = Media.Stretch.None;
            icon.LayoutTransform = new Media.ScaleTransform(scale, scale);
            Canvas.SetLeft(icon, snapPoint.ControlPoint.X - geometry.Bounds.Width * scale / 2.0);
            Canvas.SetTop(icon, snapPoint.ControlPoint.Y - geometry.Bounds.Height * scale / 2.0);
            return icon;
        }

        private ResourceDictionary resources = null;
        private ResourceDictionary SnapPointResources
        {
            get
            {
                if (resources == null)
                {
                    resources = new ResourceDictionary();
                    resources.Source = new Uri("/BCad.Core;component/SnapPoints/SnapPointIcons.xaml", UriKind.Relative);
                }

                return resources;
            }
        }

        private void clicker_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var cursor = e.GetPosition(this);
            if (panning)
            {
                var delta = lastPanPoint - cursor;
                var scale = View.ViewWidth / this.ActualWidth;
                var dx = View.BottomLeft.X + delta.X * scale;
                var dy = View.BottomLeft.Y - delta.Y * scale;
                View.UpdateView(bottomLeft: new Point(dx, dy, View.BottomLeft.Z));
                lastPanPoint = cursor;
            }

            if (InputService.DesiredInputType == InputType.Point)
            {
                var sp = GetActiveModelPoint(cursor.ToVector3());
                GenerateRubberBandLines(sp.WorldPoint);
                DrawSnapPoint(sp);
            }
        }

        private void DrawSnapPoint(TransformedSnapPoint snapPoint)
        {
            snapLayer.Children.Clear();
            if (snapPoint.Kind == SnapPointKind.None)
                return;
            snapLayer.Children.Add(GetSnapIcon(snapPoint));
        }

        private TransformedSnapPoint GetActiveModelPoint(Vector3 cursor)
        {
            return ActiveObjectSnapPoints(cursor)
                ?? GetOrthoPoint(cursor)
                ?? GetAngleSnapPoint(cursor)
                ?? GetRawModelPoint(cursor);
        }

        private TransformedSnapPoint GetRawModelPoint(Vector3 cursor)
        {
            var world = device.GetTransform(TransformState.World);
            var projection = device.GetTransform(TransformState.Projection);
            var matrix = projection * world;
            var worldPoint = Unproject(cursor);
            return new TransformedSnapPoint(worldPoint.ToPoint(), cursor, SnapPointKind.None);
        }

        private TransformedSnapPoint GetAngleSnapPoint(Vector3 cursor)
        {
            if (InputService.IsDrawing && Workspace.SettingsManager.AngleSnap)
            {
                // get distance to last point
                var last = InputService.LastPoint;
                var current = Unproject(cursor).ToPoint();
                var vector = current - last;
                var dist = vector.Length;

                // for each snap angle, find the point `dist` out on the angle vector
                Func<double, Vector> snapVector = rad =>
                    {
                        Vector radVector = null;
                        switch (Workspace.DrawingPlane)
                        {
                            case DrawingPlane.XY:
                                radVector = new Vector(Math.Cos(rad), Math.Sin(rad), Workspace.DrawingPlaneOffset);
                                break;
                            case DrawingPlane.XZ:
                                radVector = new Vector(Math.Cos(rad), Workspace.DrawingPlaneOffset, Math.Sin(rad));
                                break;
                            case DrawingPlane.YZ:
                                radVector = new Vector(Workspace.DrawingPlaneOffset, Math.Cos(rad), Math.Sin(rad));
                                break;
                            default:
                                Debug.Fail("invalid value for drawing plane");
                                break;
                        }

                        return radVector.Normalize() * dist;
                    };

                var points = from sa in Workspace.SettingsManager.SnapAngles
                             let rad = sa * Math.PI / 180.0
                             let radVector = snapVector(rad)
                             let snapPoint = (last + radVector).ToPoint()
                             let di = (cursor - Project(snapPoint.ToVector3())).Length()
                             where di <= Workspace.SettingsManager.SnapAngleDistance
                             orderby di
                             select new TransformedSnapPoint(snapPoint, Project(snapPoint.ToVector3()), SnapPointKind.None);

                // return the closest one
                return points.FirstOrDefault();
            }

            return null;
        }

        private TransformedSnapPoint GetOrthoPoint(Vector3 cursor)
        {
            if (InputService.IsDrawing && Workspace.SettingsManager.Ortho)
            {
                // if both are on the drawing plane
                var last = InputService.LastPoint;
                var current = Unproject(cursor).ToPoint();
                var delta = current - last;
                var offset = Workspace.DrawingPlaneOffset;
                Point world;
                switch (Workspace.DrawingPlane)
                {
                    case DrawingPlane.XY:
                        if (offset != last.Z && offset != current.Z)
                            return null;
                        if (Math.Abs(delta.X) > Math.Abs(delta.Y))
                            world = (last + new Vector(delta.X, 0.0, 0.0)).ToPoint();
                        else
                            world = (last + new Vector(0.0, delta.Y, 0.0)).ToPoint();
                        break;
                    case DrawingPlane.XZ:
                        if (offset != last.Y && offset != current.Y)
                            return null;
                        if (Math.Abs(delta.X) > Math.Abs(delta.Z))
                            world = (last + new Vector(delta.X, 0.0, 0.0)).ToPoint();
                        else
                            world = (last + new Vector(0.0, 0.0, delta.Z)).ToPoint();
                        break;
                    case DrawingPlane.YZ:
                        if (offset != last.X && offset != current.X)
                            return null;
                        if (Math.Abs(delta.Y) > Math.Abs(delta.Z))
                            world = (last + new Vector(0.0, delta.Y, 0.0)).ToPoint();
                        else
                            world = (last + new Vector(0.0, 0.0, delta.Z)).ToPoint();
                        break;
                    default:
                        throw new NotSupportedException("Invalid drawing plane");
                }

                Debug.Assert(world != null, "should have returned null");
                return new TransformedSnapPoint(world, cursor, SnapPointKind.None);
            }

            return null;
        }

        private TransformedSnapPoint ActiveObjectSnapPoints(Vector3 cursor)
        {
            if (Workspace.SettingsManager.PointSnap && InputService.DesiredInputType == InputType.Point)
            {
                var maxDistSq = (float)(Workspace.SettingsManager.SnapPointDistance * Workspace.SettingsManager.SnapPointDistance);
                var points = from sp in snapPoints
                             let dist = (cursor - sp.ControlPoint).LengthSquared()
                             where dist <= maxDistSq
                             orderby dist
                             select sp;
                return points.FirstOrDefault();
            }

            return null;
        }

        private Vector3 Project(Vector3 point)
        {
            var world = device.GetTransform(TransformState.World);
            var view = device.GetTransform(TransformState.View);
            var projection = device.GetTransform(TransformState.Projection);
            var matrix = projection * view * world;
            var screenPoint = Vector3.Project(
                point,
                device.Viewport.X,
                device.Viewport.Y,
                device.Viewport.Width,
                device.Viewport.Height,
                device.Viewport.MinZ,
                device.Viewport.MaxZ,
                matrix);
            return screenPoint;
        }

        private Vector3 Unproject(Vector3 point)
        {
            // not using view matrix because that scales z at 0 for display
            var world = device.GetTransform(TransformState.World);
            var projection = device.GetTransform(TransformState.Projection);
            var matrix = projection * world;
            var worldPoint = Vector3.Unproject(
                point,
                device.Viewport.X,
                device.Viewport.Y,
                device.Viewport.Width,
                device.Viewport.Height,
                device.Viewport.MinZ,
                device.Viewport.MaxZ,
                matrix);
            return worldPoint;
        }

        private bool panning = false;
        private System.Windows.Point lastPanPoint = new System.Windows.Point();

        private void clicker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var cursor = e.GetPosition(this);
            var cursorVector = cursor.ToVector3();
            var sp = GetActiveModelPoint(cursorVector);
            var selectionDist = Workspace.SettingsManager.ObjectSelectionRadius;
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    switch (InputService.DesiredInputType)
                    {
                        case InputType.Point:
                            InputService.PushValue(sp.WorldPoint);
                            break;
                        case InputType.Object:
                            var start = DateTime.UtcNow;
                            uint hitEntity = 0;
                            foreach (var entityId in lines.Keys)
                            {
                                var segments = lines[entityId];
                                for (int i = 0; i < segments.Length - 1; i++)
                                {
                                    // translate line segment to screen coordinates
                                    var p1 = Project(segments[i].Position);
                                    var p2 = Project(segments[i + 1].Position);
                                    // check that cursor is in expanded bounding box of line segment
                                    var minx = Math.Min(p1.X, p2.X) - selectionDist;
                                    var maxx = Math.Max(p1.X, p2.X) + selectionDist;
                                    var miny = Math.Min(p1.Y, p2.Y) - selectionDist;
                                    var maxy = Math.Max(p1.Y, p2.Y) + selectionDist;
                                    if (MathHelper.Between(minx, maxx, cursor.X) && MathHelper.Between(miny, maxy, cursor.Y))
                                    {
                                        // in bounding rectangle, check distance to line
                                        var x1 = p1.X - cursor.X;
                                        var x2 = p2.X - cursor.X;
                                        var y1 = p1.Y - cursor.Y;
                                        var y2 = p2.Y - cursor.Y;
                                        var dx = x2 - x1;
                                        var dy = y2 - y1;
                                        var dr2 = dx * dx + dy * dy;
                                        var D = x1 * y2 - x2 * y1;
                                        var det = (selectionDist * selectionDist * dr2) - (D * D);
                                        if (det >= 0.0)
                                        {
                                            hitEntity = entityId;
                                            break;
                                        }
                                    }
                                }
                                if (hitEntity > 0)
                                    break;
                            }

                            if (hitEntity > 0)
                            {
                                // found it
                                var ent = document.Layers.Values.SelectMany(l => l.Objects).FirstOrDefault(en => en.Id == hitEntity);
                                Debug.Assert(ent != null, "hit object not in document");
                                var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
                                InputService.PushValue(ent);
                            }
                            break;
                    }
                    break;
                case MouseButton.Middle:
                    panning = true;
                    lastPanPoint = cursor;
                    break;
                case MouseButton.Right:
                    InputService.PushValue(null);
                    break;
            }

            GenerateRubberBandLines(sp.WorldPoint);
        }

        private void clicker_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var cursor = e.GetPosition(this);
            switch (e.ChangedButton)
            {
                case MouseButton.Middle:
                    panning = false;
                    break;
            }

            var sp = GetActiveModelPoint(cursor.ToVector3());
            GenerateRubberBandLines(sp.WorldPoint);
        }
    }

    internal static class SlimDXExtensions
    {
        public static Vector3 ToVector3(this Point point)
        {
            return new Vector3((float)point.X, (float)point.Y, (float)point.Z);
        }

        public static Vector3 ToVector3(this Vector vector)
        {
            return new Vector3((float)vector.X, (float)vector.Y, (float)vector.Z);
        }

        public static Vector3 ToVector3(this System.Windows.Point point)
        {
            return new Vector3((float)point.X, (float)point.Y, 0.0f);
        }

        public static Point ToPoint(this Vector3 vector)
        {
            return new Point(vector.X, vector.Y, vector.Z);
        }
    }
}
