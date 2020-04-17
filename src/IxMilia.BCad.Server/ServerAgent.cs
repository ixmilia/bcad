using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Display;
using IxMilia.BCad.EventArguments;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.Settings;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;

namespace IxMilia.BCad.Server
{
    public class ServerAgent
    {
        private IWorkspace _workspace;
        private DisplayInteractionManager _dim;
        private JsonRpc _rpc;

        public bool IsRunning { get; private set; }

        public ServerAgent(IWorkspace workspace, JsonRpc rpc)
        {
            _workspace = workspace;
            _rpc = rpc;
            IsRunning = true;
            _dim = new DisplayInteractionManager(workspace, ProjectionStyle.OriginTopLeft);
            _dim.CurrentSnapPointUpdated += _dim_CurrentSnapPointUpdated;
            _dim.CursorStateUpdated += _dim_CursorStateUpdated;
            _dim.HotPointsUpdated += _dim_HotPointsUpdated;
            _dim.RubberBandPrimitivesChanged += _dim_RubberBandPrimitivesChanged;
            _dim.SelectionRectangleUpdated += _dim_SelectionRectangleUpdated;

            _workspace.InputService.PromptChanged += InputService_PromptChanged;
            _workspace.OutputService.LineWritten += OutputService_LineWritten;
            _workspace.SettingsService.SettingChanged += SettingsService_SettingChanged;
            _workspace.WorkspaceChanged += _workspace_WorkspaceChanged;
        }

        private void _dim_CurrentSnapPointUpdated(object sender, TransformedSnapPoint? e)
        {
            if (e.HasValue)
            {
                var clientUpdate = new ClientUpdate();
                clientUpdate.TransformedSnapPoint = e;
                PushUpdate(clientUpdate);
            }
        }

        private void SettingsService_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            var clientUpdate = new ClientUpdate();
            clientUpdate.Settings = GetSettings();
            PushUpdate(clientUpdate);
        }

        private void InputService_PromptChanged(object sender, PromptChangedEventArgs e)
        {
            var clientUpdate = new ClientUpdate();
            clientUpdate.Prompt = e.Prompt;
            PushUpdate(clientUpdate);
        }

        private void OutputService_LineWritten(object sender, WriteLineEventArgs e)
        {
            var clientUpdate = new ClientUpdate();
            clientUpdate.OutputLines = new[] { e.Line };
            PushUpdate(clientUpdate);
        }

        private void _dim_CursorStateUpdated(object sender, CursorState e)
        {
            var clientUpdate = new ClientUpdate();
            clientUpdate.CursorState = e;
            PushUpdate(clientUpdate);
        }

        private void _dim_HotPointsUpdated(object sender, IEnumerable<Point> e)
        {
            var clientUpdate = new ClientUpdate();
            clientUpdate.HotPoints = e.Select(p => new ClientPoint(p)).ToArray();
            PushUpdate(clientUpdate);
        }

        private void _dim_RubberBandPrimitivesChanged(object sender, IEnumerable<IPrimitive> primitives)
        {
            var clientUpdate = new ClientUpdate();
            clientUpdate.RubberBandDrawing = new ClientDrawing(null);
            var fallBackColor = _workspace.SettingsService.GetValue<CadColor>(DisplaySettingsProvider.BackgroundColor).GetAutoContrastingColor();
            foreach (var primitive in primitives)
            {
                AddPrimitiveToDrawing(clientUpdate.RubberBandDrawing, primitive, fallBackColor);
            }

            PushUpdate(clientUpdate);
        }

        private void _dim_SelectionRectangleUpdated(object sender, SelectionState? e)
        {
            var clientUpdate = new ClientUpdate();
            clientUpdate.SelectionState = e;
            PushUpdate(clientUpdate);
        }

        private void _workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            var doUpdate = false;
            var clientUpdate = new ClientUpdate();

            if (e.IsDirtyChange)
            {
                doUpdate = true;
                clientUpdate.IsDirty = true;
            }

            if (e.IsActiveViewPortChange)
            {
                doUpdate = true;
                clientUpdate.Transform = GetDisplayTransform();
            }

            if (e.IsDrawingChange)
            {
                doUpdate = true;
                clientUpdate.Drawing = GetDrawing();
            }

