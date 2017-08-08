// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using IxMilia.BCad.Entities;
using IxMilia.BCad.EventArguments;

namespace IxMilia.BCad.ViewModels
{
    public abstract class EditEntityViewModel : ViewModelBase, IDisposable
    {
        protected IWorkspace Workspace { get; private set; }

        protected EditEntityViewModel(IWorkspace workspace)
        {
            Workspace = workspace;
            Workspace.WorkspaceChanged += WorkspaceChanged;
        }

        public UnitFormat UnitFormat
        {
            get { return Workspace.Drawing.Settings.UnitFormat; }
        }

        public int UnitPrecision
        {
            get { return Workspace.Drawing.Settings.UnitPrecision; }
        }

        protected void ReplaceEntity(Entity oldEntity, Entity newEntity)
        {
            Workspace.Update(drawing: Workspace.Drawing.Replace(oldEntity, newEntity));
            Workspace.SelectedEntities.Clear();
            Workspace.SelectedEntities.Add(newEntity);
        }

        private void WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            OnPropertyChangedDirect(string.Empty);
        }

        public void Dispose()
        {
            Workspace.WorkspaceChanged -= WorkspaceChanged;
        }
    }
}
