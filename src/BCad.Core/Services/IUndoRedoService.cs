// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace BCad.Services
{
    public interface IUndoRedoService : IWorkspaceService
    {
        void Undo();
        void Redo();
        void ClearHistory();
        int UndoHistorySize { get; }
        int RedoHistorySize { get; }
    }
}
