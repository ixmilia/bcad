using System;
using System.IO;
using System.Threading.Tasks;
using IxMilia.BCad.Dialogs;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.FileHandlers;
using IxMilia.BCad.Plotting;
using IxMilia.BCad.Plotting.Pdf;
using IxMilia.BCad.Plotting.Svg;
using IxMilia.BCad.Services;

namespace IxMilia.BCad.Rpc
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
                case "line-type":
                    var lineTypeParameters = (LineTypeDialogParameters)parameter;
                    var clientLineTypeParameters = new ClientLineTypeParameters(lineTypeParameters);
                    var resultObject2 = await Agent.ShowDialog(id, clientLineTypeParameters);
                    if (resultObject2 != null)
                    {
                        var clientLineTypeResult = resultObject2.ToObject<ClientLineTypeResult>();
                        var lineTypeDialogResult = clientLineTypeResult.ToDialogResult();
                        result = lineTypeDialogResult;
                    }
                    break;
                case "FileSettings":
                    var settingsResult = await Agent.ShowDialog(id, parameter);
                    if (settingsResult != null)
                    {
                        if (parameter is FileSettings fileSettings)
                        {
                            result = fileSettings.Extension.ToLowerInvariant() switch
                            {
                                ".dwg" => settingsResult.ToObject<DwgFileSettings>(),
                                ".dxf" => settingsResult.ToObject<DxfFileSettings>(),
                                _ => throw new Exception("Unexpected file extension"),
                            };
                        }
                        else
                        {
                            throw new Exception("Expected file settings object");
                        }
                    }
                    break;
                case "plot":
                    var plotResult = await Agent.ShowDialog(id, parameter);
                    if (plotResult != null)
                    {
                        var plotSettings = plotResult.ToObject<ClientPlotSettings>();
                        var fileSpecification = _workspace.GetFileSpecificationFromExtension("." + plotSettings.PlotType);
                        var fileName = await _workspace.FileSystemService.GetFileNameFromUserForSave(new[] { fileSpecification });
                        if (fileName != null)
                        {
                            var viewModel = CreateAndPopulateViewModel(plotSettings, _workspace.Drawing.Settings);
                            var drawing = _workspace.Drawing.UpdateColors(plotSettings.ColorType);
                            using (var stream = await PlotToStream(viewModel, drawing, _workspace.ActiveViewPort))
                            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                            {
                                await stream.CopyToAsync(fileStream);
                            }

                            result = plotSettings;
                        }
                    }
                    break;
                case "saveChanges":
                    var saveChangesResult = await Agent.ShowDialog(id, parameter);
                    if (saveChangesResult is null)
                    {
                        result = UnsavedChangesResult.Cancel;
                    }
                    else
                    {
                        var stringValue = saveChangesResult["result"].ToString();
                        switch (stringValue)
                        {
                            case "save":
                                result = UnsavedChangesResult.Saved;
                                break;
                            case "discard":
                                result = UnsavedChangesResult.Discarded;
                                break;
                        }
                    }
                    break;
            }

            return result;
        }

        public ViewPortViewModelBase CreateAndPopulateViewModel(ClientPlotSettings settings, DrawingSettings drawingSettings, string plotTypeOverride = null)
        {
            ViewPortViewModelBase viewModel = null;
            switch (plotTypeOverride ?? settings.PlotType)
            {
                case "pdf":
                    var pdfViewModel = (PdfPlotterViewModel)_pdfPlotterFactory.CreatePlotterViewModel();
                    pdfViewModel.DisplayWidth = settings.Width;
                    pdfViewModel.DisplayHeight = settings.Height;
                    viewModel = pdfViewModel;
                    break;
                case "svg":
                    var svgViewModel = (SvgPlotterViewModel)_svgPlotterFactory.CreatePlotterViewModel();
                    svgViewModel.PlotAsDocument = true;
                    svgViewModel.DisplayWidth = settings.Width;
                    svgViewModel.DisplayHeight = settings.Height;
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
                var (scaleA, scaleB) = settings.GetUnitAdjustedScale(drawingSettings);
                viewModel.ScaleA = scaleA;
                viewModel.ScaleB = scaleB;
                viewModel.BottomLeft = new Point(topLeft.X, bottomRight.Y, 0.0);
                viewModel.TopRight = new Point(bottomRight.X, topLeft.Y, 0.0);
            }

            return viewModel;
        }

        public async Task<Stream> PlotToStream(ViewPortViewModelBase viewModel, Drawing drawing, ViewPort viewPort)
        {
            var stream = new MemoryStream();
            IPlotterFactory plotterFactory = viewModel switch
            {
                PdfPlotterViewModel _ => _pdfPlotterFactory,
                SvgPlotterViewModel _ => _svgPlotterFactory,
                _ => throw new System.Exception($"Unexpected view model: {viewModel?.GetType().Name}"),
            };
            var plotter = plotterFactory.CreatePlotter(viewModel);
            await plotter.Plot(drawing, viewPort, stream, _workspace.FileSystemService.GetContentResolverRelativeToPath(drawing.Settings.FileName));
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}
