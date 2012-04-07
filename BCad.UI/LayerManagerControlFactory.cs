using System.ComponentModel.Composition;
using BCad.UI.Controls;

namespace BCad.UI
{
    [ExportControlFactory("Default", "Layers")]
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
