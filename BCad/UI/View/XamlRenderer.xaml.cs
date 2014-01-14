using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using BCad.EventArguments;
using BCad.Extensions;
using BCad.Primitives;
using BCad.Services;

namespace BCad.UI.View
{
    /// <summary>
    /// Interaction logic for XamlRenderer.xaml
    /// </summary>
    public partial class XamlRenderer : UserControl, IRenderer
    {
        IViewControl ViewControl;
        private IWorkspace Workspace;
        private IInputService InputService;
        private Color AutoColor;
        private BindingClass BindObject = new BindingClass();

        private class BindingClass : INotifyPropertyChanged
        {
            private double thickness = 0.0;
            public double Thickness
            {
                get { return thickness; }
                set
                {
                    thickness = value;
                    OnPropertyChanged("Thickness");
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

        public XamlRenderer()
        {
            InitializeComponent();
        }

        public XamlRenderer(IViewControl viewControl, IWorkspace workspace, IInputService inputService)
            : this()
        {
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
            this.SizeChanged += (_, __) => RecalcTransform();
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
            if (Workspace == null || Workspace.ViewControl == null)
                return;
            var scale = Workspace.ViewControl.DisplayHeight / Workspace.ActiveViewPort.ViewHeight;
            var t = new TransformGroup();
            t.Children.Add(new TranslateTransform(-Workspace.ActiveViewPort.BottomLeft.X, -Workspace.ActiveViewPort.BottomLeft.Y));
            t.Children.Add(new ScaleTransform(scale, -scale));
            t.Children.Add(new TranslateTransform(0, Workspace.ViewControl.DisplayHeight));
            this.PrimitiveCanvas.RenderTransform = t;
            this.RubberBandCanvas.RenderTransform = t;
            var newThickness = 1.0 / scale;
            if (BindObject.Thickness != newThickness)
                BindObject.Thickness = newThickness;
            this.Dispatcher.BeginInvoke((Action)(() => RenderRubberBandLines()));
        }

        private void Redraw()
        {
            var start = DateTime.UtcNow;
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

            var end = DateTime.UtcNow;
            var elapsed = (end - start).TotalMilliseconds;
            Fps.Text = string.Format("Redraw time: {0} ms", elapsed);
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
            // project onto the drawing plane.  a render transform will take care of the display later
            // TODO: project onto view plane?
            var p1 = Workspace.DrawingPlane.ToXYPlane(line.P1);
            var p2 = Workspace.DrawingPlane.ToXYPlane(line.P2);
            var newLine = new Line() { X1 = p1.X, Y1 = p1.Y, X2 = p2.X, Y2 = p2.Y, Stroke = new SolidColorBrush(color) };
            SetThicknessBinding(newLine);
            canvas.Children.Add(newLine);
        }

        private void SetBinding(FrameworkElement element, string propertyName, DependencyProperty property)
        {
            var binding = new Binding(propertyName);
            binding.Source = BindObject;
            element.SetBinding(property, binding);
        }

        private void SetThicknessBinding(Shape shape)
        {
            SetBinding(shape, "Thickness", Shape.StrokeThicknessProperty);
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
