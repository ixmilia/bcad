using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.Linq;
using System.Windows.Data;
using Microsoft.Windows.Controls.Ribbon;

namespace BCad.Ribbons
{
    /// <summary>
    /// Interaction logic for DrawingSettingsRibbon.xaml
    /// </summary>
    [ExportRibbonTab("settings")]
    public partial class SettingsRibbon : RibbonTab
    {
        private IWorkspace workspace = null;
        private DrawingSettingsViewModel viewModel = null;

        public RealColor[] BackgroundColors
        {
            get
            {
                return new[]
                {
                    RealColor.Black,
                    RealColor.DarkSlateGray,
                    RealColor.CornflowerBlue,
                    RealColor.White
                };
            }
        }

        public SettingsRibbon()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public SettingsRibbon(IWorkspace workspace)
            : this()
        {
            this.workspace = workspace;
            this.viewModel = new DrawingSettingsViewModel(this.workspace);
            this.unitFormat.DataContext = this.viewModel;
            this.drawingPrecision.DataContext = this.viewModel;
            this.workspace.WorkspaceChanged += WorkspaceChanged;
            this.displayGroup.DataContext = workspace.SettingsManager;
            this.inputGroup.DataContext = new SettingsRibbonViewModel(workspace.SettingsManager);
        }

        void WorkspaceChanged(object sender, EventArguments.WorkspaceChangeEventArgs e)
        {
            this.viewModel.UpdateAll();
        }
    }

    public class DrawingSettingsViewModel : INotifyPropertyChanged
    {
        private IWorkspace workspace = null;
        internal static List<Tuple<string, int>> ArchitecturalPrecisionValues;
        internal static List<Tuple<string, int>> DecimalPrecisionValues;

        internal static IWorkspace InternalWorkspace { get; private set; }

        static DrawingSettingsViewModel()
        {
            ArchitecturalPrecisionValues = new List<Tuple<string, int>>()
            {
                Tuple.Create("1\"", 0),
                Tuple.Create("1/2\"", 2),
                Tuple.Create("1/4\"", 4),
                Tuple.Create("1/8\"", 8),
                Tuple.Create("1/16\"", 16),
                Tuple.Create("1/32\"", 32),
            };
            DecimalPrecisionValues = new List<Tuple<string, int>>();
            for (int i = 0; i <= 16; i++)
            {
                DecimalPrecisionValues.Add(Tuple.Create(i.ToString(), i));
            }
        }

        public DrawingSettingsViewModel(IWorkspace workspace)
        {
            this.workspace = workspace;
            InternalWorkspace = workspace;
        }

        public UnitFormat[] DrawingUnitValues
        {
            get
            {
                return new[]
                {
                    UnitFormat.Architectural,
                    UnitFormat.Metric
                };
            }
        }

        public UnitFormat UnitFormat
        {
            get { return workspace.Drawing.Settings.UnitFormat; }
            set
            {
                if (value == workspace.Drawing.Settings.UnitFormat)
                    return;
                workspace.UpdateDrawingSettings(workspace.Drawing.Settings.Update(unitFormat: value));
                OnPropertyChanged("UnitFormat");
                OnPropertyChanged("UnitPrecisionDisplay");
            }
        }

        public int UnitPrecision
        {
            get { return workspace.Drawing.Settings.UnitPrecision; }
            set
            {
                if (value == workspace.Drawing.Settings.UnitPrecision)
                    return;
                workspace.UpdateDrawingSettings(workspace.Drawing.Settings.Update(unitPrecision: value));
                OnPropertyChanged("UnitPrecision");
            }
        }

        public List<Tuple<string, int>> UnitPrecisionDisplay
        {
            get { return GetUnitPrecisionDisplay(workspace); }
        }

        public void UpdateAll()
        {
            OnPropertyChanged("UnitFormat");
            OnPropertyChanged("UnitPrecisionDisplay");
            OnPropertyChanged("UnitPrecision");
        }

        internal static List<Tuple<string, int>> GetUnitPrecisionDisplay(IWorkspace workspace)
        {
            switch (workspace.Drawing.Settings.UnitFormat)
            {
                case UnitFormat.Architectural:
                    return ArchitecturalPrecisionValues;
                case UnitFormat.Metric:
                    return DecimalPrecisionValues;
                default:
                    throw new ArgumentException("Invalid unit format");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var changed = PropertyChanged;
            if (changed != null)
                changed(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class IntToPrecisionConverter : IValueConverter
    {
        public IntToPrecisionConverter()
        {
        }

        public object Convert(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            int precision = (int)value;
            var display = DrawingSettingsViewModel.GetUnitPrecisionDisplay(DrawingSettingsViewModel.InternalWorkspace);
            var result = display.FirstOrDefault(x => x.Item2 == precision);
            return result;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            var tuple = value as Tuple<string, int>;
            if (tuple == null)
                return null;
            return tuple.Item2;
        }
    }
}
