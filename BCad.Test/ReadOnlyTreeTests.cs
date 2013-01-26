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
    }
}
