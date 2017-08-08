// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace IxMilia.BCad.Commands
{
    [ExportCadCommand("Zoom.Extents", "ZOOMEXTENTS", "zoomextents", "ze")]
    internal class ZoomExtentsCommand : ICadCommand
    {
        public Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            var newVp = workspace.Drawing.ShowAllViewPort(
                workspace.ActiveViewPort.Sight,
                workspace.ActiveViewPort.Up,
                workspace.ViewControl.DisplayWidth,
                workspace.ViewControl.DisplayHeight);
            if (newVp != null)
            {
                workspace.Update(activeViewPort: newVp);
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }
    }
}
