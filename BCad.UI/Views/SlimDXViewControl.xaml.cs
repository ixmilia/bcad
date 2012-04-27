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
//using SlimDX.Direct3D11;
using BCad.Objects;
using BCad.SnapPoints;

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
            public Point WorldPoint { get; private set; }
            public Vector3 ControlPoint { get; set; }
            public SnapPointKind Kind { get; private set; }

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

            //lines = new[]
            //{
            //    new LineVertex() { Position = new Vector3(0, 0, 0), Color = 0 },
            //    new LineVertex() { Position = new Vector3(100, 100, 0), Color = 0 },
            //    //new LineVertex() { Position = new Vector3(0.5f, -0.5f, 0), Color = 0 },
            //    //new LineVertex() { Position = new Vector3(0, 0, 0), Color = 0 },
            //};
        }

        private PresentParameters pp = null;

        void SlimDXViewControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            pp.BackBufferWidth = (int)ActualWidth;
            pp.BackBufferHeight = (int)ActualHeight;
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
                device.Clear(ClearFlags.Target, System.Drawing.Color.CornflowerBlue.ToArgb(), 0, 0);
                device.BeginScene();
                foreach (var lineSet in lines.Values)
                {
                    if (lineSet.Length > 0)
                        device.DrawUserPrimitives(PrimitiveType.LineStrip, lineSet.Length - 1, lineSet);
                }
                device.EndScene();
                device.Present();
            }
        }

        public override Point GetCursorPoint()
        {
            return Point.Origin;
        }

        [Import]
        private IWorkspace Workspace = null;

        [Import]
        private IView View = null;

        public void OnImportsSatisfied()
        {
            //InputService.RubberBandGeneratorChanged += UserConsole_RubberBandGeneratorChanged;
            View.ViewPortChanged += TransformationMatrixChanged;
            //Workspace.CommandExecuted += Workspace_CommandExecuted;
            //Workspace.DocumentChanging += DocumentChanging;
            Workspace.DocumentChanged += DocumentChanged;
        }

        private Dictionary<uint, LineVertex[]> lines = new Dictionary<uint, LineVertex[]>();
        private bool processingDocument = false;
        private TransformedSnapPoint[] snapPoints = new TransformedSnapPoint[0];

        private void DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            processingDocument = true;
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            lines.Clear();
            foreach (var layer in e.Document.Layers.Values)
            {
                foreach (var entity in layer.Objects)
                {
                    switch (entity.Kind)
                    {
                        case EntityKind.Line:
                            var line = (BCad.Objects.Line)entity;
                            lines[entity.Id] = new[]
                                {
                                    new LineVertex() { Position = line.P1.ToVector3(), Color = line.Color.IntColor },
                                    new LineVertex() { Position = line.P2.ToVector3(), Color = line.Color.IntColor }
                                };
                            break;
                        case EntityKind.Circle:
                            var circle = (BCad.Objects.Circle)entity;
                            lines[entity.Id] = GenerateCurveSegments(circle.Center, circle.Radius, 0.0, 360.0, circle.Color.IntColor);
                            break;
                        case EntityKind.Arc:
                            var arc = (BCad.Objects.Arc)entity;
                            lines[entity.Id] = GenerateCurveSegments(arc.Center, arc.Radius, arc.StartAngle, arc.EndAngle, arc.Color.IntColor);
                            break;
                        default:
                            System.Diagnostics.Debug.Fail("unsupported");
                            break;
                    }
                }
            }


            snapPoints = e.Document.Layers.Values.SelectMany(l => l.Objects.SelectMany(o => o.GetSnapPoints()))
                .Select(sp => new TransformedSnapPoint(sp.Point, sp.Point.ToVector3(), sp.Kind)).ToArray();
            UpdateSnapPoints(device.GetTransform(TransformState.Projection));
            sw.Stop();
            var loadTime = sw.ElapsedMilliseconds;
            processingDocument = false;
        }

        private const int FullCircleDrawingSegments = 101;
        private const double DegreesToRadians = Math.PI / 180.0;
        private const double TwoPI = Math.PI * 2.0;

        private static LineVertex[] GenerateCurveSegments(Point center, double radius, double startAngle, double endAngle, int color)
        {
            startAngle = startAngle * DegreesToRadians;
            endAngle = endAngle * DegreesToRadians;
            var coveringAngle = endAngle - startAngle;
            if (coveringAngle < 0.0) coveringAngle += TwoPI;
            var segCount = Math.Max(3, (int)(coveringAngle / TwoPI * (double)FullCircleDrawingSegments));
            var segments = new LineVertex[segCount];
            var angleDelta = coveringAngle / (double)(segCount - 1);
            var angle = startAngle;
            for (int i = 0; i < segCount; i++, angle += angleDelta)
            {
                var x = (float)(Math.Cos(angle) * radius + center.X);
                var y = (float)(Math.Sin(angle) * radius + center.Y);
                segments[i] = new LineVertex() { Position = new Vector3(x, y, 0.0f), Color = color };
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
                * SlimDX.Matrix.Scaling(2.0f / width, 2.0f / height, 0);

            device.SetTransform(TransformState.Projection, matrix);

            UpdateSnapPoints(matrix);
        }

        private void UpdateSnapPoints(SlimDX.Matrix matrix)
        {
            if (snapPoints.Length > 0)
            {
                for (int i = 0; i < snapPoints.Length; i++)
                {
                    var wp = snapPoints[i].WorldPoint.ToVector3();
                    Vector3 cp;
                    Vector3.Project(
                        ref wp, // input
                        0.0f, // x
                        0.0f, // y
                        (float)this.ActualWidth, // viewport width
                        (float)this.ActualHeight, // viewport height
                        0.0f, // z-min
                        1.0f, // z-max
                        ref matrix, // transformation matrix
                        out cp); // output
                    snapPoints[i].ControlPoint = cp;
                }
            }
        }

        private Image GetSnapIcon(TransformedSnapPoint snapPoint)
        {
            string name;
            switch (snapPoint.Kind)
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
                    throw new ArgumentException("snapPoint.Kind");
            }

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
            snapLayer.Children.Clear();
            TransformedSnapPoint snapPoint = null;
            var cursor = new Point(e.GetPosition(this)).ToVector3();
            var distSq = (float)(Workspace.SettingsManager.SnapPointDistance * Workspace.SettingsManager.SnapPointDistance);
            var currentDist = distSq;
            for (int i = 0; i < snapPoints.Length; i++)
            {
                var dist = (cursor - snapPoints[i].ControlPoint).LengthSquared();
                if (dist < currentDist)
                {
                    dist = currentDist;
                    snapPoint = snapPoints[i];
                }
            }

            if (snapPoint != null)
            {
                snapLayer.Children.Add(GetSnapIcon(snapPoint));
            }
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

        public static Point ToPoint(this Vector3 vector)
        {
            return new Point(vector.X, vector.Y, vector.Z);
        }
    }
}
