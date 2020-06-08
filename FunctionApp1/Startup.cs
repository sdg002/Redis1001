﻿using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using REDIS = StackExchange.Redis;
using System;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using System.Linq;

[assembly: FunctionsStartup(typeof(FunctionApp1.Startup))]

namespace FunctionApp1
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddEnvironmentVariables();
            IConfiguration config = configurationBuilder.Build();
            builder.Services.AddSingleton<IConfiguration>(config);

            builder.Services.Configure<RedisConfiguration>(config, redisconfig =>
            {
                var redisConfig = new RedisConfiguration();
                redisConfig.ConnectionStringAdmin = config["REDISDEMO_CNSTRING_ADMIN"];
                redisConfig.ConnectionStringTxn = config["REDISDEMO_CNSTRING_TRANSACTIONS"];
            });
            builder.Services.AddSingleton<RedisConfiguration>(provider => new RedisConfiguration
            {
                ConnectionStringAdmin = provider.GetRequiredService<IConfiguration>()["REDISDEMO_CNSTRING_ADMIN"],
                ConnectionStringTxn = provider.GetRequiredService<IConfiguration>()["REDISDEMO_CNSTRING_TRANSACTIONS"]
            });
            builder.Services.AddTransient<REDIS.IServer>(this.CreateRedisServerForManagement);

            builder.Services.AddStackExchangeRedisCache(opt =>
            {
                var redisConfig = builder.Services.BuildServiceProvider().GetService<RedisConfiguration>();
                opt.Configuration = redisConfig.ConnectionStringTxn;
            });
        }

        private REDIS.IServer CreateRedisServerForManagement(IServiceProvider provider)
        {
            var redisConfig = provider.GetService<RedisConfiguration>();
            var cnstringAdmin = redisConfig.ConnectionStringAdmin;
            //You need allowAdmin=true to call methods .FlushDatabase and .Keys()
            //https://stackexchange.github.io/StackExchange.Redis/Basics.html
            var redis = REDIS.ConnectionMultiplexer.Connect(cnstringAdmin);
            var firstEndPoint = redis.GetEndPoints().FirstOrDefault();
            if (firstEndPoint == null)
            {
                throw new ArgumentException("The endpoints collection was empty. Could not get an end point from Redis connection multiplexer.");
            }
            return redis.GetServer(firstEndPoint);
        }
    }
}