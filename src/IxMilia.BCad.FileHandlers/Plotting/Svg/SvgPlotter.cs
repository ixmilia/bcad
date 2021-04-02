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

        public override void Plot(IWorkspace workspace)
        {
            var converter = new DxfToSvgConverter();
            var fileSettings = new DxfFileSettings()
            {
                FileVersion = DxfFileVersion.R2004,
            };
            var dxfFile = DxfFileHandler.ToDxfFile(workspace.Drawing, workspace.ActiveViewPort, fileSettings);
            var viewPort = ViewModel.ViewPort;
            var viewPortWidth = ViewModel.ViewWidth / ViewModel.ViewHeight * viewPort.ViewHeight;
            var dxfRect = new ConverterDxfRect(viewPort.BottomLeft.X, viewPort.BottomLeft.X + viewPortWidth, viewPort.BottomLeft.Y, viewPort.BottomLeft.Y + viewPort.ViewHeight);
            var svgRect = new ConverterSvgRect(ViewModel.ViewWidth, ViewModel.ViewHeight);
            var options = new DxfToSvgConverterOptions(dxfRect, svgRect);
            var xml = converter.Convert(dxfFile, options);
            xml.Attribute("width").Value = $"{ViewModel.OutputWidth}";
            xml.Attribute("height").Value = $"{ViewModel.OutputHeight}";

            var writerSettings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "  "
            };
            using (var writer = XmlWriter.Create(ViewModel.Stream, writerSettings))
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
