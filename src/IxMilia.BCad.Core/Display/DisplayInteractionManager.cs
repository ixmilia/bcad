using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using IxMilia.BCad.Collections;
using IxMilia.BCad.Commands;
using IxMilia.BCad.Entities;
using IxMilia.BCad.EventArguments;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.Services;
using IxMilia.BCad.SnapPoints;

namespace IxMilia.BCad.Display
{
    public partial class DisplayInteractionManager : IViewControl
    {
        private IWorkspace _workspace;
        private bool panning;
        private bool selecting;
        private bool selectingRectangle;
        private Point lastPanPoint;
        private Point firstSelectionPoint;
        private Point currentSelectionPoint;
        private Point lastPointerPosition;
        private TaskCompletionSource<SelectionRectangle?> selectionDone;

        private Matrix4 transformationMatrix;
        private Matrix4 unprojectMatrix;
        private QuadTree<TransformedSnapPoint> snapPointsQuadTree;
        private CancellationTokenSource updateSnapPointsCancellationTokenSource = new CancellationTokenSource();
        private Task updateSnapPointsTask = new Task(() => { });
        private long lastDrawnSnapPointId;
        private long drawSnapPointId = 1;
        private object drawSnapPointIdGate = new object();
        private object lastDrawnSnapPointIdGate = new object();
        private bool lastRubberBandUpdateHadContent;
        private RubberBandGenerator rubberBandGenerator;
        private PanCommand panCommand;

        public ProjectionStyle UIProjectionStyle { get; set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public double ZoomScale { get; set; } = 1.25;

        public event EventHandler<Point> CursorWorldLocationUpdated;
        public event EventHandler<IEnumerable<Point>> HotPointsUpdated;
        public event EventHandler<IEnumerable<IPrimitive>> RubberBandPrimitivesChanged;
        public event EventHandler<SelectionState?> SelectionRectangleUpdated;
        public event EventHandler<CursorState> CursorStateUpdated;
        public event EventHandler<TransformedSnapPoint?> CurrentSnapPointUpdated;

        public DisplayInteractionManager(IWorkspace workspace, ProjectionStyle uiProjectionStyle)
        {
            _workspace = workspace;
            UIProjectionStyle = uiProjectionStyle;
            rubberBandGenerator = _workspace.RubberBandGenerator;
            panCommand = (PanCommand)_workspace.GetCommand("View.Pan").Item1;

            _workspace.WorkspaceChanged += Workspace_WorkspaceChanged;
            _workspace.CommandExecuted += Workspace_CommandExecuted;
            _workspace.RubberBandGeneratorChanged += Workspace_RubberBandGeneratorChanged;
            _workspace.SelectedEntities.CollectionChanged += SelectedEntities_CollectionChanged;
            _workspace.InputService.ValueRequested += InputService_ValueRequested;
            _workspace.InputService.ValueReceived += InputService_ValueReceived;
            _workspace.InputService.InputCanceled += InputService_InputCanceled;
            _workspace.Update(viewControl: this, isDirty: _workspace.IsDirty);

            SetCursorVisibility();
        }

        public void Resize(double width, double height)
        {
            Width = width;
            Height = height;
            ViewPortChanged();
        }

        private void SelectedEntities_CollectionChanged(object sender, EventArgs e)
        {
            UpdateHotPoints();
        }

        public int DisplayHeight => (int)Height;

        public int DisplayWidth => (int)Width;

        public Task<SelectionRectangle?> GetSelectionRectangle()
        {
            if (selectingRectangle)
                throw new InvalidOperationException("Already selecting a rectangle");
            selectingRectangle = true;
            _workspace.OutputService.WriteLine("Select first point");
            SetCursorVisibility();
            selectionDone = new TaskCompletionSource<SelectionRectangle?>();
            return selectionDone.Task;
        }

        private void InputService_ValueReceived(object sender, ValueReceivedEventArgs e)
        {
            selecting = false;
            SetCursorVisibility();
            UpdateSelectionRectangle(null);
        }

        private void InputService_ValueRequested(object sender, ValueRequestedEventArgs e)
        {
            selecting = false;
            SetCursorVisibility();
            UpdateSelectionRectangle(null);
        }

        void InputService_InputCanceled(object sender, EventArgs e)
        {
            if (selecting)
            {
                selecting = false;
                SetCursorVisibility();
                UpdateSelectionRectangle(null);
            }
            else
            {
                _workspace.SelectedEntities.Clear();
            }
        }

        private void Workspace_CommandExecuted(object sender, CadCommandExecutedEventArgs e)
        {
            selecting = false;
            SetCursorVisibility();
            UpdateSelectionRectangle(null);
        }

        private void Workspace_RubberBandGeneratorChanged(object sender, EventArgs e)
        {
            rubberBandGenerator = _workspace.RubberBandGenerator;
            UpdateRubberBandLines(unprojectMatrix.Transform(lastPointerPosition));
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
            }
        }

