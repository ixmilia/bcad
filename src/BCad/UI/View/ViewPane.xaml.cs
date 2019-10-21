// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

using IxMilia.BCad.EventArguments;
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
using PropertyPath = System.Windows.PropertyPath;
using ResourceDictionary = System.Windows.ResourceDictionary;
using Shapes = System.Windows.Shapes;
using UIElement = System.Windows.UIElement;
using Visibility = System.Windows.Visibility;
using IxMilia.BCad.Display;
using System.Windows.Input;
using System.Windows;

namespace IxMilia.BCad.UI.View
{
    /// <summary>
    /// Interaction logic for ViewPane.xaml
    /// </summary>
    public partial class ViewPane : UserControl
    {
        private DisplayInteractionManager dim;
        private AbstractCadRenderer _renderer;
        private DoubleCollection solidLine = new DoubleCollection();
        private DoubleCollection dashedLine = new DoubleCollection() { 4.0, 4.0 };
        private ResourceDictionary resources;
        private Matrix4 _transform;

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

            ClipToBounds = true;
            clicker.Cursor = Cursors.None;
            CompositionContainer.Container.SatisfyImports(this);
        }

        [OnImportsSatisfied]
        public void OnImportsSatisfied()
        {
            dim = new DisplayInteractionManager(Workspace, ProjectionStyle.OriginTopLeft);
            dim.CursorStateUpdated += (sender, args) => Invoke(() => CursorStateUpdated(sender, args));
            dim.CurrentSnapPointUpdated += (sender, args) => Invoke(() => CurrentSnapPointUpdated(sender, args));
            dim.CursorDisplayLocationUpdated += (_, args) => { BindObject.CursorScreen = args; Invoke(UpdateCursorLocation); };
            dim.CursorWorldLocationUpdated += (_, args) => { BindObject.CursorWorld = args; _renderer.UpdateRubberBandLines(); };
            dim.HotPointsUpdated += (sender, args) => Invoke(() => UpdateHotPoints(sender, args));
            dim.SelectionRectangleUpdated += (sender, args) => Invoke(() => UpdateSelectionLines(sender, args));
            clicker.MouseMove += DimMouseMove;
            clicker.MouseDown += DimMouseDown;
            clicker.MouseUp += DimMouseUp;
            clicker.MouseWheel += DimMouseWheel;
            clicker.SizeChanged += DimSizeChanged;

            BindObject = new BindingClass(Workspace);
            DataContext = BindObject;
            Workspace.WorkspaceChanged += Workspace_WorkspaceChanged;
            Workspace.SettingsService.SettingChanged += SettingsService_SettingChanged;

            SettingsService_SettingChanged(this, new SettingChangedEventArgs(string.Empty, typeof(object), null, null));

            // prepare snap point icons
            foreach (var kind in new[] { SnapPointKind.Center, SnapPointKind.EndPoint, SnapPointKind.MidPoint, SnapPointKind.Quadrant, SnapPointKind.Focus })
            {
                snapPointGeometry[kind] = GetSnapGeometry(kind);
                snapLayer.Children.Add(snapPointGeometry[kind]);
            }
        }

        private void DimMouseMove(object sender, PointerEventArgs args)
        {
            var pos = GetPointerPosition(args);
            Task.Run(() => dim.MouseMove(pos));
        }

        private void DimMouseDown(object sender, PointerButtonEventArgs args)
        {
            var pos = GetPointerPosition(args);
            var button = GetMouseButton(args);
            Task.Run(() => dim.MouseDown(pos, button));
        }

        private void DimMouseUp(object sender, PointerButtonEventArgs args)
        {
            var pos = GetPointerPosition(args);
            var button = GetMouseButton(args);
            Task.Run(() => dim.MouseUp(pos, button));
        }

        private void DimMouseWheel(object sender, MouseWheelEventArgs args)
        {
            var pos = GetPointerPosition(args);
            var direction = args.Delta < 0.0 ? MouseWheelDirection.Down : MouseWheelDirection.Up;
            Task.Run(() => dim.MouseWheel(pos, direction));
        }

        private void DimSizeChanged(object sender, SizeChangedEventArgs args)
        {
            Task.Run(() => dim.Resize(args.NewSize.Width, args.NewSize.Height));
        }

