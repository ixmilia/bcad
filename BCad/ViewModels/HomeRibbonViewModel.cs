using BCad.EventArguments;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BCad.Services;
using System;

namespace BCad.ViewModels
{
    public class HomeRibbonViewModel : INotifyPropertyChanged
    {
        private IWorkspace workspace;
        private IInputService inputService;
        private ReadOnlyLayerViewModel[] layers;
        private ReadOnlyLayerViewModel current;
        public event PropertyChangedEventHandler PropertyChanged;
        private bool ignoreLayerChange;
        private bool dontUpdateWorkspace;

        public HomeRibbonViewModel(IWorkspace workspace, IInputService inputService)
        {
            this.workspace = workspace;
            this.inputService = inputService;
            WorkspaceChanged(this, new WorkspaceChangeEventArgs(true, false, false, false, false));
            this.workspace.WorkspaceChanged += WorkspaceChanged;
            this.workspace.SelectedEntities.CollectionChanged += SelectedEntities_CollectionChanged;
        }

        void SelectedEntities_CollectionChanged(object sender, EventArgs e)
        {
            if (inputService.AllowedInputTypes == InputType.Command)
            {
                dontUpdateWorkspace = true;

                // only check if free-selecting entities
                if (workspace.SelectedEntities.Count == 0)
                {
                    // set dropdown to current active layer
                    CurrentLayer = Layers.SingleOrDefault(l => l.Name == workspace.Drawing.CurrentLayerName);
                }
                else if (workspace.SelectedEntities.Count == 1)
                {
                    // set layer dropdown to single selected entity
                    CurrentLayer = Layers.SingleOrDefault(l => l.Name == workspace.Drawing.ContainingLayer(workspace.SelectedEntities.Single()).Name);
                }
                else
                {
                    // multiple entries selected
                    var allLayerNames = workspace.SelectedEntities.Select(entity => workspace.Drawing.ContainingLayer(entity).Name);
                    if (allLayerNames.Distinct().Count() == 1)
                    {
                        // all have the same containing layer
                        CurrentLayer = Layers.SingleOrDefault(l => l.Name == allLayerNames.First());
                    }
                    else
                    {
                        // different layers, display nothing
                        CurrentLayer = null;
                    }
                }

                dontUpdateWorkspace = false;
            }
        }

        private void WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            Layers = workspace.Drawing.GetLayers().OrderBy(l => l.Name)
                .Select(l => new ReadOnlyLayerViewModel(l, workspace.SettingsManager.ColorMap))
                .ToArray();
            dontUpdateWorkspace = true;
            CurrentLayer = Layers.Single(l => l.Name == workspace.Drawing.CurrentLayerName);
            dontUpdateWorkspace = false;
        }

        public ReadOnlyLayerViewModel CurrentLayer
        {
            get { return current; }
            set
            {
                if (!ignoreLayerChange)
                {
                    current = value;
                    OnPropertyChanged();
                    if (current != null && !dontUpdateWorkspace)
                    {
                        ignoreLayerChange = true;
                        workspace.SetCurrentLayer(current.Name);
                        if (inputService.AllowedInputTypes == InputType.Command && workspace.SelectedEntities.Any())
                        {
                            var drawing = workspace.Drawing;
                            foreach (var entity in workspace.SelectedEntities)
                            {
                                drawing = drawing.Remove(entity);
                                drawing = drawing.Add(drawing.Layers.GetValue(value.Name), entity);
                            }

                            workspace.Update(drawing: drawing);
                            current = Layers.Single(l => l.Name == current.Name);
                            OnPropertyChanged();
                        }

                        ignoreLayerChange = false;
                    }
                }
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
                OnPropertyChangedDirect("CurrentLayer");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            OnPropertyChangedDirect(propertyName);
        }

        protected void OnPropertyChangedDirect(string propertyName)
        {
            var changed = PropertyChanged;
            if (changed != null)
                changed(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
