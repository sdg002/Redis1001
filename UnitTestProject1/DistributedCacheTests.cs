using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace UnitTestProject1
{
    [TestClass]
    public class DistributedCacheTests
    {
        public static IConfigurationRoot Config { get; set; }

        public static ServiceProvider _sprovider;

        [ClassInitialize]
        public static void Init(TestContext context)

        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("settings.json", optional: false);
            Config = builder.Build();

            ServiceCollection coll = new ServiceCollection();
            coll.AddStackExchangeRedisCache(options =>
            {
                string server = Config["redis-server"];
                string port = Config["redis-port"];
                string cnstring = $"{server}:{port}";
                options.Configuration = cnstring;
            });

            _sprovider = coll.BuildServiceProvider();
        }

        [TestMethod]
        public void SetGetItem()
        {
            var cache = _sprovider.GetService<IDistributedCache>();
            string key = $"key-{Guid.NewGuid()}";
            cache.SetString(key, $"value {Guid.NewGuid()}");
            var item = cache.GetString(key);
            Trace.WriteLine($"Got item from cache {item} using key={key}");
        }
    }
}