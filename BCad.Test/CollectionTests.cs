using BCad.Collections;
using Xunit;

namespace BCad.Test
{
    public class CollectionTests
    {
        [Fact]
        public void ReadOnlyDictionaryInitializerTest()
        {
            // initializer should fail
            var dict = new ReadOnlyDictionary<int, int>() { { 1, 2 } };
            Assert.Equal(0, dict.Count);
        }
    }
}
