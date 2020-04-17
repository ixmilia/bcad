namespace IxMilia.BCad.Services
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
