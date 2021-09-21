using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;
using TodoApi.Models;

namespace TodoApi.Functions
{
    public class ApiHttpTrigger
    {
        private readonly CosmosClient _cosmosClient;
        public ApiHttpTrigger(CosmosClient client) => _cosmosClient = client;

        [FunctionName("PostTodo")]
        public async Task<IActionResult> PostTodo([HttpTrigger(AuthorizationLevel.Function, "post", Route = "todo")] HttpRequest req)
        {
            string name = req.Query["name"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(false);
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name ??= data?.name;

            if (!string.IsNullOrEmpty(name))
            {
                await _cosmosClient.GetContainer("todo-db", "todos").CreateItemAsync(new TodoItem
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
        public async Task<IActionResult> FetchAllTodos([HttpTrigger(AuthorizationLevel.Function, "get", Route = "todos")] HttpRequest req)
        {
            var iterator = _cosmosClient.GetContainer("todo-db", "todos").GetItemQueryIterator<TodoItem>(new QueryDefinition("SELECT * FROM todos"));
            var results = new List<TodoItem>();

            while (iterator.HasMoreResults)
            {
                var result = await iterator.ReadNextAsync().ConfigureAwait(false);
                results.AddRange(result.Resource);
            }

            return new OkObjectResult(results);
        }
    }
}
