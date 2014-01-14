using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using BCad.EventArguments;
using BCad.Extensions;
using BCad.Primitives;
using BCad.Services;

namespace BCad.UI.View
{
    internal class XamlRenderer : Grid, IRenderer
    {
        IViewControl ViewControl;
        private IWorkspace Workspace;
        private IInputService InputService;
        private Color AutoColor;
        private Matrix4 Transformation = Matrix4.Identity;
        private Canvas PrimitiveCanvas;
        private Canvas RubberBandCanvas;
        private IEnumerable<IPrimitive> rubberBandPrimitives;

        public XamlRenderer(IViewControl viewControl, IWorkspace workspace, IInputService inputService)
        {
            PrimitiveCanvas = new Canvas() { ClipToBounds = true };
            RubberBandCanvas = new Canvas() { ClipToBounds = true };
            RubberBandCanvas.Background = Brushes.Transparent;

            this.Children.Add(PrimitiveCanvas);
            this.Children.Add(RubberBandCanvas);

            ViewControl = viewControl;
            Workspace = workspace;
            InputService = inputService;

            Workspace.CommandExecuted += (_, __) => this.RubberBandCanvas.Children.Clear();
            Workspace.WorkspaceChanged += Workspace_WorkspaceChanged;
            Workspace.SettingsManager.PropertyChanged += SettingsManager_PropertyChanged;

            this.Loaded += (_, __) =>
                {
                    foreach (var setting in new[] { Constants.BackgroundColorString })
                        SettingsManager_PropertyChanged(Workspace.SettingsManager, new PropertyChangedEventArgs(setting));

                    RecalcTransform();
                };
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case Constants.BackgroundColorString:
                    this.Background = new SolidColorBrush(Workspace.SettingsManager.BackgroundColor.ToMediaColor());
                    AutoColor = Workspace.SettingsManager.BackgroundColor.GetAutoContrastingColor().ToMediaColor();
                    break;
            }
        }

        private void Workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            var redraw = false;
            if (e.IsActiveViewPortChange)
            {
                RecalcTransform();
                redraw = true;
            }
            if (e.IsDrawingChange)
            {
                redraw = true;
                this.RubberBandCanvas.Children.Clear();
            }

            if (redraw)
            {
                this.Dispatcher.BeginInvoke((Action)(() => Redraw()));
            }
        }

        private void RecalcTransform()
        {
            Transformation = Workspace.ActiveViewPort.GetTransformationMatrixWindowsStyle(this.ActualWidth, this.ActualHeight);
            this.Dispatcher.BeginInvoke((Action)(() => RenderRubberBandLines()));
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
                        AddPrimitive(this.PrimitiveCanvas, prim, GetDrawingColor(prim.Color, layer.Color));
                    }
                }
            }
        }

        private void AddPrimitive(Canvas canvas, IPrimitive prim, Color color)
        {
            switch (prim.Kind)
            {
                case PrimitiveKind.Ellipse:
                    break;
                case PrimitiveKind.Line:
                    AddPrimitiveLine(canvas, (PrimitiveLine)prim, color);
                    break;
                case PrimitiveKind.Point:
                    break;
                case PrimitiveKind.Text:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void AddPrimitiveLine(Canvas canvas, PrimitiveLine line, Color color)
        {
            var p1 = Transformation.Transform(line.P1);
            var p2 = Transformation.Transform(line.P2);
            var newLine = new Line() { X1 = p1.X, Y1 = p1.Y, X2 = p2.X, Y2 = p2.Y, Stroke = new SolidColorBrush(color) };
            canvas.Children.Add(newLine);
        }

        private Color GetDrawingColor(IndexedColor primitiveColor, IndexedColor layerColor)
        {
            if (primitiveColor.IsAuto)
            {
                if (layerColor.IsAuto)
                {
                    return AutoColor;
                }
                else
                {
                    return layerColor.RealColor.ToMediaColor();
                }
            }
            else
            {
                return primitiveColor.RealColor.ToMediaColor();
            }
        }

        public void RenderRubberBandLines()
        {
            this.RubberBandCanvas.Children.Clear();
            var generator = InputService.PrimitiveGenerator;
            if (generator != null)
            {
                var primitives = generator(ViewControl.GetCursorPoint());
                foreach (var prim in primitives)
                {
                    AddPrimitive(this.RubberBandCanvas, prim, GetDrawingColor(prim.Color, IndexedColor.Auto));
                }
            }
        }
    }
}
