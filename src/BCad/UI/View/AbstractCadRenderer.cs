// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows.Controls;

namespace IxMilia.BCad.UI.View
{
    public abstract class AbstractCadRenderer : UserControl
    {
        public abstract void Invalidate();
        public abstract void UpdateRubberBandLines();
    }
}
