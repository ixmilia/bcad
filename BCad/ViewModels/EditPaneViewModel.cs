﻿using System;
using System.Collections.Generic;
using System.Linq;
using BCad.Entities;
using BCad.EventArguments;
using BCad.Services;

namespace BCad.ViewModels
{
    public class EditPaneViewModel : ViewModelBase
    {
        private IWorkspace workspace;
        private IInputService inputService;
        private IEnumerable<ReadOnlyLayerViewModel> layers;
        private bool ignoreLayerChange;
        private EditArcViewModel editArcViewModel;
        private EditCircleViewModel editCircleViewModel;
        private EditEllipseViewModel editEllipseViewModel;
        private EditLineViewModel editLineViewModel;
        private EditLocationViewModel editLocationViewModel;
        private EditTextViewModel editTextViewModel;

        public EditPaneViewModel(IWorkspace workspace, IInputService inputService)
        {
            this.workspace = workspace;
            this.inputService = inputService;
            workspace.WorkspaceChanged += WorkspaceChanged;
            workspace.SelectedEntities.CollectionChanged += SelectedEntities_CollectionChanged;
            AvailableColors = Enumerable.Range(0, 256).Select(i => new ColorViewModel(new IndexedColor((byte)i), workspace.SettingsManager.ColorMap[new IndexedColor((byte)i)]));
        }

        public int SelectedCount
        {
            get { return IsEditingEnabled ? workspace.SelectedEntities.Count : 0; }
        }

        public bool IsEditingEnabled
        {
            get { return inputService.AllowedInputTypes == InputType.Command && workspace.SelectedEntities.Count != 0; }
        }

        public IEnumerable<ColorViewModel> AvailableColors { get; private set; }

        public ColorViewModel SelectedColor
        {
            get
            {
                if (!IsEditingEnabled)
                    return null;

                switch (workspace.SelectedEntities.Count)
                {
                    case 0:
                        return null;
                    case 1:
                        return AvailableColors.FirstOrDefault(c => c.Color == workspace.SelectedEntities.First().Color);
                    default:
                        var selectedColors = workspace.SelectedEntities.Select(entity => entity.Color);
                        if (selectedColors.Distinct().Count() == 1)
                            return AvailableColors.FirstOrDefault(c => c.Color == selectedColors.First());
                        return null;
                }
            }
            set
            {
                var drawing = workspace.Drawing;
                var newSelectedEntities = new List<Entity>();
                foreach (var entity in workspace.SelectedEntities)
                {
                    var layerName = drawing.ContainingLayer(entity).Name;
                    drawing = drawing.Remove(entity);
                    var newEntity = UpdateColor(entity, value.Color);
                    newSelectedEntities.Add(newEntity);
                    drawing = drawing.Add(drawing.Layers.GetValue(layerName), newEntity);
                }

                workspace.Update(drawing: drawing);
                workspace.SelectedEntities.Clear();
                workspace.SelectedEntities.AddRange(newSelectedEntities);
            }
        }

        public ReadOnlyLayerViewModel SelectedLayer
        {
            get
            {
                if (!IsEditingEnabled)
                    return null;

                switch (workspace.SelectedEntities.Count)
                {
                    case 0:
                        return null;
                    case 1:
                        var layerName = ContainingLayerName(workspace.SelectedEntities.First());
                        return Layers.FirstOrDefault(l => l.Name == layerName);
                    default:
                        var selectedLayers = workspace.SelectedEntities.Select(entity => ContainingLayerName(entity));
                        if (selectedLayers.Distinct().Count() == 1)
                            return Layers.FirstOrDefault(l => l.Name == selectedLayers.First());
                        return null;
                }
            }
            set
            {
                if (!ignoreLayerChange)
                {
                    var drawing = workspace.Drawing;
                    foreach (var entity in workspace.SelectedEntities)
                    {
                        drawing = drawing.Remove(entity);
                        drawing = drawing.Add(drawing.Layers.GetValue(value.Name), entity);
                    }

                    workspace.Update(drawing: drawing);
                }
            }
        }

        public IEnumerable<ReadOnlyLayerViewModel> Layers
        {
            get { return layers; }
            set
            {
                if (layers == value)
                    return;
                layers = value;
                OnPropertyChanged();
                OnPropertyChangedDirect("SelectedLayer");
            }
        }

        public EditArcViewModel EditArcViewModel
        {
            get { return editArcViewModel; }
            set
            {
                if (editArcViewModel == value)
                    return;
                editArcViewModel = value;
                OnPropertyChanged();
            }
        }

