using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RedisBenchmark;
using System;

namespace UnitTestProject1
{
    [TestClass]
    public class BenchmarkUnitTest
    {
        [TestMethod]
        public void When_BenchMarkTool_Is_Constructed_Then_AllParametersMustBe_Initialized()
        {
            var mockCache = new Mock<IDistributedCache>();
            int countActualCacheSet = 0;
            int countActualCacheGet = 0;
            Func<string, byte[]> fnGetCache = delegate (string key)
            {
                countActualCacheGet++;
                return new byte[] { 1, 2, 3, };
            };
            Action<string, byte[], DistributedCacheEntryOptions> fnSetCache = delegate (string key, byte[] data, DistributedCacheEntryOptions options)
            {
                countActualCacheSet++;
            };
            mockCache.Setup(x => x.Get(It.IsAny<string>())).Returns<string>(fnGetCache);
            mockCache.Setup(x => x.Set(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>())).Callback<string, byte[], DistributedCacheEntryOptions>(fnSetCache);

            var logger = NullLogger<Tool>.Instance;
            int readWeight = 80;
            int writeWeight = 20;
            int sizeOfPayload = 3332;
            int countOfObjects = 999;
            int iterations = 10000;
            var tool = new Tool(
                mockCache.Object,
                readWeight, writeWeight,
                logger, iterations, countOfObjects, sizeOfPayload);
            Result r = tool.Run();
            Assert.AreEqual(r.Iterations, tool.Iterations);
            double actualProportionOfRW = (double)r.TotalWrites / r.TotalReads;
            double expectedProportionOfRW = (double)writeWeight / readWeight;
            Assert.AreEqual(actualProportionOfRW, expectedProportionOfRW, 0.05);
        }
    }
}