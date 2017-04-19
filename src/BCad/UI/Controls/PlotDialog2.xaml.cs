// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.Linq;
using System.Windows.Controls;
using BCad.Plotting;

namespace BCad.UI.Controls
{
    [ExportControl("Plot2", "Default", "Plot")]
    public partial class PlotDialog2 : BCadControl
    {
        private IWorkspace _workspace;
        private PlotDialog2ViewModel _viewModel;
        private Dictionary<PlotterFactoryMetadata, INotifyPropertyChanged> viewModelCache = new Dictionary<PlotterFactoryMetadata, INotifyPropertyChanged>();
        private IEnumerable<Lazy<IPlotterFactory, PlotterFactoryMetadata>> _plotters;
        private PlotterControl _customControl;

        public PlotDialog2()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public PlotDialog2(IWorkspace workspace, [ImportMany] IEnumerable<Lazy<IPlotterFactory, PlotterFactoryMetadata>> plotters)
            : this()
        {
            _workspace = workspace;
            _viewModel = new PlotDialog2ViewModel(plotters.Select(p => p.Metadata));
            _plotters = plotters;

            DataContext = _viewModel;
        }

        public override void OnShowing()
        {
            UpdateViewControl();
        }

        public override void Commit()
        {
            if (viewModelCache.TryGetValue(_viewModel.SelectedFactory, out var plotterViewModel))
            {
                var factory = _plotters.Single(p => p.Metadata == _viewModel.SelectedFactory).Value;
                var plotter = factory.CreatePlotter(plotterViewModel);
                _customControl?.BeforeCommit();
                plotter.Plot(_workspace);
                _customControl?.AfterCommit();
            }
        }

        private void PlotterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateViewControl();
        }

        private void UpdateViewControl()
        {
            if (!viewModelCache.ContainsKey(_viewModel.SelectedFactory))
            {
                var factory = _plotters.Single(p => p.Metadata == _viewModel.SelectedFactory).Value;
                viewModelCache[_viewModel.SelectedFactory] = factory.CreatePlotterViewModel();
            }

            var plotterViewModel = viewModelCache[_viewModel.SelectedFactory];
            var plotterMetadata = _plotters.Single(p => ReferenceEquals(p.Metadata, _viewModel.SelectedFactory)).Metadata;
            var customControlName = _customControl?.GetType().FullName;
            if (plotterMetadata.ViewTypeName != customControlName)
            {
                _customControl = null;
                plotterControl.Content = null;
                if (!string.IsNullOrEmpty(plotterMetadata.ViewTypeName))
                {
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        var type = asm.GetType(plotterMetadata.ViewTypeName);
                        if (type != null)
                        {
                            if (Activator.CreateInstance(type) is PlotterControl view)
                            {
                                view.SetWindowParent(WindowParent);
                                view.DataContext = plotterViewModel;
                                CompositionContainer.Container.SatisfyImports(view);
                                _customControl = view;
                                plotterControl.Content = _customControl;
                            }

                            break;
                        }
                    }
                }
            }

            _customControl?.OnShowing();
        }
    }
}
