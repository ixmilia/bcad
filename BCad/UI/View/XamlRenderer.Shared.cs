using System;
using System.ComponentModel;
using BCad.Entities;
using BCad.EventArguments;
using BCad.Extensions;
using BCad.Helpers;
using BCad.Primitives;

#if NETFX_CORE
// Metro
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using BCad.Metro.Extensions;
using Shapes = Windows.UI.Xaml.Shapes;
using DisplayPoint = Windows.Foundation.Point;
using DisplaySize = Windows.Foundation.Size;
#else
// WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Shapes = System.Windows.Shapes;
using DisplayPoint = System.Windows.Point;
using DisplaySize = System.Windows.Size;
#endif

namespace BCad.UI.View
{
    public partial class XamlRenderer : UserControl
    {
        private IWorkspace Workspace;
        private Matrix4 PlaneProjection;
        private BindingClass BindObject = new BindingClass();

        private class BindingClass : INotifyPropertyChanged
        {
            private double thickness = 0.0;
            public double Thickness
            {
                get { return thickness; }
                set
                {
                    if (thickness == value)
                        return;
                    thickness = value;
                    OnPropertyChanged("Thickness");
                }
            }

            private Brush autoBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            public Brush AutoBrush
            {
                get { return autoBrush; }
                set
                {
                    if (autoBrush == value)
                        return;
                    autoBrush = value;
                    OnPropertyChanged("AutoBrush");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string property)
            {
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(property));
                }
            }
        }

