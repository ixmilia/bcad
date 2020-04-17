using System.Collections.Generic;

namespace IxMilia.BCad.Collections
{
    public partial class QuadTree<T>
    {
        private class QuadTreeParent : IQuadTreeNode
        {
            public Rect Rect { get; }

            public int Count => _topLeft.Count + _topRight.Count + _bottomLeft.Count + _bottomRight.Count;

            private IQuadTreeNode _topLeft;
            private IQuadTreeNode _topRight;
            private IQuadTreeNode _bottomLeft;
            private IQuadTreeNode _bottomRight;
            private GetBoundingRectangle _getBounding;

            public QuadTreeParent(Rect rect, GetBoundingRectangle getBoundingRectangle, IEnumerable<T> items, int maxItems)
            {
                Rect = rect;

                _getBounding = getBoundingRectangle;

                var left = rect.Left;
                var top = rect.Top;
                var halfWidth = rect.Width / 2.0;
                var halfHeight = rect.Height / 2.0;

                _topLeft = new QuadTreeLeaf(new Rect(left, top, halfWidth, halfHeight), _getBounding, maxItems);
                _topRight = new QuadTreeLeaf(new Rect(left + halfWidth, top, halfWidth, halfHeight), _getBounding, maxItems);
                _bottomLeft = new QuadTreeLeaf(new Rect(left, top + halfHeight, halfWidth, halfHeight), _getBounding, maxItems);
                _bottomRight = new QuadTreeLeaf(new Rect(left + halfWidth, top + halfHeight, halfWidth, halfHeight), _getBounding, maxItems);

                foreach (var item in items)
                {
                    AddItem(item);
                }
            }

            public IQuadTreeNode AddItem(T item)
            {
                var bounding = _getBounding(item);
                if (_topLeft.Rect.Intersects(bounding))
                {
                    _topLeft = _topLeft.AddItem(item);
                }

                if (_topRight.Rect.Intersects(bounding))
                {
                    _topRight = _topRight.AddItem(item);
                }

                if (_bottomLeft.Rect.Intersects(bounding))
                {
                    _bottomLeft = _bottomLeft.AddItem(item);
                }

                if (_bottomRight.Rect.Intersects(bounding))
                {
                    _bottomRight = _bottomRight.AddItem(item);
                }

                return this;
            }

            public void AddContainedItems(HashSet<T> set, Rect rect)
            {
                _topLeft.AddContainedItems(set, rect);
                _topRight.AddContainedItems(set, rect);
                _bottomLeft.AddContainedItems(set, rect);
                _bottomRight.AddContainedItems(set, rect);
            }

            public void AddIntersectingItems(HashSet<T> set, Rect rect)
            {
                _topLeft.AddIntersectingItems(set, rect);
                _topRight.AddIntersectingItems(set, rect);
                _bottomLeft.AddIntersectingItems(set, rect);
                _bottomRight.AddIntersectingItems(set, rect);
            }
        }
    }
}
