using IxMilia.BCad.FileHandlers;
using IxMilia.BCad.Services;

namespace IxMilia.BCad.Core.Test
{
    public class TestHost
    {
        public IWorkspace Workspace { get; }

        private TestHost()
        {
            Workspace = new TestWorkspace();
            Workspace.RegisterService<IDialogService>(new TestDialogService());
            Workspace.RegisterService<IFileSystemService>(new TestFileSystemService());
            Workspace.ReaderWriterService.RegisterFileHandler(new DxfFileHandler(), true, true, ".dxf");
            Workspace.Update(drawing: new Drawing());
        }

        public static TestHost CreateHost()
        {
            return new TestHost();
        }

        public static TestHost CreateHost(params string[] layerNames)
        {
            var host = CreateHost();
            foreach (string layer in layerNames)
                host.Workspace.Add(new Layer(layer));
            return host;
        }
    }
}
