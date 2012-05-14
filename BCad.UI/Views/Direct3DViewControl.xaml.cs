using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BCad.Entities;
using BCad.EventArguments;
using BCad.Helpers;
using BCad.Primitives;
using BCad.SnapPoints;
using SlimDX;
using SlimDX.Direct3D9;
using Media = System.Windows.Media;
using Shapes = System.Windows.Shapes;

namespace BCad.UI.Views
{
    /// <summary>
    /// Interaction logic for Direct3DViewControl.xaml
    /// </summary>
    [ExportViewControl("Direct3D")]
    public partial class Direct3DViewControl : ViewControl, IRenderEngine
    {

        private class TransformedEntity
        {
            public Entity Entity { get; private set; }
            public Tuple<Color4, Vector3[]>[] LineSegments { get; private set; }

            public TransformedEntity(Entity entity, Tuple<Color4, Vector3[]>[] lineSegments)
            {
                this.Entity = entity;
                this.LineSegments = lineSegments;
            }
        }

        #region Constructors

        public Direct3DViewControl()
        {
            InitializeComponent();
            this.Loaded += (_, __) => this.content.SetRenderEngine(this);
            this.Unloaded += (_, __) => this.content.Shutdown();
        }

        [ImportingConstructor]
        public Direct3DViewControl(IWorkspace workspace, IInputService inputService, IView view)
            : this()
        {
            this.workspace = workspace;
            this.inputService = inputService;
            this.view = view;

            this.MouseWheel += OnMouseWheel;
            this.view.ViewPortChanged += ViewPortChanged;
            this.workspace.PropertyChanged += WorkspacePropertyChanged;
            this.workspace.SettingsManager.PropertyChanged += SettingsManagerPropertyChanged;
            this.workspace.CommandExecuted += CommandExecuted;
            this.workspace.SelectedEntities.CollectionChanged += SelectedEntitiesCollectionChanged;
            this.inputService.ValueRequested += InputServiceValueRequested;
            this.inputService.ValueReceived += InputServiceValueReceived;

            // load the workspace
            foreach (var setting in new[] { "Document" })
                WorkspacePropertyChanged(this.workspace, new PropertyChangedEventArgs(setting));

            // load settings
            foreach (var setting in new[] { "BackgroundColor" })
                SettingsManagerPropertyChanged(this.workspace.SettingsManager, new PropertyChangedEventArgs(setting));
        }

        #endregion

        #region TransformedSnapPoint class

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

        #endregion

        #region Member variables

        private IWorkspace workspace = null;
        private IInputService inputService = null;
        private IView view = null;
        private Matrix worldMatrix = Matrix.Identity;
        private Matrix viewMatrix = Matrix.Scaling(1.0f, 1.0f, 0.0f);
        private Matrix projectionMatrix = Matrix.Identity;
        private Matrix projectionWorldMatrix = Matrix.Identity;
        private Matrix projectionViewWorldMatrix = Matrix.Identity;
        private TransformedSnapPoint[] snapPoints = new TransformedSnapPoint[0];
        private object documentGate = new object();
        private Document document = null;
        private Device Device { get { return this.content.Device; } }
        private Color4 autoColor = new Color4();
        private Dictionary<uint, TransformedEntity> lines = new Dictionary<uint, TransformedEntity>();
        private Tuple<Color4, Vector3[]>[] rubberBandLines = null;
        private bool panning = false;
        private bool selecting = false;
        private System.Windows.Point firstSelectionPoint = new System.Windows.Point();
        private System.Windows.Point currentSelectionPoint = new System.Windows.Point();
        private System.Windows.Point lastPanPoint = new System.Windows.Point();
        private bool lastGeneratorNonNull = false;
        private SlimDX.Direct3D9.Line solidLine;
        private SlimDX.Direct3D9.Line dashedLine;

        #endregion

        #region Constants

        private const int FullCircleDrawingSegments = 101;
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

        #endregion

        #region ViewControl implementation

        public override Point GetCursorPoint()
        {
            var cursor = Mouse.GetPosition(this);
            var sp = GetActiveModelPoint(cursor.ToVector3());
            return sp.WorldPoint;
        }

