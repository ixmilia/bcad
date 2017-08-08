// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Utilities;
using IxMilia.BCad.ViewModels;

namespace IxMilia.BCad.UI.Controls
{
    /// <summary>
    /// Interaction logic for LayerManager.xaml
    /// </summary>
    [ExportControl("Layer", "Default", "Layers")]
    public partial class LayerManager : BCadControl
    {
        private IWorkspace workspace = null;
        private LayerManagerViewModel viewModel;

        public LayerManager()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public LayerManager(IWorkspace workspace)
            : this()
        {
            this.workspace = workspace;
        }

        public override void OnShowing()
        {
            this.viewModel = new LayerManagerViewModel(this.workspace);
            this.DataContext = this.viewModel;
        }

        public override void Commit()
        {
            var dwg = workspace.Drawing;

            if (this.viewModel.Layers.Any(layer => layer.IsDirty) ||
                this.viewModel.Layers.Count != dwg.Layers.Count)
            {
                // found changes, need to update
                var newLayers = new ReadOnlyTree<string, Layer>();
                foreach (var layer in from layer in this.viewModel.Layers
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
            this.viewModel.Layers.Add(new MutableLayerViewModel(
                StringUtilities.NextUniqueName("NewLayer", this.viewModel.Layers.Select(l => l.Name))));
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var removed = this.layerList.SelectedItem as MutableLayerViewModel;
            if (removed != null)
            {
                if (!this.viewModel.Layers.Remove(removed))
                {
                    Debug.Fail("Layer could not be found");
                }
            }
        }
    }
}
