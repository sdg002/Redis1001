using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Caching.Distributed;
using System.Net;

namespace FunctionApp1
{
    public class TransactionFunction
    {
        private readonly IDistributedCache _cacheTxnServerInstance;
        private readonly ILogger<TransactionFunction> _logger;

        public TransactionFunction(IDistributedCache cacheTxnServerInstance, ILogger<TransactionFunction> logger)
        {
            _logger = logger;
            _cacheTxnServerInstance = cacheTxnServerInstance;
        }

        [FunctionName("GetItem")]
        public async Task<IActionResult> GetItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getitem")] HttpRequest req
            )
        {
            _logger.LogInformation($"C# HTTP trigger function {nameof(GetItem)}");
            try
            {
                string itemindex = req.Query["itemindex"];
                if (string.IsNullOrWhiteSpace(itemindex))
                {
                    throw new ArgumentException("The request parameter 'itemindex' was not found");
                }
                int intItemIndex = 0;
                if (!int.TryParse(itemindex, out intItemIndex))
                {
                    throw new ArgumentException("The request parameter 'itemindex' could not be parsed into a number");
                }
                string cacheKey = CreateKeyFromIndex(intItemIndex);
                string json = await _cacheTxnServerInstance.GetStringAsync(cacheKey);
                if (json == null)
                {
                    throw new ArgumentException($"Invalid cache key {intItemIndex}");
                }
                var customer = Newtonsoft.Json.JsonConvert.DeserializeObject<Customer>(json);
                return new OkObjectResult(customer);
            }
            catch (ArgumentException ex)
            {
                var result = new ContentResult
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Content = ex.Message
                };
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while attempting to FlushCache", ex);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        private string CreateKeyFromIndex(int itemIndex)
        {
            return $"customer-{itemIndex}";
        }
    }
}