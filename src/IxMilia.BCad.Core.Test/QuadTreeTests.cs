using System;
using IxMilia.BCad.Collections;
using Xunit;

namespace IxMilia.BCad.Core.Test
{
    public class QuadTreeTests
    {
        [Fact]
        public void SimpleLeafTest()
        {
            var tree = new QuadTree<Tuple<int, int>>(new Rect(0, 0, 100, 100), GetTupleRect);
            tree.AddItem(Tuple.Create(25, 25));
            tree.AddItem(Tuple.Create(25, 75));
            tree.AddItem(Tuple.Create(75, 25));
            tree.AddItem(Tuple.Create(75, 75));

            Assert.Equal(4, tree.GetContainedItems(new Rect(0, 0, 100, 100)).Count);
            Assert.Equal(4, tree.GetContainedItems(new Rect(25, 25, 50, 50)).Count);
            Assert.Single(tree.GetContainedItems(new Rect(0, 0, 50, 50)));
            Assert.Single(tree.GetContainedItems(new Rect(50, 0, 50, 50)));
            Assert.Single(tree.GetContainedItems(new Rect(0, 50, 50, 50)));
            Assert.Single(tree.GetContainedItems(new Rect(50, 50, 50, 50)));
        }

        [Fact]
        public void ParentNodeTest()
        {
            var tree = new QuadTree<Tuple<int, int>>(new Rect(0, 0, 100, 100), GetTupleRect);
            tree.AddItem(Tuple.Create(25, 25));
            tree.AddItem(Tuple.Create(25, 75));
            tree.AddItem(Tuple.Create(75, 25));
            tree.AddItem(Tuple.Create(75, 75));
            tree.AddItem(Tuple.Create(50, 50));
            tree.AddItem(Tuple.Create(10, 10));

            Assert.Equal(6, tree.GetContainedItems(new Rect(0, 0, 100, 100)).Count);
            Assert.Equal(5, tree.GetContainedItems(new Rect(25, 25, 50, 50)).Count);
            Assert.Equal(3, tree.GetContainedItems(new Rect(0, 0, 50, 50)).Count);
            Assert.Equal(2, tree.GetContainedItems(new Rect(50, 0, 50, 50)).Count);
            Assert.Equal(2, tree.GetContainedItems(new Rect(0, 50, 50, 50)).Count);
            Assert.Equal(2, tree.GetContainedItems(new Rect(50, 50, 50, 50)).Count);
        }

        private static Rect GetTupleRect(Tuple<int, int> item)
        {
            return new Rect(item.Item1, item.Item2, 0, 0);
        }
    }
}
