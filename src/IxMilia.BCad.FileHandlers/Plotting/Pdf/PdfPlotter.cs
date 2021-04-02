using System.Linq;
using IxMilia.BCad.FileHandlers;
using IxMilia.Converters;
using IxMilia.Pdf;

namespace IxMilia.BCad.Plotting.Pdf
{
    internal class PdfPlotter : PlotterBase
    {
        public PdfPlotterViewModel ViewModel { get; }

        public PdfPlotter(PdfPlotterViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public override void Plot(IWorkspace workspace)
        {
            var converter = new DxfToPdfConverter();
            var fileSettings = new DxfFileSettings()
            {
                FileVersion = DxfFileVersion.R2004,
            };
            var dxfFile = DxfFileHandler.ToDxfFile(workspace.Drawing, workspace.ActiveViewPort, fileSettings);
            var page = ViewModel.Pages.Single(); // TODO: support multiple pages
            var pageWidth = new PdfMeasurement(page.ViewWidth, PdfMeasurementType.Inch);
            var pageHeight = new PdfMeasurement(page.ViewHeight, PdfMeasurementType.Inch);
            var viewPort = page.ViewPort;
            var viewPortWidth = page.ViewWidth / page.ViewHeight * viewPort.ViewHeight;
            var dxfRect = new ConverterDxfRect(viewPort.BottomLeft.X, viewPort.BottomLeft.X + viewPortWidth, viewPort.BottomLeft.Y, viewPort.BottomLeft.Y + viewPort.ViewHeight);
            var pdfRect = new ConverterPdfRect(
                new PdfMeasurement(0.0, PdfMeasurementType.Inch),
                new PdfMeasurement(page.ViewWidth, PdfMeasurementType.Inch),
                new PdfMeasurement(0.0, PdfMeasurementType.Inch),
                new PdfMeasurement(page.ViewHeight, PdfMeasurementType.Inch));
            var options = new DxfToPdfConverterOptions(pageWidth, pageHeight, dxfRect, pdfRect);
            var pdfFile = converter.Convert(dxfFile, options);
            pdfFile.Save(ViewModel.Stream);
        }
    }
}
