using System.ComponentModel;
using System.Composition;

namespace IxMilia.BCad.Plotting.Svg
{
    [ExportPlotterFactory("SVG", ViewTypeName = "IxMilia.BCad.UI.Controls.SvgPlotterControl")]
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
