using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BCad.Entities;
using BCad.EventArguments;
using BCad.Extensions;
using BCad.Helpers;
using BCad.Primitives;
using BCad.Services;
using BCad.SnapPoints;
using SlimDX;
using SlimDX.Direct3D9;
using Input = System.Windows.Input;
using Media = System.Windows.Media;
using Media3D = System.Windows.Media.Media3D;

namespace BCad.UI.Views
{
    /// <summary>
    /// Interaction logic for Direct3DViewControl.xaml
    /// </summary>
    [ExportViewControl("Direct3D")]
    public partial class Direct3DViewControl : ViewControl, IRenderEngine
    {

        #region IDisplayPrimitive

        private interface IDisplayPrimitive : IDisposable
        {
            void RenderNormal(Device device, Matrix projection, Matrix view);
            void RenderSelected(Device device, Matrix projection, Matrix view);
            Tuple<double, Point> ClosestPointToCursor(Point cursorPoint, Func<Vector3, Vector3> project);
            bool IsContained(Rect selectionRectangle, bool includePartial, Func<Vector3, Vector3> project);
        }

        private class DisplayPrimitiveLines : IDisplayPrimitive
        {
            public Color4 Color { get; private set; }
            public Vector3[] LineVerticies { get; private set; }
            private SlimDX.Direct3D9.Line solidLine = null;
            private SlimDX.Direct3D9.Line dashedLine = null;

            public DisplayPrimitiveLines(Color4 color, Vector3[] lineVerticies, SlimDX.Direct3D9.Line solidLine, SlimDX.Direct3D9.Line dashedLine)
            {
                Debug.Assert(lineVerticies.Length > 1);

                this.Color = color;
                this.LineVerticies = lineVerticies;
                this.solidLine = solidLine;
                this.dashedLine = dashedLine;
            }

            public void RenderNormal(Device device, Matrix projection, Matrix view)
            {
                // TODO: draw via user primitives
                solidLine.DrawTransformed(LineVerticies, projection * view, Color);
            }

            public void RenderSelected(Device device, Matrix projection, Matrix view)
            {
                dashedLine.DrawTransformed(LineVerticies, projection * view, Color);
            }

            public Tuple<double, Point> ClosestPointToCursor(Point cursorPoint, Func<Vector3, Vector3> project)
            {
                return cursorPoint.ClosestPoint(LineVerticies, project);
            }

            public bool IsContained(Rect selectionRectangle, bool includePartial, Func<Vector3, Vector3> project)
            {
                return selectionRectangle.Contains(LineVerticies, project, includePartial);
            }

            public void Dispose()
            {
            }
        }

        private class DisplayPrimitiveMesh : IDisplayPrimitive
        {
            public Material Material { get; private set; }
            public Mesh Mesh { get; private set; }
            public Matrix WorldMatrix { get; private set; }
            private Vector3[] boundingCorners = null;
            private Vector3[] outlineCorners = null;
            private PixelShader NormalShader = null;
            private PixelShader SelectedShader = null;

            public DisplayPrimitiveMesh(Mesh mesh, Color4 color, Matrix worldMatrix, PixelShader normalShader, PixelShader selectedShader)
            {
                this.Mesh = mesh;
                this.Material = new Material()
                {
                    Diffuse = color,
                    Emissive = color
                };
                this.WorldMatrix = worldMatrix;
                this.NormalShader = normalShader;
                this.SelectedShader = selectedShader;

                var boundingBox = Mesh.GetBoundingBox();
                this.boundingCorners = boundingBox.GetCorners();
                for (int i = 0; i < boundingCorners.Length; i++)
                {
                    this.boundingCorners[i] = Vector3.Transform(this.boundingCorners[i], worldMatrix).ToVector3();
                }

                this.outlineCorners = this.boundingCorners.Concat(new[] { this.boundingCorners[0] }).ToArray();
            }

            public void RenderNormal(Device device, Matrix projection, Matrix view)
            {
                Render(device, NormalShader);
            }

            public void RenderSelected(Device device, Matrix projection, Matrix view)
            {
                Render(device, SelectedShader);
            }

            private void Render(Device device, PixelShader shader)
            {
                device.SetTransform(TransformState.World, this.WorldMatrix);
                device.Material = Material;
                device.PixelShader = shader;
                Mesh.DrawSubset(0);
            }

