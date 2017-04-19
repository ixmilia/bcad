// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using BCad.Core.Test;
using BCad.Entities;
using BCad.Plotting;
using BCad.Plotting.Pdf;
using Xunit;

namespace BCad.FileHandlers.Test
{
    public class PdfPlotterTests : TestBase
    {
        private IPlotterFactory PlotterFactory;

        public PdfPlotterTests()
        {
            PlotterFactory = new PdfPlotterFactory()
            {
                Workspace = Workspace // faking the import
            };
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
            Assert.Equal(1, vm.Pages.Count);
            Workspace.Update(drawing: Workspace.Drawing.AddToCurrentLayer(new Line(new Point(0.0, 0.0, 0.0), new Point(8.5, 11.0, 0.0))));
            vm.SelectedPage.PageSize = PdfPageSize.Portrait;
            vm.SelectedPage.ScalingType = PlotScalingType.Absolute;
            vm.SelectedPage.ViewPortType = PlotViewPortType.Extents;
            var actual = PlotToString(vm);

            // line should be scaled to (8.5 * 72, 11 * 72)
            Assert.Contains(NormalizeToCrLf(@"
0.00 0.00 m
612.00 792.00 l
"), actual);
        }
    }
}
