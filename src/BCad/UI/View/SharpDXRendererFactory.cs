// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using BCad.UI.Shared;

namespace BCad.UI.View
{
    [ExportRendererFactory("Hardware")]
    internal class SharpDXRendererFactory : IRendererFactory
    {
        public AbstractCadRenderer CreateRenderer(IViewControl viewControl, IWorkspace workspace)
        {
            return new SharpDXRenderer(viewControl, workspace);
        }
    }
}
