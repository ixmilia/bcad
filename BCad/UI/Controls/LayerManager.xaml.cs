using System.Collections.ObjectModel;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using BCad.Collections;
using BCad.Utilities;

namespace BCad.UI.Controls
{
    /// <summary>
    /// Interaction logic for LayerManager.xaml
    /// </summary>
    [ExportControl("Layer", "Default", "Layers")]
    public partial class LayerManager : BCadControl
    {
        private IWorkspace workspace = null;
        internal LayerManagerViewModel ViewModel { get; private set; }

        public LayerManager()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public LayerManager(IWorkspace workspace)
            : this()
        {
            this.workspace = workspace;
            this.ViewModel = new LayerManagerViewModel();
            this.DataContext = this.ViewModel;
        }

        public override void OnShowing()
        {
            this.ViewModel.Layers.Clear();
            foreach (var layer in workspace.Drawing.GetLayers().OrderBy(l => l.Name))
                this.ViewModel.Layers.Add(new MutableLayer(layer));

            this.ViewModel.AvailableColors.Clear();
            for (int i = 0; i < 256; i++)
                this.ViewModel.AvailableColors.Add(new IndexedColor((byte)i));
        }

        public override void Commit()
        {
            var dwg = workspace.Drawing;

            if (this.ViewModel.Layers.Any(layer => layer.IsDirty) ||
                this.ViewModel.Layers.Count != dwg.Layers.Count)
            {
                // found changes, need to update
                var newLayers = new ReadOnlyTree<string, Layer>();
                foreach (var layer in from layer in this.ViewModel.Layers
                                      select layer.GetUpdatedLayer())
                {
                    newLayers = newLayers.Insert(layer.Name, layer);
                }

                dwg = dwg.Update(layers: newLayers);
                workspace.Update(drawing: dwg);
            }
        }

        public override void Cancel()
        {
            // do nothing (changes are uncommitted)
        }

        public override bool Validate()
        {
            // TODO: validate stuff
            return true;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Layers.Add(new MutableLayer(
                StringUtilities.NextUniqueName("NewLayer", this.ViewModel.Layers.Select(l => l.Name)), IndexedColor.Auto));
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var removed = this.layerList.SelectedItem as MutableLayer;
            if (removed != null)
            {
                if (!this.ViewModel.Layers.Remove(removed))
                {
                    Debug.Fail("Layer could not be found");
                }
            }
        }

        internal class LayerManagerViewModel
        {
            public ObservableCollection<MutableLayer> Layers { get; private set; }
            public ObservableCollection<IndexedColor> AvailableColors { get; private set; }

            public LayerManagerViewModel()
            {
                Layers = new ObservableCollection<MutableLayer>();
                AvailableColors = new ObservableCollection<IndexedColor>();
            }
        }
    }
}