        private void ViewPortChanged()
        {
            var transform = _workspace.ActiveViewPort.GetProjectionMatrix(Width, Height, UIProjectionStyle);
            var inverse = transform.Inverse();
            transform = Matrix4.CreateScale(1.0, 1.0, 0.0) * transform;
            UpdateSnapPoints(transform);
            unprojectMatrix = inverse;
            transformationMatrix = transform;
        }

        private void UpdateRubberBandLines(Point cursorWorldPoint)
        {
            var generator = rubberBandGenerator;
            var primitives = generator == null
                ? new IPrimitive[0]
                : generator.Invoke(cursorWorldPoint);
            if (generator != null || lastRubberBandUpdateHadContent)
            {
                RubberBandPrimitivesChanged?.Invoke(this, primitives);
            }

            lastRubberBandUpdateHadContent = generator != null;
        }

        private void DrawingChanged()
        {
            UpdateSnapPoints(transformationMatrix);
        }

        private Task UpdateSnapPoints(Matrix4 transform)
        {
            if (Width == 0.0 || Height == 0.0)
            {
                // if there's no viewport, there's nothing to do
                return Task.FromResult<object>(null);
            }

            var oldTokenSource = updateSnapPointsCancellationTokenSource;
            updateSnapPointsCancellationTokenSource = new CancellationTokenSource();
            oldTokenSource.Cancel();
            var token = updateSnapPointsCancellationTokenSource.Token;

            updateSnapPointsTask = Task.Run(() =>
            {
                snapPointsQuadTree = _workspace.Drawing.GetSnapPoints(transform, Width, Height, token);
            });

            return updateSnapPointsTask;
        }

        private Point Project(Point point)
        {
            return transformationMatrix.Transform(point);
        }

        private Point Unproject(Point point)
        {
            return unprojectMatrix.Transform(point);
        }

        public async Task<Point> GetCursorPoint(CancellationToken cancellationToken)
        {
            var mouse = lastPointerPosition;
            var model = await GetActiveModelPointNowAsync(mouse, cancellationToken).ConfigureAwait(false);
            return model.WorldPoint;
        }

        public async Task MouseDown(Point position, MouseButton button)
        {
            var sp = await GetActiveModelPointNowAsync(position, CancellationToken.None);

            if (button == MouseButton.Left)
            {
                if (panCommand.IsPanning == true)
                {
                    panning = true;
                    lastPanPoint = position;
                }
                else if ((_workspace.InputService.AllowedInputTypes & InputType.Point) == InputType.Point)
                {
                    _workspace.InputService.PushPoint(sp.WorldPoint);
                }
                else if ((_workspace.InputService.AllowedInputTypes & InputType.Entity) == InputType.Entity)
                {
                    var selected = GetHitEntity(position);
                    if (selected != null)
                    {
                        _workspace.InputService.PushEntity(selected);
                    }
                }
                else if ((_workspace.InputService.AllowedInputTypes & InputType.Entities) == InputType.Entities || selectingRectangle || !_workspace.IsCommandExecuting)
                {
                    if (selecting)
                    {
                        // finish selection
                        IEnumerable<Entity> entities = null;
                        if (selectingRectangle)
                        {
                            selectingRectangle = false;
                            SetCursorVisibility();
                            var topLeftScreen = new Point(Math.Min(firstSelectionPoint.X, sp.ControlPoint.X), Math.Min(firstSelectionPoint.Y, sp.ControlPoint.Y), 0.0);
                            var bottomRightScreen = new Point(Math.Max(firstSelectionPoint.X, sp.ControlPoint.X), Math.Max(firstSelectionPoint.Y, sp.ControlPoint.Y), 0.0);
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
                                Math.Min(firstSelectionPoint.X, currentSelectionPoint.X),
                                Math.Min(firstSelectionPoint.Y, currentSelectionPoint.Y),
                                Math.Abs(firstSelectionPoint.X - currentSelectionPoint.X),
                                Math.Abs(firstSelectionPoint.Y - currentSelectionPoint.Y));
                            entities = GetContainedEntities(rect, currentSelectionPoint.X < firstSelectionPoint.X);
                        }

                        selecting = false;
                        UpdateSelectionRectangle(null);
                        if (entities != null)
                        {
                            if (!_workspace.IsCommandExecuting)
                            {
                                _workspace.SelectedEntities.AddRange(entities);
                            }
                            else
                            {
                                _workspace.InputService.PushEntities(entities);
                            }
                        }
                    }
                    else
                    {
                        SelectedEntity selected = null;
                        if (selectingRectangle)
                        {
                            _workspace.OutputService.WriteLine("Select second point");
                        }
                        else
                        {
                            selected = GetHitEntity(position);
                        }

                        if (selected != null)
                        {
                            if (!_workspace.IsCommandExecuting)
                            {
                                _workspace.SelectedEntities.Add(selected.Entity);
                            }
                            else
                            {
                                _workspace.InputService.PushEntities(new[] { selected.Entity });
                            }
                        }
                        else
                        {
                            selecting = true;
                            firstSelectionPoint = sp.ControlPoint;
                            currentSelectionPoint = position;
                        }
                    }
                }
                else if (_workspace.InputService.AllowedInputTypes == InputType.None || !_workspace.IsCommandExecuting)
                {
                    // do hot-point tracking
                    var selected = GetHitEntity(position);
                    if (selected != null)
                    {
                        _workspace.SelectedEntities.Add(selected.Entity);
                    }
                }
            }
            else if (button == MouseButton.Middle)
            {
                panning = true;
                lastPanPoint = position;
            }
            else if (button == MouseButton.Right)
            {
                _workspace.InputService.PushNone();
            }
        }

