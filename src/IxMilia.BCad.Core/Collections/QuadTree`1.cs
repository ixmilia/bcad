using System.Collections.Generic;

namespace IxMilia.BCad.Collections
{
    public partial class QuadTree<T>
    {
        private const int MAX_ITEMS = 50;

        public delegate Rect GetBoundingRectangle(T item);

        private IQuadTreeNode _root;

        public QuadTree(Rect rect, GetBoundingRectangle getBoundingRectangle, int maxItems = MAX_ITEMS)
        {
            _root = new QuadTreeLeaf(rect, getBoundingRectangle, maxItems);
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
