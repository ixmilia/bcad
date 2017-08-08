// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using IxMilia.BCad.Plotting.Pdf;
using IxMilia.BCad.Services;

namespace IxMilia.BCad.UI.Controls
{
    public partial class PdfPlotterControl : PlotterControl
    {
        private PdfPlotterViewModel _viewModel;

        [Import]
        public IWorkspace Workspace { get; set; }

        public PdfPlotterControl()
        {
            InitializeComponent();
        }

        [OnImportsSatisfied]
        public void OnImportsSatisfied()
        {
            _viewModel = (PdfPlotterViewModel)DataContext;
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
            var fileName = await Workspace.FileSystemService.GetFileNameFromUserForWrite(new[] { new FileSpecification("PDF files", new[] { ".pdf" }) });
            if (fileName != null)
            {
                _viewModel.FileName = fileName;
            }
        }

        private void AddClick(object sender, RoutedEventArgs e)
        {
            _viewModel.Pages.Add(new PdfPageViewModel(Workspace));
            _viewModel.SelectedPage = _viewModel.Pages.Last();
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            _viewModel.Pages.Remove(_viewModel.SelectedPage);
            if (_viewModel.Pages.Count == 0)
            {
                _viewModel.Pages.Add(new PdfPageViewModel(Workspace));
            }

            _viewModel.SelectedPage = _viewModel.Pages.First();
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

            _viewModel.SelectedPage.UpdateViewWindow(bottomLeft, topRight);
            Show();
        }

        private async void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            await GetExportArea();
        }
    }
}