        public void MouseUp(Point position, MouseButton button)
        {
            if ((button == MouseButton.Left && panCommand.IsPanning) ||
                button == MouseButton.Middle)
            {
                panning = false;
            }
        }

        public void MouseMove(Point position)
        {
            if (_workspace == null || _workspace.InputService == null)
                return;

            lastPointerPosition = position;
            var delta = lastPanPoint - position;
            if (panning)
            {
                var vp = _workspace.ActiveViewPort;
                var scale = vp.ViewHeight / Height;
                var dx = vp.BottomLeft.X + delta.X * scale;
                var dy = vp.BottomLeft.Y - delta.Y * scale;
                _workspace.Update(activeViewPort: vp.Update(bottomLeft: new Point(dx, dy, vp.BottomLeft.Z)));
                lastPanPoint = position;
                firstSelectionPoint -= delta;
            }

            if (selecting)
            {
                var left = Math.Min(position.X, firstSelectionPoint.X);
                var top = Math.Min(position.Y, firstSelectionPoint.Y);
                var width = Math.Abs(position.X - firstSelectionPoint.X);
                var height = Math.Abs(position.Y - firstSelectionPoint.Y);
                var rect = new Rect(left, top, width, height);
                var mode = position.X < firstSelectionPoint.X
                    ? SelectionMode.PartialEntity
                    : SelectionMode.WholeEntity;
                currentSelectionPoint = position;
                UpdateSelectionRectangle(new SelectionState(rect, mode));
            }

            updateSnapPointsTask.ContinueWith(_ =>
            {
                var snapPoint = GetActiveModelPoint(position, updateSnapPointsCancellationTokenSource.Token);
                CursorWorldLocationUpdated?.Invoke(this, snapPoint.WorldPoint);
                CurrentSnapPointUpdated?.Invoke(this, snapPoint);
                UpdateRubberBandLines(snapPoint.WorldPoint);
                if ((_workspace.InputService.AllowedInputTypes & InputType.Point) == InputType.Point ||
                    selectingRectangle)
                    DrawSnapPoint(snapPoint, GetNextDrawSnapPointId());
            }, updateSnapPointsCancellationTokenSource.Token).ConfigureAwait(false);
        }

