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

        [FunctionName("BulkAddCustomers")]
        public async Task<IActionResult> BulkAddCustomers(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "bulkadd")] HttpRequest req
           )
        {
            _logger.LogInformation("C# HTTP trigger function BulkAddCustomers");

            const int MAXITEMS = 10000;

            try
            {
                string count = req.Query["count"];

                int countOfItems = (count == null) ? MAXITEMS : int.Parse(count);

                for (int i = 0; i < countOfItems; i++)
                {
                    string cacheKey = CreateKeyFromIndex(i);
                    var c = new Customer($"email-{i}", $"firstname-{i}", $"lastname-{i}");
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(c);
                    await _cacheTxnServerInstance.SetStringAsync(cacheKey, json);
                }
                return new OkObjectResult($"Count of items added to cache={countOfItems}. Structur of key= 'customer-{{index}}'");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while attempting to invoke BulkAddCustomers", ex);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        private string CreateKeyFromIndex(int itemIndex)
        {
            return $"customer-{itemIndex}";
        }
    }
}