using System;
using BCad.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BCad.Test
{
    [TestClass]
    public class QuadTreeTests
    {
        [TestMethod]
        public void SimpleLeafTest()
        {
            var tree = new QuadTree<Tuple<int, int>>(new Rect(0, 0, 100, 100), GetTupleRect);
            tree.AddItem(Tuple.Create(25, 25));
            tree.AddItem(Tuple.Create(25, 75));
            tree.AddItem(Tuple.Create(75, 25));
            tree.AddItem(Tuple.Create(75, 75));

            Assert.AreEqual(4, tree.GetContainedItems(new Rect(0, 0, 100, 100)).Count);
            Assert.AreEqual(4, tree.GetContainedItems(new Rect(25, 25, 50, 50)).Count);
            Assert.AreEqual(1, tree.GetContainedItems(new Rect(0, 0, 50, 50)).Count);
            Assert.AreEqual(1, tree.GetContainedItems(new Rect(50, 0, 50, 50)).Count);
            Assert.AreEqual(1, tree.GetContainedItems(new Rect(0, 50, 50, 50)).Count);
            Assert.AreEqual(1, tree.GetContainedItems(new Rect(50, 50, 50, 50)).Count);
        }

        [TestMethod]
        public void ParentNodeTest()
        {
            var tree = new QuadTree<Tuple<int, int>>(new Rect(0, 0, 100, 100), GetTupleRect);
            tree.AddItem(Tuple.Create(25, 25));
            tree.AddItem(Tuple.Create(25, 75));
            tree.AddItem(Tuple.Create(75, 25));
            tree.AddItem(Tuple.Create(75, 75));
            tree.AddItem(Tuple.Create(50, 50));
            tree.AddItem(Tuple.Create(10, 10));

            Assert.AreEqual(6, tree.GetContainedItems(new Rect(0, 0, 100, 100)).Count);
            Assert.AreEqual(5, tree.GetContainedItems(new Rect(25, 25, 50, 50)).Count);
            Assert.AreEqual(3, tree.GetContainedItems(new Rect(0, 0, 50, 50)).Count);
            Assert.AreEqual(2, tree.GetContainedItems(new Rect(50, 0, 50, 50)).Count);
            Assert.AreEqual(2, tree.GetContainedItems(new Rect(0, 50, 50, 50)).Count);
            Assert.AreEqual(2, tree.GetContainedItems(new Rect(50, 50, 50, 50)).Count);
        }

        private static Rect GetTupleRect(Tuple<int, int> item)
        {
            return new Rect(item.Item1, item.Item2, 0, 0);
        }
    }
}
