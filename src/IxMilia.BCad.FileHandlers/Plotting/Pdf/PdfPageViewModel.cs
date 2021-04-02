namespace IxMilia.BCad.Plotting.Pdf
{
    public class PdfPageViewModel : ViewPortViewModel
    {
        private int _pageNumber;
        public int PageNumber
        {
            get => _pageNumber;
            set
            {
                SetValue(ref _pageNumber, value);
                OnPropertyChanged(nameof(PageName));
            }
        }

        public string PageName => $"Page {PageNumber}";

        public double MaxPreviewSize => 400.0;

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

        public PdfPageViewModel(IWorkspace workspace)
            : base(workspace)
        {
        }
    }
}
