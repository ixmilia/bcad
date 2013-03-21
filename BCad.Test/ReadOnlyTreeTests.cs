using System.Linq;
using BCad.Collections;
using Xunit;

namespace BCad.Test
{
    public class ReadOnlyTreeTests
    {
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
            Assert.Equal(values.OrderBy(x => x).ToArray(), tree.GetKeys().ToArray());
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
            Assert.Equal(new int[] { 1, 3 }, tree.GetKeys().ToArray());
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
            Assert.Equal(new int[] { 1, 3, 4 }, tree.GetKeys().ToArray());
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
            //   3
            //  / \
            // 1   4
            Assert.Equal(new int[] { 1, 2, 4 }, tree.GetKeys().ToArray());
        }

        [Fact]
        public void DeleteWithBothChildrenNotRoot()
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
            Assert.Equal(new int[] { 1, 2, 3, 5 }, tree.GetKeys().ToArray());
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
