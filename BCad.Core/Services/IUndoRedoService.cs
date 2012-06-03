namespace BCad.Services
{
    public interface IUndoRedoService
    {
        void Undo();
        void Redo();
        void ClearHistory();
        int UndoHistorySize { get; }
        int RedoHistorySize { get; }
    }
}
