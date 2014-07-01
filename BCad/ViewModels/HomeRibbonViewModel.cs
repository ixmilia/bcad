using BCad.EventArguments;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BCad.ViewModels
{
    public class HomeRibbonViewModel : INotifyPropertyChanged
    {
        private IWorkspace workspace;
        private ReadOnlyLayerViewModel[] layers;
        public event PropertyChangedEventHandler PropertyChanged;
        private bool ignoreLayerChange = false;

        public HomeRibbonViewModel(IWorkspace workspace)
        {
            this.workspace = workspace;
            WorkspaceChanged(this, new WorkspaceChangeEventArgs(true, false, false, false, false, false));
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
