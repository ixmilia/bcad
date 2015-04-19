using System;
using System.Collections.Generic;
using System.Threading;

namespace BCad.UI
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
