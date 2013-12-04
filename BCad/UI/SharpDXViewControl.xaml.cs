using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BCad.Entities;
using BCad.EventArguments;
using BCad.Extensions;
using BCad.Helpers;
using BCad.Primitives;
using BCad.Services;
using BCad.SnapPoints;
using Input = System.Windows.Input;
using Media = System.Windows.Media;

namespace BCad.UI
{
    internal class TransformedSnapPoint
    {
        public Point WorldPoint;
        public Point ControlPoint;
        public SnapPointKind Kind;

        public TransformedSnapPoint(Point worldPoint, Point controlPoint, SnapPointKind kind)
        {
            WorldPoint = worldPoint;
            ControlPoint = controlPoint;
            Kind = kind;
        }
    }

    /// <summary>
    /// Interaction logic for SharpDXViewControl.xaml
    /// </summary>
    [ExportViewControl("SharpDX")]
    public partial class SharpDXViewControl : UserControl, IViewHost
    {
        private readonly CadRendererGame game;
        private readonly IWorkspace workspace;
        private readonly IInputService inputService;
        private bool panning;
        private bool selecting;
        private System.Windows.Point lastPanPoint;
        private System.Windows.Point firstSelectionPoint;
        private Matrix4 windowsTransformationMatrix;
        private Matrix4 unprojectMatrix;
        private IEnumerable<TransformedSnapPoint> snapPoints;
        private Media.Color autoColor;

        private ResourceDictionary resources = null;
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

        public SharpDXViewControl()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public SharpDXViewControl(IWorkspace workspace, IInputService inputService)
            : this()
        {
            this.workspace = workspace;
            this.inputService = inputService;
            this.workspace.WorkspaceChanged += Workspace_WorkspaceChanged;
            this.workspace.CommandExecuted += Workspace_CommandExecuted;
            this.workspace.SettingsManager.PropertyChanged += SettingsManager_PropertyChanged;
            this.inputService.ValueRequested += InputService_ValueRequested;
            this.inputService.ValueReceived += InputService_ValueReceived;

            autoColor = workspace.SettingsManager.BackgroundColor.GetAutoContrastingColor().ToMediaColor();
            UpdateCursor();
            SetCursorVisibility();
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

            game = new CadRendererGame(workspace, inputService, this);
            game.Run(surface);
        }

        public int DisplayHeight
        {
            get { return (int)ActualHeight; }
        }

        public int DisplayWidth
        {
            get { return (int)ActualWidth; }
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case Constants.BackgroundColorString:
                    autoColor = workspace.SettingsManager.BackgroundColor.GetAutoContrastingColor().ToMediaColor();
                    UpdateCursor();
                    break;
                case Constants.CursorSizeString:
                case Constants.EntitySelectionRadiusString:
                case Constants.TextCursorSizeString:
                    UpdateCursor();
                    break;
            }
        }

        private void InputService_ValueReceived(object sender, ValueReceivedEventArgs e)
        {
            ClearSnapPoints();
            SetCursorVisibility();
        }

        private void InputService_ValueRequested(object sender, ValueRequestedEventArgs e)
        {
            SetCursorVisibility();
        }

