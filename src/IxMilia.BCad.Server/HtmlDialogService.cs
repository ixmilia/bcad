using System.IO;
using System.Threading.Tasks;
using IxMilia.BCad.Dialogs;
using IxMilia.BCad.FileHandlers;
using IxMilia.BCad.Plotting;
using IxMilia.BCad.Plotting.Pdf;
using IxMilia.BCad.Plotting.Svg;
using IxMilia.BCad.Services;

namespace IxMilia.BCad.Server
{
    public class HtmlDialogService : IDialogService
    {
        internal ServerAgent Agent { get; set; }

        private IWorkspace _workspace;
        private PdfPlotterFactory _pdfPlotterFactory;
        private SvgPlotterFactory _svgPlotterFactory;

        public HtmlDialogService(IWorkspace workspace)
        {
            _workspace = workspace;
            _pdfPlotterFactory = new PdfPlotterFactory(workspace);
            _svgPlotterFactory = new SvgPlotterFactory(workspace);
        }

        public async Task<object> ShowDialog(string id, object parameter)
        {
            object result = null;
            switch (id)
            {
                case "layer":
                    var layerParameters = (LayerDialogParameters)parameter;
                    var clientLayerParameters = new ClientLayerParameters(layerParameters);
                    var resultObject = await Agent.ShowDialog(id, clientLayerParameters);
                    if (resultObject != null)
                    {
                        var clientLayerResult = resultObject.ToObject<ClientLayerResult>();
                        var layerDialogResult = clientLayerResult.ToDialogResult();
                        result = layerDialogResult;
                    }
                    break;
                case "FileSettings":
                    var settingsResult = await Agent.ShowDialog(id, parameter);
                    if (settingsResult != null)
                    {
                        result = settingsResult.ToObject<DxfFileSettings>();
                    }
                    break;
                case "plot":
                    var plotResult = await Agent.ShowDialog(id, parameter);
                    if (plotResult != null)
                    {
                        var plotSettings = plotResult.ToObject<ClientPlotSettings>();
                        var viewModel = CreateAndPopulateViewModel(plotSettings);
                        using (var stream = PlotToStream(viewModel))
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            ms.Seek(0, SeekOrigin.Begin);
                            var bytes = ms.ToArray();
                            var filename = Path.GetFileNameWithoutExtension(_workspace.Drawing.Settings.FileName ?? "Untitled") + "." + plotSettings.PlotType;
                            var mimeType = plotSettings.PlotType switch
                            {
                                "pdf" => "application/pdf",
                                "svg" => "image/svg+xml",
                                _ => "application/octet-stream",
                            };
                            Agent.DownloadFile(filename, mimeType, bytes);
                        }

                        result = plotSettings;
                    }
                    break;
            }

            return result;
        }

        public ViewPortViewModelBase CreateAndPopulateViewModel(ClientPlotSettings settings, string plotTypeOverride = null)
        {
            ViewPortViewModelBase viewModel = null;
            switch (plotTypeOverride ?? settings.PlotType)
            {
                case "pdf":
                    var pdfViewModel = (PdfPlotterViewModel)_pdfPlotterFactory.CreatePlotterViewModel();
                    pdfViewModel.Width = settings.Width;
                    pdfViewModel.Height = settings.Height;
                    viewModel = pdfViewModel;
                    break;
                case "svg":
                    var svgViewModel = (SvgPlotterViewModel)_svgPlotterFactory.CreatePlotterViewModel();
                    svgViewModel.PlotAsDocument = true;
                    svgViewModel.Width = settings.Width;
                    svgViewModel.Height = settings.Height;
                    svgViewModel.OutputWidth = settings.Width;
                    svgViewModel.OutputHeight = settings.Height;
                    viewModel = svgViewModel;
                    break;
            }

            if (viewModel is object)
            {
                var transform = _workspace.ActiveViewPort.GetTransformationMatrixWindowsStyle(Agent.Width, Agent.Height).Inverse();
                var topLeft = transform.Transform(settings.Viewport.TopLeft.ToPoint());
                var bottomRight = transform.Transform(settings.Viewport.BottomRight.ToPoint());
                viewModel.ViewPortType = settings.ViewPortType;
                viewModel.ScalingType = settings.ScalingType;
                viewModel.ScaleA = DrawingSettings.TryParseUnits(settings.ScaleA, out var scaleA) ? scaleA : 1.0;
                viewModel.ScaleB = DrawingSettings.TryParseUnits(settings.ScaleB, out var scaleB) ? scaleB : 1.0;
                viewModel.BottomLeft = new Point(topLeft.X, bottomRight.Y, 0.0);
                viewModel.TopRight = new Point(bottomRight.X, topLeft.Y, 0.0);
            }

            return viewModel;
        }

        public Stream PlotToStream(ViewPortViewModelBase viewModel)
        {
            var stream = new MemoryStream();
            viewModel.Stream = stream;
            IPlotterFactory plotterFactory = viewModel switch
            {
                PdfPlotterViewModel _ => _pdfPlotterFactory,
                SvgPlotterViewModel _ => _svgPlotterFactory,
                _ => throw new System.Exception($"Unexpected view model: {viewModel?.GetType().Name}"),
            };
            var plotter = plotterFactory.CreatePlotter(viewModel);
            plotter.Plot(_workspace);
            viewModel.Stream.Seek(0, SeekOrigin.Begin);
            return viewModel.Stream;
        }
    }
}
