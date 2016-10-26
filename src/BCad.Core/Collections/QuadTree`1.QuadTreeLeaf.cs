// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace BCad.Collections
{
    public partial class QuadTree<T>
    {
        private class QuadTreeLeaf : IQuadTreeNode
        {
            private const int MAX_ITEMS = 50;
            private List<T> _items;
            private GetBoundingRectangle _getBounding;

            public Rect Rect { get; }

            public int Count => _items.Count;

            public QuadTreeLeaf(Rect rect, GetBoundingRectangle getBoundingRectangle)
            {
                Rect = rect;
                _items = new List<T>();
                _getBounding = getBoundingRectangle;
            }

            public IQuadTreeNode AddItem(T item)
            {
                if (Rect.Contains(_getBounding(item)))
                {
                    _items.Add(item);
                    if (_items.Count > MAX_ITEMS)
                    {
                        return new QuadTreeParent(Rect, _getBounding, _items);
                    }
                }

                return this;
            }

            public void AddContainedItems(HashSet<T> set, Rect rect)
            {
                foreach (var item in _items)
                {
                    if (rect.Contains(_getBounding(item)))
                    {
                        set.Add(item);
                    }
                }
            }

            public void AddIntersectingItems(HashSet<T> set, Rect rect)
            {
                foreach (var item in _items)
                {
                    if (rect.Intersects(_getBounding(item)))
                    {
                        set.Add(item);
                    }
                }
            }
        }
    }
}
