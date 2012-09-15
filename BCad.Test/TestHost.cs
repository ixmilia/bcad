using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using BCad.Services;

namespace BCad.Test
{
    public class TestHost
    {
        [Import]
        public IWorkspace Workspace { get; private set; }

        [Import]
        public IInputService InputService { get; private set; }

        [Import]
        public IEditService EditService { get; private set; }

        private TestHost()
        {
            var catalog = new AggregateCatalog(
                    new AssemblyCatalog("BCad.exe"),
                    new AssemblyCatalog("BCad.Core.dll"),
                    new AssemblyCatalog("BCad.UI.dll")
                    );
            var container = new CompositionContainer(catalog);
            var batch = new CompositionBatch();
            container.Compose(batch);
            container.SatisfyImportsOnce(this);
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
                host.Workspace.Add(new Layer(layer, Color.Auto));
            return host;
        }
    }
}
