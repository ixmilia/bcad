using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using BCad.Utilities;
using System.Collections.Specialized;

namespace BCad.UI.Controls
{
    /// <summary>
    /// Interaction logic for LayerManager.xaml
    /// </summary>
    public partial class LayerManager : BCadControl
    {
        private IWorkspace workspace = null;

        private ObservableCollection<MutableLayer> layers = new ObservableCollection<MutableLayer>();
        private ObservableCollection<Color> availableColors = new ObservableCollection<Color>();

        public ObservableCollection<MutableLayer> Layers
        {
            get { return this.layers; }
        }

        public ObservableCollection<Color> AvailableColors
        {
            get { return this.availableColors; }
        }

        [Obsolete("The default constructor is only for the designer.  Use the parameterized one instead.")]
        public LayerManager()
        {
            InitializeComponent();
        }

        public LayerManager(IWorkspace workspace)
        {
            this.workspace = workspace;
            this.layers.Clear();
            foreach (var layer in workspace.Document.Layers.Values.OrderBy(l => l.Name))
                this.layers.Add(new MutableLayer(layer));

            for (byte i = 0; i <= 9; i++)
                availableColors.Add(new Color(i));

            InitializeComponent();
        }

        public override void Commit()
        {
            var doc = workspace.Document;

            if (this.layers.Where(layer => layer.IsDirty).Any() ||
                this.layers.Count != doc.Layers.Count)
            {
                // found changes, need to update
                var newLayers = new Dictionary<string, Layer>();
                foreach (var layer in from layer in this.layers
                                      select layer.GetUpdatedLayer())
                {
                    newLayers.Add(layer.Name, layer);
                }

                doc = doc.Update(layers: newLayers);
                workspace.Document = doc;
            }
        }

        public override void Cancel()
        {
            // do nothing (changes are uncommitted)
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            this.layers.Add(new MutableLayer(
                StringUtilities.NextUniqueName("NewLayer", this.layers.Select(l => l.Name)), Color.Auto));
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var removed = this.layerList.SelectedItem as MutableLayer;
            if (removed != null)
            {
                if (this.layers.Count == 1)
                    Debug.Fail("Cannot remove the last layer");

                if (!this.layers.Remove(removed))
                {
                    Debug.Fail("Layer could not be found");
                }
            }
        }
    }
}
