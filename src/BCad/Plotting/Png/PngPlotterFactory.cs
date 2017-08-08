// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel;
using System.Composition;

namespace IxMilia.BCad.Plotting.Png
{
    [ExportPlotterFactory("PNG", ViewTypeName = "IxMilia.BCad.UI.Controls.PngPlotterControl")]
    public class PngPlotterFactory : IPlotterFactory
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        public PlotterBase CreatePlotter(INotifyPropertyChanged viewModel)
        {
            return new PngPlotter((PngPlotterViewModel)viewModel);
        }

        public INotifyPropertyChanged CreatePlotterViewModel()
        {
            return new PngPlotterViewModel(Workspace);
        }
    }
}
