using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using BCad.FilePlotters;
using BCad.Helpers;
using BCad.Primitives;
using BCad.Services;

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

            viewModel = new PlotDialogViewModel();
            DataContext = viewModel;
        }

        [ImportingConstructor]
        public PlotDialog(IWorkspace workspace, IInputService inputService, IFileSystemService fileSystemService, [ImportMany] IEnumerable<Lazy<IFilePlotter, FilePlotterMetadata>> filePlotters)
            : this()
        {
            this.workspace = workspace;
            this.inputService = inputService;
            this.fileSystemService = fileSystemService;
            this.filePlotters = filePlotters;
        }

        public override void OnShowing()
        {
        }

        public override void Commit()
        {
            IFilePlotter plotter;
            Stream stream;
            var viewPort = GenerateViewPort();
            var pageWidth = GetWidth(viewModel.PageSize);
            var pageHeight = GetHeight(viewModel.PageSize);
            var width = pageWidth * viewModel.Dpi;
            var height = pageHeight * viewModel.Dpi;
            switch (viewModel.PlotType)
            {
                case "File":
                    var extension = Path.GetExtension(viewModel.FileName);
                    plotter = PlotterFromExtension(extension);
                    if (plotter == null) // invalid file selected
                        throw new Exception("Unknown file extension " + extension);

                    stream = new FileStream(viewModel.FileName, FileMode.Create);
                    break;
                case "Print":
                    // fake it with the png plotter
                    plotter = filePlotters.FirstOrDefault(plt => plt.Metadata.FileExtensions.Contains(".png")).Value;
                    stream = new MemoryStream();
                    break;
                default:
                    throw new NotImplementedException(); // TODO: remove this
            }

            viewPort = viewPort.Update(viewHeight: pageHeight * viewModel.ScaleA / viewModel.ScaleB);
            var entities = ProjectionHelper.ProjectTo2D(workspace.Drawing, viewPort, (int)width, (int)height);
            plotter.Plot(entities, width, height, stream);

            switch (viewModel.PlotType)
            {
                case "Print":
                    var dialog = new PrintDialog();
                    dialog.AllowPrintToFile = true;
                    dialog.PrintToFile = false;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // stream should be a png
                        stream.Flush();
                        stream.Seek(0, SeekOrigin.Begin);
                        var image = new Bitmap(stream);
                        image.SetResolution((float)viewModel.Dpi, (float)viewModel.Dpi);
                        var document = new PrintDocument();
                        document.PrinterSettings = dialog.PrinterSettings;
                        document.DefaultPageSettings.PaperSize = new PaperSize(viewModel.PageSize.ToString(), (int)(pageWidth * 100), (int)(pageHeight * 100));
                        document.PrintPage += (sender, e) =>
                            {
                                e.Graphics.DrawImage(image, new PointF());
                            };
                        document.Print();
                    }
                    break;
            }

            stream.Close();
            stream.Dispose();
            stream = null;
        }

        public override void Cancel()
        {
            // clear values?
        }

        public override bool Validate()
        {
            return viewModel.BottomLeft != null
                && viewModel.FileName != null
                && viewModel.PlotType != null
                && viewModel.TopRight != null;
        }

        private ViewPort GenerateViewPort()
        {
            switch (viewModel.ViewportType)
            {
                case ViewportType.Extents:
                    var newVp = workspace.Drawing.ShowAllViewPort(
                        workspace.ActiveViewPort.Sight,
                        workspace.ActiveViewPort.Up,
                        850,
                        1100,
                        pixelBuffer: 0);
                    return newVp;
                case ViewportType.Window:
                    return new ViewPort(viewModel.BottomLeft, workspace.ActiveViewPort.Sight, workspace.ActiveViewPort.Up, viewModel.TopRight.Y - viewModel.BottomLeft.Y);
                default:
                    throw new InvalidOperationException("unsupported viewport type");
            }
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

        private static double GetWidth(PageSize size)
        {
            switch (size)
            {
                case PageSize.Legal:
                case PageSize.Letter:
                    return 8.5;
                default:
                    throw new InvalidOperationException();
            }
        }

        private static double GetHeight(PageSize size)
        {
            switch (size)
            {
                case PageSize.Legal:
                    return 14.0;
                case PageSize.Letter:
                    return 11.0;
                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public enum ViewportType
    {
        Extents,
        Window
    }

    public enum PageSize
    {
        Letter,
        Legal
    }

    public class PlotDialogViewModel : INotifyPropertyChanged
    {
        public IEnumerable<string> AvailablePlotTypes
        {
            get { return new[] { "File", "Print" }; }
        }

        private string plotType;
        private string fileName;
        private ViewportType viewportType;
        private Point bottomLeft;
        private Point topRight;
        private double scaleA;
        private double scaleB;
        private double dpi;
        private PageSize pageSize;

        public string PlotType
        {
            get { return this.plotType; }
            set
            {
                if (this.plotType == value)
                    return;
                this.plotType = value;
                OnPropertyChanged();
            }
        }

        public string FileName
        {
            get { return this.fileName; }
            set
            {
                if (this.fileName == value)
                    return;
                this.fileName = value;
                OnPropertyChanged();
            }
        }

        public ViewportType ViewportType
        {
            get { return this.viewportType; }
            set
            {
                if (this.viewportType == value)
                    return;
                this.viewportType = value;
                OnPropertyChanged();
            }
        }

        public Point BottomLeft
        {
            get { return this.bottomLeft; }
            set
            {
                if (this.bottomLeft == value)
                    return;
                this.bottomLeft = value;
                OnPropertyChanged();
            }
        }

        public Point TopRight
        {
            get { return this.topRight; }
            set
            {
                if (this.topRight == value)
                    return;
                this.topRight = value;
                OnPropertyChanged();
            }
        }

        public double ScaleA
        {
            get { return this.scaleA; }
            set
            {
                if (this.scaleA == value)
                    return;
                this.scaleA = value;
                OnPropertyChanged();
            }
        }

        public double ScaleB
        {
            get { return this.scaleB; }
            set
            {
                if (this.scaleB == value)
                    return;
                this.scaleB = value;
                OnPropertyChanged();
            }
        }

        public double Dpi
        {
            get { return this.dpi; }
            set
            {
                if (this.dpi == value)
                    return;
                this.dpi = value;
                OnPropertyChanged();
            }
        }

        public PageSize PageSize
        {
            get { return this.pageSize; }
            set
            {
                if (this.pageSize == value)
                    return;
                this.pageSize = value;
                OnPropertyChanged();
            }
        }

        public PageSize[] AvailablePageSizes
        {
            get { return new[] { PageSize.Letter, PageSize.Legal }; }
        }

        public PlotDialogViewModel()
        {
            PlotType = AvailablePlotTypes.First();
            FileName = string.Empty;
            ViewportType = ViewportType.Extents;
            BottomLeft = Point.Origin;
            TopRight = Point.Origin;
            ScaleA = 1.0;
            ScaleB = 1.0;
            Dpi = 300;
            PageSize = PageSize.Letter;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string property = "")
        {
            var changed = PropertyChanged;
            if (changed != null)
                changed(this, new PropertyChangedEventArgs(property));
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
