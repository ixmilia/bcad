using System.ComponentModel;

namespace IxMilia.BCad.Plotting
{
    public interface IPlotterFactory
    {
        PlotterBase CreatePlotter(INotifyPropertyChanged viewModel);
        INotifyPropertyChanged CreatePlotterViewModel();
    }
}
