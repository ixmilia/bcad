// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace IxMilia.BCad.UI.View
{
    /// <summary>
    /// Interaction logic for XamlRenderer.xaml
    /// </summary>
    public partial class XamlRenderer : AbstractCadRenderer
    {
        private IViewControl ViewControl;

        public XamlRenderer()
        {
            InitializeComponent();
        }

        public XamlRenderer(IViewControl viewControl, IWorkspace workspace)
            : this()
        {
            Initialize(workspace, viewControl);
            ViewControl = viewControl;
        }
    }
}
