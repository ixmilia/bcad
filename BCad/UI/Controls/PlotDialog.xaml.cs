using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
            var viewPort = viewModel.ViewPort;
            var extension = Path.GetExtension(viewModel.FileName);
            plotter = PlotterFromExtension(extension);
            if (plotter == null) // invalid file selected
                throw new Exception("Unknown file extension " + extension);

            stream = new FileStream(viewModel.FileName, FileMode.Create);

            var entities = ProjectionHelper.ProjectTo2D(workspace.Drawing, viewPort, viewModel.PixelWidth, viewModel.PixelHeight);
            plotter.Plot(entities, viewModel.PixelWidth, viewModel.PixelHeight, stream);


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
                var size = new Size(pageWidth * 100, pageHeight * 100);
                var visual = new RenderCanvas()
                {
                    Background = new SolidColorBrush(Colors.White),
                    ViewPort = viewModel.ViewPort,
                    Drawing = workspace.Drawing,
                    PointSize = 15.0,
                    Width = size.Width,
                    Height = size.Height,
                };
                visual.Measure(size);
                visual.Arrange(new Rect(size));
                dlg.PrintVisual(visual, Path.GetFileName(workspace.Drawing.Settings.FileName));
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

        private Drawing drawing;
        private string plotType;
        private string fileName;
        private ViewportType viewportType;
        private Point bottomLeft;
        private Point topRight;
        private double scaleA;
        private double scaleB;
        private PageSize pageSize;
        private Visibility printOptVis;
        private Visibility fileOptVis;
        private int pixelWidth;
        private int pixelHeight;
        private double previewWidth;
        private double previewHeight;
        private ViewPort activeViewPort;

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

                UpdatePreviewSize();
                OnPropertyChangedDirect("ViewPort");
            }
        }

        public Drawing Drawing
        {
            get { return drawing; }
            set
            {
                if (drawing == value)
                    return;
                drawing = value;
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
                OnPropertyChangedDirect("ViewPort");
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
                OnPropertyChangedDirect("ViewPort");
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
                OnPropertyChangedDirect("ViewPort");
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
                OnPropertyChangedDirect("ViewPort");
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
                OnPropertyChangedDirect("ViewPort");
                UpdatePreviewSize();
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
                UpdatePreviewSize();
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
                UpdatePreviewSize();
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
                OnPropertyChangedDirect("ViewPort");
                UpdatePreviewSize();
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
                OnPropertyChangedDirect("ViewPort");
                UpdatePreviewSize();
            }
        }

        public ViewPort ViewPort
        {
            get
            {
                ViewPort vp;
                switch (ViewportType)
                {
                    case ViewportType.Extents:
                        vp = Drawing.ShowAllViewPort(
                            ActiveViewPort.Sight,
                            ActiveViewPort.Up,
                            850,
                            1100,
                            pixelBuffer: 0);
                        break;
                    case ViewportType.Window:
                        vp = new ViewPort(BottomLeft, ActiveViewPort.Sight, ActiveViewPort.Up, TopRight.Y - BottomLeft.Y);
                        break;
                    default:
                        throw new InvalidOperationException("unsupported viewport type");
                }

                if (PlotType == "Print")
                {
                    var desiredHeight = PlotDialog.GetHeight(PageSize);
                    vp = vp.Update(viewHeight: desiredHeight * ScaleB / ScaleA);
                }

                return vp;
            }
        }

        public double PreviewWidth
        {
            get { return previewWidth; }
            set
            {
                if (previewWidth == value)
                    return;
                previewWidth = value;
                OnPropertyChanged();
            }
        }

        public double PreviewHeight
        {
            get { return previewHeight; }
            set
            {
                if (previewHeight == value)
                    return;
                previewHeight = value;
                OnPropertyChanged();
            }
        }

        public ViewPort ActiveViewPort
        {
            get { return activeViewPort; }
            set
            {
                if (activeViewPort == value)
                    return;
                activeViewPort = value;
                OnPropertyChanged();
                OnPropertyChangedDirect("ViewPort");
            }
        }

        public PageSize[] AvailablePageSizes
        {
            get { return new[] { PageSize.Letter, PageSize.Landscape, PageSize.Legal }; }
        }

        public PlotDialogViewModel()
        {
            Drawing = new Drawing();
            PlotType = AvailablePlotTypes.First();
            FileName = string.Empty;
            ViewportType = ViewportType.Extents;
            BottomLeft = Point.Origin;
            TopRight = Point.Origin;
            ScaleA = 1.0;
            ScaleB = 1.0;
            PageSize = PageSize.Letter;
            PixelWidth = 800;
            PixelHeight = 600;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string property = "")
        {
            OnPropertyChangedDirect(property);
        }

        protected void OnPropertyChangedDirect(string property)
        {
            var changed = PropertyChanged;
            if (changed != null)
                changed(this, new PropertyChangedEventArgs(property));
        }

        private void UpdatePreviewSize()
        {
            var maxWidth = 300;
            var maxHeight = 300;
            double width, height;
            if (PlotType == "Print")
            {
                width = PlotDialog.GetWidth(PageSize);
                height = PlotDialog.GetHeight(PageSize);   
            }
            else
            {
                width = PixelWidth;
                height = PixelHeight;
            }

            if (width > height)
            {
                PreviewWidth = maxWidth;
                PreviewHeight = (height / width) * maxHeight;
            }
            else
            {
                PreviewHeight = maxHeight;
                PreviewWidth = (width / height) * maxWidth;
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
