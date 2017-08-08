// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using IxMilia.BCad.Plotting.Png;
using IxMilia.BCad.Services;

namespace IxMilia.BCad.UI.Controls
{
    public partial class PngPlotterControl : PlotterControl
    {
        private PngPlotterViewModel _viewModel;

        [Import]
        public IWorkspace Workspace { get; set; }

        public PngPlotterControl()
        {
            InitializeComponent();
        }

        [OnImportsSatisfied]
        public void OnImportsSatisfied()
        {
            _viewModel = (PngPlotterViewModel)DataContext;
        }

        public override void OnShowing()
        {
            renderCanvas.Drawing = Workspace.Drawing;
        }

        public override void BeforeCommit()
        {
            var fs = new FileStream(_viewModel.FileName, FileMode.Create);
            _viewModel.Stream = fs;
        }

        public override void AfterCommit()
        {
            _viewModel.Stream?.Flush();
            _viewModel.Stream?.Close();
        }

        private async void BrowseClick(object sender, RoutedEventArgs e)
        {
            var fileName = await Workspace.FileSystemService.GetFileNameFromUserForWrite(new[] { new FileSpecification("PNG files", new[] { ".png" }) });
            if (fileName != null)
            {
                _viewModel.FileName = fileName;
            }
        }

        private async void SelectAreaClick(object sender, RoutedEventArgs e)
        {
            await GetExportArea();
        }

        private async Task GetExportArea()
        {
            Hide();
            var selection = await Workspace.ViewControl.GetSelectionRectangle();
            if (selection == null)
                return;

            var bottomLeft = new Point(selection.TopLeftWorld.X, selection.BottomRightWorld.Y, selection.TopLeftWorld.Z);
            var topRight = new Point(selection.BottomRightWorld.X, selection.TopLeftWorld.Y, selection.BottomRightWorld.Z);
            if (topRight.Y - bottomLeft.Y == 0.0)
            {
                // ensure the view height isn't zero
                topRight = new Point(topRight.X, topRight.Y + 1.0, topRight.Z);
            }

            _viewModel.UpdateViewWindow(bottomLeft, topRight);
            Show();
        }

        private async void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            await GetExportArea();
        }
    }
}