            if (doUpdate)
            {
                PushUpdate(clientUpdate);
            }
        }

        private void PushUpdate(ClientUpdate clientUpdate)
        {
            _rpc.NotifyAsync("ClientUpdate", clientUpdate);
        }

        public void Ready(double width, double height)
        {
            _dim.Resize(width, height);
            var clientUpdate = new ClientUpdate()
            {
                Drawing = GetDrawing(),
                Settings = GetSettings(),
                Transform = GetDisplayTransform(),
            };
            PushUpdate(clientUpdate);
        }

        public void Resize(double width, double height)
        {
            _dim.Resize(width, height);
            var clientUpdate = new ClientUpdate()
            {
                Transform = GetDisplayTransform(),
            };
            PushUpdate(clientUpdate);
        }

        private ClientSettings GetSettings()
        {
            return new ClientSettings(_workspace.SettingsService);
        }

        private ClientTransform GetDisplayTransform()
        {
            var transform = _workspace.ActiveViewPort.GetDisplayTransformDirect3DStyle(_dim.Width, _dim.Height);
            return new ClientTransform(transform.Transform.ToTransposeArray(), transform.DisplayXScale, transform.DisplayYScale);
        }

        public void ChangeCurrentLayer(string currentLayer)
        {
            _workspace.Update(drawing: _workspace.Drawing.Update(currentLayerName: currentLayer));
        }

        public void Cancel()
        {
            _workspace.InputService.Cancel();
        }

        public void MouseDown(MouseButton button, double cursorX, double cursorY)
        {
            var _ = _dim.MouseDown(new Point(cursorX, cursorY, 0.0), button);
        }

        public void MouseUp(MouseButton button, double cursorX, double cursorY)
        {
            _dim.MouseUp(new Point(cursorX, cursorY, 0.0), button);
        }

        public void MouseMove(double cursorX, double cursorY)
        {
            _dim.MouseMove(new Point(cursorX, cursorY, 0.0));
        }

        public Task<bool> ExecuteCommand(string command)
        {
            return _workspace.ExecuteCommand(command);
        }

        public void SubmitInput(string value)
        {
            _dim.SubmitInput(value);
        }

        private ClientDrawing GetDrawing()
        {
            var drawing = _workspace.Drawing;
            var clientDrawing = new ClientDrawing(drawing.Settings.FileName);
            clientDrawing.CurrentLayer = drawing.CurrentLayerName;
            foreach (var layer in drawing.GetLayers())
            {
                clientDrawing.Layers.Add(layer.Name);
                if (layer.IsVisible)
                {
                    foreach (var entity in layer.GetEntities())
                    {
                        var entityColor = entity.Color ?? layer.Color;
                        foreach (var primitive in entity.GetPrimitives())
                        {
                            AddPrimitiveToDrawing(clientDrawing, primitive, fallBackColor: entityColor);
                        }
                    }
                }
            }

            clientDrawing.Layers.Sort();
            return clientDrawing;
        }

        private void AddPrimitiveToDrawing(ClientDrawing clientDrawing, IPrimitive primitive, CadColor? fallBackColor)
        {
            var primitiveColor = primitive.Color ?? fallBackColor;
            switch (primitive)
            {
                case PrimitiveEllipse ellipse:
                    var startAngle = ellipse.StartAngle.CorrectAngleDegrees();
                    var endAngle = ellipse.EndAngle.CorrectAngleDegrees();
                    if (endAngle <= startAngle)
                    {
                        endAngle += 360.0;
                    }
                    var transform = Matrix4.CreateScale(1.0, 1.0, 0.0) * ellipse.FromUnitCircle; // flatten display in z-plane
                    clientDrawing.Ellipses.Add(new ClientEllipse(startAngle, endAngle, transform.ToTransposeArray(), primitiveColor));
                    break;
                case PrimitiveLine line:
                    clientDrawing.Lines.Add(new ClientLine(line.P1, line.P2, primitiveColor));
                    break;
                case PrimitivePoint point:
                    clientDrawing.Points.Add(new ClientPointLocation(point.Location, primitiveColor));
                    break;
            }
        }

        public void Pan(double dx, double dy)
        {
            _dim.Pan(dx, dy);
        }

        public void Zoom(int cursorX, int cursorY, double delta)
        {
            var direction = delta < 0.0 ? ZoomDirection.Out : ZoomDirection.In;
            _dim.Zoom(direction, new Point(cursorX, cursorY, 0.0));
        }

        public async Task<JObject> ShowDialog(string id, object parameter)
        {
            var result = await _rpc.InvokeAsync<JObject>("ShowDialog", new { id, parameter });
            return result;
        }

        public void SetSetting(string name, string value)
        {
            _workspace.SettingsService.SetValue(name, value);
        }
    }
}
