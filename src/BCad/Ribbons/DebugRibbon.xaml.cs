// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.Windows.Controls.Ribbon;

namespace IxMilia.BCad.Ribbons
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
        }
    }
}
