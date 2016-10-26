// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using BCad.EventArguments;
using BCad.UI.Shared;

namespace BCad.UI.View
{
    /// <summary>
    /// Interaction logic for SharpDXRenderer.xaml
    /// </summary>
    public partial class SharpDXRenderer : AbstractCadRenderer
    {
        private IWorkspace workspace;
        private IViewControl viewControl;
        private CadGame game;
        private RenderCanvasViewModel viewModel = new RenderCanvasViewModel();

        public SharpDXRenderer()
        {
            InitializeComponent();
        }

        public SharpDXRenderer(IViewControl viewControl, IWorkspace workspace)
            : this()
        {
            this.workspace = workspace;
            this.viewControl = viewControl;
            game = new CadGame(workspace, viewControl);
            game.Run(surface);

            viewModel = new RenderCanvasViewModel();
            DataContext = viewModel;
            workspace.WorkspaceChanged += Workspace_WorkspaceChanged;
            workspace.SettingsManager.PropertyChanged += SettingsManager_PropertyChanged;
            workspace.SelectedEntities.CollectionChanged += SelectedEntities_CollectionChanged;
            workspace.RubberBandGeneratorChanged += RubberBandGeneratorChanged;

            this.Loaded += (_, __) =>
            {
                foreach (var setting in new[] { nameof(workspace.SettingsManager.BackgroundColor) })
                    SettingsManager_PropertyChanged(workspace.SettingsManager, new PropertyChangedEventArgs(setting));
                Workspace_WorkspaceChanged(workspace, WorkspaceChangeEventArgs.Reset());
            };

            this.SizeChanged += (_, e) => game.Resize((int)e.NewSize.Width, (int)e.NewSize.Height);
        }

        private void RubberBandGeneratorChanged(object sender, EventArgs e)
        {
            viewModel.RubberBandGenerator = workspace.RubberBandGenerator;
        }

        public override void UpdateRubberBandLines()
        {
            game.UpdateRubberBandLines();
            viewModel.CursorPoint = viewControl.GetCursorPoint().Result;
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(workspace.SettingsManager.BackgroundColor):
                    viewModel.BackgroundColor = workspace.SettingsManager.BackgroundColor;
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
