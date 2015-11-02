using System.Collections.Generic;

namespace BCad.Collections
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
