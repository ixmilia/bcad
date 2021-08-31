namespace IxMilia.BCad.Plotting.Svg
{
    public class SvgPlotterViewModel : ViewPortViewModelBase
    {
        private double _viewWidth;
        public override double DisplayWidth
        {
            get => _viewWidth;
            set
            {
                SetValue(ref _viewWidth, value);
                OnPropertyChanged(nameof(DisplayWidth));
                OnPropertyChanged(nameof(DisplayHeight));
                OnPropertyChanged(nameof(ViewPort));
            }
        }

        private double _viewHeight;
        public override double DisplayHeight
        {
            get => _viewHeight;
            set
            {
                SetValue(ref _viewHeight, value);
                OnPropertyChanged(nameof(DisplayWidth));
                OnPropertyChanged(nameof(DisplayHeight));
                OnPropertyChanged(nameof(ViewPort));
            }
        }

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
            DisplayWidth = 640.0;
            DisplayHeight = 480.0;

            OutputWidth = DisplayWidth;
            OutputHeight = DisplayHeight;
        }
    }
}
