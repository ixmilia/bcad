// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#elif WPF
using System.Windows.Controls;
#endif

namespace BCad.UI.Shared
{
    public class AbstractCadRenderer : UserControl
    {
        public virtual void Invalidate()
        {
        }

        public virtual void UpdateRubberBandLines()
        {
        }
    }
}
