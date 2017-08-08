// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace IxMilia.BCad.UI.View
{
    [ExportRendererFactory("Software")]
    internal class XamlRendererFactory : IRendererFactory
    {
        public AbstractCadRenderer CreateRenderer(IViewControl viewControl, IWorkspace workspace)
        {
            return new XamlRenderer(viewControl, workspace);
        }
    }
}
