using System;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BCad.EventArguments;
using BCad.Extensions;
using BCad.Helpers;
using BCad.Services;
using BCad.SnapPoints;
using Input = System.Windows.Input;

namespace BCad.UI
{
    internal class TransformedSnapPoint
    {
        public Point WorldPoint;
        public Point ControlPoint;
        public SnapPointKind Kind;

        public TransformedSnapPoint(Point worldPoint, Point controlPoint, SnapPointKind kind)
        {
            this.WorldPoint = worldPoint;
            this.ControlPoint = controlPoint;
            this.Kind = kind;
        }
    }

    /// <summary>
    /// Interaction logic for SharpDXViewControl.xaml
    /// </summary>
    [ExportViewControl("SharpDX")]
    public partial class SharpDXViewControl : UserControl, IViewControl
    {
        private readonly CadRendererGame game;
        private readonly IWorkspace workspace;
        private readonly IInputService inputService;
        private bool panning;
        private System.Windows.Point lastPanPoint;
        private Matrix4 direct3DTransformationMatrix;
        private Matrix4 windowsTransformationMatrix;
        private Matrix4 unprojectMatrix;
        private TransformedSnapPoint[] snapPoints;

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

            game = new CadRendererGame(workspace, inputService, this);
            game.Run(surface);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            Workspace_WorkspaceChanged(this, new WorkspaceChangeEventArgs(true, true, true, true, true));
        }

        private void Workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            if (e.IsActiveViewPortChange)
            {
                direct3DTransformationMatrix = workspace.ActiveViewPort.GetTransformationMatrixDirect3DStyle(ActualWidth, ActualHeight);
                windowsTransformationMatrix = workspace.ActiveViewPort.GetTransformationMatrixWindowsStyle(ActualWidth, ActualHeight);
                unprojectMatrix = windowsTransformationMatrix;
                unprojectMatrix.Invert();
            }
            if (e.IsDrawingChange)
            {
                DrawingChanged();
            }
        }

        private void DrawingChanged()
        {
            // populate the snap points
            snapPoints = workspace.Drawing.GetLayers().SelectMany(l => l.GetEntities().SelectMany(o => o.GetSnapPoints()))
                .Select(sp => new TransformedSnapPoint(sp.Point, Project(sp.Point), sp.Kind)).ToArray();
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
            return Unproject(new Point(mouse.X, mouse.Y, 0));
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

                    break;
                case Input.MouseButton.Middle:
                    panning = true;
                    lastPanPoint = cursor;
                    break;
                case Input.MouseButton.Right:
                    inputService.PushNone();
                    break;
            }

            //var cursor = e.GetPosition(this);
            //var cursorVector = cursor.ToVector3();
            //var sp = GetActiveModelPoint(cursorVector);
            //switch (e.ChangedButton)
            //{
            //    case Input.MouseButton.Left:
            //        else if (inputService.AllowedInputTypes.HasFlag(InputType.Entity))
            //        {
            //            var selected = GetHitEntity(cursor);
            //            if (selected != null)
            //            {
            //                inputService.PushEntity(selected);
            //            }
            //        }
            //        else if (inputService.AllowedInputTypes.HasFlag(InputType.Entities))
            //        {
            //            if (selecting)
            //            {
            //                // finish selection
            //                var rect = new System.Windows.Rect(
            //                    new System.Windows.Point(
            //                        Math.Min(firstSelectionPoint.X, currentSelectionPoint.X),
            //                        Math.Min(firstSelectionPoint.Y, currentSelectionPoint.Y)),
            //                    new System.Windows.Size(
            //                        Math.Abs(firstSelectionPoint.X - currentSelectionPoint.X),
            //                        Math.Abs(firstSelectionPoint.Y - currentSelectionPoint.Y)));
            //                var entities = GetContainedEntities(rect, currentSelectionPoint.X < firstSelectionPoint.X);
            //                selecting = false;
            //                inputService.PushEntities(entities);
            //                ForceRender();
            //            }
            //            else
            //            {
            //                // start selection
            //                var selected = GetHitEntity(cursor);
            //                if (selected != null)
            //                {
            //                    inputService.PushEntities(new[] { selected.Entity });
            //                }
            //                else
            //                {
            //                    selecting = true;
            //                    firstSelectionPoint = cursor;
            //                }
            //            }
            //        }

            //        break;
            //}

            //GenerateRubberBandLines(sp.WorldPoint);
        }

        private void OnMouseUp(object sender, Input.MouseButtonEventArgs e)
        {
            //var cursor = e.GetPosition(this);
            switch (e.ChangedButton)
            {
                case Input.MouseButton.Middle:
                    panning = false;
                    break;
            }

            //var sp = GetActiveModelPoint(cursor.ToVector3());
            //GenerateRubberBandLines(sp.WorldPoint);
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

            //if (force)
            //{
            //    ForceRender();
            //}

            //if (inputService.AllowedInputTypes.HasFlag(InputType.Point))
            //{
            //    var sp = GetActiveModelPoint(cursor.ToVector3());
            //    GenerateRubberBandLines(sp.WorldPoint);
            //    DrawSnapPoint(sp);
            //}

            //foreach (var cursorImage in new[] { pointCursorImage, entityCursorImage, textCursorImage })
            //{
            //    Canvas.SetLeft(cursorImage, (int)(cursor.X - (cursorImage.ActualWidth / 2.0)));
            //    Canvas.SetTop(cursorImage, (int)(cursor.Y - (cursorImage.ActualHeight / 2.0)));
            //}
        }

        private void OnMouseWheel(object sender, Input.MouseWheelEventArgs e)
        {
            // scale everything
            var scale = 1.25f;
            if (e.Delta > 0.0f) scale = 0.8f; // 1.0f / 1.25f

            // center zoom operation on mouse
            var cursorPoint = e.GetPosition(this);
            var cursorPos = GetCursorPoint();
            var vp = workspace.ActiveViewPort;
            var botLeft = vp.BottomLeft;

            // find relative scales
            var relHoriz = cursorPoint.X / ActualWidth;
            var relVert = cursorPoint.Y / ActualHeight;
            var viewDelta = vp.ViewHeight * (scale - 1.0);

            // set values
            workspace.Update(
                activeViewPort: vp.Update(
                    viewHeight: vp.ViewHeight * scale, bottomLeft: botLeft - new Vector(viewDelta * relHoriz, viewDelta * relVert, 0.0)));
            //var cursor = GetActiveModelPoint(e.GetPosition(this).ToVector3());
            //DrawSnapPoint(cursor);
            //GenerateRubberBandLines(cursor.WorldPoint);

            //ForceRender();
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
    }
}
