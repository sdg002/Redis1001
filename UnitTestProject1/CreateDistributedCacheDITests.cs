using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace UnitTestProject1
{
    /// <summary>
    /// Demonstrates how to create an instance of IDistributedCache via dependency injection
    /// You will need a local installation of Redis Server
    /// </summary>
    [TestClass]
    public class CreateDistributedCacheDITests
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
            string expectedData = $"value {Guid.NewGuid()}";
            var cache = _sprovider.GetService<IDistributedCache>();
            string key = $"key-{Guid.NewGuid()}";
            cache.SetString(key, expectedData);
            var actualData = cache.GetString(key);
            Trace.WriteLine($"Got item from cache {actualData} using key={key}");
            Assert.AreEqual(expectedData, actualData);
        }

        [TestMethod]
        public void Condition_Injection_Of_IDistributedCache()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("settings.json", optional: false);
            Config = builder.Build();

            ServiceCollection coll = new ServiceCollection();
            if (System.Environment.GetEnvironmentVariable("localdebug") == "1")
            {
                coll.AddDistributedMemoryCache();
            }
            else
            {
                coll.AddStackExchangeRedisCache(options =>
                {
                    string server = Config["redis-server"];
                    string port = Config["redis-port"];
                    string cnstring = $"{server}:{port}";
                    options.Configuration = cnstring;
                });
            }
            var provider = coll.BuildServiceProvider();
            var cache = provider.GetService<IDistributedCache>();
        }
    }
}