using System.ComponentModel;

namespace IxMilia.BCad.Plotting.Svg
{
    public class SvgPlotterFactory : IPlotterFactory
    {
        private IWorkspace _workspace;

        public SvgPlotterFactory(IWorkspace workspace)
        {
            _workspace = workspace;
        }

        public PlotterBase CreatePlotter(INotifyPropertyChanged viewModel)
        {
            return new SvgPlotter((SvgPlotterViewModel)viewModel);
        }

        public INotifyPropertyChanged CreatePlotterViewModel()
        {
            return new SvgPlotterViewModel(_workspace);
        }
    }
}