            public Tuple<double, Point> ClosestPointToCursor(Point cursorPoint, Func<Vector3, Vector3> project)
            {
                // if projected extents contains point, use point
                if (outlineCorners.Select(c => project(c)).ConvexHull().Contains(cursorPoint.ToVector3()))
                    return Tuple.Create(0.0, cursorPoint);
                // else, like lines
                return cursorPoint.ClosestPoint(boundingCorners, project);
            }

            public bool IsContained(Rect selectionRectangle, bool includePartial, Func<Vector3, Vector3> project)
            {
                return selectionRectangle.Contains(this.outlineCorners, project, includePartial);
            }

            public void Dispose()
            {
                this.Mesh.Dispose();
            }
        }

        #endregion

        #region TransformedEntity class

        private class TransformedEntity
        {
            public Entity Entity { get; private set; }
            public IDisplayPrimitive[] DisplayPrimitives { get; private set; }

            public TransformedEntity(Entity entity, IEnumerable<IDisplayPrimitive> displayPrimitives)
            {
                this.Entity = entity;
                this.DisplayPrimitives = displayPrimitives.ToArray();
            }

            public bool IsContained(Rect selectionRectangle, bool includePartial, Func<Vector3, Vector3> project)
            {
                if (includePartial)
                    return DisplayPrimitives.Any(p => p.IsContained(selectionRectangle, includePartial, project));
                else
                    return DisplayPrimitives.All(p => p.IsContained(selectionRectangle, includePartial, project));
            }

            public Tuple<double, Point> ClosestPointToCursor(Point cursorPoint, Func<Vector3, Vector3> project)
            {
                return DisplayPrimitives
                    .Select(p => p.ClosestPointToCursor(cursorPoint, project))
                    .Where(p => p != null)
                    .OrderBy(p => p.Item1)
                    .FirstOrDefault();
            }
        }

        #endregion

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
            this.workspace.WorkspaceChanged += WorkspaceChanged;
            this.workspace.SettingsManager.PropertyChanged += SettingsManagerPropertyChanged;
            this.workspace.CommandExecuted += CommandExecuted;
            this.workspace.SelectedEntities.CollectionChanged += SelectedEntitiesCollectionChanged;
            this.inputService.ValueRequested += InputServiceValueRequested;
            this.inputService.ValueReceived += InputServiceValueReceived;

            // load the workspace
            WorkspaceChanged(this.workspace, new WorkspaceChangeEventArgs(true, true));

            // load settings
            foreach (var setting in new[] { Constants.BackgroundColorString })
                SettingsManagerPropertyChanged(this.workspace.SettingsManager, new PropertyChangedEventArgs(setting));

            // prepare the cursor
            UpdateCursor();
            SetCursorVisibility();
            this.Loaded += (_, __) =>
                {
                    foreach (var cursorImage in new[] { pointCursorImage, entityCursorImage })
                    {
                        Canvas.SetLeft(cursorImage, -(int)(cursorImage.ActualWidth / 2.0));
                        Canvas.SetTop(cursorImage, -(int)(cursorImage.ActualHeight / 2.0));
                    }
                };
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
        private object drawingGate = new object();
        private Drawing drawing = null;
        private Device Device { get { return this.content.Device; } }
        private Color4 autoColor = new Color4();
        private Dictionary<uint, TransformedEntity> lines = new Dictionary<uint, TransformedEntity>();
        private IDisplayPrimitive[] rubberBandLines = null;
        private bool panning = false;
        private bool selecting = false;
        private bool debug = false;
        private System.Windows.Point firstSelectionPoint = new System.Windows.Point();
        private System.Windows.Point currentSelectionPoint = new System.Windows.Point();
        private System.Windows.Point lastPanPoint = new System.Windows.Point();
        private bool lastGeneratorNonNull = false;
        private SlimDX.Direct3D9.Line solidLine;
        private SlimDX.Direct3D9.Line dashedLine;
        private ShaderBytecode normalMeshBytecode = null;
        private ShaderBytecode selectedMeshBytecode = null;
        private PixelShader normalPixelShader = null;
        private PixelShader selectedPixelShader = null;

        #endregion

        #region Constants

