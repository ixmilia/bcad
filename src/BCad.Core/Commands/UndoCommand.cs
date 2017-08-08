// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace IxMilia.BCad.Commands
{
    [ExportCadCommand("Edit.Undo", "UNDO", ModifierKeys.Control, Key.Z, "undo", "u")]
    public class UndoCommandCommand : ICadCommand
    {
        public Task<bool> Execute(IWorkspace workspace, object arg)
        {
            if (workspace.UndoRedoService.UndoHistorySize == 0)
            {
                workspace.OutputService.WriteLine("Nothing to undo");
                return Task.FromResult(false);
            }

            workspace.UndoRedoService.Undo();
            return Task.FromResult(true);
        }
    }
}
