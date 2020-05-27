using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using System;

namespace UnitTestProject1
{
    [TestClass]
    public class BasicTest
    {
        public static IConfigurationRoot Config { get; set; }

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("settings.json", optional: false);
            Config = builder.Build();
        }

        //private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        //{
        //    string cacheConnection = "";// ConfigurationManager.AppSettings["CacheConnection"].ToString();
        //    return ConnectionMultiplexer.Connect(cacheConnection);
        //});

        //public static ConnectionMultiplexer Connection
        //{
        //    get
        //    {
        //        return lazyConnection.Value;
        //    }
        //}

        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}