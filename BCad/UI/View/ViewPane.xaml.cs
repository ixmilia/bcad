using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using BCad.Entities;
using BCad.EventArguments;
using BCad.Extensions;
using BCad.Helpers;
using BCad.Primitives;
using BCad.Services;
using BCad.SnapPoints;
using Input = System.Windows.Input;
using Media = System.Windows.Media;
using Shapes = System.Windows.Shapes;

namespace BCad.UI
{
    /// <summary>
    /// Interaction logic for ViewPane.xaml
    /// </summary>
    public partial class ViewPane : UserControl, IViewControl
    {
        private bool panning;
        private bool selecting;
        private bool selectingRectangle;
        private System.Windows.Point lastPanPoint;
        private System.Windows.Point firstSelectionPoint;
        private System.Windows.Point currentSelectionPoint;
        private IRenderer renderer;
        private TaskCompletionSource<SelectionRectangle> selectionDone;
        private Matrix4 windowsTransformationMatrix;
        private Matrix4 unprojectMatrix;
        private IEnumerable<TransformedSnapPoint> snapPoints;
        private DoubleCollection solidLine = new DoubleCollection();
        private DoubleCollection dashedLine = new DoubleCollection(new[] { 4.0, 4.0 });
        private ResourceDictionary resources;

        private Dictionary<SnapPointKind, GeometryDrawing> snapPointGeometry = new Dictionary<SnapPointKind, GeometryDrawing>();
        private Dictionary<SnapPointKind, Image> snapPointImage = new Dictionary<SnapPointKind, Image>();

        private ResourceDictionary SnapPointResources
        {
            get
            {
                if (resources == null)
                {
                    resources = new ResourceDictionary();
                    resources.Source = new Uri("/BCad.Core.UI;component/SnapPointIcons.xaml", UriKind.Relative);
                }

                return resources;
            }
        }

        public BindingClass BindObject { get; private set; }

        [Import]
        public IWorkspace Workspace { get; set; }

        [Import]
        public IInputService InputService { get; set; }

        [Import]
        public IOutputService OutputService { get; set; }

        [ImportMany]
        public IEnumerable<Lazy<IRendererFactory, RenderFactoryMetadata>> RendererFactories { get; set; }

        public ViewPane()
        {
            InitializeComponent();

            var cursors = new[]
                {
                    pointCursorImage,
                    entityCursorImage,
                    textCursorImage
                };
            Loaded += (_, __) =>
                {
                    foreach (var cursorImage in cursors)
                    {
                        Canvas.SetLeft(cursorImage, -(int)(cursorImage.ActualWidth / 2.0));
                        Canvas.SetTop(cursorImage, -(int)(cursorImage.ActualHeight / 2.0));
                    }
                };

            App.Container.SatisfyImports(this);
        }

        [OnImportsSatisfied]
        public void OnImportsSatisfied()
        {
            BindObject = new BindingClass(Workspace);
            DataContext = BindObject;
            Workspace.WorkspaceChanged += Workspace_WorkspaceChanged;
            Workspace.CommandExecuted += Workspace_CommandExecuted;
            Workspace.SettingsManager.PropertyChanged += SettingsManager_PropertyChanged;
            Workspace.SelectedEntities.CollectionChanged += SelectedEntities_CollectionChanged;
            InputService.ValueRequested += InputService_ValueRequested;
            InputService.ValueReceived += InputService_ValueReceived;
            InputService.InputCanceled += InputService_InputCanceled;

            SettingsManager_PropertyChanged(this, new PropertyChangedEventArgs(string.Empty));
            SetCursorVisibility();

            var factory = RendererFactories.FirstOrDefault(f => f.Metadata.FactoryName == Workspace.SettingsManager.RendererId);
            if (factory != null)
            {
                renderer = factory.Value.CreateRenderer(this, Workspace, InputService);
                renderControl.Content = renderer;
            }

            // prepare snap point icons
            foreach (var kind in new[] { SnapPointKind.Center, SnapPointKind.EndPoint, SnapPointKind.MidPoint, SnapPointKind.Quadrant })
            {
                snapPointGeometry[kind] = GetSnapGeometry(kind);
                snapPointImage[kind] = GetSnapIcon(kind);
                snapLayer.Children.Add(snapPointImage[kind]);
            }
        }

