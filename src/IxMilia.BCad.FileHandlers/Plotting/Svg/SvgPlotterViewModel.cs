namespace IxMilia.BCad.Plotting.Svg
{
    public class SvgPlotterViewModel : ViewPortViewModelBase
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

        private double _outputWidth;
        public double OutputWidth
        {
            get => _outputWidth;
            set => SetValue(ref _outputWidth, value);
        }

        private double _outputHeight;
        public double OutputHeight
        {
            get => _outputHeight;
            set => SetValue(ref _outputHeight, value);
        }

        private bool _plotAsDocument;
        public bool PlotAsDocument
        {
            get => _plotAsDocument;
            set => SetValue(ref _plotAsDocument, value);
        }

        public SvgPlotterViewModel(IWorkspace workspace)
            : base(workspace)
        {
            Width = 640.0;
            Height = 480.0;

            OutputWidth = Width;
            OutputHeight = Height;
        }
    }
}
