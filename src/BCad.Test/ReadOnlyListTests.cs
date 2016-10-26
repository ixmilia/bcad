// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using BCad.Collections;
using Xunit;

namespace BCad.Test
{
    public class ReadOnlyListTests
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
        public void LinearInsertionAndDeletionTest()
        {
            var list = ReadOnlyList<int>.Empty();
            for (int i = 0; i < 10; i++)
            {
                Assert.Equal(i, list.Count);
                list = list.Add(i);
            }

            Assert.Equal(10, list.Count);

            for (int i = 9; i >= 0; i--)
            {
                list = list.Remove(0);
                Assert.Equal(i, list.Count);
            }

            Assert.Equal(0, list.Count);
        }

        [Fact]
        public void IEnumerableCreationTest()
        {
            var array = new[] { 0, 1, 2, 3, 4, 5 };
            var list = ReadOnlyList<int>.Create(array);
            Assert.Equal(array.Length, list.Count);
            var equal = array.Zip(list, (a, b) => a == b);
            Assert.True(equal.All(x => x));
        }

        [Fact]
        public void DeleteSpecificIndexTest()
        {
            var array = new[] { 0, 1, 2, 3, 4, 5 };
            var list = ReadOnlyList<int>.Create(array);

            // remove first index
            list = list.Remove(0);
            Assert.Equal(5, list.Count);
            AssertArrayEqual(new[] { 1, 2, 3, 4, 5 }, list.ToArray());

            // remove last index
            list = list.Remove(4);
            Assert.Equal(4, list.Count);
            AssertArrayEqual(new[] { 1, 2, 3, 4 }, list.ToArray());

            // remove middle index
            list = list.Remove(1);
            Assert.Equal(3, list.Count);
            AssertArrayEqual(new[] { 1, 3, 4 }, list.ToArray());
        }
    }
}
