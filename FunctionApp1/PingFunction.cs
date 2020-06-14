using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Net;

namespace FunctionApp1
{
    /// <summary>
    /// Implement a "simpleping" without any other dependencies.
    /// Helps us do a basic sanity test of the function,regardless of Redis
    /// </summary>
    public class PingFunction
    {
        public PingFunction()
        {
        }

        [FunctionName("simpleping")]
        public IActionResult SimplePing(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "simpleping")] HttpRequest req
            )
        {
            var time = DateTime.UtcNow;
            var oResult = new ObjectResult(time);
            oResult.StatusCode = (int)HttpStatusCode.OK;
            return oResult;
        }
    }
}