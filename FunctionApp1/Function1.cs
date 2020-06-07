using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using REDIS = StackExchange.Redis;
using System.Linq;

namespace FunctionApp1
{
    public class Function1
    {
        private readonly IConfiguration _config;
        private readonly ILogger<Function1> _logger;
        private readonly RedisConfig _redisConfig;
        private readonly REDIS.IServer _redisAdminServerInstance;

        public Function1(IConfiguration config, ILogger<Function1> logger, RedisConfig redisConfig, REDIS.IServer redisServer)
        {
            _config = config;
            _logger = logger;
            _redisConfig = redisConfig;
            _redisAdminServerInstance = redisServer;
        }

        [FunctionName("BulkAddCustomers")]
        public async Task<IActionResult> BulkAddCustomers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "bulkadd")] HttpRequest req
            )
        {
            _logger.LogInformation("C# HTTP trigger function BulkAddCustomers");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

        [FunctionName("GetCachedItemCount")]
        public async Task<IActionResult> GetCachedItemCount(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getcacheditemcount")] HttpRequest req
            )
        {
            _logger.LogInformation($"C# HTTP trigger function {nameof(GetCachedItemCount)}");
            var allKeys = _redisAdminServerInstance.Keys(pattern: "*");

            int count = allKeys.Count();
            return new OkObjectResult($"Count of items in cache={count}");
        }

        [FunctionName("FlushCache")]
        public async Task<IActionResult> FlushCache(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "flushcache")] HttpRequest req
            )
        {
            _logger.LogInformation($"C# HTTP trigger function {nameof(FlushCache)}");

            await _redisAdminServerInstance.FlushDatabaseAsync();
            return new OkObjectResult($"Redis database was flushed at {DateTime.UtcNow}");
        }
    }
}