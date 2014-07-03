using BCad.EventArguments;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BCad.Services;

namespace BCad.ViewModels
{
    public class HomeRibbonViewModel : INotifyPropertyChanged
    {
        private IWorkspace workspace;
        private IInputService inputService;
        private ReadOnlyLayerViewModel[] layers;
        public event PropertyChangedEventHandler PropertyChanged;
        private bool ignoreLayerChange = false;

        public HomeRibbonViewModel(IWorkspace workspace, IInputService inputService)
        {
            this.workspace = workspace;
            this.inputService = inputService;
            WorkspaceChanged(this, new WorkspaceChangeEventArgs(true, false, false, false, false));
            this.workspace.WorkspaceChanged += WorkspaceChanged;
        }

        private void WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            Layers = workspace.Drawing.GetLayers().OrderBy(l => l.Name)
                .Select(l => new ReadOnlyLayerViewModel(l, workspace.SettingsManager.ColorMap))
                .ToArray();
        }

        public ReadOnlyLayerViewModel CurrentLayer
        {
            get { return new ReadOnlyLayerViewModel(workspace.Drawing.CurrentLayer, workspace.SettingsManager.ColorMap); }
            set
            {
                if (value != null && !ignoreLayerChange)
                {
                    ignoreLayerChange = true;
                    workspace.SetCurrentLayer(value.Name);
                    if (inputService.AllowedInputTypes == InputType.Command && workspace.SelectedEntities.Any())
                    {
                        var drawing = workspace.Drawing;
                        foreach (var entity in workspace.SelectedEntities)
                        {
                            drawing = drawing.Remove(entity);
                            drawing = drawing.Add(drawing.Layers.GetValue(value.Name), entity);
                        }

                        workspace.Update(drawing: drawing);
                    }

                    ignoreLayerChange = false;
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
