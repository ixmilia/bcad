using System.IO;
using IxMilia.BCad.Core.Test;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Plotting;
using IxMilia.BCad.Plotting.Pdf;
using IxMilia.Pdf;
using Xunit;

namespace IxMilia.BCad.FileHandlers.Test
{
    public class PdfPlotterTests : TestBase
    {
        private IPlotterFactory PlotterFactory;

        public PdfPlotterTests()
        {
            PlotterFactory = new PdfPlotterFactory(Workspace);
        }

        private string PlotToString(PdfPlotterViewModel viewModel)
        {
            using (var ms = new MemoryStream())
            {
                viewModel.Stream = ms;
                var plotter = PlotterFactory.CreatePlotter(viewModel);
                plotter.Plot(Workspace);
                ms.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(ms))
                {
                    var content = reader.ReadToEnd();
                    return content;
                }
            }
        }

        private static string NormalizeToCrLf(string s)
        {
            return s.Trim('\r', '\n').Replace("\r", "").Replace("\n", "\r\n");
        }

        [Fact]
        public void SimplePlotTest()
        {
            var vm = (PdfPlotterViewModel)PlotterFactory.CreatePlotterViewModel();
            Workspace.Update(drawing: Workspace.Drawing.AddToCurrentLayer(new Line(new Point(0.0, 0.0, 0.0), new Point(8.5, 11.0, 0.0))));
            vm.Width = new PdfMeasurement(8.5, PdfMeasurementType.Inch).AsPoints();
            vm.Height = new PdfMeasurement(11.0, PdfMeasurementType.Inch).AsPoints();
            vm.ScalingType = PlotScalingType.Absolute;
            vm.ViewPortType = PlotViewPortType.Extents;
            var actual = PlotToString(vm);

            // line should be scaled to (8.5 * 72, 11 * 72)
            Assert.Contains(NormalizeToCrLf(@"
0.00 0.00 m
612.00 792.00 l
"), actual);
        }
    }
}
