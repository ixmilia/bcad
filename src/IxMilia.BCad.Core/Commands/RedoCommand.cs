﻿using System.Threading.Tasks;

namespace IxMilia.BCad.Commands
{
    public class RedoCommand : ICadCommand
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
