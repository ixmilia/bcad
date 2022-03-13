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
                var plotter = PlotterFactory.CreatePlotter(viewModel);
                plotter.Plot(Workspace.Drawing, Workspace.ActiveViewPort, ms);
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
            vm.DisplayWidth = 8.5;
            vm.DisplayHeight = 11.0;
            vm.DisplayUnit = PdfMeasurementType.Inch;
            vm.ScalingType = PlotScalingType.Absolute;
            vm.ViewPortType = PlotViewPortType.Extents;
            var actual = PlotToString(vm);

            // line should be scaled to (8.5 * 72, 11 * 72)
            Assert.Contains(NormalizeToCrLf(@"
0.00 0.00 m
612.00 792.00 l
"), actual);
        }

        [Fact]
        public void PlotDrawingExtentsToFitTest()
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
            var actual = PlotToString(vm);

            // line should be scaled to (8.5 * 72, 11 * 72)
            Assert.Contains(NormalizeToCrLf(@"
0.00 0.00 m
612.00 792.00 l
"), actual);
        }

        [Fact]
        public void PlotWindowToFitTest()
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
            var actual = PlotToString(vm);

            // line should be scaled to (8.5 * 72, 11 * 72)
            Assert.Contains(NormalizeToCrLf(@"
0.00 0.00 m
612.00 792.00 l
"), actual);
        }

        [Fact]
        public void PlotDrawingExtentsToScaleTest()
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
            vm.ScaleA = 2.0;
            vm.ScaleB = 1.0;
            vm.ViewPortType = PlotViewPortType.Extents;
            var actual = PlotToString(vm);

            // line should be scaled to (8.5 * 72, 11 * 72)
            Assert.Contains(NormalizeToCrLf(@"
0.00 0.00 m
612.00 792.00 l
"), actual);
        }

        [Fact]
        public void PlotWindowToScaleTest()
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
            vm.ScaleA = 2.0;
            vm.ScaleB = 1.0;
            vm.ViewPortType = PlotViewPortType.Window;
            vm.UpdateViewWindow(new Point(8.5, 11.0, 0.0), new Point(25.5, 33.0, 0.0));
            var actual = PlotToString(vm);

            // line should be scaled to (8.5 * 72, 11 * 72)
            Assert.Contains(NormalizeToCrLf(@"
0.00 0.00 m
612.00 792.00 l
"), actual);
        }
    }
}
