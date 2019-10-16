// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using IxMilia.BCad.Entities;
using System.Threading.Tasks;
using StreamJsonRpc;

namespace IxMilia.BCad.Server
{
    public class ServerAgent
    {
        private IWorkspace _workspace;
        internal JsonRpc Rpc { get; set; }
        public bool IsRunning { get; private set; }

        public ServerAgent(IWorkspace workspace)
        {
            _workspace = workspace;
            IsRunning = true;
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
            Rpc.InvokeAsync<ClientUpdate>("ClientUpdate", clientUpdate);
        }

        public void Ready()
        {
            System.Diagnostics.Debugger.Launch();
            var clientUpdate = new ClientUpdate()
            {
                Drawing = GetDrawing(),
                Transform = GetTransform(),
            };
            Rpc.InvokeAsync<ClientUpdate>("ClientUpdate", clientUpdate);
        }

        public Task<bool> ExecuteCommand(string command, int x)
        {
            return _workspace.ExecuteCommand(command);
        }

        private double[] GetTransform()
        {
            var t = _workspace.ActiveViewPort.GetTransformationMatrixDirect3DStyle(1280, 720);
            return new[]
            {
                t.M11, t.M12, t.M13, t.M14,
                t.M21, t.M22, t.M23, t.M24,
                t.M31, t.M32, t.M33, t.M34,
                t.M41, t.M42, t.M43, t.M44,
            };
        }

        private ClientDrawing GetDrawing()
        {
            return new ClientDrawing()
            {
                Lines = _workspace.Drawing.GetEntities().OfType<Line>().Select(line => new ClientLine(ClientPoint.FromPoint(line.P1), ClientPoint.FromPoint(line.P2))),
            };
        }

        public ViewPort GetViewPort()
        {
            return _workspace.ActiveViewPort;
        }

        public void Pan(double width, double height, double dx, double dy)
        {
            var vp = _workspace.ActiveViewPort;
            var scale = vp.ViewHeight / height;
            dx = vp.BottomLeft.X + dx * scale;
            dy = vp.BottomLeft.Y - dy * scale;
            _workspace.Update(activeViewPort: vp.Update(bottomLeft: new Point(dx, dy, vp.BottomLeft.Z)));
        }

        public void Zoom(int cursorX, int cursorY, double width, double height, double delta)
        {
            // scale everything
            var scale = 1.25;
            if (delta > 0) scale = 0.8; // 1.0 / 1.25

            // center zoom operation on mouse
            var cursorPoint = new Point(cursorX, cursorY, 0.0);
            var vp = _workspace.ActiveViewPort;
            var oldHeight = vp.ViewHeight;
            var oldWidth = width * oldHeight / height;
            var newHeight = oldHeight * scale;
            var newWidth = oldWidth * scale;
            var heightDelta = newHeight - oldHeight;
            var widthDelta = newWidth - oldWidth;

            var relHoriz = cursorPoint.X / width;
            var relVert = (height - cursorPoint.Y) / height;
            var botLeftDelta = new Vector(relHoriz * widthDelta, relVert * heightDelta, 0.0);
            var newVp = vp.Update(
                bottomLeft: (Point)(vp.BottomLeft - botLeftDelta),
                viewHeight: vp.ViewHeight * scale);
            _workspace.Update(activeViewPort: newVp);
        }

        //public void MouseDown(MouseDownArgs args, int garbage)
        //{
        //    switch (args.Button)
        //    {
        //        case MouseButton.Left:
        //            var matrix = _workspace.ActiveViewPort.GetTransformationMatrixWindowsStyle(args.Width, args.Height);
        //            var inv = matrix.Inverse();
        //            var modelPoint = inv.Transform(args.Cursor);
        //            _workspace.InputService.PushPoint(modelPoint);
        //            break;
        //        case MouseButton.Middle:
        //            break;
        //        case MouseButton.Right:
        //            _workspace.InputService.PushNone();
        //            break;
        //    }
        //}
    }
}
