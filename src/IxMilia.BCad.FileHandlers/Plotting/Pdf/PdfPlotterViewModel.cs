using IxMilia.Pdf;

namespace IxMilia.BCad.Plotting.Pdf
{
    public class PdfPlotterViewModel : ViewPortViewModelBase
    {
        private double _displayWidth;
        public override double DisplayWidth
        {
            get => _displayWidth;
            set
            {
                SetValue(ref _displayWidth, value);
                OnPropertyChanged(nameof(DisplayWidth));
                OnPropertyChanged(nameof(DisplayHeight));
                OnPropertyChanged(nameof(ViewPort));
            }
        }

        private double _displayHeight;
        public override double DisplayHeight
        {
            get => _displayHeight;
            set
            {
                SetValue(ref _displayHeight, value);
                OnPropertyChanged(nameof(DisplayWidth));
                OnPropertyChanged(nameof(DisplayHeight));
                OnPropertyChanged(nameof(ViewPort));
            }
        }

        private PdfMeasurementType _displayUnit;
        public PdfMeasurementType DisplayUnit
        {
            get => _displayUnit;
            set
            {
                SetValue(ref _displayUnit, value);
                OnPropertyChanged(nameof(DisplayWidth));
                OnPropertyChanged(nameof(DisplayHeight));
                OnPropertyChanged(nameof(ViewPort));
            }
        }

        private PdfMeasurementType _marginUnit;
        public PdfMeasurementType MarginUnit
        {
            get => _marginUnit;
            set
            {
                SetValue(ref _marginUnit, value);
                OnPropertyChanged(nameof(MarginUnit));
            }
        }

        public PdfPlotterViewModel(IWorkspace workspace)
            : base(workspace)
        {
        }
    }
}
