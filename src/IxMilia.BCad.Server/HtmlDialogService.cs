using System.IO;
using System.Threading.Tasks;
using IxMilia.BCad.Dialogs;
using IxMilia.BCad.FileHandlers;
using IxMilia.BCad.Plotting.Svg;
using IxMilia.BCad.Services;

namespace IxMilia.BCad.Server
{
    public class HtmlDialogService : IDialogService
    {
        internal ServerAgent Agent { get; set; }

        private IWorkspace _workspace;
        private SvgPlotterFactory _svgPlotterFactory;

        public HtmlDialogService(IWorkspace workspace)
        {
            _workspace = workspace;
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
                            var filename = Path.GetFileNameWithoutExtension(_workspace.Drawing.Settings.FileName ?? "Untitled") + ".svg";
                            Agent.DownloadFile(filename, bytes);
                        }

                        result = plotSettings;
                    }
                    break;
            }

            return result;
        }

        public SvgPlotterViewModel CreateAndPopulateViewModel(ClientPlotSettings settings)
        {
            var viewModel = (SvgPlotterViewModel)_svgPlotterFactory.CreatePlotterViewModel();
            viewModel.PlotAsDocument = false;
            viewModel.ViewPortType = settings.ViewPortType;
            viewModel.ScalingType = settings.ScalingType;
            viewModel.ScaleA = DrawingSettings.TryParseUnits(settings.ScaleA, out var scaleA) ? scaleA : 1.0;
            viewModel.ScaleB = DrawingSettings.TryParseUnits(settings.ScaleB, out var scaleB) ? scaleB : 1.0;
            viewModel.BottomLeft = new Point(settings.Viewport.TopLeft.X, settings.Viewport.TopLeft.Y, 0.0);
            viewModel.TopRight = new Point(settings.Viewport.BottomRight.X, settings.Viewport.BottomRight.Y, 0.0);
            viewModel.Width = settings.Width;
            viewModel.Height = settings.Height;
            viewModel.OutputWidth = settings.Width;
            viewModel.OutputHeight = settings.Height;
            return viewModel;
        }

        public Stream PlotToStream(SvgPlotterViewModel viewModel)
        {
            var stream = new MemoryStream();
            viewModel.Stream = stream;
            var plotter = _svgPlotterFactory.CreatePlotter(viewModel);
            plotter.Plot(_workspace);
            viewModel.Stream.Seek(0, SeekOrigin.Begin);
            return viewModel.Stream;
        }
    }
}
