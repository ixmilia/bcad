// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

using IxMilia.BCad.Collections;
using IxMilia.BCad.Entities;
using IxMilia.BCad.EventArguments;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.Services;
using IxMilia.BCad.Settings;
using IxMilia.BCad.SnapPoints;
using IxMilia.BCad.UI.Extensions;

using Cursors = System.Windows.Input.Cursors;
using DependencyObject = System.Windows.DependencyObject;
using DependencyProperty = System.Windows.DependencyProperty;
using FrameworkElement = System.Windows.FrameworkElement;
using PointerButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using PointerEventArgs = System.Windows.Input.MouseEventArgs;
using MouseButton = System.Windows.Input.MouseButton;
using MouseWheelEventArgs = System.Windows.Input.MouseWheelEventArgs;
using PropertyPath = System.Windows.PropertyPath;
using ResourceDictionary = System.Windows.ResourceDictionary;
using Shapes = System.Windows.Shapes;
using SizeChangedEventArgs = System.Windows.SizeChangedEventArgs;
using UIElement = System.Windows.UIElement;
using Visibility = System.Windows.Visibility;

namespace IxMilia.BCad.UI.View
{
    /// <summary>
    /// Interaction logic for ViewPane.xaml
    /// </summary>
    public partial class ViewPane : UserControl, IViewControl
    {
        private AbstractCadRenderer _renderer;
        private bool panning;
        private bool selecting;
        private bool selectingRectangle;
        private Point lastPanPoint;
        private Point firstSelectionPoint;
        private Point currentSelectionPoint;
        private Point lastPointerPosition;
        private TaskCompletionSource<SelectionRectangle> selectionDone;
        private Matrix4 windowsTransformationMatrix;
        private Matrix4 unprojectMatrix;
        private QuadTree<TransformedSnapPoint> snapPointsQuadTree;
        private DoubleCollection solidLine = new DoubleCollection();
        private DoubleCollection dashedLine = new DoubleCollection() { 4.0, 4.0 };
        private ResourceDictionary resources;
        private CancellationTokenSource updateSnapPointsCancellationTokenSource = new CancellationTokenSource();
        private Task updateSnapPointsTask = new Task(() => { });
        private long lastDrawnSnapPointId;
        private long drawSnapPointId = 1;
        private object drawSnapPointIdGate = new object();
        private object lastDrawnSnapPointIdGate = new object();

        private Dictionary<SnapPointKind, FrameworkElement> snapPointGeometry = new Dictionary<SnapPointKind, FrameworkElement>();

        private ResourceDictionary SnapPointResources
        {
            get
            {
                if (resources == null)
                {
                    resources = new ResourceDictionary();
                    resources.Source = new Uri("UI/SnapPointIcons.xaml", UriKind.RelativeOrAbsolute);
                }

                return resources;
            }
        }

        public BindingClass BindObject { get; private set; }

        [Import]
        public IWorkspace Workspace { get; set; }

        [ImportMany]
        public IEnumerable<Lazy<IRendererFactory, RenderFactoryMetadata>> RendererFactories { get; set; }

        public ViewPane()
        {
            InitializeComponent();

            var cursors = new[]
                {
                    pointCursor,
                    entityCursor,
                    textCursor
                };
            Loaded += (_, __) =>
            {
                foreach (var cursorImage in cursors)
                {
                    Canvas.SetLeft(cursorImage, -(int)(cursorImage.ActualWidth / 2.0));
                    Canvas.SetTop(cursorImage, -(int)(cursorImage.ActualHeight / 2.0));
                }
            };

            clicker.SizeChanged += ViewPaneSizeChanged;
            ClipToBounds = true;
            clicker.Cursor = Cursors.None;
            clicker.MouseMove += OnMouseMove;
            clicker.MouseDown += OnMouseDown;
            clicker.MouseUp += OnMouseUp;
            clicker.MouseWheel += OnMouseWheel;
            CompositionContainer.Container.SatisfyImports(this);
        }

