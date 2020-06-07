using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using System;
using System.Diagnostics;
using System.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class StackExchangeClientTests
    {
        public static IConfigurationRoot Config { get; set; }
        public static ConnectionMultiplexer _redis;
        public static string _server = null;
        public static string _port = null;
        public static string _cnstring = null;
        private const int MAXITEMCOUNT = 10;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("settings.json", optional: false);
            Config = builder.Build();
            _server = Config["redis-server"];
            _port = Config["redis-port"];
            _cnstring = $"{_server}:{_port},allowAdmin=true";
            //You need allowAdmin=true to call methods .FlushDatabase and .Keys()
            _redis = ConnectionMultiplexer.Connect(_cnstring);
        }

        [TestMethod]
        public void Basic_Set_And_Get()
        {
            IDatabase db = _redis.GetDatabase();

            for (int i = 0; i < MAXITEMCOUNT; i++)
            {
                string key = $"mykey-{Guid.NewGuid()}";
                string value = $"abcdefg-{Guid.NewGuid()}";
                db.StringSet(key, value);
                string valueFromCache = db.StringGet(key);
                Trace.WriteLine($"Value of key:{key}, value:{valueFromCache}");
                Assert.AreEqual(valueFromCache, value);
            }
        }

        [TestMethod]
        public void Flush_Cache()
        {
            var cnstring = $"{_server}:{_port}";
            IServer server = _redis.GetServer(cnstring);
            server.FlushDatabase();
            Trace.WriteLine("Flush complete");
        }

        [TestMethod]
        public void GetAllKeys()
        {
            var cnstring = $"{_server}:{_port}";
            IServer server = _redis.GetServer(cnstring);
            var allKeys = server.Keys(pattern: "*");
            Assert.IsNotNull(allKeys);
            Trace.WriteLine($"Total keys={allKeys.Count()}");
            foreach (var key in allKeys)
            {
                Trace.WriteLine($"key={key}");
            }
        }
    }
}