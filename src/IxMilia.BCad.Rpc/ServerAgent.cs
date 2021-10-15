using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Display;
using IxMilia.BCad.EventArguments;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Plotting.Svg;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.Settings;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;

namespace IxMilia.BCad.Rpc
{
    public class ServerAgent
    {
        private IWorkspace _workspace;
        private DisplayInteractionManager _dim;
        private JsonRpc _jsonRpc { get; }

        public bool IsRunning { get; private set; }
        public double Width => _dim.Width;
        public double Height => _dim.Height;

        private bool _readyEventFired;
        public event EventHandler IsReady;

        public ServerAgent(IWorkspace workspace, JsonRpc rpc)
        {
            _workspace = workspace;
            _jsonRpc = rpc;
            IsRunning = true;
            _dim = new DisplayInteractionManager(workspace, ProjectionStyle.OriginTopLeft);
        }

        public void StartListening()
        {
            _dim.CurrentSnapPointUpdated += _dim_CurrentSnapPointUpdated;
            _dim.CursorStateUpdated += _dim_CursorStateUpdated;
            _dim.HotPointsUpdated += _dim_HotPointsUpdated;
            _dim.RubberBandPrimitivesChanged += _dim_RubberBandPrimitivesChanged;
            _dim.SelectionRectangleUpdated += _dim_SelectionRectangleUpdated;

            _workspace.InputService.PromptChanged += InputService_PromptChanged;
            _workspace.OutputService.LineWritten += OutputService_LineWritten;
            _workspace.SettingsService.SettingChanged += SettingsService_SettingChanged;
            _workspace.WorkspaceChanged += _workspace_WorkspaceChanged;

            _jsonRpc.StartListening();
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
            var fallBackColor = _workspace.SettingsService.GetValue<CadColor>(DisplaySettingsNames.BackgroundColor).GetAutoContrastingColor();
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
            var _ = _jsonRpc.NotifyAsync("ClientUpdate", clientUpdate);
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
            if (!_readyEventFired)
            {
                _readyEventFired = true;
                IsReady?.Invoke(this, new EventArgs());
            }
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
            return new ClientSettings(_workspace);
        }

        private ClientTransform GetDisplayTransform()
        {
            var transform = _workspace.ActiveViewPort.GetDisplayTransformDirect3DStyle(_dim.Width, _dim.Height);
            var transformArray = transform.Transform.ToTransposeArray();
            if (transformArray.Any(double.IsNaN))
            {
                return null;
            }

            var canvasTransform = _workspace.ActiveViewPort.GetTransformationMatrixWindowsStyle(_dim.Width, _dim.Height);
            var canvasTransformArray = canvasTransform.ToTransposeArray();
            if (canvasTransformArray.Any(double.IsNaN))
            {
                return null;
            }

            return new ClientTransform(transformArray, canvasTransformArray, transform.DisplayXScale, transform.DisplayYScale);
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

        public async Task ParseFile(string filePath, string data)
        {
            var bytes = Convert.FromBase64String(data);
            using var stream = new MemoryStream(bytes);

            var success = await _workspace.ReaderWriterService.TryReadDrawing(filePath, stream, out var drawing, out var viewPort);
            if (success)
            {
                _workspace.Update(drawing: drawing, activeViewPort: viewPort);
            }
        }

        public async Task<string> GetDrawingContents(string filePath, bool preserveSettings)
        {
            using var stream = new MemoryStream();
            var success = await _workspace.ReaderWriterService.TryWriteDrawing(filePath, _workspace.Drawing, _workspace.ActiveViewPort, stream, preserveSettings);
            if (success)
            {
                stream.Seek(0, SeekOrigin.Begin);
                var bytes = stream.ToArray();
                var contents = Convert.ToBase64String(bytes);
                return contents;
            }

            return null;
        }

        public string GetPlotPreview(ClientPlotSettings settings)
        {
            var htmlDialogService = (HtmlDialogService)_workspace.DialogService;
            var viewModel = (SvgPlotterViewModel)htmlDialogService.CreateAndPopulateViewModel(settings, plotTypeOverride: "svg"); // force to svg for preview
            viewModel.PlotAsDocument = false;
            viewModel.OutputWidth = settings.PreviewMaxSize;
            viewModel.OutputHeight = settings.PreviewMaxSize;
            if (settings.Width > settings.Height)
            {
                // keep width, reset height
                viewModel.OutputHeight = settings.Height / settings.Width * settings.PreviewMaxSize;
            }
            else
            {
                // keep height, reset width
                viewModel.OutputWidth = settings.Width / settings.Height * settings.PreviewMaxSize;
            }

            using (var stream = htmlDialogService.PlotToStream(viewModel))
            using (var reader = new StreamReader(stream))
            {
                var contents = reader.ReadToEnd();
                return contents;
            }
        }

        public async Task<ClientRectangle?> GetSelectionRectangle()
        {
            var rect = await _dim.GetSelectionRectangle();
            if (rect.HasValue)
            {
                return new ClientRectangle(rect.GetValueOrDefault().TopLeftScreen, rect.GetValueOrDefault().BottomRightScreen);
            }

            return null;
        }

        public void Undo()
        {
            _workspace.UndoRedoService.Undo();
        }

        public void Redo()
        {
            _workspace.UndoRedoService.Redo();
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
                case PrimitiveText text:
                    clientDrawing.Text.Add(new ClientText(text.Value, text.Location, text.Height, text.Rotation, primitiveColor));
                    break;
                case PrimitiveBezier bezier:
                    var lineSegments = 10;
                    var last = bezier.P1;
                    for (int i = 1; i <= lineSegments; i++)
                    {
                        var t = (double)i / lineSegments;
                        var next = bezier.ComputeParameterizedPoint(t);
                        clientDrawing.Lines.Add(new ClientLine(last, next, primitiveColor));
                        last = next;
                    }
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
            var result = await _jsonRpc.InvokeAsync<JObject>("ShowDialog", new { id, parameter });
            return result;
        }

        public void SetSetting(string name, string value)
        {
            _workspace.SettingsService.SetValue(name, value);
        }
    }
}
