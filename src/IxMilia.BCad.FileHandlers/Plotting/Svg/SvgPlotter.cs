using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using IxMilia.BCad.FileHandlers;
using IxMilia.Converters;

namespace IxMilia.BCad.Plotting.Svg
{
    internal class SvgPlotter : PlotterBase
    {
        public SvgPlotterViewModel ViewModel { get; }

        public SvgPlotter(SvgPlotterViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public override async Task Plot(Drawing drawing, ViewPort viewPort, Stream outputStream, Func<string, Task<byte[]>> contentResolver)
        {
            var converter = new DxfToSvgConverter();
            var fileSettings = new DxfFileSettings()
            {
                FileVersion = DxfFileVersion.R2004,
            };
            var dxfFile = DxfFileHandler.ToDxfFile(drawing, viewPort, fileSettings);
            var plotViewPort = ViewModel.ViewPort;
            var viewPortWidth = ViewModel.DisplayWidth / ViewModel.DisplayHeight * plotViewPort.ViewHeight;
            var dxfRect = new ConverterDxfRect(plotViewPort.BottomLeft.X, plotViewPort.BottomLeft.X + viewPortWidth, plotViewPort.BottomLeft.Y, plotViewPort.BottomLeft.Y + plotViewPort.ViewHeight);
            var svgRect = new ConverterSvgRect(ViewModel.DisplayWidth, ViewModel.DisplayHeight);
            var options = new DxfToSvgConverterOptions(dxfRect, svgRect, imageHrefResolver: DxfToSvgConverterOptions.CreateDataUriResolver(contentResolver));
            var xml = await converter.Convert(dxfFile, options);
            xml.Attribute("width").Value = $"{ViewModel.OutputWidth}";
            xml.Attribute("height").Value = $"{ViewModel.OutputHeight}";

            var writerSettings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "  "
            };
            using (var writer = XmlWriter.Create(outputStream, writerSettings))
            {
                if (ViewModel.PlotAsDocument)
                {
                    var doc = new XDocument(
                        new XDocumentType("svg", "-//W3C//DTD SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", null),
                        xml);
                    doc.WriteTo(writer);
                }
                else
                {
                    xml.WriteTo(writer);
                }

                writer.Flush();
            }
        }
    }
}
