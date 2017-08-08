// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using IxMilia.BCad.ViewModels;

namespace IxMilia.BCad.Ribbons
{
    /// <summary>
    /// Interaction logic for HomeRibbon.xaml
    /// </summary>
    [ExportRibbonTab("home")]
    public partial class HomeRibbon : CadRibbonTab
    {
        private HomeRibbonViewModel viewModel;

        [ImportingConstructor]
        public HomeRibbon(IWorkspace workspace)
            : base(workspace)
        {
            InitializeComponent();
            viewModel = new HomeRibbonViewModel(Workspace);
            DataContext = viewModel;
        }
    }
}
