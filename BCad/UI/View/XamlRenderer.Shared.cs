using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BCad.Collections;
using BCad.Entities;
using BCad.EventArguments;

#if NETFX_CORE
// Metro
using Windows.UI.Xaml.Controls;
#else
// WPF
using System.Windows.Controls;
#endif

namespace BCad.UI.View
{
    public partial class XamlRenderer : UserControl
    {
        private IWorkspace Workspace;
        private XamlRendererViewModel viewModel = new XamlRendererViewModel();

        public void Initialize(IWorkspace workspace)
        {
            this.Workspace = workspace;

            viewModel.SelectedEntities = workspace.SelectedEntities;
            DataContext = viewModel;
            Workspace.WorkspaceChanged += Workspace_WorkspaceChanged;
            Workspace.SettingsManager.PropertyChanged += SettingsManager_PropertyChanged;
            Workspace.SelectedEntities.CollectionChanged += SelectedEntities_CollectionChanged;

            this.Loaded += (_, __) =>
                {
                    foreach (var setting in new[] { Constants.BackgroundColorString })
                        SettingsManager_PropertyChanged(Workspace.SettingsManager, new PropertyChangedEventArgs(setting));
                    Workspace_WorkspaceChanged(Workspace, WorkspaceChangeEventArgs.Reset());
                };
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case Constants.BackgroundColorString:
                    viewModel.BackgroundColor = Workspace.SettingsManager.BackgroundColor;
                    break;
            }
        }

        private void Workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            if (e.IsActiveViewPortChange)
            {
                viewModel.ViewPort = Workspace.ActiveViewPort;
            }
            if (e.IsDrawingChange)
            {
                viewModel.Drawing = Workspace.Drawing;
            }
        }

        private void SelectedEntities_CollectionChanged(object sender, EventArgs e)
        {
            viewModel.SelectedEntities = Workspace.SelectedEntities;
        }

        public class XamlRendererViewModel : INotifyPropertyChanged
        {
            private RealColor backgroundColor = RealColor.Black;
            private ViewPort viewPort = ViewPort.CreateDefaultViewPort();
            private Drawing drawing = new Drawing();
            private double pointSize = 15.0;
            private ObservableHashSet<Entity> selectedEntities = new ObservableHashSet<Entity>();

            public RealColor BackgroundColor
            {
                get { return backgroundColor; }
                set
                {
                    if (backgroundColor == value)
                        return;
                    backgroundColor = value;
                    OnPropertyChanged();
                }
            }

            public ViewPort ViewPort
            {
                get { return viewPort; }
                set
                {
                    if (viewPort == value)
                        return;
                    viewPort = value;
                    OnPropertyChanged();
                }
            }

            public Drawing Drawing
            {
                get { return drawing; }
                set
                {
                    if (drawing == value)
                        return;
                    drawing = value;
                    OnPropertyChanged();
                }
            }

            public double PointSize
            {
                get { return pointSize; }
                set
                {
                    if (pointSize == value)
                        return;
                    pointSize = value;
                    OnPropertyChanged();
                }
            }

            public ObservableHashSet<Entity> SelectedEntities
            {
                get { return selectedEntities; }
                set
                {
                    if (selectedEntities == value)
                        return;
                    selectedEntities = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                var changed = PropertyChanged;
                if (changed != null)
                    changed(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
