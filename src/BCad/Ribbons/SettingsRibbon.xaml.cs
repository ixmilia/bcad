// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;

namespace IxMilia.BCad.Ribbons
{
    /// <summary>
    /// Interaction logic for DrawingSettingsRibbon.xaml
    /// </summary>
    [ExportRibbonTab("settings")]
    public partial class SettingsRibbon : CadRibbonTab
    {
        private SettingsRibbonViewModel viewModel = null;

        [ImportingConstructor]
        public SettingsRibbon(IWorkspace workspace)
            : base(workspace)
        {
            InitializeComponent();
            viewModel = new SettingsRibbonViewModel(Workspace);
            DataContext = viewModel;
            Workspace.WorkspaceChanged += WorkspaceChanged;
        }

        void WorkspaceChanged(object sender, EventArguments.WorkspaceChangeEventArgs e)
        {
            viewModel.UpdateProperty();
        }
    }
}