        #endregion

        #region IRenderEngine implementation

        public void OnDeviceCreated(object sender, EventArgs e)
        {
        }

        public void OnDeviceDestroyed(object sender, EventArgs e)
        {
        }

        public void OnDeviceLost(object sender, EventArgs e)
        {
        }

        public void OnDeviceReset(object sender, EventArgs e)
        {
            if (solidLine != null)
                solidLine.Dispose();
            if (dashedLine != null)
                dashedLine.Dispose();
            solidLine = new SlimDX.Direct3D9.Line(Device);
            dashedLine = new SlimDX.Direct3D9.Line(Device)
            {
                Width = 1.0f,
                Pattern = 0xF0F0F0F,
                PatternScale = 1
            };
            Device.SetRenderState(RenderState.Lighting, false);
        }

        public void OnMainLoop(object sender, EventArgs args)
        {
            lock (documentGate)
            {
                foreach (var entityId in lines.Keys)
                {
                    var len = lines[entityId].LineSegments.Length;
                    for (int i = 0; i < len; i++)
                    {
                        var color = lines[entityId].LineSegments[i].Item1;
                        var lineSet = lines[entityId].LineSegments[i].Item2;
                        Debug.Assert(lineSet.Length > 0);
                        if (workspace.SelectedEntities.ContainsHash(entityId.GetHashCode()))
                        {
                            dashedLine.DrawTransformed(lineSet, projectionViewWorldMatrix, color);
                        }
                        else
                        {
                            solidLine.DrawTransformed(lineSet, projectionViewWorldMatrix, color);
                        }
                    }
                }

                if (rubberBandLines != null)
                {
                    for (int i = 0; i < rubberBandLines.Length; i++)
                    {
                        var color = rubberBandLines[i].Item1;
                        var lineSet = rubberBandLines[i].Item2;
                        Debug.Assert(lineSet.Length > 0);
                        solidLine.DrawTransformed(lineSet, projectionViewWorldMatrix, autoColor);
                    }
                }

                if (selecting)
                {
                    var line = currentSelectionPoint.X < firstSelectionPoint.X
                        ? dashedLine
                        : solidLine;
                    var a = new Vector2((float)currentSelectionPoint.X, (float)currentSelectionPoint.Y);
                    var b = new Vector2((float)currentSelectionPoint.X, (float)firstSelectionPoint.Y);
                    var c = new Vector2((float)firstSelectionPoint.X, (float)firstSelectionPoint.Y);
                    var d = new Vector2((float)firstSelectionPoint.X, (float)currentSelectionPoint.Y);
                    var e = new Vector2((float)currentSelectionPoint.X, (float)currentSelectionPoint.Y);
                    line.Draw(new[] { a, b }, autoColor);
                    line.Draw(new[] { b, c }, autoColor);
                    line.Draw(new[] { c, d }, autoColor);
                    line.Draw(new[] { d, e }, autoColor);
                    line.Draw(new[] { e, a }, autoColor);
                }
            }
        }

        #endregion

        #region PropertyChanged functions

        private void SettingsManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            bool redraw = false;
            switch (e.PropertyName)
            {
                case "BackgroundColor":
                    var bg = workspace.SettingsManager.BackgroundColor;
                    this.content.ClearColor = bg;
                    var backgroundColor = (bg.R << 16) | (bg.G << 8) | bg.B;
                    var brightness = System.Drawing.Color.FromArgb(backgroundColor).GetBrightness();
                    var color = brightness < 0.67 ? 0xFFFFFF : 0x000000;
                    autoColor = new Color4((0xFF << 24) | color);
                    ForceRender();
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

        private void WorkspacePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Document":
                    DocumentChanged(workspace.Document);
                    break;
                default:
                    break;
            }
        }

