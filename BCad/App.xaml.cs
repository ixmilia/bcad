using System.Windows;
using System.Reflection;
using System.ComponentModel.Composition.Hosting;

namespace BCad
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static CompositionContainer Container { get; private set; }

        public App()
        {
            //base.OnStartup(e);
            this.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;

            var catalog = new AggregateCatalog(
                    new AssemblyCatalog(Assembly.GetExecutingAssembly()),
                    new AssemblyCatalog("BCad.Core.dll"),
                    new AssemblyCatalog("BCad.Core.UI.dll"),
                    new AssemblyCatalog("BCad.FileHandlers.dll"),
                    new AssemblyCatalog("BCad.UI.dll")
                    );
            if (Container == null)
                Container = new CompositionContainer(catalog);
            var batch = new CompositionBatch();
            Container.Compose(batch);
        }
    }
}
