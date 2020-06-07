using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using REDIS = StackExchange.Redis;
using System;

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

            var redisConfig = new RedisConfig();
            config.Bind(redisConfig);
            builder.Services.AddSingleton<RedisConfig>(redisConfig);
            builder.Services.AddSingleton<RedisConfig>(provider => new RedisConfig
            {
                Server = provider.GetRequiredService<IConfiguration>()["REDISDEMO_REDISSERVER"],
                Port = provider.GetRequiredService<IConfiguration>()["REDISDEMO_REDISPORT"]
            });
            builder.Services.AddSingleton<REDIS.IServer>(this.CreateRedisServer);
        }

        private REDIS.IServer CreateRedisServer(IServiceProvider provider)
        {
            var redisConfig = provider.GetService<RedisConfig>();
            var cnstringAdmin = $"{redisConfig.Server}:{redisConfig.Port},allowAdmin=true";
            var cnstring = $"{redisConfig.Server}:{redisConfig.Port}";
            //You need allowAdmin=true to call methods .FlushDatabase and .Keys()
            var redis = REDIS.ConnectionMultiplexer.Connect(cnstringAdmin);
            return redis.GetServer(cnstring);
        }
    }
}