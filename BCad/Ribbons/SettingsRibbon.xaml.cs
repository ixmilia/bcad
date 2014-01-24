using System.Composition;
using Microsoft.Windows.Controls.Ribbon;

namespace BCad.Ribbons
{
    /// <summary>
    /// Interaction logic for SettingsRibbon.xaml
    /// </summary>
    [ExportRibbonTab("settings")]
    public partial class SettingsRibbon : RibbonTab
    {
        private IWorkspace workspace;

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
            DataContext = workspace.SettingsManager;
        }
    }
}
