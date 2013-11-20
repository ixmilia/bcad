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

        private ObservableCollection<MutableLayer> layers = new ObservableCollection<MutableLayer>();
        private ObservableCollection<IndexedColor> availableColors = new ObservableCollection<IndexedColor>();

        public ObservableCollection<MutableLayer> Layers
        {
            get { return this.layers; }
        }

        public ObservableCollection<IndexedColor> AvailableColors
        {
            get { return this.availableColors; }
        }

        public LayerManager()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public LayerManager(IWorkspace workspace)
        {
            this.workspace = workspace;
            this.layers.Clear();
            foreach (var layer in workspace.Drawing.GetLayers().OrderBy(l => l.Name))
                this.layers.Add(new MutableLayer(layer));

            for (int i = 0; i < 256; i++)
                availableColors.Add(new IndexedColor((byte)i));

            InitializeComponent();
        }

        public override void Commit()
        {
            var dwg = workspace.Drawing;

            if (this.layers.Where(layer => layer.IsDirty).Any() ||
                this.layers.Count != dwg.Layers.Count)
            {
                // found changes, need to update
                var newLayers = new ReadOnlyTree<string, Layer>();
                foreach (var layer in from layer in this.layers
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
            this.layers.Add(new MutableLayer(
                StringUtilities.NextUniqueName("NewLayer", this.layers.Select(l => l.Name)), IndexedColor.Auto));
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var removed = this.layerList.SelectedItem as MutableLayer;
            if (removed != null)
            {
                if (!this.layers.Remove(removed))
                {
                    Debug.Fail("Layer could not be found");
                }
            }
        }
    }
}
