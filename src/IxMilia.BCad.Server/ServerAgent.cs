﻿// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using IxMilia.BCad.Display;
using IxMilia.BCad.EventArguments;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;
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
            _dim.CursorStateUpdated += _dim_CursorStateUpdated;

            _workspace.WorkspaceChanged += _workspace_WorkspaceChanged;
        }

        private void _dim_CursorStateUpdated(object sender, CursorState e)
        {
            var clientUpdate = new ClientUpdate();
            clientUpdate.CursorState = e;
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
            _rpc.NotifyAsync("ClientUpdate", clientUpdate);
        }

        public void Ready(double width, double height)
        {
            _dim.Resize(width, height);
            var clientUpdate = new ClientUpdate()
            {
                Drawing = GetDrawing(),
                Transform = GetTransformMatrix(),
            };
            _rpc.NotifyAsync("ClientUpdate", clientUpdate);
        }

        private double[] GetTransformMatrix()
        {
            return _workspace.ActiveViewPort.GetTransformationMatrixDirect3DStyle(_dim.Width, _dim.Height).ToTransposeArray();
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

        private ClientDrawing GetDrawing()
        {
            var drawing = _workspace.Drawing;
            var clientDrawing = new ClientDrawing(drawing.Settings.FileName);
            var autoColor = CadColor.White;
            foreach (var layer in drawing.GetLayers())
            {
                var layerColor = layer.Color ?? autoColor;
                foreach (var entity in layer.GetEntities())
                {
                    var entityColor = entity.Color ?? layerColor;
                    foreach (var primitive in entity.GetPrimitives())
                    {
                        var primitiveColor = primitive.Color ?? entityColor;
                        switch (primitive)
                        {
                            case PrimitiveEllipse ellipse:
                                var startAngle = ellipse.StartAngle.CorrectAngleDegrees();
                                var endAngle = ellipse.EndAngle.CorrectAngleDegrees();
                                if (endAngle < startAngle)
                                {
                                    endAngle += 360.0;
                                }
                                clientDrawing.Ellipses.Add(new ClientEllipse(startAngle, endAngle, ellipse.FromUnitCircle.ToTransposeArray(), primitiveColor));
                                break;
                            case PrimitiveLine line:
                                clientDrawing.Lines.Add(new ClientLine(line.P1, line.P2, primitiveColor));
                                break;
                        }
                    }
                }
            }

            return clientDrawing;
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
    }
}
