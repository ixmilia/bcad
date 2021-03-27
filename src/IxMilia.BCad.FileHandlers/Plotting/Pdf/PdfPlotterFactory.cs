using System.ComponentModel;

namespace IxMilia.BCad.Plotting.Pdf
{
    public class PdfPlotterFactory : IPlotterFactory
    {
        private IWorkspace _workspace;

        public PdfPlotterFactory(IWorkspace workspace)
        {
            _workspace = workspace;
        }

        public PlotterBase CreatePlotter(INotifyPropertyChanged viewModel)
        {
            return new PdfPlotter((PdfPlotterViewModel)viewModel);
        }

        public INotifyPropertyChanged CreatePlotterViewModel()
        {
            return new PdfPlotterViewModel(_workspace);
        }
    }
}
