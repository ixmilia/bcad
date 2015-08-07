using System.Collections.ObjectModel;
using System.Linq;
using BCad.UI;

namespace BCad.ViewModels
{
    public class LayerManagerViewModel
    {
        private IWorkspace workspace;

        public ObservableCollection<MutableLayerViewModel> Layers { get; private set; }

        public ObservableCollection<ColorViewModel> AvailableColors { get; private set; }

        public LayerManagerViewModel(IWorkspace workspace)
        {
            this.workspace = workspace;
            Layers = new ObservableCollection<MutableLayerViewModel>(
                this.workspace.Drawing.GetLayers().OrderBy(l => l.Name)
                .Select(l => new MutableLayerViewModel(l)));
            AvailableColors = new ObservableCollection<ColorViewModel>(CadColors.AllColors.Select(color => new ColorViewModel(color)));
        }
    }
}
