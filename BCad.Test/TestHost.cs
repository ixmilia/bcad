using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace BCad.Test
{
    public class TestHost
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        public TestHost()
        {
            var catalog = new AggregateCatalog(
                    new AssemblyCatalog("BCad.exe"),
                    new AssemblyCatalog("BCad.Commands.dll"),
                    new AssemblyCatalog("BCad.Core.dll"),
                    new AssemblyCatalog("BCad.UI.dll")
                    );
            var container = new CompositionContainer(catalog);
            var batch = new CompositionBatch();
            container.Compose(batch);
            container.SatisfyImportsOnce(this);
            Workspace.Document = new Document();
        }

        public static IWorkspace CreateWorkspace()
        {
            return new TestHost().Workspace;
        }

        public static IWorkspace CreateWorkspace(params string[] layerNames)
        {
            var workspace = CreateWorkspace();
            foreach (string layer in layerNames)
                workspace.Add(new Layer(layer, Color.Auto));
            return workspace;
        }
    }
}
