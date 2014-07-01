using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BCad.Collections;
using BCad.Entities;
using BCad.EventArguments;

#if BCAD_METRO
using Windows.UI.Xaml.Controls;
#endif

#if BCAD_WPF
using System.Windows.Controls;
#endif

namespace BCad.UI.View
{
    public partial class XamlRenderer : UserControl
    {
        private IWorkspace Workspace;
        private RenderCanvasViewModel viewModel = new RenderCanvasViewModel();

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

        public void UpdateRubberBandLines()
        {
            // TODO:
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case Constants.BackgroundColorString:
                    viewModel.BackgroundColor = Workspace.SettingsManager.BackgroundColor;
                    break;
                case Constants.ColorMapString:
                    viewModel.ColorMap = Workspace.SettingsManager.ColorMap;
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
    }
}