        private void DocumentChanged(Document document)
        {
            lock (documentGate)
            {
                this.document = document;
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                lines.Clear();
                var red = System.Drawing.Color.Red.ToArgb();
                foreach (var layer in document.Layers.Values.Where(l => l.IsVisible))
                {
                    // TODO: parallelize this.  requires `lines` to be concurrent dictionary
                    var start = DateTime.UtcNow;
                    foreach (var entity in layer.Entities)
                    {
                        lines[entity.Id] = GenerateEntitySegments(entity, layer.Color);
                    }
                    var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
                }

                snapPoints = document.Layers.Values.SelectMany(l => l.Entities.SelectMany(o => o.GetSnapPoints()))
                    .Select(sp => new TransformedSnapPoint(sp.Point, sp.Point.ToVector3(), sp.Kind)).ToArray();
                UpdateSnapPoints(projectionMatrix);
                rubberBandLines = null;
                sw.Stop();
                var loadTime = sw.ElapsedMilliseconds;
            }

            ForceRender();
        }

        private void ViewPortChanged(object sender, ViewPortChangedEventArgs e)
        {
            var width = (float)view.ViewWidth;
            var height = (float)(view.ViewWidth * this.ActualHeight / this.ActualWidth);
            projectionMatrix = Matrix.Identity
                * Matrix.Translation((float)-view.BottomLeft.X, (float)-view.BottomLeft.Y, 0)
                * Matrix.Translation(-width / 2.0f, -height / 2.0f, 0)
                * Matrix.Scaling(2.0f / width, 2.0f / height, 1.0f);
            projectionWorldMatrix = projectionMatrix * worldMatrix;
            projectionViewWorldMatrix = projectionMatrix * viewMatrix * worldMatrix;

            Device.SetTransform(TransformState.Projection, projectionMatrix);
            Device.SetTransform(TransformState.View, viewMatrix);
            Device.SetTransform(TransformState.World, worldMatrix);
            UpdateSnapPoints(projectionMatrix);
            ForceRender();
        }

        private void SelectedEntitiesCollectionChanged(object sender, EventArgs e)
        {
            ForceRender();
        }

        private void CommandExecuted(object sender, CommandExecutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke((Action)(() => this.snapLayer.Children.Clear()));
            rubberBandLines = null;
            selecting = false;
            ForceRender();
        }

        private void InputServiceValueReceived(object sender, ValueReceivedEventArgs e)
        {
            selecting = false;
            ForceRender();
        }

        private void InputServiceValueRequested(object sender, ValueRequestedEventArgs e)
        {
            selecting = false;
            ForceRender();
        }

        #endregion

        #region Primitive generator functions

        private Color4 GetDisplayColor(Color layerColor, Color primitiveColor)
        {
            var color = autoColor;
            if (!primitiveColor.IsAuto)
                color = new Color4(primitiveColor.ToInt());
            if (!layerColor.IsAuto)
                color = new Color4(layerColor.ToInt());
            return color;
        }

        private void GenerateRubberBandLines(Point worldPoint)
        {
            var generator = inputService.PrimitiveGenerator;
            rubberBandLines = generator == null
                ? null
                : generator(worldPoint).Select(p => Tuple.Create<Color4, Vector3[]>(autoColor, GeneratePrimitiveSegments(p))).ToArray();

            if (generator != null || lastGeneratorNonNull)
            {
                ForceRender();
            }

            lastGeneratorNonNull = generator != null;
        }

        private TransformedEntity GenerateEntitySegments(Entity entity, Color layerColor)
        {
            return new TransformedEntity(entity,
                (from prim in entity.GetPrimitives()
                 select Tuple.Create<Color4, Vector3[]>(GetDisplayColor(layerColor, prim.Color), GeneratePrimitiveSegments(prim))).ToArray());
        }

