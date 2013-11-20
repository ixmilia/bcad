using System.Windows;
using System.Reflection;
using System.Composition.Hosting;
using System;
using System.IO;

namespace BCad
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IDisposable
    {
        public static CompositionHost Container { get; private set; }

        public App()
        {
            //base.OnStartup(e);
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;

            var currentAssembly = typeof(App).GetTypeInfo().Assembly;
            var assemblyDir = Path.GetDirectoryName(currentAssembly.Location);
            var configuration = new ContainerConfiguration()
                .WithAssemblies(new[]
                {
                    currentAssembly,
                    Assembly.LoadFile(Path.Combine(assemblyDir, "BCad.Core.dll")),
                    Assembly.LoadFile(Path.Combine(assemblyDir, "BCad.Core.UI.dll")),
                    Assembly.LoadFile(Path.Combine(assemblyDir, "BCad.FileHandlers.dll")),
                    Assembly.LoadFile(Path.Combine(assemblyDir, "BCad.UI.dll")),
                });
            Container = configuration.CreateContainer();
        }

        public void Dispose()
        {
            if (Container != null)
            {
                Container.Dispose();
                Container = null;
            }
        }
    }
}