        private void Workspace_CommandExecuted(object sender, CommandExecutedEventArgs e)
        {
            ClearSnapPoints();
            SetCursorVisibility();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

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
            }
        }

        private void ViewPortChanged()
        {
            windowsTransformationMatrix = workspace.ActiveViewPort.GetTransformationMatrixWindowsStyle(ActualWidth, ActualHeight);
            unprojectMatrix = windowsTransformationMatrix;
            unprojectMatrix.Invert();
            windowsTransformationMatrix = Matrix4.CreateScale(1, 1, 0) * windowsTransformationMatrix;
            UpdateSnapPoints();
        }

        private void DrawingChanged()
        {
            UpdateSnapPoints();
        }

        private void ClearSnapPoints()
        {
            snapLayer.Children.Clear();
        }

        private void UpdateSnapPoints()
        {
            // populate the snap points
            snapPoints = workspace.Drawing.GetLayers().SelectMany(l => l.GetEntities().SelectMany(o => o.GetSnapPoints()))
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
            var mouse = Input.Mouse.GetPosition(this);
            return GetActiveModelPoint(mouse.ToPoint()).WorldPoint;
        }

        private void OnMouseDown(object sender, Input.MouseButtonEventArgs e)
        {
            var cursor = e.GetPosition(this);
            var sp = GetActiveModelPoint(cursor.ToPoint());
            switch (e.ChangedButton)
            {
                case Input.MouseButton.Left:
                    if ((inputService.AllowedInputTypes & InputType.Point) == InputType.Point)
                    {
                        inputService.PushPoint(sp.WorldPoint);
                    }
                    else if ((inputService.AllowedInputTypes & InputType.Entity) == InputType.Entity)
                    {
                        var selected = GetHitEntity(cursor);
                        if (selected != null)
                        {
                            inputService.PushEntity(selected);
                        }
                    }
                    else if ((inputService.AllowedInputTypes & InputType.Entities) == InputType.Entities)
                    {
                        if (selecting)
                        {
                            // finish selection
                            //var rect = new System.Windows.Rect(
                            //    new System.Windows.Point(
                            //        Math.Min(firstSelectionPoint.X, currentSelectionPoint.X),
                            //        Math.Min(firstSelectionPoint.Y, currentSelectionPoint.Y)),
                            //    new System.Windows.Size(
                            //        Math.Abs(firstSelectionPoint.X - currentSelectionPoint.X),
                            //        Math.Abs(firstSelectionPoint.Y - currentSelectionPoint.Y)));
                            //var entities = GetContainedEntities(rect, currentSelectionPoint.X < firstSelectionPoint.X);
                            //selecting = false;
                            //inputService.PushEntities(entities);
                            //ForceRender();
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
                                //selecting = true;
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
            //bool force = false;
            var cursor = e.GetPosition(this);
            var delta = lastPanPoint - cursor;
            if (panning)
            {
                var vp = workspace.ActiveViewPort;
                var scale = vp.ViewHeight / this.ActualHeight;
                var dx = vp.BottomLeft.X + delta.X * scale;
                var dy = vp.BottomLeft.Y - delta.Y * scale;
                workspace.Update(activeViewPort: vp.Update(bottomLeft: new Point(dx, dy, vp.BottomLeft.Z)));
                lastPanPoint = cursor;
                //firstSelectionPoint -= delta;
                //force = true;
            }

            var real = GetCursorPoint();
            positionText.Text = string.Format("Cursor: {0},{1}; Real: {2:F0},{3:F0},{4:F0}", cursor.X, cursor.Y, real.X, real.Y, real.Z);

            //if (selecting)
            //{
            //    currentSelectionPoint = cursor;
            //    force = true;
            //}

            if ((inputService.AllowedInputTypes & InputType.Point) == InputType.Point)
            {
                var sp = GetActiveModelPoint(cursor.ToPoint());
                DrawSnapPoint(sp);
            }

            foreach (var cursorImage in new[] { pointCursorImage, entityCursorImage, textCursorImage })
            {
                Canvas.SetLeft(cursorImage, (int)(cursor.X - (cursorImage.ActualWidth / 2.0)));
                Canvas.SetTop(cursorImage, (int)(cursor.Y - (cursorImage.ActualHeight / 2.0)));
            }
        }

        private void UpdateCursor()
        {
            var pen = new Media.Pen(new Media.SolidColorBrush(autoColor), 1);

            var cursorSize = workspace.SettingsManager.CursorSize / 2.0 + 0.5;
            pointCursorImage.Source = new Media.DrawingImage(
                new Media.GeometryDrawing()
            {
                Geometry = new Media.GeometryGroup()
                {
                    Children = new Media.GeometryCollection(new[]
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

            var textSize = workspace.SettingsManager.TextCursorSize / 2.0 + 0.5;
            textCursorImage.Source = new Media.DrawingImage(
                new Media.GeometryDrawing()
            {
                Geometry = new Media.GeometryGroup()
                {
                    Children = new Media.GeometryCollection(new[]
                        {
                            //new Media.LineGeometry(new System.Windows.Point(0, -cursorSize), new System.Windows.Point(0, cursorSize))
                            new Media.LineGeometry(new System.Windows.Point(0, -textSize), new System.Windows.Point(0, textSize))
                        })
                },
                Pen = pen
            });
        }

        private void SetCursorVisibility()
        {
            Func<InputType[], Visibility> getVisibility = types =>
                types.Any(t => (inputService.AllowedInputTypes & t) == t)
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

        private void OnMouseWheel(object sender, Input.MouseWheelEventArgs e)
        {
            // scale everything
            var scale = 1.25;
            if (e.Delta > 0) scale = 0.8; // 1.0 / 1.25

            // center zoom operation on mouse
            var cursorPoint = e.GetPosition(this);
            var vp = workspace.ActiveViewPort;
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
                bottomLeft: vp.BottomLeft - botLeftDelta,
                viewHeight: vp.ViewHeight * scale);
            workspace.Update(activeViewPort: newVp);
            var cursor = GetActiveModelPoint(cursorPoint.ToPoint());
            DrawSnapPoint(cursor);
        }

        private TransformedSnapPoint GetActiveModelPoint(Point cursor)
        {
            return GetActiveSnapPoint(cursor)
                ?? GetOrthoPoint(cursor)
                ?? GetAngleSnapPoint(cursor)
                ?? GetRawModelPoint(cursor);
        }

        private TransformedSnapPoint GetActiveSnapPoint(Point cursor)
        {
            if (workspace.SettingsManager.PointSnap && (inputService.AllowedInputTypes & InputType.Point) == InputType.Point)
            {
                var maxDistSq = (float)(workspace.SettingsManager.SnapPointDistance * workspace.SettingsManager.SnapPointDistance);
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
            if (inputService.IsDrawing && workspace.SettingsManager.Ortho)
            {
                // if both are on the drawing plane
                var last = inputService.LastPoint;
                var current = Unproject(cursor);
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

                Debug.Assert(world != null, "should have returned null");
                return new TransformedSnapPoint(world, cursor, SnapPointKind.None);
            }

            return null;
        }

        private TransformedSnapPoint GetAngleSnapPoint(Point cursor)
        {
            if (inputService.IsDrawing && workspace.SettingsManager.AngleSnap)
            {
                // get distance to last point
                var last = inputService.LastPoint;
                var current = Unproject(cursor);
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
                             let di = (cursor - Project(snapPoint)).Length
                             where di <= workspace.SettingsManager.SnapAngleDistance
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

        private SelectedEntity GetHitEntity(System.Windows.Point cursor)
        {
            var screenPoint = cursor.ToPoint();
            var start = DateTime.UtcNow;
            var selectionRadius = workspace.SettingsManager.EntitySelectionRadius;
            var selectionRadius2 = selectionRadius * selectionRadius;
            var entities = from entity in workspace.Drawing.GetEntities()
                           let dist = ClosestPoint(entity, screenPoint)
                           where dist.Item1 < selectionRadius2
                           orderby dist.Item1
                           select new SelectedEntity(entity, dist.Item2);
            var selected = entities.FirstOrDefault();
            var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
            inputService.WriteLineDebug("GetHitEntity in {0} ms", elapsed);

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
                    return ClosestPoint(el.GetProjectedVerticies(windowsTransformationMatrix), screenPoint);
                case PrimitiveKind.Line:
                    var line = (PrimitiveLine)primitive;
                    return ClosestPoint(new[]
                    {
                        windowsTransformationMatrix.Transform(line.P1),
                        windowsTransformationMatrix.Transform(line.P2)
                    }, screenPoint);
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

        private static Tuple<double, Point> ClosestPoint(Point[] screenVerticies, Point screenPoint)
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
                         select Tuple.Create(dist, screenVerticies[i] + offset);
            var selected = points.FirstOrDefault();
            return selected;
        }

        private void DrawSnapPoint(TransformedSnapPoint snapPoint)
        {
            ClearSnapPoints();
            if (snapPoint.Kind == SnapPointKind.None)
                return;
            snapLayer.Children.Add(GetSnapIcon(snapPoint));
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

            var geometry = ((Media.GeometryDrawing)SnapPointResources[name]).Clone();
            var scale = workspace.SettingsManager.SnapPointSize;
            geometry.Pen = new Media.Pen(new Media.SolidColorBrush(workspace.SettingsManager.SnapPointColor.ToMediaColor()), 0.2);
            var di = new Media.DrawingImage(geometry);
            var icon = new Image(); // TODO: reuse icons if possible
            icon.Source = di;
            icon.Stretch = Media.Stretch.None;
            icon.LayoutTransform = new Media.ScaleTransform(scale, scale);
            Canvas.SetLeft(icon, snapPoint.ControlPoint.X - geometry.Bounds.Width * scale / 2.0);
            Canvas.SetTop(icon, snapPoint.ControlPoint.Y - geometry.Bounds.Height * scale / 2.0);
            return icon;
        }
    }
}