        private Vector3[] GeneratePrimitiveSegments(IPrimitive primitive)
        {
            Vector3[] segments;
            switch (primitive.Kind)
            {
                case PrimitiveKind.Line:
                    var line = (PrimitiveLine)primitive;
                    segments = new[] {
                        line.P1.ToVector3(),
                        line.P2.ToVector3()
                    };
                    break;
                case PrimitiveKind.Ellipse:
                    var el = (PrimitiveEllipse)primitive;
                    double startAngle = el.StartAngle;
                    double endAngle = el.EndAngle;
                    double radiusX = el.MajorAxis.Length;
                    double radiusY = radiusX * el.MinorAxisRatio;
                    var center = el.Center;
                    var normal = el.Normal;
                    var right = el.MajorAxis;

                    normal = normal.Normalize();
                    right = right.Normalize();
                    var up = normal.Cross(right).Normalize();
                    startAngle *= MathHelper.DegreesToRadians;
                    endAngle *= MathHelper.DegreesToRadians;
                    var coveringAngle = endAngle - startAngle;
                    if (coveringAngle < 0.0) coveringAngle += MathHelper.TwoPI;
                    var segCount = Math.Max(3, (int)(coveringAngle / MathHelper.TwoPI * (double)FullCircleDrawingSegments));
                    segments = new Vector3[segCount];
                    var angleDelta = coveringAngle / (double)(segCount - 1);
                    var angle = startAngle;
                    var transformation = Matrix.Identity;
                    transformation.M11 = (float)right.X;
                    transformation.M12 = (float)right.Y;
                    transformation.M13 = (float)right.Z;
                    transformation.M21 = (float)up.X;
                    transformation.M22 = (float)up.Y;
                    transformation.M23 = (float)up.Z;
                    transformation.M31 = (float)normal.X;
                    transformation.M32 = (float)normal.Y;
                    transformation.M33 = (float)normal.Z;
                    transformation.M41 = (float)center.X;
                    transformation.M42 = (float)center.Y;
                    transformation.M43 = (float)center.Z;
                    var start = DateTime.UtcNow;
                    for (int i = 0; i < segCount; i++, angle += angleDelta)
                    {
                        var result = Vector3.Transform(
                            new Vector3((float)(Math.Cos(angle) * radiusX), (float)(Math.Sin(angle) * radiusY), 0.0f),
                            transformation);
                        segments[i] = new Vector3(result.X / result.W, result.Y / result.W, result.Z / result.W);
                    }
                    var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
                    break;
                default:
                    throw new ArgumentException("entity.Kind");
            }

            return segments;
        }

        #endregion

        #region SnapPointFunctions

        private void DrawSnapPoint(TransformedSnapPoint snapPoint)
        {
            snapLayer.Children.Clear();
            if (snapPoint.Kind == SnapPointKind.None)
                return;
            snapLayer.Children.Add(GetSnapIcon(snapPoint));
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
                            Device.Viewport.X, // x
                            Device.Viewport.Y, // y
                            Device.Viewport.Width, // viewport width
                            Device.Viewport.Height, // viewport height
                            Device.Viewport.MinZ, // z-min
                            Device.Viewport.MaxZ, // z-max
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
            var scale = workspace.SettingsManager.SnapPointSize;
            geometry.Pen = new Media.Pen(new Media.SolidColorBrush(workspace.SettingsManager.SnapPointColor), 0.2);
            var di = new Media.DrawingImage(geometry);
            var icon = new Image();
            icon.Source = di;
            icon.Stretch = Media.Stretch.None;
            icon.LayoutTransform = new Media.ScaleTransform(scale, scale);
            Canvas.SetLeft(icon, snapPoint.ControlPoint.X - geometry.Bounds.Width * scale / 2.0);
            Canvas.SetTop(icon, snapPoint.ControlPoint.Y - geometry.Bounds.Height * scale / 2.0);
            return icon;
        }

        #endregion

        #region GetPoint functions

        private TransformedSnapPoint GetActiveModelPoint(Vector3 cursor)
        {
            return ActiveEntitySnapPoints(cursor)
                ?? GetOrthoPoint(cursor)
                ?? GetAngleSnapPoint(cursor)
                ?? GetRawModelPoint(cursor);
        }

        private TransformedSnapPoint GetRawModelPoint(Vector3 cursor)
        {
            var matrix = projectionMatrix * worldMatrix;
            var worldPoint = Unproject(cursor);
            return new TransformedSnapPoint(worldPoint.ToPoint(), cursor, SnapPointKind.None);
        }

