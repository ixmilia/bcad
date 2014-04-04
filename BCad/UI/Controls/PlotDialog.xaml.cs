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
using System.Windows.Media.Imaging;
using BCad.FilePlotters;
using BCad.Helpers;
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
        private IFilePlotter pngPlotter = null;
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

            this.pngPlotter = filePlotters.FirstOrDefault(plt => plt.Metadata.FileExtensions.Contains(".png")).Value;
            this.viewModel.Plotter = pngPlotter;
        }

        public override void OnShowing()
        {
            this.viewModel.Drawing = workspace.Drawing;
            this.viewModel.UpdateThumbnail();
        }

        public override void Commit()
        {
            switch (this.viewModel.PlotType)
            {
                case "File":
                    FilePlot();
                    break;
                case "Print":
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
            var viewPort = GenerateViewPort();
            var pageWidth = GetWidth(viewModel.PageSize);
            var pageHeight = GetHeight(viewModel.PageSize);
            var width = pageWidth * viewModel.Dpi;
            var height = pageHeight * viewModel.Dpi;
            var extension = Path.GetExtension(viewModel.FileName);
            plotter = PlotterFromExtension(extension);
            if (plotter == null) // invalid file selected
                throw new Exception("Unknown file extension " + extension);

            stream = new FileStream(viewModel.FileName, FileMode.Create);

            viewPort = viewPort.Update(viewHeight: pageHeight * viewModel.ScaleA / viewModel.ScaleB); // TODO: assumes drawing units are inches
            var entities = ProjectionHelper.ProjectTo2D(workspace.Drawing, viewPort, (int)width, (int)height);
            plotter.Plot(entities, width, height, stream);


            stream.Close();
            stream.Dispose();
            stream = null;
        }

        private void Print()
        {
            var viewPort = GenerateViewPort();
            var pageWidth = GetWidth(viewModel.PageSize);
            var pageHeight = GetHeight(viewModel.PageSize);
            viewPort = viewPort.Update(viewHeight: pageHeight * viewModel.ScaleA / viewModel.ScaleB); // TODO: assumes drawing units are inches
            using (var dialog = new PrintDialog())
            {
                dialog.AllowPrintToFile = true;
                dialog.PrintToFile = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    using (var document = new PrintDocument())
                    {
                        document.PrinterSettings = dialog.PrinterSettings; // "Letter" "Letter Rotated"
                        var desiredWidth = (int)(pageWidth * 100);
                        var desiredHeight = (int)(pageHeight * 100);
                        foreach (PaperSize size in document.PrinterSettings.PaperSizes)
                        {
                            if (size.Width == desiredWidth && size.Height == desiredHeight)
                            {
                                document.DefaultPageSettings.PaperSize = size;
                                break;
                            }
                        }
                        document.PrintPage += (sender, e) =>
                        {
                            var width = e.PageSettings.PrintableArea.Width / 100 * viewModel.Dpi;
                            var height = e.PageSettings.PrintableArea.Height / 100 * viewModel.Dpi;
                            var entities = ProjectionHelper.ProjectTo2D(workspace.Drawing, viewPort, (int)width, (int)height);
                            using (var stream = new MemoryStream())
                            {
                                pngPlotter.Plot(entities, width, height, stream);
                                stream.Flush();
                                stream.Seek(0, SeekOrigin.Begin);
                                using (var image = new Bitmap(stream))
                                {
                                    image.SetResolution((float)viewModel.Dpi, (float)viewModel.Dpi);
                                    e.Graphics.DrawImage(image, e.PageSettings.PrintableArea.Location);
                                }
                            }
                        };
                        document.Print();
                    }
                }
            }
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

    public enum ViewportType
    {
        Extents,
        Window
    }

    public enum PageSize
    {
        Letter,
        Legal,
        Landscape
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
        private BitmapImage thumbnail;
        private Visibility printOptVis;
        private Visibility fileOptVis;
        private int pixelWidth;
        private int pixelHeight;

        public Drawing Drawing { get; internal set; }
        public IFilePlotter Plotter { get; internal set; }

        public string PlotType
        {
            get { return this.plotType; }
            set
            {
                if (this.plotType == value)
                    return;
                this.plotType = value;
                OnPropertyChanged();
                switch (this.plotType)
                {
                    case "File":
                        FileOptionsVisibility = Visibility.Visible;
                        PrintOptionsVisibility = Visibility.Hidden;
                        break;
                    case "Print":
                        FileOptionsVisibility = Visibility.Hidden;
                        PrintOptionsVisibility = Visibility.Visible;
                        break;
                }

                UpdateThumbnail();
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
                UpdateThumbnail();
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
                UpdateThumbnail();
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
                UpdateThumbnail();
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
                UpdateThumbnail();
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
                UpdateThumbnail();
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
                UpdateThumbnail();
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
                UpdateThumbnail();
            }
        }

        public BitmapImage Thumbnail
        {
            get { return this.thumbnail; }
            set
            {
                if (this.thumbnail == value)
                    return;
                this.thumbnail = value;
                OnPropertyChanged();
            }
        }

        public Visibility PrintOptionsVisibility
        {
            get { return this.printOptVis; }
            private set
            {
                if (this.printOptVis == value)
                    return;
                this.printOptVis = value;
                OnPropertyChanged();
            }
        }

        public Visibility FileOptionsVisibility
        {
            get { return this.fileOptVis; }
            private set
            {
                if (this.fileOptVis == value)
                    return;
                this.fileOptVis = value;
                OnPropertyChanged();
            }
        }

        public int PixelWidth
        {
            get { return this.pixelWidth; }
            set
            {
                if (this.pixelWidth == value)
                    return;
                this.pixelWidth = value;
                OnPropertyChanged();
                UpdateThumbnail();
            }
        }

        public int PixelHeight
        {
            get { return this.pixelHeight; }
            set
            {
                if (this.pixelHeight == value)
                    return;
                this.pixelHeight = value;
                OnPropertyChanged();
                UpdateThumbnail();
            }
        }

        public PageSize[] AvailablePageSizes
        {
            get { return new[] { PageSize.Letter, PageSize.Landscape, PageSize.Legal }; }
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
            PixelWidth = 800;
            PixelHeight = 600;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string property = "")
        {
            var changed = PropertyChanged;
            if (changed != null)
                changed(this, new PropertyChangedEventArgs(property));
        }

        public void UpdateThumbnail()
        {
            if (Drawing == null)
                return;
            var stream = new MemoryStream();

            var pageWidth = PlotType == "File" ? PixelWidth : PlotDialog.GetWidth(PageSize);
            var pageHeight = PlotType == "File" ? PixelHeight : PlotDialog.GetHeight(PageSize);

            int width, height;
            if (pageWidth < pageHeight)
            {
                height = 300;
                width = (int)(pageWidth / pageHeight * height);
            }
            else
            {
                width = 300;
                height = (int)(pageHeight / pageWidth * width);
            }

            var viewPort = Drawing.ShowAllViewPort(Vector.ZAxis, Vector.YAxis, width, height, 0);
            if (PlotType == "Print")
                viewPort = viewPort.Update(viewHeight: pageHeight * ScaleA / ScaleB); // TODO: assumes drawing units are inches
            else
                viewPort = viewPort.Update(viewHeight: height * ScaleA / ScaleB);
            var entities = ProjectionHelper.ProjectTo2D(Drawing, viewPort, width, height);
            Plotter.Plot(entities, width, height, stream);
            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            var bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = stream;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();

            Thumbnail = bi;
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
