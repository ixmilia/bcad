using System;
using BCad.Entities;
using BCad.EventArguments;

namespace BCad.ViewModels
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
