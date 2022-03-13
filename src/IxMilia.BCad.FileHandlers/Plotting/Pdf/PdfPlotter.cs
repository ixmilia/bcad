using System.IO;
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

        public override void Plot(Drawing drawing, ViewPort viewPort, Stream outputStream)
        {
            var converter = new DxfToPdfConverter();
            var fileSettings = new DxfFileSettings()
            {
                FileVersion = DxfFileVersion.R2004,
            };
            var dxfFile = DxfFileHandler.ToDxfFile(drawing, viewPort, fileSettings);
            var pageWidth = new PdfMeasurement(ViewModel.DisplayWidth, ViewModel.DisplayUnit);
            var pageHeight = new PdfMeasurement(ViewModel.DisplayHeight, ViewModel.DisplayUnit);
            var plotViewPort = ViewModel.ViewPort;
            var viewPortWidth = ViewModel.DisplayWidth / ViewModel.DisplayHeight * plotViewPort.ViewHeight;
            var dxfRect = new ConverterDxfRect(plotViewPort.BottomLeft.X, plotViewPort.BottomLeft.X + viewPortWidth, plotViewPort.BottomLeft.Y, plotViewPort.BottomLeft.Y + plotViewPort.ViewHeight);
            var pdfRect = new ConverterPdfRect(
                new PdfMeasurement(0.0, ViewModel.DisplayUnit),
                new PdfMeasurement(ViewModel.DisplayWidth, ViewModel.DisplayUnit),
                new PdfMeasurement(0.0, ViewModel.DisplayUnit),
                new PdfMeasurement(ViewModel.DisplayHeight, ViewModel.DisplayUnit));
            var options = new DxfToPdfConverterOptions(pageWidth, pageHeight, dxfRect, pdfRect);
            var pdfFile = converter.Convert(dxfFile, options);
            pdfFile.Save(outputStream);
        }
    }
}
