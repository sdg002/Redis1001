using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class InMemoryCacheTests
    {
        [TestMethod]
        public void ExampleTestMethod()
        {
            var expectedData = new byte[] { 100, 200 };

            var opts = Options.Create<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions());
            IDistributedCache cache = new MemoryDistributedCache(opts);
            cache.Set("key1", expectedData);
            var cachedData = cache.Get("key1");

            Assert.AreEqual(expectedData, cachedData);

            //Use the variable cache as an input to any class which expects IDistributedCache
        }
    }
}