        private const int FullCircleDrawingSegments = 101;
        private const int LowQualityCircleDrawingSegments = 51;
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
            var cursor = Input.Mouse.GetPosition(this);
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
            if (normalMeshBytecode != null)
                normalMeshBytecode.Dispose();
            if (selectedMeshBytecode != null)
                selectedMeshBytecode.Dispose();
            if (normalPixelShader != null)
                normalPixelShader.Dispose();
            if (selectedPixelShader != null)
                selectedPixelShader.Dispose();

            solidLine = new SlimDX.Direct3D9.Line(Device);
            dashedLine = new SlimDX.Direct3D9.Line(Device)
            {
                Width = 1.0f,
                Pattern = 0xF0F0F0F,
                PatternScale = 1
            };

            // prepare shader bytecode
            normalMeshBytecode = ShaderBytecode.Compile(@"
float4 PShader(float4 color : COLOR0) : SV_Target
{
    return color;
}
", "PShader", "ps_3_0", ShaderFlags.None);

            selectedMeshBytecode = ShaderBytecode.Compile(@"
float4 PShader(float2 position : SV_POSITION, float4 color : COLOR0) : SV_Target
{
    int p = position.x + position.y;
    if (p % 2 == 0)
        return color;
    else
        return float4(0.0f, 0.0f, 0.0f, 0.0f);
}
", "PShader", "ps_3_0", ShaderFlags.None);
            normalPixelShader = new PixelShader(Device, normalMeshBytecode);
            selectedPixelShader = new PixelShader(Device, selectedMeshBytecode);

            Device.SetRenderState(RenderState.Lighting, true);
            Device.SetRenderState(RenderState.AlphaBlendEnable, true);
            Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
            DrawingChanged(drawing);
        }

        public void OnMainLoop(object sender, EventArgs args)
        {
            lock (drawingGate)
            {
                var start = DateTime.UtcNow;

                Device.SetTransform(TransformState.Projection, projectionMatrix);
                Device.SetTransform(TransformState.View, viewMatrix);

                var selected = workspace.SelectedEntities;
                foreach (var entityId in lines.Keys)
                {
                    var ent = lines[entityId];
                    var prims = ent.DisplayPrimitives;
                    var len = prims.Length;
                    for (int i = 0; i < len; i++)
                    {
                        if (selected.ContainsHash(entityId.GetHashCode()))
                        {
                            prims[i].RenderSelected(Device, projectionMatrix, viewMatrix);
                        }
                        else
                        {
                            prims[i].RenderNormal(Device, projectionMatrix, viewMatrix);
                        }
                    }
                }

                if (rubberBandLines != null)
                {
                    for (int i = 0; i < rubberBandLines.Length; i++)
                    {
                        rubberBandLines[i].RenderNormal(Device, projectionMatrix, viewMatrix);
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
                case Constants.BackgroundColorString:
                    var bg = workspace.SettingsManager.BackgroundColor;
                    this.content.ClearColor = bg;
                    var backgroundColor = (bg.R << 16) | (bg.G << 8) | bg.B;
                    var brightness = System.Drawing.Color.FromArgb(backgroundColor).GetBrightness();
                    var color = brightness < 0.67 ? 0xFFFFFF : 0x000000;
                    autoColor = new Color4((0xFF << 24) | color);
                    ForceRender();
                    UpdateCursor();
                    break;
                case Constants.DebugString:
                    debug = workspace.SettingsManager.Debug;
                    UpdateSnapPoints(projectionMatrix);
                    break;
                case Constants.AngleSnapString:
                case Constants.OrthoString:
                case Constants.PointSnapString:
                    redraw = true;
                    break;
                default:
                    break;
            }

            if (redraw)
            {
                var cursor = Input.Mouse.GetPosition(this);
                var sp = GetActiveModelPoint(cursor.ToVector3());
                GenerateRubberBandLines(sp.WorldPoint);
                DrawSnapPoint(sp);
            }
        }

        private void WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            if (e.IsDrawingChange)
                DrawingChanged(workspace.Drawing);
        }

        private void DrawingChanged(Drawing drawing)
        {
            lock (drawingGate)
            {
                this.drawing = drawing;
                var start = DateTime.UtcNow;
                Parallel.ForEach(lines.Values, ent => Parallel.ForEach(ent.DisplayPrimitives, p => p.Dispose()));

                // TODO: diff the drawing and only remove/generate the necessary elements
                lines.Clear();
                foreach (var layer in drawing.Layers.Values.Where(l => l.IsVisible))
                {
                    // TODO: parallelize this.  requires `lines` to be concurrent dictionary
                    foreach (var entity in layer.Entities)
                    {
                        lines[entity.Id] = GenerateEntitySegments(entity, layer.Color);
                    }
                }

                // populate the snap points
                snapPoints = drawing.Layers.Values.SelectMany(l => l.Entities.SelectMany(o => o.GetSnapPoints()))
                    .Select(sp => new TransformedSnapPoint(sp.Point, sp.Point.ToVector3(), sp.Kind)).ToArray();
                
                // ensure they have correct values
                UpdateSnapPoints(projectionMatrix);

                // clear rubber band lines
                rubberBandLines = null;
                var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
                inputService.WriteLineDebug("DrawingChanged in {0} ms", elapsed);
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
            SetCursorVisibility();
        }

        private void InputServiceValueReceived(object sender, ValueReceivedEventArgs e)
        {
            selecting = false;
            ForceRender();
            SetCursorVisibility();
        }

        private void InputServiceValueRequested(object sender, ValueRequestedEventArgs e)
        {
            var cursor = (Vector3)this.Dispatcher.Invoke((Func<Vector3>)(() =>
            {
                snapLayer.Children.Clear();
                return Project(Input.Mouse.GetPosition(this).ToVector3());
            }));
            GenerateRubberBandLines(GetActiveModelPoint(cursor).WorldPoint);
            selecting = false;
            ForceRender();
            SetCursorVisibility();
        }

        #endregion

        #region Primitive generator functions

        private Color4 GetDisplayColor(Color layerColor, Color primitiveColor)
        {
            Color4 display;
            if (!primitiveColor.IsAuto)
                display = new Color4(primitiveColor.ToInt());
            else if (!layerColor.IsAuto)
                display = new Color4(layerColor.ToInt());
            else
                display = autoColor;

            return display;
        }

        private void GenerateRubberBandLines(Point worldPoint)
        {
            var generator = inputService.PrimitiveGenerator;
            rubberBandLines = generator == null
                ? null
                : generator(worldPoint).Select(p => GenerateDisplayPrimitive(p, autoColor, false)).ToArray();

            if (generator != null || lastGeneratorNonNull)
            {
                ForceRender();
            }

            lastGeneratorNonNull = generator != null;
        }

        private TransformedEntity GenerateEntitySegments(Entity entity, Color layerColor)
        {
            return new TransformedEntity(entity,
                entity.GetPrimitives().Select(p => GenerateDisplayPrimitive(p, GetDisplayColor(layerColor, p.Color))));
        }

        private IDisplayPrimitive GenerateDisplayPrimitive(IPrimitive primitive, Color4 color, bool highQuality = true)
        {
            IDisplayPrimitive display;
            Vector normal = null, right = null, up = null;
            Media3D.Matrix3D trans;
            switch (primitive.Kind)
            {
                case PrimitiveKind.Text:
                    var text = (PrimitiveText)primitive;
                    var f = System.Drawing.SystemFonts.DefaultFont;
                    var sc = (float)text.Height;
                    var rad = text.Rotation * MathHelper.DegreesToRadians;
                    normal = text.Normal;
                    right = new Vector(Math.Cos(rad), Math.Sin(rad), 0.0).Normalize();
                    up = normal.Cross(right).Normalize();
                    var mesh = Mesh.CreateText(Device, f, text.Value, highQuality ? 0.0f : 0.1f, float.Epsilon);
                    trans = PrimitiveExtensions.FromUnitCircleProjection(normal, right, up, text.Location, sc, sc, sc);
                    display = new DisplayPrimitiveMesh(mesh, color, trans.ToMatrix(), normalPixelShader, selectedPixelShader);
                    break;
                case PrimitiveKind.Line:
                    var line = (PrimitiveLine)primitive;
                    display = new DisplayPrimitiveLines(
                        color,
                        new[] {
                            line.P1.ToVector3(),
                            line.P2.ToVector3()
                        },
                        solidLine,
                        dashedLine);
                    break;
                case PrimitiveKind.Ellipse:
                    var el = (PrimitiveEllipse)primitive;
                    double startAngle = el.StartAngle;
                    double endAngle = el.EndAngle;
                    double radiusX = el.MajorAxis.Length;
                    double radiusY = radiusX * el.MinorAxisRatio;
                    var center = el.Center;
                    normal = el.Normal;
                    right = el.MajorAxis;

                    normal = normal.Normalize();
                    right = right.Normalize();
                    up = normal.Cross(right).Normalize();
                    startAngle *= MathHelper.DegreesToRadians;
                    endAngle *= MathHelper.DegreesToRadians;
                    var coveringAngle = endAngle - startAngle;
                    if (coveringAngle < 0.0) coveringAngle += MathHelper.TwoPI;
                    var fullSegCount = highQuality ? FullCircleDrawingSegments : LowQualityCircleDrawingSegments;
                    var segCount = Math.Max(3, (int)(coveringAngle / MathHelper.TwoPI * (double)fullSegCount));
                    var segments = new Vector3[segCount];
                    var angleDelta = coveringAngle / (double)(segCount - 1);
                    var angle = startAngle;
                    trans = PrimitiveExtensions.FromUnitCircleProjection(normal, right, up, center, radiusX, radiusY, 1.0);
                    var start = DateTime.UtcNow;
                    for (int i = 0; i < segCount; i++, angle += angleDelta)
                    {
                        var result = (new Point(Math.Cos(angle), Math.Sin(angle), 0).ToPoint3D()) * trans;
                        segments[i] = result.ToVector3();
                    }
                    var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
                    display = new DisplayPrimitiveLines(
                        color,
                        segments,
                        solidLine,
                        dashedLine);
                    break;
                default:
                    throw new ArgumentException("primitive.Kind");
            }

            return display;
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
                Dispatcher.BeginInvoke((Action)(() =>
                    {
                        debugLayer.Children.Clear();
                        if (this.debug)
                        {
                            foreach (var sp in snapPoints.Where(s => s.Kind != SnapPointKind.None))
                            {
                                debugLayer.Children.Add(GetSnapIcon(sp, Media.Colors.Cyan));
                            }
                        }
                    }));
            }
            var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
        }

        private Image GetSnapIcon(TransformedSnapPoint snapPoint, Media.Color? color = null)
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

            var geometry = ((Media.GeometryDrawing)SnapPointResources[name]).Clone();
            var scale = workspace.SettingsManager.SnapPointSize;
            geometry.Pen = new Media.Pen(new Media.SolidColorBrush(color ?? workspace.SettingsManager.SnapPointColor), 0.2);
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
                    var drawingPlane = workspace.DrawingPlane;
                    var offset = drawingPlane.Point;
                    if (drawingPlane.Normal == Vector.ZAxis)
                    {
                        radVector = new Vector(Math.Cos(rad), Math.Sin(rad), offset.Z);
                    }
                    else if (drawingPlane.Normal == Vector.YAxis)
                    {
                        radVector = new Vector(Math.Cos(rad), offset.Y, Math.Sin(rad));
                    }
                    else if (drawingPlane.Normal == Vector.XAxis)
                    {
                        radVector = new Vector(offset.X, Math.Cos(rad), Math.Sin(rad));
                    }
                    else
                    {
                        Debug.Fail("invalid value for drawing plane");
                    }

                    return radVector.Normalize() * dist;
                };

                var points = from sa in workspace.SettingsManager.SnapAngles
                             let rad = sa * MathHelper.DegreesToRadians
                             let radVector = snapVector(rad)
                             let snapPoint = last + radVector
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
                var drawingPlane = workspace.DrawingPlane;
                var offset = drawingPlane.Point;
                Point world;

                if (drawingPlane.Normal == Vector.ZAxis)
                {
                    if (offset.Z != last.Z && offset.Z != current.Z)
                        return null;
                    if (Math.Abs(delta.X) > Math.Abs(delta.Y))
                        world = last + new Vector(delta.X, 0.0, 0.0);
                    else
                        world = last + new Vector(0.0, delta.Y, 0.0);
                }
                else if (drawingPlane.Normal == Vector.ZAxis)
                {
                    if (offset.Y != last.Y && offset.Y != current.Y)
                        return null;
                    if (Math.Abs(delta.X) > Math.Abs(delta.Z))
                        world = last + new Vector(delta.X, 0.0, 0.0);
                    else
                        world = last + new Vector(0.0, 0.0, delta.Z);
                }
                else if (drawingPlane.Normal == Vector.ZAxis)
                {
                    if (offset.X != last.X && offset.X != current.X)
                        return null;
                    if (Math.Abs(delta.Y) > Math.Abs(delta.Z))
                        world = last + new Vector(0.0, delta.Y, 0.0);
                    else
                        world = last + new Vector(0.0, 0.0, delta.Z);
                }
                else
                {
                    throw new NotSupportedException("Invalid drawing plane");
                }

                Debug.Assert(world != null, "should have returned null");
                return new TransformedSnapPoint(world, cursor, SnapPointKind.None);
            }

