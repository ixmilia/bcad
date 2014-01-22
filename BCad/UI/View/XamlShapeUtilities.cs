using System;
using BCad.Extensions;
using BCad.Helpers;
using BCad.Primitives;

#if NETFX_CORE
// Metro
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using DisplayPoint = Windows.Foundation.Point;
using DisplaySize = Windows.Foundation.Size;
#else
// WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DisplayPoint = System.Windows.Point;
using DisplaySize = System.Windows.Size;
#endif

namespace BCad.UI.View
{
    internal static class XamlShapeUtilities
    {
        public static FrameworkElement CreateElementForCanvas(Matrix4 projection, IPrimitive primitive, IndexedColor color)
        {
            switch (primitive.Kind)
            {
                case PrimitiveKind.Ellipse:
                    return CreateEllipseForCanvas(projection, (PrimitiveEllipse)primitive, color);
                case PrimitiveKind.Line:
                    return CreateLineForCanvas(projection, (PrimitiveLine)primitive, color);
                case PrimitiveKind.Point:
                    return CreatePointForCanvas(projection, (PrimitivePoint)primitive, color);
                case PrimitiveKind.Text:
                    return CreateTextForCanvas(projection, (PrimitiveText)primitive, color);
                default:
                    throw new NotImplementedException();
            }
        }

        private static Shape CreateEllipseForCanvas(Matrix4 projection, PrimitiveEllipse ellipse, IndexedColor color)
        {
            // find axis endpoints
            var projected = ProjectionHelper.Project(ellipse, projection);
            var radiusX = projected.MajorAxis.Length;
            var radiusY = radiusX * projected.MinorAxisRatio;

            Shape shape;
            if (projected.StartAngle == 0.0 && projected.EndAngle == 360.0)
            {
                // full circle
                shape = new Ellipse() { Width = radiusX * 2.0, Height = radiusY * 2.0 };
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
                var topLeft = new Point(projected.Center.X - radiusX, projected.Center.Y - radiusY, 0.0);
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

            return shape;
        }

        private static Line CreateLineForCanvas(Matrix4 projection, PrimitiveLine line, IndexedColor color)
        {
            var p1 = projection.Transform(line.P1);
            var p2 = projection.Transform(line.P2);
            return new Line() { X1 = p1.X, Y1 = p1.Y, X2 = p2.X, Y2 = p2.Y };
        }

        private static Shape CreatePointForCanvas(Matrix4 projection, PrimitivePoint point, IndexedColor color)
        {
            const int size = 15;
            var loc = projection.Transform(point.Location);
            return new Path()
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
        }

        private static TextBlock CreateTextForCanvas(Matrix4 projection, PrimitiveText text, IndexedColor color)
        {
            var location = projection.Transform(text.Location);
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
            return t;
        }
    }
}