        public void Initialize(IWorkspace workspace)
        {
            this.Workspace = workspace;

            Workspace.WorkspaceChanged += Workspace_WorkspaceChanged;
            Workspace.SettingsManager.PropertyChanged += SettingsManager_PropertyChanged;

            this.Loaded += (_, __) =>
                {
                    foreach (var setting in new[] { Constants.BackgroundColorString })
                        SettingsManager_PropertyChanged(Workspace.SettingsManager, new PropertyChangedEventArgs(setting));

                    RecalcTransform();
                };
            this.SizeChanged += (_, __) => RecalcTransform();
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case Constants.BackgroundColorString:
                    this.Background = new SolidColorBrush(Workspace.SettingsManager.BackgroundColor.ToMediaColor());
                    var autoColor = Workspace.SettingsManager.BackgroundColor.GetAutoContrastingColor().ToMediaColor();
                    this.BindObject.AutoBrush = new SolidColorBrush(autoColor);
                    break;
            }
        }

        private void Workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            var redraw = false;
            if (e.IsActiveViewPortChange)
            {
                RecalcTransform();
            }
            if (e.IsDrawingChange)
            {
                redraw = true;
            }

            if (redraw)
            {
                BeginInvoke(Redraw);
            }
        }

        private void RecalcTransform()
        {
            if (Workspace == null || Workspace.ViewControl == null)
                return;

            var displayPlane = new Plane(Workspace.ActiveViewPort.BottomLeft, Workspace.ActiveViewPort.Sight);
            PlaneProjection = displayPlane.ToXYPlaneProjection();

            var scale = Workspace.ViewControl.DisplayHeight / Workspace.ActiveViewPort.ViewHeight;
            var t = new TransformGroup();
            t.Children.Add(new TranslateTransform() { X = -Workspace.ActiveViewPort.BottomLeft.X, Y = -Workspace.ActiveViewPort.BottomLeft.Y });
            t.Children.Add(new ScaleTransform() { ScaleX = scale, ScaleY = -scale });
            t.Children.Add(new TranslateTransform() { X = 0, Y = Workspace.ViewControl.DisplayHeight });
            this.PrimitiveCanvas.RenderTransform = t;
            BindObject.Thickness = 1.0 / scale;
        }

        private void BeginInvoke(Action action)
        {
#if !NETFX_CORE
                this.Dispatcher.BeginInvoke((Action)(() =>
                {
#endif
            action();
#if !NETFX_CORE
                }));
#endif
        }

        private void Redraw()
        {
            var drawing = Workspace.Drawing;
            this.PrimitiveCanvas.Children.Clear();
            foreach (var layer in drawing.GetLayers())
            {
                foreach (var entity in layer.GetEntities())
                {
                    foreach (var prim in entity.GetPrimitives())
                    {
                        AddPrimitive(prim, GetColor(prim.Color, layer.Color), entity);
                    }
                }
            }
        }

        private void AddPrimitive(IPrimitive prim, IndexedColor color, Entity containingEntity)
        {
            FrameworkElement element;
            switch (prim.Kind)
            {
                case PrimitiveKind.Ellipse:
                    element = CreatePrimitiveEllipse((PrimitiveEllipse)prim, color);
                    break;
                case PrimitiveKind.Line:
                    element = CreatePrimitiveLine((PrimitiveLine)prim, color);
                    break;
                case PrimitiveKind.Point:
                    element = CreatePrimitivePoint((PrimitivePoint)prim, color);
                    break;
                case PrimitiveKind.Text:
                    element = CreatePrimitiveText((PrimitiveText)prim, color);
                    break;
                default:
                    throw new NotImplementedException();
            }

            element.Tag = containingEntity;
            PrimitiveCanvas.Children.Add(element);
        }

        private Shape CreatePrimitiveEllipse(PrimitiveEllipse ellipse, IndexedColor color)
        {
            // find axis endpoints
            var projected = ProjectionHelper.Project(ellipse, PlaneProjection);
            var radiusX = projected.MajorAxis.Length;
            var radiusY = radiusX * projected.MinorAxisRatio;

            Shape shape;
            if (projected.StartAngle == 0.0 && projected.EndAngle == 360.0)
            {
                // full circle
                shape = new Shapes.Ellipse() { Width = radiusX * 2.0, Height = radiusY * 2.0 };
                shape.RenderTransform = new RotateTransform()
                {
                    Angle = Math.Atan2(projected.MajorAxis.Y, projected.MajorAxis.X) * MathHelper.RadiansToDegrees,
                    CenterX = radiusX,
                    CenterY = radiusY
                };
                Canvas.SetLeft(shape, projected.Center.X - radiusX);
                Canvas.SetTop(shape, projected.Center.Y - radiusY);
            }
            else
            {
                // arc
                var endAngle = ellipse.EndAngle;
                if (endAngle < ellipse.StartAngle) endAngle += 360.0;
                var startPoint = projected.GetStartPoint();
                var endPoint = projected.GetEndPoint();
                shape = new Path()
                {
                    Data = new GeometryGroup()
                    {
                        Children = new GeometryCollection()
                        {
                            new PathGeometry()
                            {
                                Figures = new PathFigureCollection()
                                {
                                    new PathFigure()
                                    {
                                        StartPoint = new DisplayPoint(startPoint.X, startPoint.Y),
                                        Segments = new PathSegmentCollection()
                                        {
                                            new ArcSegment()
                                            {
                                                IsLargeArc = (endAngle - ellipse.StartAngle) > 180.0,
                                                Point = new DisplayPoint(endPoint.X, endPoint.Y),
                                                SweepDirection = SweepDirection.Clockwise,
                                                RotationAngle = Math.Atan2(projected.MajorAxis.Y, projected.MajorAxis.X) * MathHelper.RadiansToDegrees,
                                                Size = new DisplaySize(radiusX, radiusY)
                                            }
                                        }
                                    },
                                }
                            }
                        }
                    }
                };
            }

            SetThicknessBinding(shape);
            SetColorBinding(shape, color);
            return shape;
        }

        private Shapes.Line CreatePrimitiveLine(PrimitiveLine line, IndexedColor color)
        {
            var p1 = PlaneProjection.Transform(line.P1);
            var p2 = PlaneProjection.Transform(line.P2);
            var newLine = new Shapes.Line() { X1 = p1.X, Y1 = p1.Y, X2 = p2.X, Y2 = p2.Y };
            SetThicknessBinding(newLine);
            SetColorBinding(newLine, color);
            return newLine;
        }

        private Path CreatePrimitivePoint(PrimitivePoint point, IndexedColor color)
        {
            const int size = 15;
            var loc = PlaneProjection.Transform(point.Location);
            var path = new Path()
            {
                Data = new GeometryGroup()
                {
                    Children = new GeometryCollection()
                    {
                        new PathGeometry()
                        {
                            Figures = new PathFigureCollection()
                            {
                                new PathFigure()
                                {
                                    StartPoint = new DisplayPoint(loc.X - size, loc.Y),
                                    Segments = new PathSegmentCollection()
                                    {
                                        new LineSegment()
                                        {
                                            Point = new DisplayPoint(loc.X + size, loc.Y)
                                        }
                                    }
                                },
                                new PathFigure()
                                {
                                    StartPoint = new DisplayPoint(loc.X, loc.Y - size),
                                    Segments = new PathSegmentCollection()
                                    {
                                        new LineSegment()
                                        {
                                            Point = new DisplayPoint(loc.X, loc.Y + size)
                                        }
                                    }
                                },
                            }
                        }
                    }
                }
            };
            SetThicknessBinding(path);
            SetColorBinding(path, color);
            return path;
        }

        private TextBlock CreatePrimitiveText(PrimitiveText text, IndexedColor color)
        {
            var location = PlaneProjection.Transform(text.Location);
            var t = new TextBlock();
            t.Text = text.Value;
            t.FontFamily = new FontFamily("Consolas");
            t.FontSize = text.Height * 0.75; // 0.75 = 72ppi/96dpi
            var trans = new TransformGroup();
            trans.Children.Add(new ScaleTransform() { ScaleX = 1, ScaleY = -1 });
            trans.Children.Add(new RotateTransform() { Angle = text.Rotation, CenterX = 0, CenterY = -text.Height });
            t.RenderTransform = trans;
            Canvas.SetLeft(t, location.X);
            Canvas.SetTop(t, location.Y + text.Height);
            if (color.IsAuto)
                SetBinding(t, "AutoBrush", TextBlock.ForegroundProperty);
            else
                t.Foreground = new SolidColorBrush(color.RealColor.ToMediaColor());
            return t;
        }

        private static IndexedColor GetColor(IndexedColor primitiveColor, IndexedColor layerColor)
        {
            return primitiveColor.IsAuto ? layerColor : primitiveColor;
        }

        private void SetBinding(FrameworkElement element, string propertyName, DependencyProperty property)
        {
            var binding = new Binding() { Path = new PropertyPath(propertyName) };
            binding.Source = BindObject;
            element.SetBinding(property, binding);
        }

        private void SetThicknessBinding(Shape shape)
        {
            SetBinding(shape, "Thickness", Shape.StrokeThicknessProperty);
        }

        private void SetColorBinding(Shape shape, IndexedColor color)
        {
            if (color.IsAuto)
            {
                SetBinding(shape, "AutoBrush", Shape.StrokeProperty);
            }
            else
            {
                shape.Stroke = new SolidColorBrush(color.RealColor.ToMediaColor());
            }
        }
    }
}