        private TransformedSnapPoint GetAngleSnapPoint(Vector3 cursor)
        {
            if (inputService.IsDrawing && workspace.SettingsManager.AngleSnap)
            {
                // get distance to last point
                var last = inputService.LastPoint;
                var current = Unproject(cursor).ToPoint();
                var vector = current - last;
                var dist = vector.Length;

                // for each snap angle, find the point `dist` out on the angle vector
                Func<double, Vector> snapVector = rad =>
                {
                    Vector radVector = null;
                    switch (workspace.DrawingPlane)
                    {
                        case DrawingPlane.XY:
                            radVector = new Vector(Math.Cos(rad), Math.Sin(rad), workspace.DrawingPlaneOffset);
                            break;
                        case DrawingPlane.XZ:
                            radVector = new Vector(Math.Cos(rad), workspace.DrawingPlaneOffset, Math.Sin(rad));
                            break;
                        case DrawingPlane.YZ:
                            radVector = new Vector(workspace.DrawingPlaneOffset, Math.Cos(rad), Math.Sin(rad));
                            break;
                        default:
                            Debug.Fail("invalid value for drawing plane");
                            break;
                    }

                    return radVector.Normalize() * dist;
                };

                var points = from sa in workspace.SettingsManager.SnapAngles
                             let rad = sa * MathHelper.DegreesToRadians
                             let radVector = snapVector(rad)
                             let snapPoint = (last + radVector).ToPoint()
                             let di = (cursor - Project(snapPoint.ToVector3())).Length()
                             where di <= workspace.SettingsManager.SnapAngleDistance
                             orderby di
                             select new TransformedSnapPoint(snapPoint, Project(snapPoint.ToVector3()), SnapPointKind.None);

                // return the closest one
                return points.FirstOrDefault();
            }

            return null;
        }

