using System;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Reflection;
using BCad.Services;

namespace BCad.Test
{
    public class TestHost : IDisposable
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        [Import]
        public IInputService InputService { get; set; }

        private CompositionHost container;

        private TestHost()
        {
            var currentAssembly = typeof(App).GetTypeInfo().Assembly;
            var assemblyDir = Path.GetDirectoryName(currentAssembly.Location);
            var configuration = new ContainerConfiguration()
                .WithAssemblies(new[]
                {
                    currentAssembly,
                    Assembly.LoadFile(Path.Combine(assemblyDir, "BCad.exe")),
                    Assembly.LoadFile(Path.Combine(assemblyDir, "BCad.Core.dll")),
                    Assembly.LoadFile(Path.Combine(assemblyDir, "BCad.UI.dll")),
                });
            container = configuration.CreateContainer();
            container.SatisfyImports(this);
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
                host.Workspace.Add(new Layer(layer, IndexedColor.Auto));
            return host;
        }

        public void Dispose()
        {
            if (container != null)
            {
                container.Dispose();
                container = null;
            }
        }
    }
}
