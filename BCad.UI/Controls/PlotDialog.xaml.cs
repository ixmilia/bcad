using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BCad.FileHandlers;
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
        private IExportService exportService = null;
        private IFileSystemService fileSystemService = null;
        private IEnumerable<Lazy<IFilePlotter, IFilePlotterMetadata>> filePlotters = null;

        private PlotDialogViewModel viewModel = null;

        public PlotDialog()
        {
            InitializeComponent();

            viewModel = new PlotDialogViewModel();
            DataContext = viewModel;
        }

        [ImportingConstructor]
        public PlotDialog(IWorkspace workspace, IInputService inputService, IExportService exportService, IFileSystemService fileSystemService, [ImportMany] IEnumerable<Lazy<IFilePlotter, IFilePlotterMetadata>> filePlotters)
            : this()
        {
            this.workspace = workspace;
            this.inputService = inputService;
            this.exportService = exportService;
            this.fileSystemService = fileSystemService;
            this.filePlotters = filePlotters;
        }

        public override void Commit()
        {
            var extension = Path.GetExtension(viewModel.FileName);
            var plotter = PlotterFromExtension(extension);
            if (plotter == null) // invalid file selected
                throw new Exception("Unknown file extension " + extension);

            var viewPort = GenerateViewPort();
            var width = Math.Abs(viewModel.TopRight.X - viewModel.BottomLeft.X);
            var height = Math.Abs(viewModel.TopRight.Y - viewModel.BottomLeft.Y);
            using (var file = new FileStream(viewModel.FileName, FileMode.Create))
            {
                var entities = exportService.ProjectTo2D(workspace.Drawing, viewPort);
                plotter.Plot(entities, width, height, file);
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
            return new ViewPort(viewModel.BottomLeft, workspace.ActiveViewPort.Sight, workspace.ActiveViewPort.Up, viewModel.TopRight.Y - viewModel.BottomLeft.Y);
        }

        private IFilePlotter PlotterFromExtension(string extension)
        {
            var plotter = filePlotters.FirstOrDefault(r => r.Metadata.FileExtensions.Contains(extension));
            if (plotter == null)
                return null;
            return plotter.Value;
        }

        private void BrowseClick(object sender, RoutedEventArgs e)
        {
            var filename = fileSystemService.GetFileNameFromUserForWrite(filePlotters.Select(f => new FileSpecification(f.Metadata.DisplayName, f.Metadata.FileExtensions)));
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
            // TODO: generalize getting viewports for zoom, etc.
            // prompt for viewport
            var firstPoint = await inputService.GetPoint(new UserDirective("First corner of view box"));
            if (firstPoint.Cancel || !firstPoint.HasValue)
                return;

            var secondPoint = await inputService.GetPoint(new UserDirective("Second corner of view box"), (p) =>
            {
                var a = firstPoint.Value;
                var b = new Point(p.X, firstPoint.Value.Y, firstPoint.Value.Z);
                var c = new Point(p.X, p.Y, firstPoint.Value.Z);
                var d = new Point(firstPoint.Value.X, p.Y, firstPoint.Value.Z);
                return new[]
                    {
                        new PrimitiveLine(a, b),
                        new PrimitiveLine(b, c),
                        new PrimitiveLine(c, d),
                        new PrimitiveLine(d, a)
                    };
            });
            if (secondPoint.Cancel || !secondPoint.HasValue)
                return;

            // find bottom left and top right
            var size = secondPoint.Value - firstPoint.Value;
            var width = Math.Abs(size.X);
            var height = Math.Abs(size.Y);
            viewModel.BottomLeft = new Point(Math.Min(firstPoint.Value.X, secondPoint.Value.X), Math.Min(firstPoint.Value.Y, secondPoint.Value.Y), firstPoint.Value.Z);
            viewModel.TopRight = new Point(viewModel.BottomLeft.X + width, viewModel.BottomLeft.Y + height, viewModel.BottomLeft.Z);
        }
    }

    public class PlotDialogViewModel : INotifyPropertyChanged
    {
        public IEnumerable<string> AvailablePlotTypes
        {
            get { return new[] { "File" }; }
        }

        private string plotType;
        private string fileName;
        private Point bottomLeft;
        private Point topRight;

        public string PlotType
        {
            get { return this.plotType; }
            set
            {
                if (this.plotType == value)
                    return;
                this.plotType = value;
                OnPropertyChanged("PlotType");
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
                OnPropertyChanged("FileName");
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
                OnPropertyChanged("BottomLeft");
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
                OnPropertyChanged("TopRight");
            }
        }

        public PlotDialogViewModel()
        {
            PlotType = AvailablePlotTypes.First();
            FileName = string.Empty;
            BottomLeft = Point.Origin;
            TopRight = Point.Origin;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
