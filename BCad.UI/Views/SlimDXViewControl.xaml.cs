using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel.Composition;
using BCad.EventArguments;
using SlimDX;
using SlimDX.Direct3D9;
using System.Windows.Interop;
using BCad.Objects;
using BCad.SnapPoints;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.ComponentModel;

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
            var cursor = GetActiveModelPoint(e.GetPosition(this).ToVector3());
            DrawSnapPoint(cursor);
            GenerateRubberBandLines(cursor.WorldPoint);
        }

        private Device device;

        void WindowLoaded(object sender, RoutedEventArgs e)
        {
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

            device.SetTransform(TransformState.Projection, SlimDX.Matrix.Identity);
            device.SetTransform(TransformState.View, SlimDX.Matrix.Identity);
            device.SetTransform(TransformState.World, SlimDX.Matrix.Identity);

            d3dimage.Lock();
            d3dimage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, device.GetBackBuffer(0, 0).ComPointer);
            d3dimage.Unlock();

            device.VertexDeclaration = new VertexDeclaration(device, new[] {
                    new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
                    new VertexElement(0, 12, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0) });
            device.SetRenderState(RenderState.Lighting, false);

            CompositionTarget.Rendering += OnRendering;
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
            switch (e.PropertyName)
            {
                case "BackgroundColor":
                    var c = Workspace.SettingsManager.BackgroundColor;
                    backgroundColor = (c.R << 16) | (c.G << 8) | c.B;
                    var brightness = System.Drawing.Color.FromArgb(backgroundColor).GetBrightness();
                    autoColor = brightness < 0.67 ? 0xFFFFFF : 0x000000;
                    break;
                default:
                    break;
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

        private void DocumentChanged(Document document)
        {
            processingDocument = true;
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            lines.Clear();
            var red = System.Drawing.Color.Red.ToArgb();
            foreach (var layer in document.Layers.Values)
            {
                foreach (var entity in layer.Objects)
                {
                    lines[entity.Id] = GenerateEntitySegments(entity, layer.Color).ToArray();
                }
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
                        SlimDX.Matrix.Scaling(new Vector3((float)radius))
                        * SlimDX.Matrix.RotationZ(-(float)Math.Atan2(normal.Y, normal.X))
                        * SlimDX.Matrix.RotationX(-(float)Math.Atan2(normal.Y, normal.Z))
                        * SlimDX.Matrix.RotationY((float)Math.Atan2(normal.X, normal.Z))
                        * SlimDX.Matrix.Translation(center.ToVector3());
                    for (int i = 0; i < segCount; i++, angle += angleDelta)
                    {
                        var x = (float)Math.Cos(angle);
                        var y = (float)Math.Sin(angle);
                        var result = Vector3.Transform(new Vector3(x, y, 0.0f), transformation);
                        segments[i] = new LineVertex()
                        {
                            Position = new Vector3(result.X / result.W, result.Y / result.W, result.Z / result.W),
                            Color = color
                        };
                    }
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
            var matrix = SlimDX.Matrix.Identity
                * SlimDX.Matrix.Translation((float)-View.BottomLeft.X, (float)-View.BottomLeft.Y, 0)
                * SlimDX.Matrix.Translation(-width / 2.0f, -height / 2.0f, 0)
                * SlimDX.Matrix.Scaling(2.0f / width, 2.0f / height, 1.0f);

            device.SetTransform(TransformState.Projection, matrix);
            device.SetTransform(TransformState.View, SlimDX.Matrix.Scaling(1, 1, 0));

            UpdateSnapPoints(matrix);
        }

        private void UpdateSnapPoints(SlimDX.Matrix matrix)
        {
            if (snapPoints.Length > 0)
            {
                for (int i = 0; i < snapPoints.Length; i++)
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
                }
            }
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

            var geometry = (GeometryDrawing)SnapPointResources[name];
            var scale = Workspace.SettingsManager.SnapPointSize;
            geometry.Pen = new Pen(Brushes.Yellow, 0.2);
            var di = new DrawingImage(geometry);
            var icon = new Image();
            icon.Source = di;
            icon.Stretch = Stretch.None;
            icon.LayoutTransform = new ScaleTransform(scale, scale);
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
                ?? GetRawModelPoint(cursor);
        }

        private TransformedSnapPoint GetRawModelPoint(Vector3 cursor)
        {
            var world = device.GetTransform(TransformState.World);
            //var view = device.GetTransform(TransformState.View);
            var projection = device.GetTransform(TransformState.Projection);
            var matrix = projection * world;
            var worldPoint = Vector3.Unproject(
                cursor,
                device.Viewport.X,
                device.Viewport.Y,
                device.Viewport.Width,
                device.Viewport.Height,
                device.Viewport.MinZ,
                device.Viewport.MaxZ,
                matrix);
            return new TransformedSnapPoint(worldPoint.ToPoint(), cursor, SnapPointKind.None);
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

        private bool panning = false;
        private System.Windows.Point lastPanPoint = new System.Windows.Point();

        private void clicker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var cursor = e.GetPosition(this);
            var sp = GetActiveModelPoint(e.GetPosition(this).ToVector3());
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    switch (InputService.DesiredInputType)
                    {
                        case InputType.Point:
                            InputService.PushValue(sp.WorldPoint);
                            break;
                        case InputType.Object:
                            // TODO: object selection NYI
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
