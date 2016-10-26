// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace BCad.Collections
{
    public partial class QuadTree<T>
    {
        private class QuadTreeParent : IQuadTreeNode
        {
            public Rect Rect { get; }

            public int Count => NodeTopLeft.Count + NodeTopRight.Count + NodeBottomLeft.Count + NodeBottomRight.Count;

            public IQuadTreeNode NodeTopLeft { get; private set; }
            public IQuadTreeNode NodeTopRight{ get; private set; }
            public IQuadTreeNode NodeBottomLeft { get; private set; }
            public IQuadTreeNode NodeBottomRight { get; private set; }

            private GetBoundingRectangle _getBounding;

            public QuadTreeParent(Rect rect, GetBoundingRectangle getBoundingRectangle, IEnumerable<T> items)
            {
                Rect = rect;

                _getBounding = getBoundingRectangle;

                var left = rect.Left;
                var top = rect.Top;
                var halfWidth = rect.Width / 2.0;
                var halfHeight = rect.Height / 2.0;

                NodeTopLeft = new QuadTreeLeaf(new Rect(left, top, halfWidth, halfHeight), _getBounding);
                NodeTopRight = new QuadTreeLeaf(new Rect(left + halfWidth, top, halfWidth, halfHeight), _getBounding);
                NodeBottomLeft = new QuadTreeLeaf(new Rect(left, top + halfHeight, halfWidth, halfHeight), _getBounding);
                NodeBottomRight = new QuadTreeLeaf(new Rect(left + halfWidth, top + halfHeight, halfWidth, halfHeight), _getBounding);

                foreach (var item in items)
                {
                    AddItem(item);
                }
            }

            public IQuadTreeNode AddItem(T item)
            {
                var bounding = _getBounding(item);
                if (NodeTopLeft.Rect.Intersects(bounding))
                {
                    NodeTopLeft = NodeTopLeft.AddItem(item);
                }

                if (NodeTopRight.Rect.Intersects(bounding))
                {
                    NodeTopRight = NodeTopRight.AddItem(item);
                }

                if (NodeBottomLeft.Rect.Intersects(bounding))
                {
                    NodeBottomLeft = NodeBottomLeft.AddItem(item);
                }

                if (NodeBottomRight.Rect.Intersects(bounding))
                {
                    NodeBottomRight = NodeBottomRight.AddItem(item);
                }

                return this;
            }

            public void AddContainedItems(HashSet<T> set, Rect rect)
            {
                NodeTopLeft.AddContainedItems(set, rect);
                NodeTopRight.AddContainedItems(set, rect);
                NodeBottomLeft.AddContainedItems(set, rect);
                NodeBottomRight.AddContainedItems(set, rect);
            }

            public void AddIntersectingItems(HashSet<T> set, Rect rect)
            {
                NodeTopLeft.AddIntersectingItems(set, rect);
                NodeTopRight.AddIntersectingItems(set, rect);
                NodeBottomLeft.AddIntersectingItems(set, rect);
                NodeBottomRight.AddIntersectingItems(set, rect);
            }
        }
    }
}