        private void CurrentSnapPointUpdated(object sender, TransformedSnapPoint? e)
        {
            if (e.HasValue)
            {
                var snapPoint = e.GetValueOrDefault();
                var geometry = snapPointGeometry[snapPoint.Kind];
                var scale = Workspace.SettingsService.GetValue<double>(DisplaySettingsProvider.SnapPointSize);
                Canvas.SetLeft(geometry, snapPoint.ControlPoint.X - geometry.ActualWidth * scale / 2.0);
                Canvas.SetTop(geometry, snapPoint.ControlPoint.Y - geometry.ActualHeight * scale / 2.0);
                geometry.Visibility = Visibility.Visible;
            }
            else
            {
                foreach (UIElement child in snapLayer.Children)
                {
                    child.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void CursorStateUpdated(object sender, CursorState e)
        {
            pointCursor.Visibility = (e & CursorState.Point) == CursorState.Point ? Visibility.Visible : Visibility.Collapsed;
            entityCursor.Visibility = (e & CursorState.Object) == CursorState.Object ? Visibility.Visible : Visibility.Collapsed;
            textCursor.Visibility = (e & CursorState.Text) == CursorState.Text ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SettingsService_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.SettingName) || e.SettingName == WpfSettingsProvider.RendererId)
            {
                SetRenderer();
            }
            if (string.IsNullOrEmpty(e.SettingName) || e.SettingName == DisplaySettingsProvider.BackgroundColor)
            {
                var autoColor = Workspace.SettingsService.GetValue<CadColor>(DisplaySettingsProvider.BackgroundColor).GetAutoContrastingColor();
                var selectionColor = autoColor.With(a: 25);
                var autoColorUI = autoColor.ToUIColor();
                BindObject.AutoBrush = new SolidColorBrush(autoColorUI);
                BindObject.SelectionBrush = new SolidColorBrush(selectionColor.ToUIColor());
            }
            if (string.IsNullOrEmpty(e.SettingName) || e.SettingName == DisplaySettingsProvider.CursorSize)
            {
                var cursorSize = Workspace.SettingsService.GetValue<int>(DisplaySettingsProvider.CursorSize) / 2.0 + 0.5;
                BindObject.LeftCursorExtent = new Point(-cursorSize, 0, 0);
                BindObject.RightCursorExtent = new Point(cursorSize, 0, 0);
                BindObject.TopCursorExtent = new Point(0, -cursorSize, 0);
                BindObject.BottomCursorExtent = new Point(0, cursorSize, 0);

                // only update the cursor location after the previous four binding calls have appropriately propagated
                // to the UI
                Invoke(UpdateCursorLocation);
            }
            if (string.IsNullOrEmpty(e.SettingName) || e.SettingName == DisplaySettingsProvider.EntitySelectionRadius)
            {
                var entitySize = Workspace.SettingsService.GetValue<double>(DisplaySettingsProvider.EntitySelectionRadius);
                BindObject.EntitySelectionTopLeft = new Point(-entitySize, -entitySize, 0);
                BindObject.EntitySelectionTopRight = new Point(entitySize, -entitySize, 0);
                BindObject.EntitySelectionBottomLeft = new Point(-entitySize, entitySize, 0);
                BindObject.EntitySelectionBottomRight = new Point(entitySize, entitySize, 0);

                // only update the cursor location after the previous four binding calls have appropriately propagated
                // to the UI
                Invoke(UpdateCursorLocation);
            }
            if (string.IsNullOrEmpty(e.SettingName) || e.SettingName == DisplaySettingsProvider.TextCursorSize)
            {
                var textSize = Workspace.SettingsService.GetValue<int>(DisplaySettingsProvider.TextCursorSize) / 2.0 + 0.5;
                BindObject.TextCursorStart = new Point(0, -textSize, 0);
                BindObject.TextCursorStart = new Point(0, textSize, 0);

                // only update the cursor location after the previous two binding calls have appropriately propagated
                // to the UI
                Invoke(UpdateCursorLocation);
            }
            if (string.IsNullOrEmpty(e.SettingName) || e.SettingName == DisplaySettingsProvider.HotPointColor)
            {
                BindObject.HotPointBrush = new SolidColorBrush(Workspace.SettingsService.GetValue<CadColor>(DisplaySettingsProvider.HotPointColor).ToUIColor());
            }
            if (string.IsNullOrEmpty(e.SettingName) ||
                e.SettingName == DisplaySettingsProvider.SnapPointColor ||
                e.SettingName == DisplaySettingsProvider.SnapPointSize)
            {
                var snapPointSize = Workspace.SettingsService.GetValue<double>(DisplaySettingsProvider.SnapPointSize);
                BindObject.SnapPointTransform = new ScaleTransform() { ScaleX = snapPointSize, ScaleY = snapPointSize };
                BindObject.SnapPointBrush = new SolidColorBrush(Workspace.SettingsService.GetValue<CadColor>(DisplaySettingsProvider.SnapPointColor).ToUIColor());
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
                _renderer = factory.Value.CreateRenderer(Workspace.ViewControl, Workspace);
            }

            renderControl.Content = _renderer;
        }

        private void Invoke(Action action)
        {
            Dispatcher.Invoke(action, DispatcherPriority.Background);
        }

        private void Workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            if (e.IsDrawingChange)
            {
                BindObject.Refresh();
            }

            if (e.IsActiveViewPortChange)
            {
                _transform = Workspace.ActiveViewPort.GetTransformationMatrixWindowsStyle(dim.Width, dim.Height);
            }
        }

