// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;

namespace IxMilia.BCad.UI.View
{
    public partial class ViewPane
    {
        private class CancellableComparer<T> : IComparer<T> where T : IComparable
        {
            private CancellationToken cancellationToken;

            public CancellableComparer(CancellationToken cancellationToken)
            {
                this.cancellationToken = cancellationToken;
            }

            public int Compare(T x, T y)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return x.CompareTo(y);
            }
        }
    }
}
