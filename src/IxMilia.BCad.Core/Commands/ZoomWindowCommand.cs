// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Primitives;

namespace IxMilia.BCad.Commands
{
    [ExportCadCommand("Zoom.Window", "ZOOMWINDOW", "zoomwindow", "zw")]
    internal class ZoomWindowCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            var selection = await workspace.ViewControl.GetSelectionRectangle();
            if (selection == null)
                return false;

            var transform = workspace.ActiveViewPort.GetTransformationMatrixWindowsStyle(workspace.ViewControl.DisplayWidth, workspace.ViewControl.DisplayHeight);
            var newVp = GetBoundingPrimitives(selection.TopLeftWorld, selection.BottomRightWorld).ShowAllViewPort(
                workspace.ActiveViewPort.Sight,
                workspace.ActiveViewPort.Up,
                workspace.ViewControl.DisplayWidth,
                workspace.ViewControl.DisplayHeight,
                pixelBuffer: 0);

            if (newVp == null)
                return false;

            workspace.Update(activeViewPort: newVp);
            return true;
        }

        private static PrimitiveLine[] GetBoundingPrimitives(Point p1, Point p2)
        {
            var a = p1;
            var b = new Point(p1.X, p2.Y, p1.Z);
            var c = new Point(p2.X, p1.Y, p2.Z);
            var d = p2;
            return new[]
            {
                new PrimitiveLine(a, b),
                new PrimitiveLine(a, c),
                new PrimitiveLine(b, d),
                new PrimitiveLine(c, d)
            };
        }
    }
}
