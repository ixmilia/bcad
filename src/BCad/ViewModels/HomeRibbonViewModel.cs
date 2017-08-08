// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using IxMilia.BCad.EventArguments;

namespace IxMilia.BCad.ViewModels
{
    public class HomeRibbonViewModel : ViewModelBase
    {
        private IWorkspace workspace;
        private ReadOnlyLayerViewModel[] layers;
        private ReadOnlyLayerViewModel currentLayer;
        private bool ignoreLayerChange;

        public HomeRibbonViewModel(IWorkspace workspace)
        {
            this.workspace = workspace;
            WorkspaceChanged(this, new WorkspaceChangeEventArgs(true, false, false, false, false));
            this.workspace.WorkspaceChanged += WorkspaceChanged;
        }

        private void WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            ignoreLayerChange = true;
            Layers = workspace.Drawing.GetLayers().OrderBy(l => l.Name)
                .Select(l => new ReadOnlyLayerViewModel(l))
                .ToArray();
            CurrentLayer = Layers.First(l => l.Name == workspace.Drawing.CurrentLayer.Name);
            ignoreLayerChange = false;
        }

        public ReadOnlyLayerViewModel CurrentLayer
        {
            get { return currentLayer; }
            set
            {
                if (currentLayer == value)
                    return;
                currentLayer = value;
                if (!ignoreLayerChange)
                {
                    workspace.SetCurrentLayer(value.Name);
                }

                OnPropertyChanged();
            }
        }

        public ReadOnlyLayerViewModel[] Layers
        {
            get { return layers; }
            set
            {
                if (layers == value)
                    return;
                layers = value;
                OnPropertyChanged();
            }
        }
    }
}
