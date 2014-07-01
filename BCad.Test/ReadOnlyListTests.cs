using System.Linq;
using BCad.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BCad.Test
{
    [TestClass]
    public class ReadOnlyListTests
    {
        private void AssertArrayEqual<T>(T[] expected, T[] actual)
        {
            if (expected == null)
                Assert.IsNull(actual);
            if (expected != null)
                Assert.IsNotNull(actual);
            if (expected.Length != actual.Length)
                Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        public void LinearInsertionAndDeletionTest()
        {
            var list = ReadOnlyList<int>.Empty();
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(i, list.Count);
                list = list.Add(i);
            }

            Assert.AreEqual(10, list.Count);

            for (int i = 9; i >= 0; i--)
            {
                list = list.Remove(0);
                Assert.AreEqual(i, list.Count);
            }

            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void IEnumerableCreationTest()
        {
            var array = new[] { 0, 1, 2, 3, 4, 5 };
            var list = ReadOnlyList<int>.Create(array);
            Assert.AreEqual(array.Length, list.Count);
            var equal = array.Zip(list, (a, b) => a == b);
            Assert.IsTrue(equal.All(x => x));
        }

        [TestMethod]
        public void DeleteSpecificIndexTest()
        {
            var array = new[] { 0, 1, 2, 3, 4, 5 };
            var list = ReadOnlyList<int>.Create(array);

            // remove first index
            list = list.Remove(0);
            Assert.AreEqual(5, list.Count);
            AssertArrayEqual(new[] { 1, 2, 3, 4, 5 }, list.ToArray());

            // remove last index
            list = list.Remove(4);
            Assert.AreEqual(4, list.Count);
            AssertArrayEqual(new[] { 1, 2, 3, 4 }, list.ToArray());

            // remove middle index
            list = list.Remove(1);
            Assert.AreEqual(3, list.Count);
            AssertArrayEqual(new[] { 1, 3, 4 }, list.ToArray());
        }
    }
}
