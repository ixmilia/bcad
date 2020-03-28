// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace IxMilia.BCad.Commands
{
    [ExportCadCommand("File.Plot", "PLOT", ModifierKeys.Control, Key.P, "plot")]
    public class PlotCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var result = (bool)(await workspace.DialogService.ShowDialog("plot", null));
            return result == true;
        }
    }
}