        private TransformedSnapPoint GetOrthoPoint(Vector3 cursor)
        {
            if (inputService.IsDrawing && workspace.SettingsManager.Ortho)
            {
                // if both are on the drawing plane
                var last = inputService.LastPoint;
                var current = Unproject(cursor).ToPoint();
                var delta = current - last;
                var offset = workspace.DrawingPlaneOffset;
                Point world;
                switch (workspace.DrawingPlane)
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

        private TransformedSnapPoint ActiveEntitySnapPoints(Vector3 cursor)
        {
            if (workspace.SettingsManager.PointSnap && inputService.DesiredInputType == InputType.Point)
            {
                var maxDistSq = (float)(workspace.SettingsManager.SnapPointDistance * workspace.SettingsManager.SnapPointDistance);
                var points = from sp in snapPoints
                             let dist = (cursor - sp.ControlPoint).LengthSquared()
                             where dist <= maxDistSq
                             orderby dist
                             select sp;
                return points.FirstOrDefault();
            }

            return null;
        }

        #endregion

        #region Mouse functions

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var cursor = e.GetPosition(this);
            var cursorVector = cursor.ToVector3();
            var sp = GetActiveModelPoint(cursorVector);
            Entity ent = null;
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    switch (inputService.DesiredInputType)
                    {
                        case InputType.Point:
                            inputService.PushValue(sp.WorldPoint);
                            break;
                        case InputType.Entity:
                            ent = GetHitEntity(cursor);
                            if (ent != null)
                            {
                                inputService.PushValue(ent);
                            }

                            break;
                        case InputType.Entities:
                            if (selecting)
                            {
                                // finish selection
                                var rect = new System.Windows.Rect(
                                    new System.Windows.Point(
                                        Math.Min(firstSelectionPoint.X, currentSelectionPoint.X),
                                        Math.Min(firstSelectionPoint.Y, currentSelectionPoint.Y)),
                                    new System.Windows.Size(
                                        Math.Abs(firstSelectionPoint.X - currentSelectionPoint.X),
                                        Math.Abs(firstSelectionPoint.Y - currentSelectionPoint.Y)));
                                var entities = GetContainedEntities(rect, currentSelectionPoint.X < firstSelectionPoint.X);
                                selecting = false;
                                inputService.PushValue(entities);
                                ForceRender();
                            }
                            else
                            {
                                // start selection
                                ent = GetHitEntity(cursor);
                                if (ent != null)
                                {
                                    inputService.PushValue(ent);
                                }
                                else
                                {
                                    selecting = true;
                                    firstSelectionPoint = cursor;
                                }
                            }

                            break;
                    }
                    break;
                case MouseButton.Middle:
                    panning = true;
                    lastPanPoint = cursor;
                    break;
                case MouseButton.Right:
                    inputService.PushValue(null);
                    break;
            }

            GenerateRubberBandLines(sp.WorldPoint);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
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

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            bool force = false;
            var cursor = e.GetPosition(this);
            var delta = lastPanPoint - cursor;
            if (panning)
            {
                var scale = view.ViewWidth / this.ActualWidth;
                var dx = view.BottomLeft.X + delta.X * scale;
                var dy = view.BottomLeft.Y - delta.Y * scale;
                view.UpdateView(bottomLeft: new Point(dx, dy, view.BottomLeft.Z));
                lastPanPoint = cursor;
                firstSelectionPoint -= delta;
                force = true;
            }

            if (selecting)
            {
                currentSelectionPoint = cursor;
                force = true;
            }

            if (force)
            {
                ForceRender();
            }

            if (inputService.DesiredInputType == InputType.Point)
            {
                var sp = GetActiveModelPoint(cursor.ToVector3());
                GenerateRubberBandLines(sp.WorldPoint);
                DrawSnapPoint(sp);
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // scale everything
            var scale = 1.25f;
            if (e.Delta > 0.0f) scale = 0.8f; // 1.0f / 1.25f

            // center zoom operation on mouse
            var cursorPoint = e.GetPosition(this);
            var controlPoint = new Point(cursorPoint);
            var cursorPos = Unproject(controlPoint.ToVector3()).ToPoint();
            var botLeft = view.BottomLeft;

            // find relative scales
            var viewHeight = this.ActualHeight / this.ActualWidth * view.ViewWidth;
            var relHoriz = controlPoint.X / this.ActualWidth;
            var relVert = controlPoint.Y / this.ActualHeight;
            var viewWidthDelta = view.ViewWidth * (scale - 1.0);
            var viewHeightDelta = viewHeight * (scale - 1.0);

            // set values
            view.UpdateView(viewWidth: view.ViewWidth * scale, bottomLeft: (botLeft - new Vector(viewWidthDelta * relHoriz, viewHeightDelta * relVert, 0.0)).ToPoint());
            var cursor = GetActiveModelPoint(e.GetPosition(this).ToVector3());
            DrawSnapPoint(cursor);
            GenerateRubberBandLines(cursor.WorldPoint);

            ForceRender();
        }

        #endregion

        #region Misc functions

        private IEnumerable<Entity> GetContainedEntities(Rect selectionRect, bool includePartial)
        {
            var start = DateTime.UtcNow;
            var entities = new List<Entity>();
            foreach (var entityId in lines.Keys)
            {
                // project all 8 bounding box coordinates to the screen and create a bigger bounding rectangle
                var entity = lines[entityId].Entity;
                var box = entity.BoundingBox;
                var projectedBox = new[]
                {
                    Project(box.MinimumPoint.ToVector3()),
                    Project((box.MinimumPoint + new Vector(box.Size.X, 0.0, 0.0)).ToPoint().ToVector3()),
                    Project((box.MinimumPoint + new Vector(0.0, box.Size.Y, 0.0)).ToPoint().ToVector3()),
                    Project((box.MinimumPoint + new Vector(box.Size.X, box.Size.Y, 0.0)).ToPoint().ToVector3()),
                    Project((box.MinimumPoint + new Vector(0.0, 0.0, box.Size.Z)).ToPoint().ToVector3()),
                    Project((box.MinimumPoint + new Vector(box.Size.X, 0.0, box.Size.Z)).ToPoint().ToVector3()),
                    Project((box.MinimumPoint + new Vector(0.0, box.Size.Y, box.Size.Z)).ToPoint().ToVector3()),
                    Project((box.MinimumPoint + new Vector(box.Size.X, box.Size.Y, box.Size.Z)).ToPoint().ToVector3())
                };
                var screenRect = GetBoundingRectangle(projectedBox);
                bool isContained = false;

                if (selectionRect.Contains(screenRect))
                {
                    // regardless of selection type, this will match
                    isContained = true;
                }
                else
                {
                    // project all line segments to screen space
                    var segmentCollection = lines[entityId].LineSegments;
                    var projectedPoints = from prim in segmentCollection
                                          let points = prim.Item2
                                          select points.Select(p => Project(p).ToWindowsPoint());
                    var flattenedPoints = projectedPoints.SelectMany(x => x);

                    if (includePartial)
                    {
                        // if any point is in the rectangle OR any segment intersects a rectangle edge
                        if (flattenedPoints.Any(p => selectionRect.Contains(p)))
                        {
                            isContained = true;
                        }
                        else
                        {
                            var selectionLines = new[]
                                {
                                    new PrimitiveLine(new Point(selectionRect.TopLeft), new Point(selectionRect.TopRight)),
                                    new PrimitiveLine(new Point(selectionRect.TopRight), new Point(selectionRect.BottomRight)),
                                    new PrimitiveLine(new Point(selectionRect.BottomRight), new Point(selectionRect.BottomLeft)),
                                    new PrimitiveLine(new Point(selectionRect.BottomLeft), new Point(selectionRect.TopRight))
                                };
                            if (projectedPoints
                                .Select(p => p.Zip(p.Skip(1), (a, b) => new PrimitiveLine(new Point(a), new Point(b))))
                                .SelectMany(x => x).Any(l => selectionLines.Any(s => s.IntersectsInXY(l))))
                            {
                                isContained = true;
                            }
                        }
                    }
                    else
                    {
                        // all points must be in rectangle
                        if (flattenedPoints.All(p => selectionRect.Contains(p)))
                        {
                            isContained = true;
                        }
                    }
                }

                if (isContained)
                {
                    entities.Add(entity);
                }
            }

            var ellapsed = (DateTime.UtcNow - start).TotalMilliseconds;
            return entities;
        }

        private Entity GetHitEntity(System.Windows.Point cursor)
        {
            var start = DateTime.UtcNow;
            var selectionDist = workspace.SettingsManager.EntitySelectionRadius;
            uint hitEntity = 0;
            foreach (var entityId in lines.Keys)
            {
                var segments = lines[entityId];
                for (int i = 0; i < segments.LineSegments.Length && hitEntity == 0; i++)
                {
                    var points = segments.LineSegments[i].Item2;
                    for (int j = 0; j < points.Length - 1; j++)
                    {
                        // translate line segment to screen coordinates
                        var p1 = Project(points[j]);
                        var p2 = Project(points[j + 1]);
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
                                // TODO: how to find closest instead of first
                                hitEntity = entityId;
                                break;
                            }
                        }
                    }
                }
            }

