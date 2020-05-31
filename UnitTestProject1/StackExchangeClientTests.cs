using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using System;
using System.Diagnostics;

namespace UnitTestProject1
{
    [TestClass]
    public class StackExchangeClientTests
    {
        public static IConfigurationRoot Config { get; set; }
        public static ConnectionMultiplexer _redis;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("settings.json", optional: false);
            Config = builder.Build();
            string server = Config["redis-server"];
            string port = Config["redis-port"];
            string cnstring = $"{server}:{port}";
            _redis = ConnectionMultiplexer.Connect(cnstring);
        }

        [TestMethod]
        public void Basic_Set_And_Get()
        {
            IDatabase db = _redis.GetDatabase();
            string key = $"mykey-{Guid.NewGuid()}";
            string value = $"abcdefg-{Guid.NewGuid()}";
            db.StringSet(key, value);
            string valueFromCache = db.StringGet(key);
            Trace.WriteLine($"Value of key:{key}, value:{valueFromCache}");
            Assert.AreEqual(valueFromCache, value);
        }
    }
}