        private Point GetPointerPosition(PointerEventArgs e)
        {
            return e.GetPosition(clicker).ToPoint();
        }

        private Display.MouseButton GetMouseButton(PointerButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    return Display.MouseButton.Left;
                case MouseButton.Middle:
                    return Display.MouseButton.Middle;
                case MouseButton.Right:
                    return Display.MouseButton.Right;
                default:
                    // TODO
                    return Display.MouseButton.Left;
            }
        }

        private void UpdateCursorLocation()
        {
            foreach (var cursorImage in new[] { pointCursor, entityCursor, textCursor})
            {
                Canvas.SetLeft(cursorImage, (int)(BindObject.CursorScreen.X - (cursorImage.ActualWidth / 2.0)));
                Canvas.SetTop(cursorImage, (int)(BindObject.CursorScreen.Y - (cursorImage.ActualHeight / 2.0)));
            }
        }

        private void UpdateSelectionLines(object sender, SelectionState? e)
        {
            if (e.HasValue)
            {
                var state = e.GetValueOrDefault();
                var rect = state.Rectangle;

                var topLeft = new Point(rect.Left, rect.Top, 0.0);
                var topRight = new Point(rect.Left + rect.Width, rect.Top, 0.0);
                var bottomLeft = new Point(rect.Left, rect.Top + rect.Height, 0.0);
                var bottomRight = new Point(rect.Left + rect.Width, rect.Top + rect.Height, 0.0);

                selectionLine1.X1 = topLeft.X;
                selectionLine1.Y1 = topLeft.Y;
                selectionLine1.X2 = topRight.X;
                selectionLine1.Y2 = topRight.Y;

                selectionLine2.X1 = topRight.X;
                selectionLine2.Y1 = topRight.Y;
                selectionLine2.X2 = bottomRight.X;
                selectionLine2.Y2 = bottomRight.Y;

                selectionLine3.X1 = bottomRight.X;
                selectionLine3.Y1 = bottomRight.Y;
                selectionLine3.X2 = bottomLeft.X;
                selectionLine3.Y2 = bottomLeft.Y;

                selectionLine4.X1 = bottomLeft.X;
                selectionLine4.Y1 = bottomLeft.Y;
                selectionLine4.X2 = topLeft.X;
                selectionLine4.Y2 = topLeft.Y;

                var dash = state.Mode == Display.SelectionMode.PartialEntity
                    ? dashedLine
                    : solidLine;
                SetStrokeDashArray(selectionLine1, dash);
                SetStrokeDashArray(selectionLine2, dash);
                SetStrokeDashArray(selectionLine3, dash);
                SetStrokeDashArray(selectionLine4, dash);

                selectionRect.Width = rect.Width;
                selectionRect.Height = rect.Height;
                Canvas.SetLeft(selectionRect, rect.Left);
                Canvas.SetTop(selectionRect, rect.Top);
                selectionRect.Visibility = Visibility.Visible;

                foreach (var l in new[] { selectionLine1, selectionLine2, selectionLine3, selectionLine4 })
                {
                    l.Visibility = Visibility.Visible;
                }
            }
            else
            {
                selectionRect.Visibility = Visibility.Collapsed;
                foreach (var l in new[] { selectionLine1, selectionLine2, selectionLine3, selectionLine4 })
                {
                    l.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void SetStrokeDashArray(Shapes.Shape shape, DoubleCollection collection)
        {
            shape.StrokeDashArray = collection;
        }

        private void UpdateHotPoints(object sender, IEnumerable<Point> points)
        {
            foreach (var point in points)
            {
                AddHotPointIcon(point);
            }
        }

        private void AddHotPointIcon(Point location)
        {
            var screen = _transform.Transform(location);
            var size = Workspace.SettingsService.GetValue<double>(DisplaySettingsProvider.EntitySelectionRadius);
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
