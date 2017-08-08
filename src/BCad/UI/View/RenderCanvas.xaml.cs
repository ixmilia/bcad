// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.UI.Extensions;
using DisplayPoint = System.Windows.Point;
using DisplaySize = System.Windows.Size;
using P_Metadata = System.Windows.FrameworkPropertyMetadata;
using Shapes = System.Windows.Shapes;

namespace IxMilia.BCad.UI.View
{
    /// <summary>
    /// Interaction logic for RenderCanvas.xaml
    /// </summary>
    public partial class RenderCanvas : UserControl
    {
        public static readonly DependencyProperty ViewPortProperty =
            DependencyProperty.Register("ViewPort", typeof(ViewPort), typeof(RenderCanvas), new P_Metadata(ViewPort.CreateDefaultViewPort(), OnViewPortPropertyChanged));
        public static readonly DependencyProperty DrawingProperty =
            DependencyProperty.Register("Drawing", typeof(Drawing), typeof(RenderCanvas), new P_Metadata(new Drawing(), OnDrawingPropertyChanged));
        public static readonly DependencyProperty PointSizeProperty =
            DependencyProperty.Register("PointSize", typeof(double), typeof(RenderCanvas), new P_Metadata(15.0, OnPointSizePropertyChanged));
        public static readonly DependencyProperty SelectedEntitiesProperty =
            DependencyProperty.Register("SelectedEntities", typeof(ObservableHashSet<Entity>), typeof(RenderCanvas), new P_Metadata(new ObservableHashSet<Entity>(), OnSelectedEntitiesPropertyChanged));
        public static readonly DependencyProperty DefaultColorProperty =
            DependencyProperty.Register("DefaultColor", typeof(Color), typeof(RenderCanvas), new P_Metadata(default(Color), OnDefaultColorPropertyChanged));
        public static readonly DependencyProperty SupportedPrimitiveTypesProperty =
            DependencyProperty.Register("SupportedPrimitiveTypes", typeof(PrimitiveKind), typeof(RenderCanvas), new P_Metadata(PrimitiveKind.Ellipse | PrimitiveKind.Line | PrimitiveKind.Point | PrimitiveKind.Text, SupportedPrimitiveTypesPropertyChanged));
        public static readonly DependencyProperty CursorPointProperty =
            DependencyProperty.Register("CursorPoint", typeof(Point), typeof(RenderCanvas), new P_Metadata(new Point(), OnCursorPointPropertyChanged));
        public static readonly DependencyProperty RubberBandGeneratorProperty =
            DependencyProperty.Register("RubberBandGenerator", typeof(RubberBandGenerator), typeof(RenderCanvas), new P_Metadata(null, OnRubberBandGeneratorPropertyChanged));
        public static readonly DependencyProperty ColorOverrideProperty =
            DependencyProperty.Register("ColorOverride", typeof(CadColor?), typeof(RenderCanvas), new P_Metadata(null, OnColorOverridePropertyChanged));

        private static void OnViewPortPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = source as RenderCanvas;
            if (control != null)
            {
                control.RecalcTransform();
            }
        }

