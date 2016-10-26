// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using BCad.UI.Shared;

namespace BCad.UI
{
    public interface IRendererFactory
    {
        AbstractCadRenderer CreateRenderer(IViewControl viewControl, IWorkspace workspace);
    }
}