        [OnImportsSatisfied]
        public void OnImportsSatisfied()
        {
            BindObject = new BindingClass(Workspace);
            DataContext = BindObject;
            Workspace.Update(viewControl: this, isDirty: Workspace.IsDirty);
            Workspace.WorkspaceChanged += Workspace_WorkspaceChanged;
            Workspace.CommandExecuted += Workspace_CommandExecuted;
            Workspace.SettingsService.SettingChanged += SettingsService_SettingChanged;
            Workspace.SelectedEntities.CollectionChanged += SelectedEntities_CollectionChanged;
            Workspace.InputService.ValueRequested += InputService_ValueRequested;
            Workspace.InputService.ValueReceived += InputService_ValueReceived;
            Workspace.InputService.InputCanceled += InputService_InputCanceled;

            SettingsService_SettingChanged(this, new SettingChangedEventArgs(string.Empty, typeof(object), null, null));
            SetCursorVisibility();

            // prepare snap point icons
            foreach (var kind in new[] { SnapPointKind.Center, SnapPointKind.EndPoint, SnapPointKind.MidPoint, SnapPointKind.Quadrant, SnapPointKind.Focus })
            {
                snapPointGeometry[kind] = GetSnapGeometry(kind);
                snapLayer.Children.Add(snapPointGeometry[kind]);
            }
        }

        private void ViewPaneSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Workspace != null)
            {
                ViewPortChanged();
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
            Workspace.OutputService.WriteLine("Select first point");
            SetCursorVisibility();
            selectionDone = new TaskCompletionSource<SelectionRectangle>();
            return selectionDone.Task;
        }

