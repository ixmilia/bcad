using System.IO;
using System.Threading.Tasks;
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

        private async Task<string> PlotToStringAsync(PdfPlotterViewModel viewModel)
        {
            using (var ms = new MemoryStream())
            {
                var plotter = PlotterFactory.CreatePlotter(viewModel);
                await plotter.Plot(Workspace.Drawing, Workspace.ActiveViewPort, ms, Workspace.FileSystemService.ReadAllBytesAsync);
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
        public async Task SimplePlotTest()
        {
            var vm = (PdfPlotterViewModel)PlotterFactory.CreatePlotterViewModel();
            Workspace.Update(drawing: Workspace.Drawing.AddToCurrentLayer(new Line(new Point(0.0, 0.0, 0.0), new Point(8.5, 11.0, 0.0))));
            vm.DisplayWidth = 8.5;
            vm.DisplayHeight = 11.0;
            vm.DisplayUnit = PdfMeasurementType.Inch;
            vm.ScalingType = PlotScalingType.Absolute;
            vm.ViewPortType = PlotViewPortType.Extents;
            var actual = await PlotToStringAsync(vm);

            // line should be scaled to (8.5 * 72, 11 * 72)
            Assert.Contains(NormalizeToCrLf(@"
0.00 0.00 m
612.00 792.00 l
"), actual);
        }

        [Fact]
        public async Task PlotDrawingExtentsToFitTest()
        {
            //     / (25.5, 33) # 25.5 = 8.5 * 2 + 8.5
            //    /             # 33 = 11 * 2 + 11
            //   /
            //  /
            // / (8.5, 11)
            var vm = (PdfPlotterViewModel)PlotterFactory.CreatePlotterViewModel();
            Workspace.Update(drawing: Workspace.Drawing.AddToCurrentLayer(new Line(new Point(8.5, 11.0, 0.0), new Point(25.5, 33.0, 0.0))));
            vm.DisplayWidth = 8.5;
            vm.DisplayHeight = 11.0;
            vm.DisplayUnit = PdfMeasurementType.Inch;
            vm.ScalingType = PlotScalingType.ToFit;
            vm.ViewPortType = PlotViewPortType.Extents;
            var actual = await PlotToStringAsync(vm);

            // line should be scaled to (8.5 * 72, 11 * 72)
            Assert.Contains(NormalizeToCrLf(@"
0.00 0.00 m
612.00 792.00 l
"), actual);
        }

        [Fact]
        public async Task PlotWindowToFitTest()
        {
            //     / (25.5, 33) # 25.5 = 8.5 * 2 + 8.5
            //    /             # 33 = 11 * 2 + 11
            //   /
            //  /
            // / (8.5, 11)
            var vm = (PdfPlotterViewModel)PlotterFactory.CreatePlotterViewModel();
            Workspace.Update(drawing: Workspace.Drawing.AddToCurrentLayer(new Line(new Point(8.5, 11.0, 0.0), new Point(25.5, 33.0, 0.0))));
            vm.DisplayWidth = 8.5;
            vm.DisplayHeight = 11.0;
            vm.DisplayUnit = PdfMeasurementType.Inch;
            vm.ScalingType = PlotScalingType.ToFit;
            vm.ViewPortType = PlotViewPortType.Window;
            vm.UpdateViewWindow(new Point(8.5, 11.0, 0.0), new Point(25.5, 33.0, 0.0));
            var actual = await PlotToStringAsync(vm);

            // line should be scaled to (8.5 * 72, 11 * 72)
            Assert.Contains(NormalizeToCrLf(@"
0.00 0.00 m
612.00 792.00 l
"), actual);
        }

        [Fact]
        public async Task PlotDrawingExtentsToScaleTest()
        {
            //     / (25.5, 33) # 25.5 = 8.5 * 2 + 8.5
            //    /             # 33 = 11 * 2 + 11
            //   /
            //  /
            // / (8.5, 11)
            var vm = (PdfPlotterViewModel)PlotterFactory.CreatePlotterViewModel();
            Workspace.Update(drawing: Workspace.Drawing.AddToCurrentLayer(new Line(new Point(8.5, 11.0, 0.0), new Point(25.5, 33.0, 0.0))));
            vm.DisplayWidth = 8.5;
            vm.DisplayHeight = 11.0;
            vm.DisplayUnit = PdfMeasurementType.Inch;
            vm.ScalingType = PlotScalingType.Absolute;
            vm.ScaleA = 1.0;
            vm.ScaleB = 2.0;
            vm.ViewPortType = PlotViewPortType.Extents;
            var actual = await PlotToStringAsync(vm);

            // line should be scaled to (8.5 * 72, 11 * 72)
            Assert.Contains(NormalizeToCrLf(@"
0.00 0.00 m
612.00 792.00 l
"), actual);
        }

        [Fact]
        public async Task PlotWindowToScaleTest()
        {
            //     / (25.5, 33) # 25.5 = 8.5 * 2 + 8.5
            //    /             # 33 = 11 * 2 + 11
            //   /
            //  /
            // / (8.5, 11)
            var vm = (PdfPlotterViewModel)PlotterFactory.CreatePlotterViewModel();
            Workspace.Update(drawing: Workspace.Drawing.AddToCurrentLayer(new Line(new Point(8.5, 11.0, 0.0), new Point(25.5, 33.0, 0.0))));
            vm.DisplayWidth = 8.5;
            vm.DisplayHeight = 11.0;
            vm.DisplayUnit = PdfMeasurementType.Inch;
            vm.ScalingType = PlotScalingType.Absolute;
            vm.ScaleA = 1.0;
            vm.ScaleB = 2.0;
            vm.ViewPortType = PlotViewPortType.Window;
            vm.UpdateViewWindow(new Point(8.5, 11.0, 0.0), new Point(25.5, 33.0, 0.0));
            var actual = await PlotToStringAsync(vm);

            // line should be scaled to (8.5 * 72, 11 * 72)
            Assert.Contains(NormalizeToCrLf(@"
0.00 0.00 m
612.00 792.00 l
"), actual);
        }

        [Fact]
        public async Task PlotWithMarginTest()
        {
            var vm = (PdfPlotterViewModel)PlotterFactory.CreatePlotterViewModel();
            Workspace.Update(drawing: Workspace.Drawing.AddToCurrentLayer(new Line(new Point(0.0, 0.0, 0.0), new Point(4.0, 5.25, 0.0))));
            vm.DisplayWidth = 8.5;
            vm.DisplayHeight = 11.0;
            vm.DisplayUnit = PdfMeasurementType.Inch;
            vm.ScalingType = PlotScalingType.Absolute;
            vm.ScaleA = 2.0;
            vm.ScaleB = 1.0;
            vm.Margin = 0.25;
            vm.MarginUnit = PdfMeasurementType.Inch;
            vm.ViewPortType = PlotViewPortType.Extents;
            var actual = await PlotToStringAsync(vm);

            Assert.Contains(NormalizeToCrLf(@"
18.00 18.00 m
594.00 774.00 l
"), actual);
        }
    }
}