        public void SubmitInput(string text)
        {
            if (_workspace.InputService.AllowedInputTypes.HasFlag(InputType.Directive) &&
                _workspace.InputService.AllowedDirectives.Contains(text))
            {
                _workspace.InputService.PushDirective(text);
            }
            else if (_workspace.InputService.AllowedInputTypes.HasFlag(InputType.Distance))
            {
                if (string.IsNullOrEmpty(text))
                {
                    _workspace.InputService.PushNone();
                }
                else if (DrawingSettings.TryParseUnits(text, out var dist))
                {
                    _workspace.InputService.PushDistance(dist);
                }
            }
            else if (_workspace.InputService.AllowedInputTypes.HasFlag(InputType.Point))
            {
                var cursorPoint = _workspace.ViewControl.GetCursorPoint(CancellationToken.None).Result;
                if (_workspace.InputService.TryParsePoint(text, cursorPoint, _workspace.InputService.LastPoint, out var point))
                {
                    _workspace.InputService.PushPoint(point);
                }
            }
            else if (_workspace.InputService.AllowedInputTypes.HasFlag(InputType.Command))
            {
                _workspace.InputService.PushCommand(string.IsNullOrEmpty(text) ? null : text);
            }
            else if (_workspace.InputService.AllowedInputTypes.HasFlag(InputType.Text))
            {
                _workspace.InputService.PushText(text ?? string.Empty);
            }
        }

        private long GetNextDrawSnapPointId()
        {
            lock (drawSnapPointIdGate)
            {
                var next = drawSnapPointId++;
                return next;
            }
        }

        private void UpdateSelectionRectangle(SelectionState? selectionState)
        {
            SelectionRectangleUpdated?.Invoke(this, selectionState);
        }

        private void SetCursorVisibility()
        {
            var state = CursorState.None;
            if (selectingRectangle)
            {
                state = CursorState.Point;
            }
            else
            {
                if ((_workspace.InputService.AllowedInputTypes & InputType.Command) == InputType.Command)
                {
                    state |= CursorState.Point;
                    state |= CursorState.Object;
                }
                if ((_workspace.InputService.AllowedInputTypes & InputType.Distance) == InputType.Distance)
                {
                    state |= CursorState.Point;
                }
                if ((_workspace.InputService.AllowedInputTypes & InputType.Point) == InputType.Point)
                {
                    state |= CursorState.Point;
                }
                if ((_workspace.InputService.AllowedInputTypes & InputType.Entities) == InputType.Entities)
                {
                    state |= CursorState.Object;
                }
                if ((_workspace.InputService.AllowedInputTypes & InputType.Entity) == InputType.Entity)
                {
                    state |= CursorState.Object;
                }
                if ((_workspace.InputService.AllowedInputTypes & InputType.Text) == InputType.Text)
                {
                    state |= CursorState.Text;
                }
                if (panCommand.IsPanning)
                {
                    state |= CursorState.Pan;
                }
            }

            CursorStateUpdated?.Invoke(this, state);
        }

        public void Zoom(ZoomDirection direction, Point zoomCenter)
        {
            // scale everything
            double scale = ZoomScale;
            switch (direction)
            {
                case ZoomDirection.In:
                    scale = 1.0 / scale;
                    break;
                case ZoomDirection.Out:
                    break;
            }

            // center zoom operation on mouse
            lastPointerPosition = zoomCenter;
            var vp = _workspace.ActiveViewPort;
            var oldHeight = vp.ViewHeight;
            var oldWidth = Width * oldHeight / Height;
            var newHeight = oldHeight * scale;
            var newWidth = oldWidth * scale;
            var heightDelta = newHeight - oldHeight;
            var widthDelta = newWidth - oldWidth;

            var relHoriz = zoomCenter.X / Width;
            var relVert = (Height - zoomCenter.Y) / Height;
            var botLeftDelta = new Vector(relHoriz * widthDelta, relVert * heightDelta, 0.0);
            var newVp = vp.Update(
                bottomLeft: (Point)(vp.BottomLeft - botLeftDelta),
                viewHeight: vp.ViewHeight * scale);
            _workspace.Update(activeViewPort: newVp);

            updateSnapPointsTask.ContinueWith(_ =>
            {
                var snapPoint = GetActiveModelPoint(zoomCenter, updateSnapPointsCancellationTokenSource.Token);
                CursorWorldLocationUpdated?.Invoke(this, snapPoint.WorldPoint);
                if ((_workspace.InputService.AllowedInputTypes & InputType.Point) == InputType.Point)
                    DrawSnapPoint(snapPoint, GetNextDrawSnapPointId());
            }).ConfigureAwait(false);
        }

        public void MouseWheel(Point position, MouseWheelDirection direction)
        {
            switch (direction)
            {
                case MouseWheelDirection.Up:
                    Zoom(ZoomDirection.In, position);
                    break;
                case MouseWheelDirection.Down:
                    Zoom(ZoomDirection.Out, position);
                    break;
            }
        }

