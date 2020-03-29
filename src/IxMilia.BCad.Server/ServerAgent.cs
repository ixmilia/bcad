// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
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
                clientUpdate.Transform = GetTransformMatrix();
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

        public void ServerUpdate(ServerUpdate serverUpdate)
        {
            double[] transform = null;
            if (serverUpdate.DisplayUpdate.HasValue)
            {
                switch (serverUpdate.DisplayUpdate.GetValueOrDefault())
                {
                    case DisplayUpdate.ZoomIn:
                    case DisplayUpdate.ZoomOut:
                        break;
                }
            }

            var clientUpdate = new ClientUpdate();
            clientUpdate.Transform = transform;
            PushUpdate(clientUpdate);
        }

        public void Ready(double width, double height)
        {
            _dim.Resize(width, height);
            var clientUpdate = new ClientUpdate()
            {
                Drawing = GetDrawing(),
                Settings = GetSettings(),
                Transform = GetTransformMatrix(),
            };
            PushUpdate(clientUpdate);
        }

        public void Resize(double width, double height)
        {
            _dim.Resize(width, height);
            var clientUpdate = new ClientUpdate()
            {
                Transform = GetTransformMatrix(),
            };
            PushUpdate(clientUpdate);
        }

        private ClientSettings GetSettings()
        {
            return new ClientSettings()
            {
                BackgroundColor = _workspace.SettingsService.GetValue<CadColor>(DisplaySettingsProvider.BackgroundColor),
                CursorSize = _workspace.SettingsService.GetValue<int>(DisplaySettingsProvider.CursorSize),
                Debug = _workspace.SettingsService.GetValue<bool>(DefaultSettingsProvider.Debug),
                EntitySelectionRadius = _workspace.SettingsService.GetValue<double>(DisplaySettingsProvider.EntitySelectionRadius),
                HotPointColor = _workspace.SettingsService.GetValue<CadColor>(DisplaySettingsProvider.HotPointColor),
                SnapPointColor = _workspace.SettingsService.GetValue<CadColor>(DisplaySettingsProvider.SnapPointColor),
                SnapPointSize = _workspace.SettingsService.GetValue<double>(DisplaySettingsProvider.SnapPointSize),
                TextCursorSize = _workspace.SettingsService.GetValue<int>(DisplaySettingsProvider.TextCursorSize),
            };
        }

        private double[] GetTransformMatrix()
        {
            return _workspace.ActiveViewPort.GetTransformationMatrixDirect3DStyle(_dim.Width, _dim.Height).ToTransposeArray();
        }

        public void ChangeCurrentLayer(string currentLayer)
        {
            _workspace.Update(drawing: _workspace.Drawing.Update(currentLayerName: currentLayer));
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
            var autoColor = _workspace.SettingsService.GetValue<CadColor>(DisplaySettingsProvider.BackgroundColor).GetAutoContrastingColor();
            foreach (var layer in drawing.GetLayers())
            {
                clientDrawing.Layers.Add(layer.Name);
                if (layer.IsVisible)
                {
                    var layerColor = layer.Color ?? autoColor;
                    foreach (var entity in layer.GetEntities())
                    {
                        var entityColor = entity.Color ?? layerColor;
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

        private void AddPrimitiveToDrawing(ClientDrawing clientDrawing, IPrimitive primitive, CadColor fallBackColor)
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
    }
}
