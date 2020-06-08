using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using REDIS = StackExchange.Redis;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Caching.Distributed;

namespace FunctionApp1
{
    public class ManagementFunction
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ManagementFunction> _logger;
        private readonly RedisConfiguration _redisConfig;
        private readonly REDIS.IServer _redisAdminServerInstance;
        private readonly IDistributedCache _cacheTxnServerInstance;

        public ManagementFunction(
            IConfiguration config,
            ILogger<ManagementFunction> logger,
            RedisConfiguration redisConfig,
            REDIS.IServer redisServer,
            IDistributedCache cacheTxnServerInstance)
        {
            _config = config;
            _logger = logger;
            _redisConfig = redisConfig;
            _redisAdminServerInstance = redisServer;
            _cacheTxnServerInstance = cacheTxnServerInstance;
        }

        [FunctionName("GetCachedItemCount")]
        public async Task<IActionResult> GetCachedItemCount(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getcacheditemcount")] HttpRequest req
            )
        {
            _logger.LogInformation($"C# HTTP trigger function {nameof(GetCachedItemCount)}");
            try
            {
                var allKeys = _redisAdminServerInstance.Keys(pattern: "*");

                int count = allKeys.Count();
                return new OkObjectResult($"Count of items in cache={count}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while attempting to invoke GetCachedItemCount", ex);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [FunctionName("FlushCache")]
        public async Task<IActionResult> FlushCache(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "flushcache")] HttpRequest req
            )
        {
            _logger.LogInformation($"C# HTTP trigger function {nameof(FlushCache)}");
            try
            {
                await _redisAdminServerInstance.FlushDatabaseAsync();
                return new OkObjectResult($"Redis database was flushed at {DateTime.UtcNow}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while attempting to FlushCache", ex);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [FunctionName("Ping")]
        public IActionResult Ping(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping")] HttpRequest req
            )
        {
            _logger.LogInformation($"C# HTTP trigger function {nameof(Ping)}");
            try
            {
                var time = DateTime.UtcNow;

                var oResult = new ObjectResult(time);
                oResult.StatusCode = (int)HttpStatusCode.OK;
                return oResult;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while attempting to invoke GetCachedItemCount", ex);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}