        public void Pan(double dx, double dy)
        {
            var vp = _workspace.ActiveViewPort;
            var scale = vp.ViewHeight / Height;
            dx = vp.BottomLeft.X + dx * scale;
            dy = vp.BottomLeft.Y - dy * scale;
            _workspace.Update(activeViewPort: vp.Update(bottomLeft: new Point(dx, dy, vp.BottomLeft.Z)));
        }

        private async Task<TransformedSnapPoint> GetActiveModelPointNowAsync(Point cursor, CancellationToken cancellationToken)
        {
            return await UpdateSnapPoints(transformationMatrix).ContinueWith(_ => GetActiveModelPoint(cursor, cancellationToken)).ConfigureAwait(false);
        }

        private TransformedSnapPoint GetActiveModelPoint(Point cursor, CancellationToken cancellationToken)
        {
            return GetActiveSnapPoint(cursor, cancellationToken)
                ?? GetOrthoPoint(cursor)
                ?? GetAngleSnapPoint(cursor, cancellationToken)
                ?? GetRawModelPoint(cursor);
        }

        private TransformedSnapPoint? GetActiveSnapPoint(Point cursor, CancellationToken cancellationToken)
        {
            if (_workspace.SettingsService.GetValue<bool>(DisplaySettingsNames.PointSnap) &&
                ((_workspace.InputService.AllowedInputTypes & InputType.Point) == InputType.Point) ||
                selectingRectangle)
            {
                var snapPointDistance = _workspace.SettingsService.GetValue<double>(DisplaySettingsNames.SnapPointDistance);
                var size = snapPointDistance * 2;
                var nearPoints = snapPointsQuadTree
                    .GetContainedItems(new Rect(cursor.X - snapPointDistance, cursor.Y - snapPointDistance, size, size));
                var points = nearPoints
                    .Select(p => Tuple.Create((cursor - p.ControlPoint).LengthSquared, p))
                    .OrderBy(t => t.Item1, new CancellableComparer<double>(cancellationToken))
                    .ToList();
                if (points.Count > 0)
                {
                    return points[0].Item2;
                }
            }

            return null;
        }

