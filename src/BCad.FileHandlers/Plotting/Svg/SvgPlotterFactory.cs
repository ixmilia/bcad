// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel;
using System.Composition;

namespace BCad.Plotting.Svg
{
    [ExportPlotterFactory("SVG", ViewTypeName = "BCad.UI.Controls.SvgPlotterControl")]
    public class SvgPlotterFactory : IPlotterFactory
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        public PlotterBase CreatePlotter(INotifyPropertyChanged viewModel)
        {
            return new SvgPlotter((SvgPlotterViewModel)viewModel);
        }

        public INotifyPropertyChanged CreatePlotterViewModel()
        {
            return new SvgPlotterViewModel(Workspace);
        }
    }
}
