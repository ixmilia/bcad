// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel;

namespace IxMilia.BCad.Plotting
{
    public interface IPlotterFactory
    {
        PlotterBase CreatePlotter(INotifyPropertyChanged viewModel);
        INotifyPropertyChanged CreatePlotterViewModel();
    }
}
