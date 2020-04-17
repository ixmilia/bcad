using System;
using System.Collections.Generic;
using System.Linq;
using IxMilia.BCad.Collections;
using Xunit;

namespace IxMilia.BCad.Core.Test
{
    public class ReadOnlyTreeTests
    {
        private void AssertArrayEqual<T>(T[] expected, T[] actual)
        {
            if (expected == null)
                Assert.Null(actual);
            if (expected != null)
                Assert.NotNull(actual);
            if (expected.Length != actual.Length)
                Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], actual[i]);
            }
        }

        [Fact]
        public void LinearInsertTest()
        {
            int treeSize = 20;
            var tree = new ReadOnlyTree<int, int>();
            for (int i = 0; i < treeSize; i++)
            {
                tree = tree.Insert(i, i * i);
                Assert.Equal(i + 1, tree.Count);
            }

            Assert.Equal(treeSize, tree.Count);
            for (int i = 0; i < treeSize; i++)
            {
                int value;
                Assert.True(tree.TryFind(i, out value));
                Assert.Equal(i * i, value);
            }
        }

        [Fact]
        public void BalancedInsertionTest()
        {
            //       4
            //      / \
            //     /   \
            //    2     6
            //   / \   / \
            //  1   3 5   7
            var tree = new ReadOnlyTree<int, int>();
            foreach (var i in new[] { 4, 2, 6, 1, 5, 3, 7 })
            {
                tree = tree.Insert(i, i * i);
            }

            Assert.Equal(7, tree.Count);
            for (int i = 1; i <= 7; i++)
            {
                int value;
                Assert.True(tree.TryFind(i, out value));
                Assert.Equal(i * i, value);
            }
        }

        [Fact]
        public void DeleteWhenEmptyTest()
        {
            var tree = CreateTree();
            tree = tree.Delete(0);
            Assert.Equal(0, tree.Count);
        }

        [Fact]
        public void DeleteSingleItemTest()
        {
            var tree = CreateTree(0);
            tree = tree.Delete(0);
            Assert.Equal(0, tree.Count);
        }

        [Fact]
        public void DeleteWithNoRebalancingTest()
        {
            var tree = CreateTree(2, 1, 3);
            
            tree = tree.Delete(1);
            Assert.Equal(2, tree.Count);
            Assert.False(tree.KeyExists(1));
            Assert.True(tree.KeyExists(2));
            Assert.True(tree.KeyExists(3));

            tree = tree.Delete(3);
            Assert.Equal(1, tree.Count);
            Assert.False(tree.KeyExists(1));
            Assert.True(tree.KeyExists(2));
            Assert.False(tree.KeyExists(3));
        }

        [Fact]
        public void DeleteFullTreeTest()
        {
            var numbers = Enumerable.Range(0, 20).ToArray();
            var tree = CreateTree(numbers);

            for (var i = 19; i >= 0; i--)
            {
                tree = tree.Delete(i);
                Assert.Equal(i, tree.Count);
            }
        }

        [Fact]
        public void ToListTest()
        {
            var values = new int[]{ 4, 2, 6, 1, 5, 3, 7 };
            var tree = CreateTree(values); // create a balanced tree
            AssertArrayEqual(values.OrderBy(x => x).ToArray(), tree.GetKeys().ToArray());
        }

        [Fact]
        public void DeleteWithBothChildren()
        {
            // bug fix
            //   2
            //  / \
            // 1   3
            var tree = CreateTree(2, 1, 3);
            tree = tree.Delete(2);
            //   3
            //  /
            // 1
            AssertArrayEqual(new int[] { 1, 3 }, tree.GetKeys().ToArray());
        }

        [Fact]
        public void DeleteWithLeftAndRightChildren1()
        {
            // bug fix
            //   2
            //  / \
            // 1   3
            //      \
            //       4
            var tree = CreateTree(2, 1, 3, 4);
            tree = tree.Delete(2);
            //   3
            //  / \
            // 1   4
            AssertArrayEqual(new int[] { 1, 3, 4 }, tree.GetKeys().ToArray());
        }

        [Fact]
        public void DeleteWithLeftAndRightChildren2()
        {
            // bug fix
            //     3
            //    / \
            //   2   4
            //  /
            // 1
            var tree = CreateTree(3, 2, 4, 1);
            tree = tree.Delete(3);
            //   2
            //  / \
            // 1   4
            AssertArrayEqual(new int[] { 1, 2, 4 }, tree.GetKeys().ToArray());
        }

        [Fact]
        public void DeleteWithBothChildrenNotRoot1()
        {
            // bug fix
            //   2
            //  / \
            // 1   4
            //    / \
            //   3   5
            var tree = CreateTree(2, 1, 4, 3, 5);
            tree = tree.Delete(4);
            //   2
            //  / \
            // 1   5
            //    /
            //   3
            AssertArrayEqual(new int[] { 1, 2, 3, 5 }, tree.GetKeys().ToArray());
        }

        [Fact]
        public void DeleteWithBothChildrenNotRoot2()
        {
            // bug fix
            //     4
            //    / \
            //   2   5
            //  / \
            // 1   3
            var tree = CreateTree(4, 2, 5, 1, 3);
            tree = tree.Delete(2);
            //   4
            //  / \
            // 1   5
            //  \
            //   3
            AssertArrayEqual(new int[] { 1, 3, 4, 5 }, tree.GetKeys().ToArray());
        }

        [Fact]
        public void DeleteWhenRightChildOnlyHasLeftChild()
        {
            // bug fix
            //   1
            //  / \
            // 0   3
            //    /
            //   2
            var tree = CreateTree(1, 0, 3, 2);
            tree = tree.Delete(1);
            //   2
            //  / \
            // 0   3
            AssertArrayEqual(new int[] { 0, 2, 3 }, tree.GetKeys().ToArray());
        }

        [Fact]
        public void DeleteRebalanceTest()
        {
            // bug fix
            var tree = CreateTree(6, 4, 3, 7, 8, 9, 2);
            tree = tree.Delete(6);
            tree = tree.Delete(8);
            AssertArrayEqual(new int[] { 2, 3, 4, 7, 9 }, tree.GetKeys().ToArray());
        }

        [Fact]
        public void RandomTreeTest()
        {
            var rand = new Random();
            var max = 100;
            for (int i = 0; i < 1000; i++)
            {
                var insertOrder = new List<int>();
                var deleteOrder = new List<int>();
                var operation = "insert";
                try
                {
                    var tree = CreateTree();

                    // insert into the tree
                    operation = "insert";
                    for (int j = 0; j < max; j++)
                    {
                        var key = rand.Next(max);
                        if (!tree.KeyExists(key))
                        {
                            var preCount = tree.Count;
                            insertOrder.Add(key);
                            tree = tree.Insert(key, 0);
                            var postCount = tree.Count;
                            Assert.Equal(1, postCount - preCount);
                        }
                    }

                    // remove from the tree
                    operation = "delete";
                    while (tree.Count > 0)
                    {
                        var key = rand.Next(max);
                        if (tree.KeyExists(key))
                        {
                            var preCount = tree.Count;
                            deleteOrder.Add(key);
                            tree = tree.Delete(key);
                            var postCount = tree.Count;
                            Assert.Equal(1, preCount - postCount);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error doing operation {0}: insert order: [{1}], delete order: [{2}]",
                        operation,
                        string.Join(", ", insertOrder),
                        string.Join(", ", deleteOrder)),
                        ex);
                }
            }
        }

        [Fact]
        public void MutableBatchInsertTest()
        {
            var array = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var tree = ReadOnlyTree<int, int>.FromEnumerable(array, i => i);
            Assert.Equal(array.Length, tree.Count);
            AssertArrayEqual(array, tree.GetKeys().ToArray());
            AssertArrayEqual(array, tree.GetValues().ToArray());
        }

        private ReadOnlyTree<int, int> CreateTree(params int[] values)
        {
            var tree = new ReadOnlyTree<int, int>();
            foreach (var v in values)
                tree = tree.Insert(v, v * v);
            Assert.Equal(values.Length, tree.Count);
            return tree;
        }
    }
}
