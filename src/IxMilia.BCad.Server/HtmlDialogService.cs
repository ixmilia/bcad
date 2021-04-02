using System.IO;
using System.Linq;
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

        public ViewModelBase CreateAndPopulateViewModel(ClientPlotSettings settings, string plotTypeOverride = null)
        {
            ViewModelBase viewModel = null;
            switch (plotTypeOverride ?? settings.PlotType)
            {
                case "pdf":
                    {
                        var pdfViewModel = (PdfPlotterViewModel)_pdfPlotterFactory.CreatePlotterViewModel();
                        var pageViewModel = pdfViewModel.Pages.Single(); // TODO: support multiple pages
                        pageViewModel.ViewPortType = settings.ViewPortType;
                        pageViewModel.ScalingType = settings.ScalingType;
                        pageViewModel.ScaleA = DrawingSettings.TryParseUnits(settings.ScaleA, out var scaleA) ? scaleA : 1.0;
                        pageViewModel.ScaleB = DrawingSettings.TryParseUnits(settings.ScaleB, out var scaleB) ? scaleB : 1.0;
                        pageViewModel.BottomLeft = new Point(settings.Viewport.TopLeft.X, settings.Viewport.TopLeft.Y, 0.0);
                        pageViewModel.TopRight = new Point(settings.Viewport.BottomRight.X, settings.Viewport.BottomRight.Y, 0.0);
                        pageViewModel.Width = settings.Width;
                        pageViewModel.Height = settings.Height;
                        viewModel = pdfViewModel;
                    }
                    break;
                case "svg":
                    {
                        var svgViewModel = (SvgPlotterViewModel)_svgPlotterFactory.CreatePlotterViewModel();
                        svgViewModel.PlotAsDocument = true;
                        svgViewModel.ViewPortType = settings.ViewPortType;
                        svgViewModel.ScalingType = settings.ScalingType;
                        svgViewModel.ScaleA = DrawingSettings.TryParseUnits(settings.ScaleA, out var scaleA) ? scaleA : 1.0;
                        svgViewModel.ScaleB = DrawingSettings.TryParseUnits(settings.ScaleB, out var scaleB) ? scaleB : 1.0;
                        svgViewModel.BottomLeft = new Point(settings.Viewport.TopLeft.X, settings.Viewport.TopLeft.Y, 0.0);
                        svgViewModel.TopRight = new Point(settings.Viewport.BottomRight.X, settings.Viewport.BottomRight.Y, 0.0);
                        svgViewModel.Width = settings.Width;
                        svgViewModel.Height = settings.Height;
                        svgViewModel.OutputWidth = settings.Width;
                        svgViewModel.OutputHeight = settings.Height;
                        viewModel = svgViewModel;
                    }
                    break;
            }

            return viewModel;
        }

        public Stream PlotToStream(ViewModelBase viewModel)
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
