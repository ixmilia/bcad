// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace BCad.Collections
{
    public partial class QuadTree<T>
    {
        public delegate Rect GetBoundingRectangle(T item);

        private IQuadTreeNode _root;
        private GetBoundingRectangle _getBounding;

        public QuadTree(Rect rect, GetBoundingRectangle getBoundingRectangle)
        {
            _root = new QuadTreeLeaf(rect, getBoundingRectangle);
            _getBounding = getBoundingRectangle;
        }

        public int Count
        {
            get { return _root.Count; }
        }

        public void AddItem(T item)
        {
            _root = _root.AddItem(item);
        }

        public HashSet<T> GetContainedItems(Rect rect)
        {
            var set = new HashSet<T>();
            _root.AddContainedItems(set, rect);
            return set;
        }

        public HashSet<T> GetIntersectingItems(Rect rect)
        {
            var set = new HashSet<T>();
            _root.AddIntersectingItems(set, rect);
            return set;
        }
    }
}
