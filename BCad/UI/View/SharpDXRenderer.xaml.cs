using System;
using System.ComponentModel;
using System.Windows.Controls;
using BCad.EventArguments;
using BCad.Services;

namespace BCad.UI.View
{
    /// <summary>
    /// Interaction logic for SharpDXRenderer.xaml
    /// </summary>
    public partial class SharpDXRenderer : UserControl, IRenderer
    {
        private IWorkspace workspace;
        private RenderCanvasViewModel viewModel = new RenderCanvasViewModel();

        public SharpDXRenderer()
        {
            InitializeComponent();
        }

        public SharpDXRenderer(IViewControl viewControl, IWorkspace workspace, IInputService inputService)
            : this()
        {
            this.workspace = workspace;
            var game = new CadGame(workspace);
            game.Run(surface);

            viewModel = new RenderCanvasViewModel();
            DataContext = viewModel;
            workspace.WorkspaceChanged += Workspace_WorkspaceChanged;
            workspace.SettingsManager.PropertyChanged += SettingsManager_PropertyChanged;
            workspace.SelectedEntities.CollectionChanged += SelectedEntities_CollectionChanged;

            this.Loaded += (_, __) =>
            {
                foreach (var setting in new[] { Constants.BackgroundColorString })
                    SettingsManager_PropertyChanged(workspace.SettingsManager, new PropertyChangedEventArgs(setting));
                Workspace_WorkspaceChanged(workspace, WorkspaceChangeEventArgs.Reset());
            };
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case Constants.BackgroundColorString:
                    viewModel.BackgroundColor = workspace.SettingsManager.BackgroundColor;
                    break;
                case Constants.ColorMapString:
                    viewModel.ColorMap = workspace.SettingsManager.ColorMap;
                    break;
            }
        }

        private void Workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            if (e.IsActiveViewPortChange)
            {
                viewModel.ViewPort = workspace.ActiveViewPort;
            }
            if (e.IsDrawingChange)
            {
                viewModel.Drawing = workspace.Drawing;
            }
        }

        private void SelectedEntities_CollectionChanged(object sender, EventArgs e)
        {
            viewModel.SelectedEntities = workspace.SelectedEntities;
        }
    }
}
