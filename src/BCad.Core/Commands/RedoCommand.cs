// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace BCad.Commands
{
    [ExportCadCommand("Edit.Redo", "REDO", ModifierKeys.Control, Key.Y, "redo", "re", "r")]
    public class RedoCommandCommand : ICadCommand
    {
        public Task<bool> Execute(IWorkspace workspace, object arg)
        {
            if (workspace.UndoRedoService.RedoHistorySize == 0)
            {
                workspace.OutputService.WriteLine("Nothing to redo");
                return Task.FromResult<bool>(false);
            }

            workspace.UndoRedoService.Redo();
            return Task.FromResult(true);
        }
    }
}