        private void SettingsService_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.SettingName) || e.SettingName == WpfSettingsProvider.RendererId)
            {
                SetRenderer();
            }
            if (string.IsNullOrEmpty(e.SettingName) || e.SettingName == WpfSettingsProvider.BackgroundColor)
            {
                var autoColor = Workspace.SettingsService.GetValue<CadColor>(WpfSettingsProvider.BackgroundColor).GetAutoContrastingColor();
                var selectionColor = autoColor.With(a: 25);
                var autoColorUI = autoColor.ToUIColor();
                BindObject.AutoBrush = new SolidColorBrush(autoColorUI);
                BindObject.SelectionBrush = new SolidColorBrush(selectionColor.ToUIColor());
            }
            if (string.IsNullOrEmpty(e.SettingName) || e.SettingName == WpfSettingsProvider.CursorSize)
            {
                var cursorSize = Workspace.SettingsService.GetValue<int>(WpfSettingsProvider.CursorSize) / 2.0 + 0.5;
                BindObject.LeftCursorExtent = new Point(-cursorSize, 0, 0);
                BindObject.RightCursorExtent = new Point(cursorSize, 0, 0);
                BindObject.TopCursorExtent = new Point(0, -cursorSize, 0);
                BindObject.BottomCursorExtent = new Point(0, cursorSize, 0);

                // only update the cursor location after the previous four binding calls have appropriately propagated
                // to the UI
                Invoke(UpdateCursorLocation);
            }
            if (string.IsNullOrEmpty(e.SettingName) || e.SettingName == WpfSettingsProvider.EntitySelectionRadius)
            {
                var entitySize = Workspace.SettingsService.GetValue<double>(WpfSettingsProvider.EntitySelectionRadius);
                BindObject.EntitySelectionTopLeft = new Point(-entitySize, -entitySize, 0);
                BindObject.EntitySelectionTopRight = new Point(entitySize, -entitySize, 0);
                BindObject.EntitySelectionBottomLeft = new Point(-entitySize, entitySize, 0);
                BindObject.EntitySelectionBottomRight = new Point(entitySize, entitySize, 0);

                // only update the cursor location after the previous four binding calls have appropriately propagated
                // to the UI
                Invoke(UpdateCursorLocation);
            }
            if (string.IsNullOrEmpty(e.SettingName) || e.SettingName == WpfSettingsProvider.TextCursorSize)
            {
                var textSize = Workspace.SettingsService.GetValue<int>(WpfSettingsProvider.TextCursorSize) / 2.0 + 0.5;
                BindObject.TextCursorStart = new Point(0, -textSize, 0);
                BindObject.TextCursorStart = new Point(0, textSize, 0);

                // only update the cursor location after the previous two binding calls have appropriately propagated
                // to the UI
                Invoke(UpdateCursorLocation);
            }
            if (string.IsNullOrEmpty(e.SettingName) || e.SettingName == WpfSettingsProvider.HotPointColor)
            {
                BindObject.HotPointBrush = new SolidColorBrush(Workspace.SettingsService.GetValue<CadColor>(WpfSettingsProvider.HotPointColor).ToUIColor());
            }
            if (string.IsNullOrEmpty(e.SettingName) ||
                e.SettingName == WpfSettingsProvider.SnapPointColor ||
                e.SettingName == WpfSettingsProvider.SnapPointSize)
            {
                var snapPointSize = Workspace.SettingsService.GetValue<double>(WpfSettingsProvider.SnapPointSize);
                BindObject.SnapPointTransform = new ScaleTransform() { ScaleX = snapPointSize, ScaleY = snapPointSize };
                BindObject.SnapPointBrush = new SolidColorBrush(Workspace.SettingsService.GetValue<CadColor>(WpfSettingsProvider.SnapPointColor).ToUIColor());
                BindObject.SnapPointStrokeThickness = snapPointSize == 0.0
                    ? 1.0
                    : 3.0 / snapPointSize;
            }
        }

        private void SetRenderer()
        {
            renderControl.Content = null;
            var disposable = _renderer as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }

            var factory = RendererFactories.FirstOrDefault(f => f.Metadata.FactoryName == Workspace.SettingsService.GetValue<string>(WpfSettingsProvider.RendererId));
            if (factory != null)
            {
                _renderer = factory.Value.CreateRenderer(this, Workspace);
            }

            renderControl.Content = _renderer;
        }

        private void Invoke(Action action)
        {
            Dispatcher.Invoke(action, DispatcherPriority.Background);
        }

        private T Invoke<T>(Func<T> func)
        {
            return Dispatcher.Invoke(func);
        }

        private void BeginInvoke(Action action)
        {
            Dispatcher.BeginInvoke(action);
        }

        private void InputService_ValueReceived(object sender, ValueReceivedEventArgs e)
        {
            selecting = false;
            ClearSnapPoints();
            SetCursorVisibility();
            SetSelectionLineVisibility(Visibility.Collapsed);
        }

        private void InputService_ValueRequested(object sender, ValueRequestedEventArgs e)
        {
            selecting = false;
            SetCursorVisibility();
            SetSelectionLineVisibility(Visibility.Collapsed);
        }

        void InputService_InputCanceled(object sender, EventArgs e)
        {
            if (selecting)
            {
                selecting = false;
                SetCursorVisibility();
                SetSelectionLineVisibility(Visibility.Collapsed);
            }
            else
            {
                Workspace.SelectedEntities.Clear();
            }
        }

        private void Workspace_CommandExecuted(object sender, CadCommandExecutedEventArgs e)
        {
            selecting = false;
            ClearSnapPoints();
            SetCursorVisibility();
            SetSelectionLineVisibility(Visibility.Collapsed);
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

        private void ViewPortChanged()
        {
            windowsTransformationMatrix = Workspace.ActiveViewPort.GetTransformationMatrixWindowsStyle(ActualWidth, ActualHeight);
            unprojectMatrix = windowsTransformationMatrix.Inverse();
            windowsTransformationMatrix = Matrix4.CreateScale(1, 1, 0) * windowsTransformationMatrix;
            UpdateHotPoints();
            UpdateSnapPoints();
        }

        private void DrawingChanged()
        {
            UpdateSnapPoints();
        }

        private void ClearSnapPoints()
        {
            foreach (UIElement child in snapLayer.Children)
            {
                child.Visibility = Visibility.Collapsed;
            }
        }

        private Task UpdateSnapPoints(bool allowCancellation = true)
        {
            var oldTokenSource = updateSnapPointsCancellationTokenSource;
            updateSnapPointsCancellationTokenSource = new CancellationTokenSource();
            oldTokenSource.Cancel();
            var token = updateSnapPointsCancellationTokenSource.Token;

            var width = ActualWidth;
            var height = ActualHeight;
            updateSnapPointsTask = Task.Run(() =>
            {
                // populate the snap points
                var transformedQuadTree = new QuadTree<TransformedSnapPoint>(new Rect(0, 0, width, height), t => new Rect(t.ControlPoint.X, t.ControlPoint.Y, 0.0, 0.0));
                foreach (var layer in Workspace.Drawing.GetLayers(token))
                {
                    token.ThrowIfCancellationRequested();
                    foreach (var entity in layer.GetEntities(token))
                    {
                        token.ThrowIfCancellationRequested();
                        foreach (var snapPoint in entity.GetSnapPoints())
                        {
                            transformedQuadTree.AddItem(new TransformedSnapPoint(snapPoint.Point, Project(snapPoint.Point), snapPoint.Kind));
                        }
                    }
                }

                snapPointsQuadTree = transformedQuadTree;
            });

            return updateSnapPointsTask;
        }

        private Point Project(Point point)
        {
            return windowsTransformationMatrix.Transform(point);
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

        private Point GetPointerPosition(PointerEventArgs e)
        {
            lastPointerPosition = e.GetPosition(clicker).ToPoint();
            return lastPointerPosition;
        }

        private bool IsLeftButtonPressed(PointerButtonEventArgs e)
        {
            return e.ChangedButton == MouseButton.Left;
        }

        private bool IsMiddleButtonPressed(PointerButtonEventArgs e)
        {
            return e.ChangedButton == MouseButton.Middle;
        }

        private bool IsRightButtonPressed(PointerButtonEventArgs e)
        {
            return e.ChangedButton == MouseButton.Right;
        }

        private async void OnMouseDown(object sender, PointerButtonEventArgs e)
        {
            var cursor = GetPointerPosition(e);
            var sp = await GetActiveModelPointNowAsync(cursor, CancellationToken.None);

            if (IsLeftButtonPressed(e))
            {
                if ((Workspace.InputService.AllowedInputTypes & InputType.Point) == InputType.Point)
                {
                    Workspace.InputService.PushPoint(sp.WorldPoint);
                }
                else if ((Workspace.InputService.AllowedInputTypes & InputType.Entity) == InputType.Entity)
                {
                    var selected = GetHitEntity(cursor);
                    if (selected != null)
                    {
                        Workspace.InputService.PushEntity(selected);
                    }
                }
                else if ((Workspace.InputService.AllowedInputTypes & InputType.Entities) == InputType.Entities || selectingRectangle || !Workspace.IsCommandExecuting)
                {
                    if (selecting)
                    {
                        // finish selection
                        IEnumerable<Entity> entities = null;
                        if (selectingRectangle)
                        {
                            selectingRectangle = false;
                            ClearSnapPoints();
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
                        SetSelectionLineVisibility(Visibility.Collapsed);
                        if (entities != null)
                        {
                            if (!Workspace.IsCommandExecuting)
                            {
                                Workspace.SelectedEntities.AddRange(entities);
                            }
                            else
                            {
                                Workspace.InputService.PushEntities(entities);
                            }
                        }
                    }
                    else
                    {
                        SelectedEntity selected = null;
                        if (selectingRectangle)
                        {
                            Workspace.OutputService.WriteLine("Select second point");
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
                                Workspace.InputService.PushEntities(new[] { selected.Entity });
                            }
                        }
                        else
                        {
                            selecting = true;
                            firstSelectionPoint = sp.ControlPoint;
                            currentSelectionPoint = cursor;
                            SetSelectionLineVisibility(Visibility.Visible);
                        }
                    }
                }
                else if (Workspace.InputService.AllowedInputTypes == InputType.None || !Workspace.IsCommandExecuting)
                {
                    // do hot-point tracking
                    var selected = GetHitEntity(cursor);
                    if (selected != null)
                    {
                        Workspace.SelectedEntities.Add(selected.Entity);
                    }
                }
            }
            else if (IsMiddleButtonPressed(e))
            {
                panning = true;
                lastPanPoint = cursor;
            }
            else if (IsRightButtonPressed(e))
            {
                Workspace.InputService.PushNone();
            }
        }

        private void OnMouseUp(object sender, PointerButtonEventArgs e)
        {
            if (IsMiddleButtonPressed(e))
            {
                panning = false;
            }
        }

        private void OnMouseMove(object sender, PointerEventArgs e)
        {
            if (Workspace == null || Workspace.InputService == null)
                return;

            var cursor = GetPointerPosition(e);
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
            UpdateCursorLocation();

            updateSnapPointsTask.ContinueWith(_ =>
            {
                var snapPoint = GetActiveModelPoint(cursor, updateSnapPointsCancellationTokenSource.Token);
                Invoke(() => BindObject.CursorWorld = snapPoint.WorldPoint);
                _renderer.UpdateRubberBandLines();
                if ((Workspace.InputService.AllowedInputTypes & InputType.Point) == InputType.Point ||
                    selectingRectangle)
                    DrawSnapPoint(snapPoint, GetNextDrawSnapPointId());
            }).ConfigureAwait(false);
        }

        private void UpdateCursorLocation()
        {
            foreach (var cursorImage in new[] { pointCursor, entityCursor, textCursor})
            {
                Canvas.SetLeft(cursorImage, (int)(BindObject.CursorScreen.X - (cursorImage.ActualWidth / 2.0)));
                Canvas.SetTop(cursorImage, (int)(BindObject.CursorScreen.Y - (cursorImage.ActualHeight / 2.0)));
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

        private void SetSelectionLineVisibility(Visibility vis)
        {
            if (vis == Visibility.Visible)
            {
                UpdateSelectionLines();
            }

            BeginInvoke((Action)(() =>
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
            SetStrokeDashArray(selectionLine1, dash);
            SetStrokeDashArray(selectionLine2, dash);
            SetStrokeDashArray(selectionLine3, dash);
            SetStrokeDashArray(selectionLine4, dash);

            var left = Math.Min(currentSelectionPoint.X, firstSelectionPoint.X);
            var top = Math.Min(currentSelectionPoint.Y, firstSelectionPoint.Y);
            selectionRect.Width = Math.Abs(currentSelectionPoint.X - firstSelectionPoint.X);
            selectionRect.Height = Math.Abs(currentSelectionPoint.Y - firstSelectionPoint.Y);
            Canvas.SetLeft(selectionRect, left);
            Canvas.SetTop(selectionRect, top);
        }

        private void SetStrokeDashArray(Shapes.Shape shape, DoubleCollection collection)
        {
            shape.StrokeDashArray = collection;
        }

        private void SetCursorVisibility()
        {
            if (selectingRectangle)
            {
                Invoke(() =>
                {
                    pointCursor.Visibility = Visibility.Visible;
                    entityCursor.Visibility = Visibility.Collapsed;
                    textCursor.Visibility = Visibility.Collapsed;
                });
            }
            else
            {
                Func<InputType[], Visibility> getVisibility = types =>
                    types.Any(t => (Workspace.InputService.AllowedInputTypes & t) == t)
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                Invoke(() =>
                {
                    pointCursor.Visibility = getVisibility(new[]
                    {
                        InputType.Command,
                        InputType.Distance,
                        InputType.Point
                    });
                    entityCursor.Visibility = getVisibility(new[]
                    {
                        InputType.Command,
                        InputType.Entities,
                        InputType.Entity
                    });
                    textCursor.Visibility = getVisibility(new[]
                    {
                        InputType.Text
                    });
                });
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var delta = e.Delta;

            // scale everything
            var scale = 1.25;
            if (delta > 0) scale = 0.8; // 1.0 / 1.25

            // center zoom operation on mouse
            var cursorPoint = GetPointerPosition(e);
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

            updateSnapPointsTask.ContinueWith(_ =>
            {
                var snapPoint = GetActiveModelPoint(cursorPoint, updateSnapPointsCancellationTokenSource.Token);
                Invoke(() => BindObject.CursorWorld = snapPoint.WorldPoint);
                _renderer.UpdateRubberBandLines();
                if ((Workspace.InputService.AllowedInputTypes & InputType.Point) == InputType.Point)
                    DrawSnapPoint(snapPoint, GetNextDrawSnapPointId());
            }).ConfigureAwait(false);
        }

        private async Task<TransformedSnapPoint> GetActiveModelPointNowAsync(Point cursor, CancellationToken cancellationToken)
        {
            return await UpdateSnapPoints().ContinueWith(_ => GetActiveModelPoint(cursor, cancellationToken)).ConfigureAwait(false);
        }

        private TransformedSnapPoint GetActiveModelPoint(Point cursor, CancellationToken cancellationToken)
        {
            return GetActiveSnapPoint(cursor, cancellationToken)
                ?? GetOrthoPoint(cursor)
                ?? GetAngleSnapPoint(cursor, cancellationToken)
                ?? GetRawModelPoint(cursor);
        }

        private TransformedSnapPoint GetActiveSnapPoint(Point cursor, CancellationToken cancellationToken)
        {
            if (Workspace.SettingsService.GetValue<bool>(WpfSettingsProvider.PointSnap) &&
                ((Workspace.InputService.AllowedInputTypes & InputType.Point) == InputType.Point) ||
                selectingRectangle)
            {
                var snapPointDistance = Workspace.SettingsService.GetValue<double>(WpfSettingsProvider.SnapPointDistance);
                var size = snapPointDistance * 2;
                var nearPoints = snapPointsQuadTree
                    .GetContainedItems(new Rect(cursor.X - snapPointDistance, cursor.Y - snapPointDistance, size, size));
                var points = nearPoints
                    .Select(p => Tuple.Create((cursor - p.ControlPoint).LengthSquared, p))
                    .OrderBy(t => t.Item1, new CancellableComparer<double>(cancellationToken));
                return points.FirstOrDefault()?.Item2;
            }

            return null;
        }

        private TransformedSnapPoint GetOrthoPoint(Point cursor)
        {
            if (Workspace.IsDrawing && Workspace.SettingsService.GetValue<bool>(WpfSettingsProvider.Ortho))
            {
                // if both are on the drawing plane
                var last = Workspace.InputService.LastPoint;
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

        private TransformedSnapPoint GetAngleSnapPoint(Point cursor, CancellationToken cancellationToken)
        {
            if (Workspace.IsDrawing && Workspace.SettingsService.GetValue<bool>(WpfSettingsProvider.AngleSnap))
            {
                // get distance to last point
                var last = Workspace.InputService.LastPoint;
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

                var points = new List<Tuple<double, TransformedSnapPoint>>();
                var snapAngleDistance = Workspace.SettingsService.GetValue<double>(WpfSettingsProvider.SnapAngleDistance);
                foreach (var snapAngle in Workspace.SettingsService.GetValue<double[]>(WpfSettingsProvider.SnapAngles))
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

                return points.OrderBy(p => p.Item1).FirstOrDefault()?.Item2;
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

        private SelectedEntity GetHitEntity(Point screenPoint)
        {
            var selectionRadius = Workspace.SettingsService.GetValue<double>(WpfSettingsProvider.EntitySelectionRadius);
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
                            AddHotPointIcon(el.StartPoint());
                            AddHotPointIcon(el.MidPoint());
                            AddHotPointIcon(el.EndPoint());
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
            var size = Workspace.SettingsService.GetValue<double>(WpfSettingsProvider.EntitySelectionRadius);
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
            SetAutoBinding(line, Shapes.Shape.StrokeProperty, nameof(BindObject.HotPointBrush));
            hotPointLayer.Children.Add(line);
        }

        private void SetAutoBinding(DependencyObject element, DependencyProperty property, string path)
        {
            var binding = new Binding() { Path = new PropertyPath(path) };
            binding.Source = BindObject;
            BindingOperations.SetBinding(element, property, binding);
        }

        private void DrawSnapPoint(TransformedSnapPoint snapPoint, long drawId)
        {
            lock (lastDrawnSnapPointIdGate)
            {
                if (drawId > lastDrawnSnapPointId)
                {
                    lastDrawnSnapPointId = drawId;
                    BeginInvoke(() =>
                    {
                        ClearSnapPoints();
                        var dist = (snapPoint.ControlPoint - lastPointerPosition).Length;
                        if (dist <= Workspace.SettingsService.GetValue<double>(WpfSettingsProvider.SnapPointDistance) && snapPoint.Kind != SnapPointKind.None)
                        {
                            var geometry = snapPointGeometry[snapPoint.Kind];
                            var scale = Workspace.SettingsService.GetValue<double>(WpfSettingsProvider.SnapPointSize);
                            Canvas.SetLeft(geometry, snapPoint.ControlPoint.X - geometry.ActualWidth * scale / 2.0);
                            Canvas.SetTop(geometry, snapPoint.ControlPoint.Y - geometry.ActualHeight * scale / 2.0);
                            geometry.Visibility = Visibility.Visible;
                        }
                    });
                }
            }
        }

        private FrameworkElement GetSnapGeometry(SnapPointKind kind)
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
                case SnapPointKind.Focus:
                    name = "FocusPointIcon";
                    break;
                default:
                    throw new ArgumentException("snapPoint.Kind");
            }

            if (name == null)
                return null;

            var geometry = (Canvas)SnapPointResources[name];
            geometry.Visibility = Visibility.Collapsed;
            SetAutoBinding(geometry, RenderTransformProperty, nameof(BindObject.SnapPointTransform));
            geometry.DataContext = BindObject;
            return geometry;
        }
    }
}
