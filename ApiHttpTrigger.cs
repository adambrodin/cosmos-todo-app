using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CosmosApi.Models;
using System.Collections.Generic;

namespace CosmosApi.Functions
{
    public static class ApiHttpTrigger
    {
        [FunctionName("PostTodo")]
        public static async Task<IActionResult> PostTodo(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "todo")] HttpRequest req,
            [CosmosDB(databaseName: "todo-db", collectionName: "todos",
    ConnectionStringSetting = "CosmosDbConnectionString"
    )]IAsyncCollector<dynamic> asyncCollector)
        {
            string name = req.Query["name"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(false);
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name ??= data?.name;

            if (!string.IsNullOrEmpty(name))
            {
                await asyncCollector.AddAsync(new CosmosApi.Models.TodoItem
                {
                    Id = System.Guid.NewGuid().ToString(),
                    Name = name
                }).ConfigureAwait(false);
            }

            string responseMessage = string.IsNullOrEmpty(name)
                ? "Invalid name."
                : $"Todo {name} created successfully";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("FetchAllTodos")]
        public static IActionResult FetchAllTodos(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "todos")] HttpRequest req,
    [CosmosDB(databaseName: "todo-db", collectionName: "todos",
    ConnectionStringSetting = "CosmosDbConnectionString"
    , SqlQuery ="SELECT * FROM todos")] IEnumerable<TodoItem> fetchedTodos, ILogger logger)
        {
            return new OkObjectResult(fetchedTodos);
        }
    }
}
