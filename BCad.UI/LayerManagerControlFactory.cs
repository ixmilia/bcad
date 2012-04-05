using System.ComponentModel.Composition;

namespace BCad.UI
{
    [Export(typeof(IControlFactory))]
    [ExportMetadata("ControlId", "Default")]
    [ExportMetadata("Title", "Layers")]
    internal class LayerManagerControlFactory : IControlFactory
    {
        [Import]
        public IWorkspace Workspace { get; private set; }

        public BCadControl Generate()
        {
            return new LayerManager(this.Workspace);
        }
    }
}