        private static void OnDrawingPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = source as RenderCanvas;
            if (control != null)
            {
                control.Redraw();
                control.CheckAllSelection();
            }
        }

        private static void OnPointSizePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = source as RenderCanvas;
            if (control != null)
            {
                control.RecalcTransform();
            }
        }

        private static void OnSelectedEntitiesPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = source as RenderCanvas;
            if (control != null)
            {
                var oldCollection = e.OldValue as ObservableHashSet<Entity>;
                var newCollection = e.NewValue as ObservableHashSet<Entity>;
                control.ResetCollectionChangedEvent(oldCollection, newCollection);
                control.CheckAllSelection();
            }
        }

        private static void OnDefaultColorPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var canvas = source as RenderCanvas;
            if (canvas != null)
            {
                canvas.Redraw();
            }
        }

        private static void SupportedPrimitiveTypesPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var canvas = source as RenderCanvas;
            if (canvas != null)
            {
                canvas.Redraw();
                canvas.CheckAllSelection();
            }
        }

        private static void OnCursorPointPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var canvas = source as RenderCanvas;
            if (canvas != null)
            {
                canvas.UpdateRubberBandLines();
            }
        }

        private static void OnRubberBandGeneratorPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var canvas = source as RenderCanvas;
            if (canvas != null)
            {
                canvas.UpdateRubberBandLines();
            }
        }

        private static void OnColorOverridePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var canvas = source as RenderCanvas;
            if (canvas != null)
            {
                canvas.Redraw();
                canvas.CheckAllSelection();
            }
        }

        private static DoubleCollection solidLineInstance = new DoubleCollection();
        private static DoubleCollection dashedLineInstance = new DoubleCollection() { 4.0, 4.0 };
        private Func<DoubleCollection> solidLineGenerator = () => solidLineInstance;
        private Func<DoubleCollection> dashedLineGenerator = () => dashedLineInstance;
        private BindingClass BindObject = new BindingClass();
        private Matrix4 PlaneProjection = Matrix4.Identity;
        private Dictionary<uint, IList<FrameworkElement>> entityMap = new Dictionary<uint, IList<FrameworkElement>>();
        private List<FrameworkElement> currentlySelected = new List<FrameworkElement>();

        public RenderCanvas()
        {
            InitializeComponent();
            this.SizeChanged += (_, __) => RecalcTransform();
            this.SelectedEntities.CollectionChanged += SelectedEntities_CollectionChanged;
        }

        private void ResetCollectionChangedEvent(ObservableHashSet<Entity> oldCollection, ObservableHashSet<Entity> newCollection)
        {
            if (oldCollection != null)
                oldCollection.CollectionChanged -= SelectedEntities_CollectionChanged;
            if (newCollection != null)
                newCollection.CollectionChanged += SelectedEntities_CollectionChanged;
        }

        private void SelectedEntities_CollectionChanged(object sender, EventArgs e)
        {
            CheckAllSelection();
        }

        public ViewPort ViewPort
        {
            get { return (ViewPort)GetValue(ViewPortProperty); }
            set { SetValue(ViewPortProperty, value); }
        }

        public Drawing Drawing
        {
            get { return (Drawing)GetValue(DrawingProperty); }
            set { SetValue(DrawingProperty, value); }
        }

        public double PointSize
        {
            get { return (double)GetValue(PointSizeProperty); }
            set { SetValue(PointSizeProperty, value); }
        }

        public ObservableHashSet<Entity> SelectedEntities
        {
            get { return (ObservableHashSet<Entity>)GetValue(SelectedEntitiesProperty); }
            set { SetValue(SelectedEntitiesProperty, value); }
        }

        public Color DefaultColor
        {
            get { return (Color)GetValue(DefaultColorProperty); }
            set { SetValue(DefaultColorProperty, value); }
        }

        public PrimitiveKind SupportedPrimitiveTypes
        {
            get { return (PrimitiveKind)GetValue(SupportedPrimitiveTypesProperty); }
            set { SetValue(SupportedPrimitiveTypesProperty, value); }
        }

        public Point CursorPoint
        {
            get { return (Point)GetValue(CursorPointProperty); }
            set { SetValue(CursorPointProperty, value); }
        }

        public RubberBandGenerator RubberBandGenerator
        {
            get { return (RubberBandGenerator)GetValue(RubberBandGeneratorProperty); }
            set { SetValue(RubberBandGeneratorProperty, value); }
        }

        public CadColor? ColorOverride
        {
            get { return (CadColor?)GetValue(ColorOverrideProperty); }
            set { SetValue(ColorOverrideProperty, value); }
        }

        private void RecalcTransform()
        {
            var displayPlane = new Plane(ViewPort.BottomLeft, ViewPort.Sight);
            PlaneProjection = displayPlane.ToXYPlaneProjection();

            var scale = ActualHeight / ViewPort.ViewHeight;
            var t = new TransformGroup();
            t.Children.Add(new TranslateTransform() { X = -ViewPort.BottomLeft.X, Y = -ViewPort.BottomLeft.Y });
            t.Children.Add(new ScaleTransform() { ScaleX = scale, ScaleY = -scale });
            t.Children.Add(new TranslateTransform() { X = 0, Y = ActualHeight });
            entityCanvas.RenderTransform = t;
            rubberBandCanvas.RenderTransform = t;

            BindObject.Thickness = 1.0 / scale;
            BindObject.Scale = new ScaleTransform() { ScaleX = PointSize / scale, ScaleY = PointSize / scale };
        }

        private void CheckAllSelection()
        {
            // unselect existing entities
            foreach (var selected in currentlySelected)
            {
                SetUnselected(selected);
            }

            currentlySelected.Clear();

            // select new entities
            var newSelected = SelectedEntities;
            IList<FrameworkElement> entities;
            foreach (var entity in newSelected)
            {
                if (entityMap.TryGetValue(entity.Id, out entities))
                {
                    foreach (var element in entities)
                    {
                        SetSelected(element);
                        currentlySelected.Add(element);
                    }
                }
            }
        }

        private void SetSelected(FrameworkElement element)
        {
            if (element is Shape)
            {
                ((Shape)element).StrokeDashArray = dashedLineGenerator();
            }
            else if (element is TextBlock)
            {
                element.Opacity = 0.5;
            }
            else if (element is Grid)
            {
                var grid = (Grid)element;
                if (grid.Children.Count == 1 && grid.Children[0] is Path)
                {
                    ((Path)grid.Children[0]).StrokeDashArray = dashedLineGenerator();
                }
                else
                {
                    Debug.Assert(false, "unexpected grid child");
                }
            }
            else
            {
                Debug.Assert(false, "unexpected canvas child");
            }
        }

        private void SetUnselected(FrameworkElement element)
        {
            if (element is Shape)
            {
                ((Shape)element).StrokeDashArray = solidLineGenerator();
            }
            else if (element is TextBlock)
            {
                element.Opacity = 1.0;
            }
            else if (element is Grid)
            {
                var grid = (Grid)element;
                if (grid.Children.Count == 1 && grid.Children[0] is Path)
                {
                    ((Path)grid.Children[0]).StrokeDashArray = solidLineGenerator();
                }
                else
                {
                    Debug.Assert(false, "unexpected grid child");
                }
            }
            else
            {
                Debug.Assert(false, "unexpected canvas child");
            }
        }

        private void Redraw()
        {
            this.entityCanvas.Children.Clear();
            this.entityMap.Clear();
            this.currentlySelected.Clear();
            var supported = SupportedPrimitiveTypes;
            var defaultColor = DefaultColor.ToColor();
            foreach (var layer in Drawing.GetLayers().Where(l => l.IsVisible))
            {
                var layerColor = layer.Color ?? defaultColor;
                foreach (var entity in layer.GetEntities())
                {
                    var entityColor = entity.Color ?? layerColor;
                    foreach (var prim in entity.GetPrimitives())
                    {
                        var primitiveColor = prim.Color ?? entityColor;
                        AddPrimitive(prim, primitiveColor, entity, supported, entityCanvas);
                    }
                }
            }
        }

        private void UpdateRubberBandLines()
        {
            this.rubberBandCanvas.Children.Clear();
            var generator = RubberBandGenerator;
            var supported = SupportedPrimitiveTypes;
            if (generator != null)
            {
                foreach (var prim in generator(CursorPoint))
                {
                    AddPrimitive(prim, prim.Color, null, supported, rubberBandCanvas);
                }
            }
        }

        private void AddPrimitive(IPrimitive prim, CadColor? color, Entity containingEntity, PrimitiveKind supported, Canvas canvas)
        {
            FrameworkElement element = null;
            switch (prim.Kind)
            {
                case PrimitiveKind.Ellipse:
                    if ((supported & PrimitiveKind.Ellipse) == PrimitiveKind.Ellipse)
                        element = CreatePrimitiveEllipse((PrimitiveEllipse)prim, color);
                    break;
                case PrimitiveKind.Line:
                    if ((supported & PrimitiveKind.Line) == PrimitiveKind.Line)
                        element = CreatePrimitiveLine((PrimitiveLine)prim, color);
                    break;
                case PrimitiveKind.Point:
                    if ((supported & PrimitiveKind.Point) == PrimitiveKind.Point)
                        element = CreatePrimitivePoint((PrimitivePoint)prim, color);
                    break;
                case PrimitiveKind.Text:
                    if ((supported & PrimitiveKind.Text) == PrimitiveKind.Text)
                        element = CreatePrimitiveText((PrimitiveText)prim, color);
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (element != null)
            {
                element.Tag = containingEntity;
                canvas.Children.Add(element);
                if (containingEntity != null)
                {
                    if (!entityMap.ContainsKey(containingEntity.Id))
                        entityMap.Add(containingEntity.Id, new List<FrameworkElement>());
                    entityMap[containingEntity.Id].Add(element);
                }
            }
        }

        private Shape CreatePrimitiveEllipse(PrimitiveEllipse ellipse, CadColor? color)
        {
            // rendering zero-radius ellipses (and arcs/circles) can cause the WPF layout engine to throw
            if (ellipse.MajorAxis.LengthSquared == 0.0)
                return null;

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
                var startPoint = projected.StartPoint();
                var endPoint = projected.EndPoint();
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
            SetColor(shape, color);
            return shape;
        }

        private Shapes.Line CreatePrimitiveLine(PrimitiveLine line, CadColor? color)
        {
            var p1 = PlaneProjection.Transform(line.P1);
            var p2 = PlaneProjection.Transform(line.P2);
            var newLine = new Shapes.Line() { X1 = p1.X, Y1 = p1.Y, X2 = p2.X, Y2 = p2.Y };
            SetThicknessBinding(newLine);
            SetColor(newLine, color);
            return newLine;
        }

        private Grid CreatePrimitivePoint(PrimitivePoint point, CadColor? color)
        {
            const double size = 0.5;
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
                                    StartPoint = new DisplayPoint(-size, 0.0),
                                    Segments = new PathSegmentCollection()
                                    {
                                        new LineSegment()
                                        {
                                            Point = new DisplayPoint(size, 0.0)
                                        }
                                    }
                                },
                                new PathFigure()
                                {
                                    StartPoint = new DisplayPoint(0.0, -size),
                                    Segments = new PathSegmentCollection()
                                    {
                                        new LineSegment()
                                        {
                                            Point = new DisplayPoint(0.0, size)
                                        }
                                    }
                                },
                            }
                        }
                    }
                },
                StrokeThickness = 1.0 / PointSize
            };
            SetBinding(path, "Scale", Path.RenderTransformProperty);
            SetColor(path, color);

            var grid = new Grid();
            grid.Children.Add(path);
            grid.RenderTransform = new TranslateTransform() { X = loc.X, Y = loc.Y };

            return grid;
        }

        private TextBlock CreatePrimitiveText(PrimitiveText text, CadColor? color)
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
            SetColor(t, TextBlock.ForegroundProperty, color);
            return t;
        }

        private void SetBinding(DependencyObject element, string propertyName, DependencyProperty property)
        {
            var binding = new Binding() { Path = new PropertyPath(propertyName) };
            binding.Source = BindObject;
            BindingOperations.SetBinding(element, property, binding);
        }

        private void SetThicknessBinding(Shape shape)
        {
            SetBinding(shape, "Thickness", Shape.StrokeThicknessProperty);
        }

        private void SetColor(Shape shape, CadColor? color)
        {
            SetColor(shape, Shape.StrokeProperty, color);
        }

        private void SetColor(FrameworkElement element, DependencyProperty property, CadColor? color)
        {
            if (color == null)
            {
                SetBinding(element, "AutoBrush", property);
            }
            else
            {
                var uicolor = ColorOverride.HasValue
                    ? ColorOverride.GetValueOrDefault().ToUIColor()
                    : Color.FromArgb(color.Value.A, color.Value.R, color.Value.G, color.Value.B);
                element.SetValue(property, new SolidColorBrush(uicolor));
            }
        }
    }
}
