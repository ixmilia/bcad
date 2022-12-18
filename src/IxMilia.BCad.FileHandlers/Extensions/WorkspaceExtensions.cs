using IxMilia.BCad.FileHandlers;

namespace IxMilia.BCad.Extensions
{
    public static class WorkspaceExtensions
    {
        public static void RegisterFileHandlers(this IWorkspace workspace)
        {
            workspace.ReaderWriterService.RegisterFileHandler(new AscFileHandler(), true, false, ".asc");
            workspace.ReaderWriterService.RegisterFileHandler(new DwgFileHandler(), true, true, ".dwg");
            workspace.ReaderWriterService.RegisterFileHandler(new DxfFileHandler(), true, true, ".dxf");
            workspace.ReaderWriterService.RegisterFileHandler(new IgesFileHandler(), true, true, ".igs", "iges");
            workspace.ReaderWriterService.RegisterFileHandler(new JsonFileHandler(), true, true, ".json");
            workspace.ReaderWriterService.RegisterFileHandler(new StepFileHandler(), true, false, ".stp", ".step");
            workspace.ReaderWriterService.RegisterFileHandler(new StlFileHandler(), true, false, ".stl");
        }
    }
}
