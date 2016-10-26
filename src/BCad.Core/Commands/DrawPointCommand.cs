// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using BCad.Entities;

namespace BCad.Commands
{
    [ExportCadCommand("Draw.Point", "POINT", "point", "p")]
    public class DrawPointCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var location = await workspace.InputService.GetPoint(new UserDirective("Location"));
            if (location.Cancel) return false;
            if (!location.HasValue) return true;
            workspace.AddToCurrentLayer(new Location(location.Value, null));
            return true;
        }
    }
}
