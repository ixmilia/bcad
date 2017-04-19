// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel;
using System.Composition;

namespace BCad.Plotting.Pdf
{
    [ExportPlotterFactory("PDF", ViewTypeName = "BCad.UI.Controls.PdfPlotterControl")]
    public class PdfPlotterFactory : IPlotterFactory
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        public PlotterBase CreatePlotter(INotifyPropertyChanged viewModel)
        {
            return new PdfPlotter((PdfPlotterViewModel)viewModel);
        }

        public INotifyPropertyChanged CreatePlotterViewModel()
        {
            return new PdfPlotterViewModel(Workspace);
        }
    }
}