        private TransformedSnapPoint? GetOrthoPoint(Point cursor)
        {
            if (_workspace.IsDrawing && _workspace.SettingsService.GetValue<bool>(DisplaySettingsNames.Ortho))
            {
                // if both are on the drawing plane
                var last = _workspace.InputService.LastPoint;
                var current = Unproject(cursor);
                var delta = current - last;
                var drawingPlane = _workspace.DrawingPlane;
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

        private TransformedSnapPoint? GetAngleSnapPoint(Point cursor, CancellationToken cancellationToken)
        {
            if (_workspace.IsDrawing && _workspace.SettingsService.GetValue<bool>(DisplaySettingsNames.AngleSnap))
            {
                // get distance to last point
                var last = _workspace.InputService.LastPoint;
                var current = Unproject(cursor);
                var vector = current - last;
                var dist = vector.Length;

                // for each snap angle, find the point `dist` out on the angle vector
                Func<double, Vector> snapVector = rad =>
                {
                    Vector radVector = default(Vector);
                    var drawingPlane = _workspace.DrawingPlane;
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
                        Debug.Assert(false, "invalid value for drawing plane");
                    }

                    return radVector.Normalize() * dist;
                };

                var points = new List<Tuple<double, TransformedSnapPoint>>();
                var snapAngleDistance = _workspace.SettingsService.GetValue<double>(DisplaySettingsNames.SnapAngleDistance);
                foreach (var snapAngle in _workspace.SettingsService.GetValue<double[]>(DisplaySettingsNames.SnapAngles))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var radians = snapAngle * MathHelper.DegreesToRadians;
                    var radVector = snapVector(radians);
                    var snapPoint = last + radVector;
                    var distance = (cursor - Project(snapPoint)).Length;
                    if (distance < snapAngleDistance)
                    {
                        points.Add(Tuple.Create(distance, new TransformedSnapPoint(snapPoint, Project(snapPoint), SnapPointKind.None)));
                    }
                }

                points = points.OrderBy(p => p.Item1).ToList();
                if (points.Count > 0)
                {
                    return points[0].Item2;
                }
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
            return entity.GetPrimitives().SelectMany(p => p.GetProjectedVerticies(transformationMatrix));
        }

        private IEnumerable<Entity> GetContainedEntities(Rect selectionRect, bool includePartial)
        {
            var entities = _workspace.Drawing.GetLayers().Where(l => l.IsVisible).SelectMany(l => l.GetEntities()).Where(e => selectionRect.Contains(ProjectedChain(e), includePartial));
            return entities;
        }

        private SelectedEntity GetHitEntity(Point screenPoint)
        {
            var selectionRadius = _workspace.SettingsService.GetValue<double>(DisplaySettingsNames.EntitySelectionRadius);
            var selectionRadius2 = selectionRadius * selectionRadius;
            var entities = from layer in _workspace.Drawing.GetLayers().Where(l => l.IsVisible)
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
            return primitive.MapPrimitive<Tuple<double, Point>>(
                ellipse => ClosestPoint(ellipse.GetProjectedVerticies(transformationMatrix, 360).ToArray(), screenPoint),
                line => ClosestPoint(new[] { transformationMatrix.Transform(line.P1), transformationMatrix.Transform(line.P2) }, screenPoint),
                point =>
                {
                    var displayPoint = transformationMatrix.Transform(point.Location);
                    var dist = (displayPoint - screenPoint).Length;
                    return Tuple.Create(dist, point.Location);
                },
                text =>
                {
                    var rad = text.Rotation * MathHelper.DegreesToRadians;
                    var right = new Vector(Math.Cos(rad), Math.Sin(rad), 0.0).Normalize() * text.Width;
                    var up = text.Normal.Cross(right).Normalize() * text.Height;
                    var borderPoints = new[]
                    {
                        transformationMatrix.Transform(text.Location),
                        transformationMatrix.Transform(text.Location + right),
                        transformationMatrix.Transform(text.Location + up),
                        transformationMatrix.Transform(text.Location + right + up)
                    };
                    if (borderPoints.ConvexHull().PolygonContains(screenPoint))
                        return Tuple.Create(0.0, screenPoint);
                    return ClosestPoint(borderPoints, screenPoint);
                },
                bezier => Tuple.Create(0.0, screenPoint), // TODO
                image =>
                {
                    var rad = image.Rotation * MathHelper.DegreesToRadians;
                    var right = new Vector(Math.Cos(rad), Math.Sin(rad), 0.0).Normalize() * image.Width;
                    var up = Vector.ZAxis.Cross(right).Normalize() * image.Height;
                    var imagePoints = new[]
                    {
                        transformationMatrix.Transform(image.Location),
                        transformationMatrix.Transform(image.Location + right),
                        transformationMatrix.Transform(image.Location + up),
                        transformationMatrix.Transform(image.Location + right + up)
                    };
                    if (imagePoints.ConvexHull().PolygonContains(screenPoint))
                        return Tuple.Create(0.0, screenPoint);
                    return ClosestPoint(imagePoints, screenPoint);
                }
            );
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
            if (_workspace.IsCommandExecuting)
            {
                return;
            }

            var hotPoints = new List<Point>();
            foreach (var primitive in _workspace.SelectedEntities.SelectMany(entity => entity.GetPrimitives()))
            {
                hotPoints.AddRange(primitive.MapPrimitive<Point[]>(
                    ellipse => ellipse.IsClosed
                        ? new[] { ellipse.Center, ellipse.GetPoint(0.0), ellipse.GetPoint(90.0), ellipse.GetPoint(180.0), ellipse.GetPoint(270.0) }
                        : new[] { ellipse.Center, ellipse.StartPoint(), ellipse.MidPoint(), ellipse.EndPoint() },
                    line => new[] { line.P1, (line.P1 + line.P2) * 0.5, line.P2 },
                    point => new[] { point.Location },
                    text => new[] { text.Location },
                    bezier => new[] { bezier.P1, bezier.P2, bezier.P3, bezier.P4 },
                    image => new[] { image.Location }
                ));
            }

            HotPointsUpdated?.Invoke(this, hotPoints);
        }

        private void DrawSnapPoint(TransformedSnapPoint snapPoint, long drawId)
        {
            lock (lastDrawnSnapPointIdGate)
            {
                if (drawId > lastDrawnSnapPointId)
                {
                    lastDrawnSnapPointId = drawId;
                    var snapDistance = _workspace.SettingsService.GetValue<double>(DisplaySettingsNames.SnapPointDistance);
                    snapDistance *= snapDistance;
                    var distSquared = (snapPoint.ControlPoint - lastPointerPosition).LengthSquared;
                    if (distSquared <= snapDistance && snapPoint.Kind != SnapPointKind.None)
                    {
                        CurrentSnapPointUpdated?.Invoke(this, snapPoint);
                    }
                    else
                    {
                        CurrentSnapPointUpdated?.Invoke(this, null);
                    }
                }
            }
        }
    }
}
