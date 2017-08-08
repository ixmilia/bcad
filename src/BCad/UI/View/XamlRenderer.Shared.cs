// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using IxMilia.BCad.EventArguments;
using IxMilia.BCad.Settings;

namespace IxMilia.BCad.UI.View
{
    public partial class XamlRenderer : AbstractCadRenderer
    {
        private IWorkspace Workspace;
        private RenderCanvasViewModel viewModel = new RenderCanvasViewModel();

        public void Initialize(IWorkspace workspace, IViewControl viewControl)
        {
            this.Workspace = workspace;
            this.ViewControl = viewControl;

            viewModel.SelectedEntities = workspace.SelectedEntities;
            DataContext = viewModel;
            Workspace.WorkspaceChanged += Workspace_WorkspaceChanged;
            Workspace.SettingsService.SettingChanged += Workspace_SettingChanged;
            Workspace.SelectedEntities.CollectionChanged += SelectedEntities_CollectionChanged;
            Workspace.RubberBandGeneratorChanged += Workspace_RubberBandGeneratorChanged;

            this.Loaded += (_, __) =>
                {
                    SetBackgroundColor();
                    Workspace_WorkspaceChanged(Workspace, WorkspaceChangeEventArgs.Reset());
                };
        }

        void Workspace_RubberBandGeneratorChanged(object sender, EventArgs e)
        {
            viewModel.RubberBandGenerator = Workspace.RubberBandGenerator;
        }

        public override void Invalidate()
        {
            // this invalidates itself on every change so this can be a no-op
        }

        public override void UpdateRubberBandLines()
        {
            viewModel.CursorPoint = ViewControl.GetCursorPoint(CancellationToken.None).Result;
        }

        private void Workspace_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            switch (e.SettingName)
            {
                case nameof(WpfSettingsProvider.BackgroundColor):
                    SetBackgroundColor();
                    break;
            }
        }

        private void SetBackgroundColor()
        {
            viewModel.BackgroundColor = Workspace.SettingsService.GetValue<CadColor>(WpfSettingsProvider.BackgroundColor);
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
