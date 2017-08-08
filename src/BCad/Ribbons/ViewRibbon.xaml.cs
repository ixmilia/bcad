// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;

namespace IxMilia.BCad.Ribbons
{
    /// <summary>
    /// Interaction logic for ViewRibbon.xaml
    /// </summary>
    [ExportRibbonTab("view")]
    public partial class ViewRibbon : CadRibbonTab
    {
        [ImportingConstructor]
        public ViewRibbon(IWorkspace workspace)
            : base(workspace)
        {
            InitializeComponent();
        }
    }
}