            return document.Layers.Values.SelectMany(l => l.Entities).FirstOrDefault(en => en.Id == hitEntity);
        }

        private void ForceRender()
        {
            this.content.ForceRendering();
        }

        private Vector3 Project(Vector3 point)
        {
            var screenPoint = Vector3.Project(
                point,
                Device.Viewport.X,
                Device.Viewport.Y,
                Device.Viewport.Width,
                Device.Viewport.Height,
                Device.Viewport.MinZ,
                Device.Viewport.MaxZ,
                projectionViewWorldMatrix);
            return screenPoint;
        }

        private Vector3 Unproject(Vector3 point)
        {
            // not using view matrix because that scales z at 0 for display
            var worldPoint = Vector3.Unproject(
                point,
                Device.Viewport.X,
                Device.Viewport.Y,
                Device.Viewport.Width,
                Device.Viewport.Height,
                Device.Viewport.MinZ,
                Device.Viewport.MaxZ,
                projectionWorldMatrix);
            return worldPoint;
        }

        private static Rect GetBoundingRectangle(params Vector3[] points)
        {
            Debug.Assert(points.Length > 0);
            float minX, minY, maxX, maxY;
            minX = maxX = points[0].X;
            minY = maxY = points[0].Y;
            for (int i = 1; i < points.Length; i++)
            {
                minX = Math.Min(minX, points[i].X);
                maxX = Math.Max(maxX, points[i].X);
                minY = Math.Min(minY, points[i].Y);
                maxY = Math.Max(maxY, points[i].Y);
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        #endregion

    }
}
