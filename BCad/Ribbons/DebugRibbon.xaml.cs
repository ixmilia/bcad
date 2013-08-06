using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Windows.Controls.Ribbon;

namespace BCad.Ribbons
{
    /// <summary>
    /// Interaction logic for DebugRibbon.xaml
    /// </summary>
    [ExportRibbonTab("debug")]
    public partial class DebugRibbon : RibbonTab
    {
        private IWorkspace workspace = null;

        public DebugRibbon()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public DebugRibbon(IWorkspace workspace)
            : this()
        {
            this.workspace = workspace;
        }

        private void DebugButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            string inputFile, outputFile;
            if (string.Compare(Environment.MachineName, "bisquik", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // home machine
                inputFile = @"D:\Code\BCad\BCad\BellevueHouse.dxf";
                outputFile = @"D:\Personal\Desktop\bh.svg";
            }
            else
            {
                // work machine
                inputFile = @"C:\Users\brettfo\Documents\GitHub\BCad\BCad\RoundyHouse.dxf";
                outputFile = @"D:\Private\Desktop\test.svg";
            }

            workspace.ExecuteCommand("File.Open", inputFile).Wait();
            workspace.ExecuteCommand("File.SaveAs", outputFile).Wait();
            System.Diagnostics.Process.Start(outputFile);
        }
    }
}
