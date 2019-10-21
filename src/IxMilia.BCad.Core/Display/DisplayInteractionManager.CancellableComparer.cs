using System;
using System.Collections.Generic;
using System.Threading;

namespace IxMilia.BCad.Display
{
    public partial class DisplayInteractionManager
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
