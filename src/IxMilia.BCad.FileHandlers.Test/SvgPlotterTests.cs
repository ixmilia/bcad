using System.IO;
using IxMilia.BCad.Core.Test;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Plotting;
using IxMilia.BCad.Plotting.Svg;
using Xunit;


namespace IxMilia.BCad.FileHandlers.Test
{
    public class SvgPlotterTests : TestBase
    {
        private IPlotterFactory PlotterFactory;

        public SvgPlotterTests()
        {
            PlotterFactory = new SvgPlotterFactory(Workspace);
        }

        private string PlotToString(SvgPlotterViewModel viewModel)
        {
            using (var ms = new MemoryStream())
            {
                var plotter = PlotterFactory.CreatePlotter(viewModel);
                plotter.Plot(Workspace.Drawing, Workspace.ActiveViewPort, ms, Workspace.FileSystemService.ReadAllBytesAsync);
                ms.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(ms))
                {
                    var content = reader.ReadToEnd();
                    return content;
                }
            }
        }

        private static string NormalizeToLf(string s)
        {
            return s.Trim('\r', '\n').Replace("\r", "");
        }

        [Fact]
        public void PlotWithMarginTest()
        {
            var vm = (SvgPlotterViewModel)PlotterFactory.CreatePlotterViewModel();
            Workspace.Update(drawing: Workspace.Drawing.AddToCurrentLayer(new Line(new Point(0.0, 0.0, 0.0), new Point(600.0, 440.0, 0.0))));
            vm.DisplayWidth = 640.0;
            vm.DisplayHeight = 480.0;
            vm.ScalingType = PlotScalingType.Absolute;
            vm.ScaleA = 1.0;
            vm.ScaleB = 1.0;
            vm.Margin = 20.0;
            vm.ViewPortType = PlotViewPortType.Extents;
            var actual = NormalizeToLf(PlotToString(vm));

            Assert.Contains(@"viewBox=""0 0 640.0 480.0""", actual);
            Assert.Contains(NormalizeToLf(@"
<!-- this group corrects for display margins -->
  <g transform=""translate(20.0 -20.0)"">
"), actual);
        }
    }
}
