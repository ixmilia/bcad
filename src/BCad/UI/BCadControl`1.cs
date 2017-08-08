// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows.Controls;

namespace IxMilia.BCad.UI
{
    public abstract class BCadControl<TResult> : UserControl where TResult : class
    {
        public TResult Result { get; protected set; }
    }
}