            return null;
        }

        private TransformedSnapPoint ActiveEntitySnapPoints(Vector3 cursor)
        {
            if (workspace.SettingsManager.PointSnap && inputService.AllowedInputTypes.HasFlag(InputType.Point))
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

        private void OnMouseDown(object sender, Input.MouseButtonEventArgs e)
        {
            var cursor = e.GetPosition(this);
            var cursorVector = cursor.ToVector3();
            var sp = GetActiveModelPoint(cursorVector);
            switch (e.ChangedButton)
            {
                case Input.MouseButton.Left:
                    if (inputService.AllowedInputTypes.HasFlag(InputType.Point))
                    {
                        inputService.PushPoint(sp.WorldPoint);
                    }
                    else if (inputService.AllowedInputTypes.HasFlag(InputType.Entity))
                    {
                        var selected = GetHitEntity(cursor);
                        if (selected != null)
                        {
                            inputService.PushEntity(selected);
                        }
                    }
                    else if (inputService.AllowedInputTypes.HasFlag(InputType.Entities))
                    {
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
                            inputService.PushEntities(entities);
                            ForceRender();
                        }
                        else
                        {
                            // start selection
                            var selected = GetHitEntity(cursor);
                            if (selected != null)
                            {
                                inputService.PushEntities(new[] { selected.Entity });
                            }
                            else
                            {
                                selecting = true;
                                firstSelectionPoint = cursor;
                            }
                        }
                    }

                    break;
                case Input.MouseButton.Middle:
                    panning = true;
                    lastPanPoint = cursor;
                    break;
                case Input.MouseButton.Right:
                    inputService.PushNone();
                    break;
            }

            GenerateRubberBandLines(sp.WorldPoint);
        }

