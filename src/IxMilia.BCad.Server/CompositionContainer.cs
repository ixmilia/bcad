using System;
using System.Composition.Hosting;
using System.Reflection;
using IxMilia.BCad.FileHandlers;

namespace IxMilia.BCad.Server
{
    public class CompositionContainer : IDisposable
    {
        public static CompositionHost Container { get; private set; }

        static CompositionContainer()
        {
            var currentAssembly = typeof(CompositionContainer).GetTypeInfo().Assembly;
            var configuration = new ContainerConfiguration()
                .WithAssemblies(new[]
                {
                    currentAssembly,
                    typeof(Drawing).GetTypeInfo().Assembly, // BCad.Core.dll
                    typeof(DxfFileHandler).GetTypeInfo().Assembly // BCad.FileHandlers.dll
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
