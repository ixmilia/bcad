using System;
using System.Collections.Generic;
using System.Linq;
using BCad.EventArguments;
using BCad.Services;

namespace BCad.ViewModels
{
    public class EditPaneViewModel : ViewModelBase
    {
        private IWorkspace workspace;
        private IInputService inputService;
        private IEnumerable<ReadOnlyLayerViewModel> layers;
        private bool ignoreLayerChange;

        public EditPaneViewModel(IWorkspace workspace, IInputService inputService)
        {
            this.workspace = workspace;
            this.inputService = inputService;
            workspace.WorkspaceChanged += WorkspaceChanged;
            workspace.SelectedEntities.CollectionChanged += SelectedEntities_CollectionChanged;
        }

        public int SelectedCount
        {
            get { return inputService.AllowedInputTypes != InputType.Command ? 0 : workspace.SelectedEntities.Count; }
        }

        public bool IsEditingEnabled
        {
            get { return SelectedCount != 0; }
        }

        public ReadOnlyLayerViewModel SelectedItemsLayer
        {
            get
            {
                var selectedEntities = workspace.SelectedEntities.ToList();
                switch (selectedEntities.Count)
                {
                    case 0:
                        return null;
                    case 1:
                        return layers.FirstOrDefault(l => l.Name == workspace.Drawing.ContainingLayer(selectedEntities.First()).Name);
                    default:
                        var selectedLayers = selectedEntities.Select(entity => workspace.Drawing.ContainingLayer(entity).Name);
                        if (selectedLayers.Distinct().Count() == 1)
                            return layers.FirstOrDefault(l => l.Name == selectedLayers.First());
                        return null;
                }
            }
            set
            {
                if (!ignoreLayerChange)
                {
                    var selectedEntities = workspace.SelectedEntities.ToList();
                    var drawing = workspace.Drawing;
                    foreach (var entity in selectedEntities)
                    {
                        drawing = drawing.Remove(entity);
                        drawing = drawing.Add(drawing.Layers.GetValue(value.Name), entity);
                    }

                    workspace.Update(drawing: drawing);
                }
            }
        }

        public IEnumerable<ReadOnlyLayerViewModel> Layers
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

        void WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            ignoreLayerChange = true;
            Layers = workspace.Drawing.GetLayers().OrderBy(l => l.Name).Select(l => new ReadOnlyLayerViewModel(l, workspace.SettingsManager.ColorMap));
            OnPropertyChangedDirect("SelectedItemsLayer");
            ignoreLayerChange = false;
        }

        void SelectedEntities_CollectionChanged(object sender, EventArgs e)
        {
            OnPropertyChangedDirect(string.Empty);
        }
    }
}
