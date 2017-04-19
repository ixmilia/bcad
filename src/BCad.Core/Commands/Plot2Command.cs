// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace BCad.Commands
{
    [ExportCadCommand("File.Plot2", "PLOT2", "p2")]
    public class Plot2Command : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var result = await workspace.DialogFactoryService.ShowDialog("Plot2", "Default");
            return result == true;
        }
    }
}
