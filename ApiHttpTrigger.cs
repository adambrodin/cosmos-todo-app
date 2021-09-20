using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AdamBrodin.Functions
{
    public static class ApiHttpTrigger
    {
        // Defines what database to connect to
        [FunctionName("ApiHttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "todo")] HttpRequest req,
            ILogger log, [CosmosDB(databaseName: "todo-db", collectionName: "todos",
    ConnectionStringSetting = "CosmosDbConnectionString"
    )]IAsyncCollector<dynamic> asyncCollector)
        {
            string name = req.Query["name"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(false);
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name ??= data?.name;

            if (!string.IsNullOrEmpty(name))
            {
                await asyncCollector.AddAsync(new
                {
                    id = System.Guid.NewGuid().ToString(),
                    name
                }).ConfigureAwait(false);
            }

            string responseMessage = string.IsNullOrEmpty(name)
                ? "Invalid name."
                : $"Todo {name} created successfully";

            return new OkObjectResult(responseMessage);
        }
    }
}
