using System;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using BCad.FilePlotters;
using BCad.Helpers;
using BCad.Services;
using BCad.UI.View;

namespace BCad.UI.Controls
{
    /// <summary>
    /// Interaction logic for PlotDialog.xaml
    /// </summary>
    [ExportControl("Plot", "Default", "Plot")]
    public partial class PlotDialog : BCadControl
    {
        private IWorkspace workspace = null;
        private IInputService inputService = null;
        private IFileSystemService fileSystemService = null;
        private IEnumerable<Lazy<IFilePlotter, FilePlotterMetadata>> filePlotters = null;
        private PlotDialogViewModel viewModel = null;

        public PlotDialog()
        {
            InitializeComponent();

        }

        [ImportingConstructor]
        public PlotDialog(IWorkspace workspace, IInputService inputService, IFileSystemService fileSystemService, [ImportMany] IEnumerable<Lazy<IFilePlotter, FilePlotterMetadata>> filePlotters)
            : this()
        {
            this.workspace = workspace;
            this.inputService = inputService;
            this.fileSystemService = fileSystemService;
            this.filePlotters = filePlotters;

            viewModel = new PlotDialogViewModel();
            DataContext = viewModel;
        }

        public override void OnShowing()
        {
            viewModel.Drawing = workspace.Drawing;
            viewModel.ActiveViewPort = workspace.ActiveViewPort;
        }

        public override void Commit()
        {
            switch (this.viewModel.PlotType)
            {
                case PlotType.File:
                    FilePlot();
                    break;
                case PlotType.Print:
                    Print();
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void FilePlot()
        {
            IFilePlotter plotter;
            Stream stream;
            var viewPort = viewModel.ViewPort;
            var extension = Path.GetExtension(viewModel.FileName);
            plotter = PlotterFromExtension(extension);
            if (plotter == null) // invalid file selected
                throw new Exception("Unknown file extension " + extension);

            stream = new FileStream(viewModel.FileName, FileMode.Create);

            var entities = ProjectionHelper.ProjectTo2D(workspace.Drawing, viewPort, viewModel.PixelWidth, viewModel.PixelHeight);
            plotter.Plot(entities, viewModel.ColorMap, viewModel.PixelWidth, viewModel.PixelHeight, stream);


            stream.Close();
            stream.Dispose();
            stream = null;
        }

        private void Print()
        {
            var pageWidth = GetWidth(viewModel.PageSize);
            var pageHeight = GetHeight(viewModel.PageSize);

            var dlg = new PrintDialog();
            if (dlg.ShowDialog() == true)
            {
                var pageSize = new Size(pageWidth * 100, pageHeight * 100);
                var printWidth = dlg.PrintableAreaWidth;
                var printHeight = dlg.PrintableAreaHeight;
                var sideMargin = (pageSize.Width - printWidth) / 2;
                var topMargin = (pageSize.Height - printHeight) / 2;
                var grid = new Grid()
                {
                    Width = pageSize.Width,
                    Height = pageSize.Height
                };
                var canvas = new RenderCanvas()
                {
                    Background = new SolidColorBrush(Colors.White),
                    ViewPort = viewModel.ViewPort.Update(viewHeight: printHeight / 100),
                    Drawing = workspace.Drawing,
                    ColorMap = viewModel.ColorMap,
                    PointSize = 15.0,
                    Width = printWidth,
                    Height = printHeight,
                    Margin = new Thickness(sideMargin, topMargin, sideMargin, topMargin),
                    ClipToBounds = true,
                };
                grid.Children.Add(canvas);
                grid.Measure(pageSize);
                grid.Arrange(new Rect(pageSize));
                dlg.PrintVisual(grid, Path.GetFileName(workspace.Drawing.Settings.FileName));
            }
        }

        public override void Cancel()
        {
            // clear values?
        }

        public override bool Validate()
        {
            return viewModel.FileName != null;
        }

        private IFilePlotter PlotterFromExtension(string extension)
        {
            var plotter = filePlotters.FirstOrDefault(r => r.Metadata.FileExtensions.Contains(extension));
            if (plotter == null)
                return null;
            return plotter.Value;
        }

        private async void BrowseClick(object sender, RoutedEventArgs e)
        {
            var filename = await fileSystemService.GetFileNameFromUserForWrite(filePlotters.Select(f => new FileSpecification(f.Metadata.DisplayName, f.Metadata.FileExtensions)));
            if (filename != null)
            {
                viewModel.FileName = filename;
            }
        }

        private async void SelectAreaClick(object sender, RoutedEventArgs e)
        {
            Hide();
            await GetExportArea();
            Show();
        }

        private async Task GetExportArea()
        {
            var selection = await workspace.ViewControl.GetSelectionRectangle();
            if (selection == null)
                return;

            viewModel.BottomLeft = new Point(selection.TopLeftWorld.X, selection.BottomRightWorld.Y, selection.TopLeftWorld.Z);
            viewModel.TopRight = new Point(selection.BottomRightWorld.X, selection.TopLeftWorld.Y, selection.BottomRightWorld.Z);
        }

        internal static double GetWidth(PageSize size)
        {
            switch (size)
            {
                case PageSize.Legal:
                case PageSize.Letter:
                    return 8.5;
                case PageSize.Landscape:
                    return 11.0;
                default:
                    throw new InvalidOperationException();
            }
        }

        internal static double GetHeight(PageSize size)
        {
            switch (size)
            {
                case PageSize.Legal:
                    return 14.0;
                case PageSize.Letter:
                    return 11.0;
                case PageSize.Landscape:
                    return 8.5;
                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public class EnumMatchToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string checkValue = value.ToString();
            string targetValue = parameter.ToString();
            return checkValue.Equals(targetValue,
                     StringComparison.InvariantCultureIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            bool useValue = (bool)value;
            string targetValue = parameter.ToString();
            if (useValue)
                return Enum.Parse(targetType, targetValue);

            return null;
        }
    }

    public class DoubleStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (value.GetType() == typeof(double) && targetType == typeof(string))
            {
                return ((double)value).ToString();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0.0;

            if (value.GetType() == typeof(string) && targetType == typeof(double))
            {
                return double.Parse((string)value);
            }

            return 0.0;
        }
    }
}
