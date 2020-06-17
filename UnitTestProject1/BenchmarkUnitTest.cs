using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RedisBenchmark;
using System;
using System.Collections.Generic;
using System.Threading;

namespace UnitTestProject1
{
    [TestClass]
    public class BenchmarkUnitTest
    {
        private Random _random;

        [TestInitialize]
        public void Init()
        {
            _random = new Random(DateTime.Now.Second);
        }

        [TestMethod]
        public void When_BenchMarkTool_Is_Constructed_Then_AllParametersMustBe_Initialized()
        {
            var mockCache = new Mock<IDistributedCache>();
            int countActualCacheSet = 0;
            int countActualCacheGet = 0;
            Func<string, byte[]> fnGetCache = delegate (string key)
            {
                countActualCacheGet++;
                SleepForRandomInterval();
                return new byte[] { 1, 2, 3, };
            };
            Action<string, byte[], DistributedCacheEntryOptions> fnSetCache = delegate (string key, byte[] data, DistributedCacheEntryOptions options)
            {
                countActualCacheSet++;
                SleepForRandomInterval();
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
            Result result = tool.Run();
            Assert.AreEqual(result.Iterations, tool.Iterations);
            double actualProportionOfRW = (double)result.TotalWrites / result.TotalReads;
            double expectedProportionOfRW = (double)writeWeight / readWeight;
            Assert.AreEqual(actualProportionOfRW, expectedProportionOfRW, 0.05);

            Assert.IsNotNull(result.ReadLatency);
            Assert.IsNotNull(result.WriteLatency);

            Assert.IsTrue(result.ReadLatency.MaxLatency > 0);
            Assert.IsTrue(result.ReadLatency.MinLatency > 0);
            Assert.IsTrue(result.ReadLatency.MeanLatency > 0);
            Assert.IsTrue(result.ReadLatency.MeanLatency > result.ReadLatency.MinLatency);
            Assert.IsTrue(result.ReadLatency.MaxLatency > result.ReadLatency.MeanLatency);
            Assert.IsTrue(result.ReadLatency.NinetyFiveReadPercentLatency > result.ReadLatency.MeanLatency);
            Assert.IsTrue(result.ReadLatency.MaxLatency > result.ReadLatency.NinetyFiveReadPercentLatency);

            Assert.IsTrue(result.WriteLatency.MaxLatency > 0);
            Assert.IsTrue(result.WriteLatency.MinLatency > 0);
            Assert.IsTrue(result.WriteLatency.MeanLatency > 0);
            Assert.IsTrue(result.WriteLatency.MeanLatency > result.ReadLatency.MinLatency);
            Assert.IsTrue(result.WriteLatency.MaxLatency > result.ReadLatency.MeanLatency);
            Assert.IsTrue(result.WriteLatency.NinetyFiveReadPercentLatency > result.ReadLatency.MeanLatency);
            Assert.IsTrue(result.WriteLatency.MaxLatency > result.ReadLatency.NinetyFiveReadPercentLatency);
        }

        private void SleepForRandomInterval()
        {
            int delayMs = _random.Next() % 5 + 1;
            Thread.Sleep(delayMs);
        }

        [TestMethod]
        public void Compute97Percentile()
        {
            var tool = new RedisBenchmark.Tool(null, 0, 0, null, 0, 0, 0);
            List<long> dummyObservations = new List<long>();
            for (int i = 1; i <= 1000; i++)
            {
                dummyObservations.Add(i);
            }
            dummyObservations.Sort((o1, o2) => _random.Next());
            double actual95 = tool.Compute95Percentile(dummyObservations);
            double expected95 = 950;
            Assert.AreEqual(actual95, expected95);
        }
    }
}