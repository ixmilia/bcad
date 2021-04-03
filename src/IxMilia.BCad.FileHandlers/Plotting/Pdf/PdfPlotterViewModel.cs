namespace IxMilia.BCad.Plotting.Pdf
{
    public class PdfPlotterViewModel : ViewPortViewModelBase
    {
        private double _width;
        public double Width
        {
            get => _width;
            set
            {
                SetValue(ref _width, value);
                OnPropertyChanged(nameof(ViewWidth));
                OnPropertyChanged(nameof(ViewHeight));
                OnPropertyChanged(nameof(ViewPort));
            }
        }

        private double _height;
        public double Height
        {
            get => _height;
            set
            {
                SetValue(ref _height, value);
                OnPropertyChanged(nameof(ViewWidth));
                OnPropertyChanged(nameof(ViewHeight));
                OnPropertyChanged(nameof(ViewPort));
            }
        }

        public override double ViewWidth => Width;

        public override double ViewHeight => Height;

        public PdfPlotterViewModel(IWorkspace workspace)
            : base(workspace)
        {
        }
    }
}
