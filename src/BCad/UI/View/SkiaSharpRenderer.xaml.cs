// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.Settings;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace IxMilia.BCad.UI.View
{
    [ExportSetting(SelectedEntityDrawStyle, typeof(SelectedEntityDrawStyle), View.SelectedEntityDrawStyle.Glow)]
    internal class SkiaSharpSettings
    {
        public const string Prefix = "Skia.";
        public const string SelectedEntityDrawStyle = Prefix + nameof(SelectedEntityDrawStyle);
    }

    /// <summary>
    /// Interaction logic for SkiaSharpRenderer.xaml
    /// </summary>
    public partial class SkiaSharpRenderer : AbstractCadRenderer
    {
        public readonly IViewControl ViewControl;
        public readonly IWorkspace Workspace;

        private readonly SKPathEffect DashedLines;
        private CancellationTokenSource renderCancellationTokenSource = new CancellationTokenSource();

        public SkiaSharpRenderer(IViewControl viewControl, IWorkspace workspace)
        {
            ViewControl = viewControl;
            Workspace = workspace;
            InitializeComponent();
            DashedLines = SKPathEffect.CreateDash(new[] { 4.0f, 4.0f }, 0.0f);
            // TODO: tune these
            Workspace.CommandExecuted += (o, args) => Invalidate();
            Workspace.CommandExecuting += (o, args) => Invalidate();
            Workspace.RubberBandGeneratorChanged += (o, args) => Invalidate();
            Workspace.SelectedEntities.CollectionChanged += (o, args) => Invalidate();
            Workspace.SettingsService.SettingChanged += (o, args) => Invalidate();
            Workspace.WorkspaceChanged += (o, args) => Invalidate();
        }

        private void RendererLoaded(object sender, RoutedEventArgs e)
        {
            var presentationSource = PresentationSource.FromVisual(this);
            var matrix = presentationSource.CompositionTarget.TransformToDevice;
            RenderTransform = new ScaleTransform(matrix.M11, matrix.M22);
        }

        public override void Invalidate()
        {
            var oldTokenSource = renderCancellationTokenSource;
            renderCancellationTokenSource = new CancellationTokenSource();
            oldTokenSource.Cancel();
            // TODO: what if PaintSurface() re-enters before the previous one has cancelled?
            Dispatcher.BeginInvoke((Action)(() => element.InvalidateVisual()));
        }

        public override void UpdateRubberBandLines()
        {
            Invalidate();
        }

        private void PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var cancellationToken = renderCancellationTokenSource.Token;
            var selectedDrawStyle = Workspace.SettingsService.GetValue<SelectedEntityDrawStyle>(SkiaSharpSettings.SelectedEntityDrawStyle);
            var canvas = e.Surface.Canvas;
            var backgroundColor = Workspace.SettingsService.GetValue<CadColor>(WpfSettingsProvider.BackgroundColor);
            canvas.Clear(backgroundColor.ToSKColor());
            var transform = Workspace.ActiveViewPort.GetTransformationMatrixWindowsStyle(ActualWidth, ActualHeight);
            // TODO: canvas.SetMatrix() instead of transforming everything?  might make text rotation difficult
            using (var paint = new SKPaint())
            {
                try
                {
                    paint.IsAntialias = true;
                    paint.StrokeCap = SKStrokeCap.Butt;
                    var defaultColor = backgroundColor.GetAutoContrastingColor().ToSKColor();

                    // draw entities
                    foreach (var layer in Workspace.Drawing.GetLayers())
                    {
                        if (!layer.IsVisible)
                        {
                            continue;
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                        var layerColor = layer.Color.HasValue
                            ? layer.Color.GetValueOrDefault().ToSKColor()
                            : defaultColor;
                        foreach (var entity in layer.GetEntities())
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var entityColor = entity.Color.HasValue
                                ? entity.Color.GetValueOrDefault().ToSKColor()
                                : layerColor;
                            var isEntitySelected = Workspace.SelectedEntities.Contains(entity);
                            paint.PathEffect = isEntitySelected && selectedDrawStyle == SelectedEntityDrawStyle.Dashed
                                ? DashedLines
                                : null;
                            foreach (var primitive in entity.GetPrimitives())
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                var primitiveColor = primitive.Color.HasValue
                                    ? primitive.Color.GetValueOrDefault().ToSKColor()
                                    : entityColor;
                                paint.Color = primitiveColor;
                                if (isEntitySelected && selectedDrawStyle == SelectedEntityDrawStyle.Glow)
                                {
                                    var oldColor = paint.Color;
                                    var oldWidth = paint.StrokeWidth;
                                    paint.Color = paint.Color.WithAlpha(64);
                                    paint.StrokeWidth = 8.0f;
                                    DrawPrimitive(canvas, transform, paint, primitive);
                                    paint.Color = oldColor;
                                    paint.StrokeWidth = oldWidth;
                                }

                                DrawPrimitive(canvas, transform, paint, primitive);
                            }
                        }
                    }

                    // draw rubber band lines
                    var generator = Workspace.RubberBandGenerator;
                    if (generator != null)
                    {
                        paint.Color = defaultColor;
                        paint.PathEffect = null;
                        var cursorPoint = ViewControl.GetCursorPoint(cancellationToken).Result;
                        foreach (var primitive in generator(cursorPoint))
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            DrawPrimitive(canvas, transform, paint, primitive);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // don't allow this exception to escape; drawing should end gracefully
                }
            }
        }

        private void DrawPrimitive(SKCanvas canvas, Matrix4 transform, SKPaint paint, IPrimitive primitive)
        {
            var scale = ActualHeight / Workspace.ActiveViewPort.ViewHeight;
            var oldStrokeWidth = paint.StrokeWidth;
            switch (primitive.Kind)
            {
                case PrimitiveKind.Ellipse:
                    {
                        paint.IsStroke = true;
                        var el = (PrimitiveEllipse)primitive;
                        paint.StrokeWidth += (float)(el.Thickness * scale);
                        using (var path = new SKPath())
                        {
                            path.MoveTo(transform.Transform(el.StartPoint()).ToSKPoint());
                            var startAngle = (el.StartAngle + 1.0).CorrectAngleDegrees();
                            var endAngle = el.EndAngle.CorrectAngleDegrees();
                            if (endAngle < startAngle)
                            {
                                endAngle += MathHelper.ThreeSixty;
                            }

                            for (var angle = startAngle; angle <= endAngle; angle += 1.0)
                            {
                                path.LineTo(transform.Transform(el.GetPoint(angle)).ToSKPoint());
                            }

                            path.LineTo(transform.Transform(el.EndPoint()).ToSKPoint());
                            canvas.DrawPath(path, paint);
                        }
                        break;
                    }
                case PrimitiveKind.Line:
                    {
                        paint.IsStroke = true;
                        var line = (PrimitiveLine)primitive;
                        paint.StrokeWidth += (float)(line.Thickness * scale);
                        var p1 = transform.Transform(line.P1);
                        var p2 = transform.Transform(line.P2);
                        canvas.DrawLine((float)p1.X, (float)p1.Y, (float)p2.X, (float)p2.Y, paint);
                        break;
                    }
                case PrimitiveKind.Point:
                    {
                        paint.IsStroke = true;
                        var point = (PrimitivePoint)primitive;
                        var loc = transform.Transform(point.Location);
                        var pointSizeHalf = Workspace.SettingsService.GetValue<double>(WpfSettingsProvider.PointSize) * 0.5;
                        canvas.DrawLine((float)(loc.X - pointSizeHalf), (float)loc.Y, (float)(loc.X + pointSizeHalf), (float)loc.Y, paint);
                        canvas.DrawLine((float)loc.X, (float)(loc.Y - pointSizeHalf), (float)loc.X, (float)(loc.Y + pointSizeHalf), paint);
                        break;
                    }
                case PrimitiveKind.Text:
                    {
                        paint.IsStroke = false;
                        var text = (PrimitiveText)primitive;
                        var loc = transform.Transform(text.Location);
                        paint.TextSize = (float)((ActualHeight / Workspace.ActiveViewPort.ViewHeight) * text.Height);
                        canvas.Save();
                        canvas.RotateDegrees(-(float)text.Rotation, (float)loc.X, (float)loc.Y);
                        canvas.DrawText(text.Value, (float)loc.X, (float)loc.Y, paint);
                        canvas.Restore();
                        break;
                    }
            }

            paint.StrokeWidth = oldStrokeWidth;
        }
    }
}
