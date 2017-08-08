// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace IxMilia.BCad.Collections
{
    public partial class QuadTree<T>
    {
        private interface IQuadTreeNode
        {
            Rect Rect { get; }
            int Count { get; }
            IQuadTreeNode AddItem(T item);
            void AddContainedItems(HashSet<T> set, Rect rect);
            void AddIntersectingItems(HashSet<T> set, Rect rect);
        }
    }
}
