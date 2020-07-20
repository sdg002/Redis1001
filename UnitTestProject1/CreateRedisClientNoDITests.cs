using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using System;
using System.Diagnostics;

namespace UnitTestProject1
{
    /// <summary>
    /// Demonstrates how to create an instance of IDistributedCache through direct construction
    /// You will need a local installation of Redis Server
    /// </summary>
    [TestClass]
    public class CreateRedisClientNoDITests
    {
        [TestMethod]
        public void Create_Instance_Of_IDatabase_By_Constructing_ConnectionMultiplexer()
        {
            string server = "localhost";
            string port = "6379";
            string cnstring = $"{server}:{port}";
            string expectedStringData = "Hello world";

            var redisMx = StackExchange.Redis.ConnectionMultiplexer.Connect(cnstring);
            var database = redisMx.GetDatabase();
            database.StringSet("key002", new RedisValue(expectedStringData));
            var actualStringData = database.StringGet("key002");
            Assert.AreEqual(expectedStringData, actualStringData.ToString());
        }

        [TestMethod]
        public void Create_Instance_Of_IDistributedCache_By_Constructing_RedisClient()
        {
            try
            {
                string server = "localhost";
                string port = "6379";
                string cnstring = $"{server}:{port}";

                var redisOptions = new RedisCacheOptions
                {
                    ConfigurationOptions = new ConfigurationOptions()
                };
                redisOptions.ConfigurationOptions.EndPoints.Add(cnstring);
                var opts = Options.Create<RedisCacheOptions>(redisOptions);

                IDistributedCache cache = new Microsoft.Extensions.Caching.StackExchangeRedis.RedisCache(opts);
                string expectedStringData = "Hello world";
                cache.Set("key003", System.Text.Encoding.UTF8.GetBytes(expectedStringData));
                var dataFromCache = cache.Get("key003");
                var actualCachedStringData = System.Text.Encoding.UTF8.GetString(dataFromCache);
                Assert.AreEqual(expectedStringData, actualCachedStringData);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}