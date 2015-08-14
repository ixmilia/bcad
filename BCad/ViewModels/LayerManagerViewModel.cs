using System.Collections.ObjectModel;
using System.Linq;

namespace BCad.ViewModels
{
    public class LayerManagerViewModel
    {
        private IWorkspace workspace;

        public ObservableCollection<MutableLayerViewModel> Layers { get; private set; }

        public LayerManagerViewModel(IWorkspace workspace)
        {
            this.workspace = workspace;
            Layers = new ObservableCollection<MutableLayerViewModel>(
                this.workspace.Drawing.GetLayers().OrderBy(l => l.Name)
                .Select(l => new MutableLayerViewModel(l)));
        }
    }
}
