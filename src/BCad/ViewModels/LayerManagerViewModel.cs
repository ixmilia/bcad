// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Linq;

namespace IxMilia.BCad.ViewModels
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