        public EditCircleViewModel EditCircleViewModel
        {
            get { return editCircleViewModel; }
            set
            {
                if (editCircleViewModel == value)
                    return;
                editCircleViewModel = value;
                OnPropertyChanged();
            }
        }

        public EditEllipseViewModel EditEllipseViewModel
        {
            get { return editEllipseViewModel; }
            set
            {
                if (editEllipseViewModel == value)
                    return;
                editEllipseViewModel = value;
                OnPropertyChanged();
            }
        }

        public EditLineViewModel EditLineViewModel
        {
            get { return editLineViewModel; }
            set
            {
                if (editLineViewModel == value)
                    return;
                editLineViewModel = value;
                OnPropertyChanged();
            }
        }

        public EditLocationViewModel EditLocationViewModel
        {
            get { return editLocationViewModel; }
            set
            {
                if (editLocationViewModel == value)
                    return;
                editLocationViewModel = value;
                OnPropertyChanged();
            }
        }

        public EditTextViewModel EditTextViewModel
        {
            get { return editTextViewModel; }
            set
            {
                if (editTextViewModel == value)
                    return;
                editTextViewModel = value;
                OnPropertyChanged();
            }
        }

        private void WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            ignoreLayerChange = true;
            Layers = workspace.Drawing.GetLayers().OrderBy(l => l.Name).Select(l => new ReadOnlyLayerViewModel(l, workspace.SettingsManager.ColorMap));
            ignoreLayerChange = false;
        }

        private void SelectedEntities_CollectionChanged(object sender, EventArgs e)
        {
            SetEditableViewModels();
            OnPropertyChangedDirect(string.Empty);
        }

        private void SetEditableViewModels()
        {
            // clear all
            if (EditArcViewModel != null)
                EditArcViewModel.Dispose();
            EditArcViewModel = null;

            if (EditCircleViewModel != null)
                EditCircleViewModel.Dispose();
            EditCircleViewModel = null;

            if (EditEllipseViewModel != null)
                EditEllipseViewModel.Dispose();
            EditEllipseViewModel = null;

            if (EditLineViewModel != null)
                EditLineViewModel.Dispose();
            EditLineViewModel = null;

            if (EditLocationViewModel != null)
                EditLocationViewModel.Dispose();
            EditLocationViewModel = null;

            if (EditTextViewModel != null)
                EditTextViewModel.Dispose();
            EditTextViewModel = null;

            if (IsEditingEnabled && workspace.SelectedEntities.Count == 1)
            {
                SetEditableViewModel(workspace.SelectedEntities.First());
            }
        }

        private void SetEditableViewModel(Entity entity)
        {
            switch (entity.Kind)
            {
                case EntityKind.Aggregate:
                    break;
                case EntityKind.Arc:
                    EditArcViewModel = new EditArcViewModel(workspace, (Arc)entity);
                    break;
                case EntityKind.Circle:
                    EditCircleViewModel = new EditCircleViewModel(workspace, (Circle)entity);
                    break;
                case EntityKind.Ellipse:
                    EditEllipseViewModel = new EditEllipseViewModel(workspace, (Ellipse)entity);
                    break;
                case EntityKind.Line:
                    EditLineViewModel = new EditLineViewModel(workspace, (Line)entity);
                    break;
                case EntityKind.Location:
                    EditLocationViewModel = new EditLocationViewModel(workspace, (Location)entity);
                    break;
                case EntityKind.Polyline:
                    break;
                case EntityKind.Text:
                    EditTextViewModel = new EditTextViewModel(workspace, (Text)entity);
                    break;
                default:
                    break;
            }
        }

        private string ContainingLayerName(Entity entity)
        {
            var layer = workspace.Drawing.ContainingLayer(entity);
            if (layer != null)
                return layer.Name;
            return null;
        }

        private Entity UpdateColor(Entity entity, IndexedColor newColor)
        {
            switch (entity.Kind)
            {
                case EntityKind.Aggregate:
                    return ((AggregateEntity)entity).Update(color: newColor);
                case EntityKind.Arc:
                    return ((Arc)entity).Update(color: newColor);
                case EntityKind.Circle:
                    return ((Circle)entity).Update(color: newColor);
                case EntityKind.Ellipse:
                    return ((Ellipse)entity).Update(color: newColor);
                case EntityKind.Line:
                    return ((Line)entity).Update(color: newColor);
                case EntityKind.Location:
                    return ((Location)entity).Update(color: newColor);
                case EntityKind.Polyline:
                    return ((Polyline)entity).Update(color: newColor);
                case EntityKind.Text:
                    return ((Text)entity).Update(color: newColor);
                default:
                    throw new InvalidOperationException("Unsupported entity type");
            }
        }
    }
}