        private void SelectedEntities_CollectionChanged(object sender, EventArgs e)
        {
            UpdateHotPoints();
        }

        public int DisplayHeight
        {
            get { return (int)ActualHeight; }
        }

        public int DisplayWidth
        {
            get { return (int)ActualWidth; }
        }

        public Task<SelectionRectangle> GetSelectionRectangle()
        {
            if (selectingRectangle)
                throw new InvalidOperationException("Already selecting a rectangle");
            selectingRectangle = true;
            OutputService.WriteLine("Select first point");
            SetCursorVisibility();
            selectionDone = new TaskCompletionSource<SelectionRectangle>();
            return selectionDone.Task;
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == Constants.BackgroundColorString)
            {
                var autoColor = Workspace.SettingsManager.BackgroundColor.GetAutoContrastingColor().ToMediaColor();
                BindObject.AutoBrush = new SolidColorBrush(Color.FromRgb(autoColor.R, autoColor.G, autoColor.B));
                BindObject.SelectionBrush = new SolidColorBrush(Color.FromArgb(25, autoColor.R, autoColor.G, autoColor.B));
                BindObject.CursorPen = new Pen(new SolidColorBrush(autoColor), 1.0);
            }
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == Constants.CursorSizeString)
            {
                var cursorSize = Workspace.SettingsManager.CursorSize / 2.0 + 0.5;
                BindObject.LeftCursorExtent = new System.Windows.Point(-cursorSize, 0);
                BindObject.RightCursorExtent = new System.Windows.Point(cursorSize, 0);
                BindObject.TopCursorExtent = new System.Windows.Point(0, -cursorSize);
                BindObject.BottomCursorExtent = new System.Windows.Point(0, cursorSize);
            }
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == Constants.EntitySelectionRadiusString)
            {
                var entitySize = Workspace.SettingsManager.EntitySelectionRadius;
                BindObject.EntitySelectionTopLeft = new System.Windows.Point(-entitySize, -entitySize);
                BindObject.EntitySelectionTopRight= new System.Windows.Point(entitySize, -entitySize);
                BindObject.EntitySelectionBottomLeft = new System.Windows.Point(-entitySize, entitySize);
                BindObject.EntitySelectionBottomRight = new System.Windows.Point(entitySize, entitySize);
            }
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == Constants.TextCursorSizeString)
            {
                var textSize = Workspace.SettingsManager.TextCursorSize / 2.0 + 0.5;
                BindObject.TextCursorStart = new System.Windows.Point(0, -textSize);
                BindObject.TextCursorStart = new System.Windows.Point(0, textSize);
            }
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == Constants.HotPointColorString)
            {
                BindObject.HotPointBrush = new SolidColorBrush(Workspace.SettingsManager.HotPointColor.ToMediaColor());
            }
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == Constants.SnapPointColorString)
            {
                BindObject.SnapPointPen = new Pen(new SolidColorBrush(Workspace.SettingsManager.SnapPointColor.ToMediaColor()), 3.0 / Workspace.SettingsManager.SnapPointSize);
            }
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == Constants.SnapPointSizeString)
            {
                BindObject.SnapPointTransform = new ScaleTransform(Workspace.SettingsManager.SnapPointSize, Workspace.SettingsManager.SnapPointSize);
            }
        }

        private async void InputService_ValueReceived(object sender, ValueReceivedEventArgs e)
        {
            selecting = false;
            var point = await GetCursorPointAsync();
            ClearSnapPoints();
            SetCursorVisibility();
            SetSelectionLineVisibility(Visibility.Hidden);
        }

        private void InputService_ValueRequested(object sender, ValueRequestedEventArgs e)
        {
            selecting = false;
            SetCursorVisibility();
            SetSelectionLineVisibility(Visibility.Hidden);
        }

        void InputService_InputCanceled(object sender, EventArgs e)
        {
            if (selecting)
            {
                selecting = false;
                SetCursorVisibility();
                SetSelectionLineVisibility(Visibility.Hidden);
            }
            else
            {
                Workspace.SelectedEntities.Clear();
            }
        }

        private async void Workspace_CommandExecuted(object sender, CommandExecutedEventArgs e)
        {
            selecting = false;
            var point = await GetCursorPointAsync();
            ClearSnapPoints();
            SetCursorVisibility();
            SetSelectionLineVisibility(Visibility.Hidden);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (Workspace != null)
                ViewPortChanged();
        }

        private void Workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            if (e.IsActiveViewPortChange)
            {
                ViewPortChanged();
            }
            if (e.IsDrawingChange)
            {
                DrawingChanged();
                BindObject.Refresh();
            }
        }

        private async void ViewPortChanged()
        {
            windowsTransformationMatrix = Workspace.ActiveViewPort.GetTransformationMatrixWindowsStyle(ActualWidth, ActualHeight);
            unprojectMatrix = windowsTransformationMatrix;
            unprojectMatrix.Invert();
            windowsTransformationMatrix = Matrix4.CreateScale(1, 1, 0) * windowsTransformationMatrix;
            UpdateSnapPoints();
            UpdateHotPoints();
            var point = await GetCursorPointAsync();
        }

        private void DrawingChanged()
        {
            UpdateSnapPoints();
        }

        private void ClearSnapPoints()
        {
            foreach (UIElement child in snapLayer.Children)
            {
                child.Visibility = Visibility.Hidden;
            }
        }

        private void UpdateSnapPoints()
        {
            // populate the snap points
            snapPoints = Workspace.Drawing.GetLayers().SelectMany(l => l.GetEntities().SelectMany(o => o.GetSnapPoints()))
                .Select(sp => new TransformedSnapPoint(sp.Point, Project(sp.Point), sp.Kind));
        }

        private Point Project(Point point)
        {
            return windowsTransformationMatrix.Transform(point);
        }

        private Point Unproject(Point point)
        {
            return unprojectMatrix.Transform(point);
        }

        public Point GetCursorPoint()
        {
            var mouse = Dispatcher.Invoke(() => Input.Mouse.GetPosition(this));
            var model = GetActiveModelPoint(mouse.ToPoint());
            return model.WorldPoint;
        }

        public Task<Point> GetCursorPointAsync()
        {
            return Task.Factory.StartNew<Point>(() => GetCursorPoint());
        }

        private async void OnMouseDown(object sender, Input.MouseButtonEventArgs e)
        {
            var cursor = e.GetPosition(this);
            var sp = await GetActiveModelPointAsync(cursor.ToPoint());
            switch (e.ChangedButton)
            {
                case Input.MouseButton.Left:
                    if ((InputService.AllowedInputTypes & InputType.Point) == InputType.Point)
                    {
                        InputService.PushPoint(sp.WorldPoint);
                    }
                    else if ((InputService.AllowedInputTypes & InputType.Entity) == InputType.Entity)
                    {
                        var selected = GetHitEntity(cursor);
                        if (selected != null)
                        {
                            InputService.PushEntity(selected);
                        }
                    }
                    else if ((InputService.AllowedInputTypes & InputType.Entities) == InputType.Entities || selectingRectangle || !Workspace.IsCommandExecuting)
                    {
                        if (selecting)
                        {
                            // finish selection
                            IEnumerable<Entity> entities = null;
                            if (selectingRectangle)
                            {
                                selectingRectangle = false;
                                var topLeftScreen = new Point(Math.Min(firstSelectionPoint.X, cursor.X), Math.Min(firstSelectionPoint.Y, cursor.Y), 0.0);
                                var bottomRightScreen = new Point(Math.Max(firstSelectionPoint.X, cursor.X), Math.Max(firstSelectionPoint.Y, cursor.Y), 0.0);
                                var selection = new SelectionRectangle(
                                    topLeftScreen,
                                    bottomRightScreen,
                                    Unproject(topLeftScreen),
                                    Unproject(bottomRightScreen));
                                selectionDone.SetResult(selection);
                            }
                            else
                            {
                                var rect = new Rect(
                                new System.Windows.Point(
                                    Math.Min(firstSelectionPoint.X, currentSelectionPoint.X),
                                    Math.Min(firstSelectionPoint.Y, currentSelectionPoint.Y)),
                                new Size(
                                    Math.Abs(firstSelectionPoint.X - currentSelectionPoint.X),
                                    Math.Abs(firstSelectionPoint.Y - currentSelectionPoint.Y)));
                                entities = GetContainedEntities(rect, currentSelectionPoint.X < firstSelectionPoint.X);
                            }

                            selecting = false;
                            SetSelectionLineVisibility(Visibility.Hidden);
                            if (entities != null)
                            {
                                if (!Workspace.IsCommandExecuting)
                                {
                                    Workspace.SelectedEntities.AddRange(entities);
                                }
                                else
                                {
                                    InputService.PushEntities(entities);
                                }
                            }
                        }
                        else
                        {
                            SelectedEntity selected = null;
                            if (selectingRectangle)
                            {
                                OutputService.WriteLine("Select second point");
                            }
                            else
                            {
                                selected = GetHitEntity(cursor);
                            }

                            if (selected != null)
                            {
                                if (!Workspace.IsCommandExecuting)
                                {
                                    Workspace.SelectedEntities.Add(selected.Entity);
                                }
                                else
                                {
                                    InputService.PushEntities(new[] { selected.Entity });
                                }
                            }
                            else
                            {
                                selecting = true;
                                firstSelectionPoint = cursor;
                                currentSelectionPoint = cursor;
                                SetSelectionLineVisibility(Visibility.Visible);
                            }
                        }
                    }
                    else if (InputService.AllowedInputTypes == InputType.None || !Workspace.IsCommandExecuting)
                    {
                        // do hot-point tracking
                        var selected = GetHitEntity(cursor);
                        if (selected != null)
                        {
                            Workspace.SelectedEntities.Add(selected.Entity);
                        }
                    }

                    break;
                case Input.MouseButton.Middle:
                    panning = true;
                    lastPanPoint = cursor;
                    break;
                case Input.MouseButton.Right:
                    InputService.PushNone();
                    break;
            }
        }

        private void OnMouseUp(object sender, Input.MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case Input.MouseButton.Middle:
                    panning = false;
                    break;
            }
        }

        private void OnMouseMove(object sender, Input.MouseEventArgs e)
        {
            if (Workspace == null || InputService == null)
                return;

            var cursor = e.GetPosition(this);
            var delta = lastPanPoint - cursor;
            if (panning)
            {
                var vp = Workspace.ActiveViewPort;
                var scale = vp.ViewHeight / this.ActualHeight;
                var dx = vp.BottomLeft.X + delta.X * scale;
                var dy = vp.BottomLeft.Y - delta.Y * scale;
                Workspace.Update(activeViewPort: vp.Update(bottomLeft: new Point(dx, dy, vp.BottomLeft.Z)));
                lastPanPoint = cursor;
                firstSelectionPoint -= delta;
            }

            if (selecting)
            {
                currentSelectionPoint = cursor;
                UpdateSelectionLines();
            }

            BindObject.CursorScreen = cursor;
            foreach (var cursorImage in new[] { pointCursorImage, entityCursorImage, textCursorImage })
            {
                Canvas.SetLeft(cursorImage, (int)(cursor.X - (cursorImage.ActualWidth / 2.0)));
                Canvas.SetTop(cursorImage, (int)(cursor.Y - (cursorImage.ActualHeight / 2.0)));
            }

            new Task((Action)(() =>
            {
                var snapPoint = GetActiveModelPoint(cursor.ToPoint());
                BindObject.CursorWorld = snapPoint.WorldPoint;
                renderer.UpdateRubberBandLines();
                if ((InputService.AllowedInputTypes & InputType.Point) == InputType.Point)
                    DrawSnapPoint(snapPoint);
            })).Start();
        }

        private void SetSelectionLineVisibility(Visibility vis)
        {
            if (vis == Visibility.Visible)
            {
                UpdateSelectionLines();
            }

            Dispatcher.BeginInvoke((Action)(() =>
            {
                selectionLine1.Visibility = vis;
                selectionLine2.Visibility = vis;
                selectionLine3.Visibility = vis;
                selectionLine4.Visibility = vis;
                selectionRect.Visibility = vis;
            }));
        }

        private void UpdateSelectionLines()
        {
            selectionLine1.X1 = currentSelectionPoint.X;
            selectionLine1.Y1 = currentSelectionPoint.Y;
            selectionLine1.X2 = currentSelectionPoint.X;
            selectionLine1.Y2 = firstSelectionPoint.Y;

            selectionLine2.X1 = currentSelectionPoint.X;
            selectionLine2.Y1 = firstSelectionPoint.Y;
            selectionLine2.X2 = firstSelectionPoint.X;
            selectionLine2.Y2 = firstSelectionPoint.Y;

            selectionLine3.X1 = firstSelectionPoint.X;
            selectionLine3.Y1 = firstSelectionPoint.Y;
            selectionLine3.X2 = firstSelectionPoint.X;
            selectionLine3.Y2 = currentSelectionPoint.Y;

            selectionLine4.X1 = firstSelectionPoint.X;
            selectionLine4.Y1 = currentSelectionPoint.Y;
            selectionLine4.X2 = currentSelectionPoint.X;
            selectionLine4.Y2 = currentSelectionPoint.Y;

            var dash = !selectingRectangle && currentSelectionPoint.X < firstSelectionPoint.X
                ? dashedLine
                : solidLine;
            selectionLine1.StrokeDashArray = dash;
            selectionLine2.StrokeDashArray = dash;
            selectionLine3.StrokeDashArray = dash;
            selectionLine4.StrokeDashArray = dash;

            var left = Math.Min(currentSelectionPoint.X, firstSelectionPoint.X);
            var top = Math.Min(currentSelectionPoint.Y, firstSelectionPoint.Y);
            selectionRect.Width = Math.Abs(currentSelectionPoint.X - firstSelectionPoint.X);
            selectionRect.Height = Math.Abs(currentSelectionPoint.Y - firstSelectionPoint.Y);
            Canvas.SetLeft(selectionRect, left);
            Canvas.SetTop(selectionRect, top);
        }

        private void SetCursorVisibility()
        {
            if (selectingRectangle)
            {
                Dispatcher.Invoke(() =>
                {
                    pointCursorImage.Visibility = Visibility.Visible;
                    entityCursorImage.Visibility = Visibility.Hidden;
                    textCursorImage.Visibility = Visibility.Hidden;
                });
            }
            else
            {
                Func<InputType[], Visibility> getVisibility = types =>
                    types.Any(t => (InputService.AllowedInputTypes & t) == t)
                        ? Visibility.Visible
                        : Visibility.Hidden;

                Dispatcher.Invoke(() =>
                {
                    pointCursorImage.Visibility = getVisibility(new[]
                    {
                        InputType.Command,
                        InputType.Distance,
                        InputType.Point
                    });
                    entityCursorImage.Visibility = getVisibility(new[]
                    {
                        InputType.Command,
                        InputType.Entities,
                        InputType.Entity
                    });
                    textCursorImage.Visibility = getVisibility(new[]
                    {
                        InputType.Text
                    });
                });
            }
        }

        private async void OnMouseWheel(object sender, Input.MouseWheelEventArgs e)
        {
            // scale everything
            var scale = 1.25;
            if (e.Delta > 0) scale = 0.8; // 1.0 / 1.25

            // center zoom operation on mouse
            var cursorPoint = e.GetPosition(this);
            var vp = Workspace.ActiveViewPort;
            var oldHeight = vp.ViewHeight;
            var oldWidth = ActualWidth * oldHeight / ActualHeight;
            var newHeight = oldHeight * scale;
            var newWidth = oldWidth * scale;
            var heightDelta = newHeight - oldHeight;
            var widthDelta = newWidth - oldWidth;

            var relHoriz = cursorPoint.X / ActualWidth;
            var relVert = (ActualHeight - cursorPoint.Y) / ActualHeight;
            var botLeftDelta = new Vector(relHoriz * widthDelta, relVert * heightDelta, 0.0);
            var newVp = vp.Update(
                bottomLeft: (Point)(vp.BottomLeft - botLeftDelta),
                viewHeight: vp.ViewHeight * scale);
            Workspace.Update(activeViewPort: newVp);
            var cursor = await GetActiveModelPointAsync(cursorPoint.ToPoint());
            DrawSnapPoint(cursor);
        }

        private TransformedSnapPoint GetActiveModelPoint(Point cursor)
        {
            return GetActiveSnapPoint(cursor)
                ?? GetOrthoPoint(cursor)
                ?? GetAngleSnapPoint(cursor)
                ?? GetRawModelPoint(cursor);
        }

        private Task<TransformedSnapPoint> GetActiveModelPointAsync(Point cursor)
        {
            return Task.Factory.StartNew<TransformedSnapPoint>(() => GetActiveModelPoint(cursor));
        }

        private TransformedSnapPoint GetActiveSnapPoint(Point cursor)
        {
            if (Workspace.SettingsManager.PointSnap && (InputService.AllowedInputTypes & InputType.Point) == InputType.Point)
            {
                var maxDistSq = Workspace.SettingsManager.SnapPointDistance * Workspace.SettingsManager.SnapPointDistance;
                var points = from sp in snapPoints
                             let dist = (cursor - sp.ControlPoint).LengthSquared
                             where dist <= maxDistSq
                             orderby dist
                             select sp;
                return points.FirstOrDefault();
            }

            return null;
        }

        private TransformedSnapPoint GetOrthoPoint(Point cursor)
        {
            if (Workspace.IsDrawing && Workspace.SettingsManager.Ortho)
            {
                // if both are on the drawing plane
                var last = InputService.LastPoint;
                var current = Unproject(cursor);
                var delta = current - last;
                var drawingPlane = Workspace.DrawingPlane;
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
                else if (drawingPlane.Normal == Vector.YAxis)
                {
                    if (offset.Y != last.Y && offset.Y != current.Y)
                        return null;
                    if (Math.Abs(delta.X) > Math.Abs(delta.Z))
                        world = last + new Vector(delta.X, 0.0, 0.0);
                    else
                        world = last + new Vector(0.0, 0.0, delta.Z);
                }
                else if (drawingPlane.Normal == Vector.XAxis)
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

                return new TransformedSnapPoint(world, cursor, SnapPointKind.None);
            }

            return null;
        }

        private TransformedSnapPoint GetAngleSnapPoint(Point cursor)
        {
            if (Workspace.IsDrawing && Workspace.SettingsManager.AngleSnap)
            {
                // get distance to last point
                var last = InputService.LastPoint;
                var current = Unproject(cursor);
                var vector = current - last;
                var dist = vector.Length;

                // for each snap angle, find the point `dist` out on the angle vector
                Func<double, Vector> snapVector = rad =>
                {
                    Vector radVector = default(Vector);
                    var drawingPlane = Workspace.DrawingPlane;
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

                var points = from sa in Workspace.SettingsManager.SnapAngles
                             let rad = sa * MathHelper.DegreesToRadians
                             let radVector = snapVector(rad)
                             let snapPoint = last + radVector
                             let di = (cursor - Project(snapPoint)).Length
                             where di <= Workspace.SettingsManager.SnapAngleDistance
                             orderby di
                             select new TransformedSnapPoint(snapPoint, Project(snapPoint), SnapPointKind.None);

                // return the closest one
                return points.FirstOrDefault();
            }

            return null;
        }

        private TransformedSnapPoint GetRawModelPoint(Point cursor)
        {
            var world = Unproject(cursor);
            return new TransformedSnapPoint(world, cursor, SnapPointKind.None);
        }

        private IEnumerable<Point> ProjectedChain(Entity entity)
        {
            return entity.GetPrimitives().SelectMany(p => p.GetProjectedVerticies(windowsTransformationMatrix));
        }

        private IEnumerable<Point> ProjectedChain(IPrimitive primitive)
        {
            switch (primitive.Kind)
            {
                case PrimitiveKind.Ellipse:
                    return ((PrimitiveEllipse)primitive).GetProjectedVerticies(windowsTransformationMatrix, 360);
                default:
                    return primitive.GetProjectedVerticies(windowsTransformationMatrix);
            }
        }

        private IEnumerable<Entity> GetContainedEntities(Rect selectionRect, bool includePartial)
        {
            var entities = Workspace.Drawing.GetLayers().Where(l => l.IsVisible).SelectMany(l => l.GetEntities()).Where(e => selectionRect.Contains(ProjectedChain(e), includePartial));
            return entities;
        }

        private SelectedEntity GetHitEntity(System.Windows.Point cursor)
        {
            var screenPoint = cursor.ToPoint();
            var selectionRadius = Workspace.SettingsManager.EntitySelectionRadius;
            var selectionRadius2 = selectionRadius * selectionRadius;
            var entities = from layer in Workspace.Drawing.GetLayers().Where(l => l.IsVisible)
                           from entity in layer.GetEntities()
                           let dist = ClosestPoint(entity, screenPoint)
                           where dist.Item1 < selectionRadius2
                           orderby dist.Item1
                           select new SelectedEntity(entity, dist.Item2);
            var selected = entities.FirstOrDefault();
            return selected;
        }

        private Tuple<double, Point> ClosestPoint(Entity entity, Point screenPoint)
        {
            return entity.GetPrimitives()
                .Select(prim => ClosestPoint(prim, screenPoint))
                .OrderBy(p => p.Item1)
                .FirstOrDefault();
        }

        private Tuple<double, Point> ClosestPoint(IPrimitive primitive, Point screenPoint)
        {
            switch (primitive.Kind)
            {
                case PrimitiveKind.Ellipse:
                    var el = (PrimitiveEllipse)primitive;
                    return ClosestPoint(el.GetProjectedVerticies(windowsTransformationMatrix, 360).ToArray(), screenPoint);
                case PrimitiveKind.Line:
                    var line = (PrimitiveLine)primitive;
                    return ClosestPoint(new[]
                    {
                        windowsTransformationMatrix.Transform(line.P1),
                        windowsTransformationMatrix.Transform(line.P2)
                    }, screenPoint);
                case PrimitiveKind.Point:
                    // the closest point is the only point present
                    var point = (PrimitivePoint)primitive;
                    var displayPoint = windowsTransformationMatrix.Transform(point.Location);
                    var dist = (displayPoint - screenPoint).Length;
                    return Tuple.Create(dist, point.Location);
                case PrimitiveKind.Text:
                    var text = (PrimitiveText)primitive;
                    var rad = text.Rotation * MathHelper.DegreesToRadians;
                    var right = new Vector(Math.Cos(rad), Math.Sin(rad), 0.0).Normalize() * text.Width;
                    var up = text.Normal.Cross(right).Normalize() * text.Height;
                    var borderPoints = new[]
                    {
                        windowsTransformationMatrix.Transform(text.Location),
                        windowsTransformationMatrix.Transform(text.Location + right),
                        windowsTransformationMatrix.Transform(text.Location + up),
                        windowsTransformationMatrix.Transform(text.Location + right + up)
                    };
                    if (borderPoints.ConvexHull().PolygonContains(screenPoint))
                        return Tuple.Create(0.0, screenPoint);
                    return ClosestPoint(borderPoints, screenPoint);
                default:
                    throw new InvalidOperationException();
            }
        }

        private Tuple<double, Point> ClosestPoint(Point[] screenVerticies, Point screenPoint)
        {
            var points = from i in Enumerable.Range(0, screenVerticies.Length - 1)
                         // translate line segment to screen coordinates
                         let p1 = (screenVerticies[i])
                         let p2 = (screenVerticies[i + 1])
                         let segment = new PrimitiveLine(p1, p2)
                         let closest = segment.ClosestPoint(screenPoint)
                         let dist = (closest - screenPoint).LengthSquared
                         orderby dist
                         // simple unproject via interpolation
                         let pct = (closest - p1).Length / (p2 - p1).Length
                         let vec = screenVerticies[i + 1] - screenVerticies[i]
                         let newLen = vec.Length * pct
                         let offset = vec.Normalize() * newLen
                         select Tuple.Create(dist, Unproject(screenVerticies[i] + offset));
            var selected = points.FirstOrDefault();
            return selected;
        }

        private void UpdateHotPoints()
        {
            hotPointLayer.Children.Clear();
            if (Workspace.IsCommandExecuting)
                return;
            foreach (var primitive in Workspace.SelectedEntities.SelectMany(entity => entity.GetPrimitives()))
            {
                switch (primitive.Kind)
                {
                    case PrimitiveKind.Ellipse:
                        var el = (PrimitiveEllipse)primitive;
                        AddHotPointIcon(el.Center);
                        if (el.IsClosed)
                        {
                            AddHotPointIcon(el.GetPoint(0.0));
                            AddHotPointIcon(el.GetPoint(90.0));
                            AddHotPointIcon(el.GetPoint(180.0));
                            AddHotPointIcon(el.GetPoint(270.0));
                        }
                        else
                        {
                            AddHotPointIcon(el.GetStartPoint());
                            AddHotPointIcon(el.GetMidPoint());
                            AddHotPointIcon(el.GetEndPoint());
                        }
                        break;
                    case PrimitiveKind.Line:
                        var line = (PrimitiveLine)primitive;
                        AddHotPointIcon(line.P1);
                        AddHotPointIcon((line.P1 + line.P2) * 0.5);
                        AddHotPointIcon(line.P2);
                        break;
                    case PrimitiveKind.Point:
                        var point = (PrimitivePoint)primitive;
                        AddHotPointIcon(point.Location);
                        break;
                    case PrimitiveKind.Text:
                        var text = (PrimitiveText)primitive;
                        AddHotPointIcon(text.Location);
                        break;
                }
            }
        }

        private void AddHotPointIcon(Point location)
        {
            var screen = Project(location);
            var size = Workspace.SettingsManager.EntitySelectionRadius;
            var a = new Point(screen.X - size, screen.Y + size, 0.0); // top left
            var b = new Point(screen.X + size, screen.Y + size, 0.0); // top right
            var c = new Point(screen.X + size, screen.Y - size, 0.0); // bottom right
            var d = new Point(screen.X - size, screen.Y - size, 0.0); // bottom left
            AddHotPointLine(a, b);
            AddHotPointLine(b, c);
            AddHotPointLine(c, d);
            AddHotPointLine(d, a);
        }

        private void AddHotPointLine(Point start, Point end)
        {
            var line = new Shapes.Line() { X1 = start.X, Y1 = start.Y, X2 = end.X, Y2 = end.Y, StrokeThickness = 2 };
            SetAutoBinding(line, Shapes.Line.StrokeProperty, "HotPointBrush");
            hotPointLayer.Children.Add(line);
        }

        private void SetAutoBinding(DependencyObject element, DependencyProperty property, string path)
        {
            var binding = new Binding() { Path = new PropertyPath(path) };
            binding.Source = BindObject;
            BindingOperations.SetBinding(element, property, binding);
        }

        private void DrawSnapPoint(TransformedSnapPoint snapPoint)
        {
            Dispatcher.BeginInvoke((Action)(() =>
                {
                    ClearSnapPoints();
                    var dist = (snapPoint.ControlPoint - Input.Mouse.GetPosition(this).ToPoint()).Length;
                    if (dist <= Workspace.SettingsManager.SnapPointDistance && snapPoint.Kind != SnapPointKind.None)
                    {
                        var geometry = snapPointGeometry[snapPoint.Kind];
                        var icon = snapPointImage[snapPoint.Kind];
                        var scale = Workspace.SettingsManager.SnapPointSize;
                        Canvas.SetLeft(icon, snapPoint.ControlPoint.X - geometry.Bounds.Width * scale / 2.0);
                        Canvas.SetTop(icon, snapPoint.ControlPoint.Y - geometry.Bounds.Height * scale / 2.0);
                        icon.Visibility = System.Windows.Visibility.Visible;
                    }
                }));
        }

        private GeometryDrawing GetSnapGeometry(SnapPointKind kind)
        {
            string name;
            switch (kind)
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

            return (GeometryDrawing)SnapPointResources[name];
        }

        private Image GetSnapIcon(SnapPointKind kind)
        {
            var geometry = snapPointGeometry[kind];
            SetAutoBinding(geometry, GeometryDrawing.PenProperty, "SnapPointPen");
            var di = new DrawingImage(geometry);
            var icon = new Image();
            icon.Source = di;
            icon.Stretch = Media.Stretch.None;
            SetAutoBinding(icon, FrameworkElement.LayoutTransformProperty, "SnapPointTransform");
            icon.Visibility = Visibility.Hidden;
            return icon;
        }
    }
}