        private void OnMouseUp(object sender, Input.MouseButtonEventArgs e)
        {
            var cursor = e.GetPosition(this);
            switch (e.ChangedButton)
            {
                case Input.MouseButton.Middle:
                    panning = false;
                    break;
            }

            var sp = GetActiveModelPoint(cursor.ToVector3());
            GenerateRubberBandLines(sp.WorldPoint);
        }

        private void OnMouseMove(object sender, Input.MouseEventArgs e)
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

            if (inputService.AllowedInputTypes.HasFlag(InputType.Point))
            {
                var sp = GetActiveModelPoint(cursor.ToVector3());
                GenerateRubberBandLines(sp.WorldPoint);
                DrawSnapPoint(sp);
            }

            foreach (var cursorImage in new[] { pointCursorImage, entityCursorImage })
            {
                Canvas.SetLeft(cursorImage, (int)(cursor.X - (cursorImage.ActualWidth / 2.0)));
                Canvas.SetTop(cursorImage, (int)(cursor.Y - (cursorImage.ActualHeight / 2.0)));
            }
        }

        private void OnMouseWheel(object sender, Input.MouseWheelEventArgs e)
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
            view.UpdateView(viewWidth: view.ViewWidth * scale, bottomLeft: botLeft - new Vector(viewWidthDelta * relHoriz, viewHeightDelta * relVert, 0.0));
            var cursor = GetActiveModelPoint(e.GetPosition(this).ToVector3());
            DrawSnapPoint(cursor);
            GenerateRubberBandLines(cursor.WorldPoint);

            ForceRender();
        }

        #endregion

        #region Misc functions

        private void SetCursorVisibility()
        {
            Func<InputType[], System.Windows.Visibility> getVisibility = types =>
                types.Any(t => inputService.AllowedInputTypes.HasFlag(t))
                    ? System.Windows.Visibility.Visible
                    : System.Windows.Visibility.Hidden;

            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                pointCursorImage.Visibility = getVisibility(new[]
                {
                    InputType.Command,
                    InputType.Point
                });
                entityCursorImage.Visibility = getVisibility(new[]
                {
                    InputType.Command,
                    InputType.Entities,
                    InputType.Entity
                });
            }));
        }

        private void UpdateCursor()
        {
            var color = Media.Color.FromRgb((byte)(autoColor.Red * 255), (byte)(autoColor.Green * 255), (byte)(autoColor.Blue * 255));
            var pen = new Media.Pen(new Media.SolidColorBrush(color), 1);

            var cursorSize = (double)workspace.SettingsManager.CursorSize / 2.0 + 0.5;
            pointCursorImage.Source = new Media.DrawingImage(
                new Media.GeometryDrawing()
                {
                    Geometry = new Media.GeometryGroup()
                    {
                        Children = new Media.GeometryCollection(new []
                        {
                            new Media.LineGeometry(new System.Windows.Point(-cursorSize, 0), new System.Windows.Point(cursorSize, 0)),
                            new Media.LineGeometry(new System.Windows.Point(0, -cursorSize), new System.Windows.Point(0, cursorSize))
                        })
                    },
                    Pen = pen
                });

            var entitySize = workspace.SettingsManager.EntitySelectionRadius;
            entityCursorImage.Source = new Media.DrawingImage(
                new Media.GeometryDrawing()
                {
                    Geometry = new Media.GeometryGroup()
                    {
                        Children = new Media.GeometryCollection(new[]
                        {
                            new Media.LineGeometry(new System.Windows.Point(-entitySize, -entitySize), new System.Windows.Point(entitySize, -entitySize)),
                            new Media.LineGeometry(new System.Windows.Point(entitySize, -entitySize), new System.Windows.Point(entitySize, entitySize)),
                            new Media.LineGeometry(new System.Windows.Point(entitySize, entitySize), new System.Windows.Point(-entitySize, entitySize)),
                            new Media.LineGeometry(new System.Windows.Point(-entitySize, entitySize), new System.Windows.Point(-entitySize, -entitySize))
                        })
                    },
                    Pen = pen
                });
        }

        private IEnumerable<Entity> GetContainedEntities(Rect selectionRect, bool includePartial)
        {
            var start = DateTime.UtcNow;
            var entities = new ConcurrentBag<Entity>();
            Parallel.ForEach(lines.Keys, entityId =>
                {
                    var entity = lines[entityId];
                    if (entity.IsContained(selectionRect, includePartial, Project))
                        entities.Add(entity.Entity);
                });

            var ellapsed = (DateTime.UtcNow - start).TotalMilliseconds;
            inputService.WriteLineDebug("GetContainedEntites in {0} ms", ellapsed);
            return entities;
        }

        private SelectedEntity GetHitEntity(System.Windows.Point cursor)
        {
            var start = DateTime.UtcNow;
            var selectionRadius = workspace.SettingsManager.EntitySelectionRadius;
            var selectionRadius2 = selectionRadius * selectionRadius;
            var cursorPoint = new Point(cursor);
            var entities = from entityId in lines.Keys
                           let dist = lines[entityId].ClosestPointToCursor(cursorPoint, Project)
                           where dist != null && dist.Item1 < selectionRadius2
                           orderby dist.Item1
                           select new
                           {
                               EntityId = entityId,
                               Distance = dist.Item1,
                               SelectionPoint = dist.Item2
                           };

            var selected = entities.FirstOrDefault();
            var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
            inputService.WriteLineDebug("GetHitEntity in {0} ms", elapsed);

            if (selected == null)
            {
                return null;
            }
            else
            {
                start = DateTime.UtcNow;
                var entity = drawing.Layers.Values.SelectMany(l => l.Entities).Single(en => en.Id == selected.EntityId);
                var elapsed2 = (DateTime.UtcNow - start).TotalMilliseconds;
                inputService.WriteLineDebug("GetHitEntity(selection) in {0} ms", elapsed2);
                return new SelectedEntity(entity, selected.SelectionPoint);
            }
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

        #endregion

    }
}
