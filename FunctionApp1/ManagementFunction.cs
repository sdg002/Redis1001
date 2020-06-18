using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RedisBenchmark;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using REDIS = StackExchange.Redis;

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
                _logger.LogError(ex, "Error while attempting to invoke GetCachedItemCount");
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
                _logger.LogError(ex, "Error while attempting to FlushCache");
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
                _logger.LogError(ex, "Error while attempting to invoke Ping");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [FunctionName("Benchmark")]
        public async Task<IActionResult> Benchmark(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "benchmark")] HttpRequest req
            )
        {
            _logger.LogInformation($"C# HTTP trigger function {nameof(Benchmark)}");
            try
            {
                string siterations = req.Query.ContainsKey("iterations") ? req.Query["iterations"].ToString() : "10000";
                int iterations = int.Parse(siterations);

                string sAllocations = req.Query.ContainsKey("allocations") ? req.Query["allocations"].ToString() : "1000";
                int allocations = int.Parse(sAllocations);

                string sPayloadSize = req.Query.ContainsKey("payload") ? req.Query["payload"].ToString() : "1000";
                int payloadSize = int.Parse(sPayloadSize);

                string sReadWeight = req.Query.ContainsKey("readweight") ? req.Query["readweight"].ToString() : "80";
                int readWeight = int.Parse(sReadWeight);

                string sWriteWeight = req.Query.ContainsKey("writeweight") ? req.Query["writeweight"].ToString() : "80";
                int writeWeight = int.Parse(sWriteWeight);

                await _redisAdminServerInstance.FlushDatabaseAsync();
                var tool = new Tool(
                    _cacheTxnServerInstance,
                    readWeight, writeWeight,
                    NullLogger<Tool>.Instance,
                    iterations, allocations, payloadSize);

                var latencyResult = tool.Run();
                var oResult = new ObjectResult(latencyResult);
                oResult.StatusCode = (int)HttpStatusCode.OK;
                return oResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while attempting to invoke {nameof(Benchmark)}");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}