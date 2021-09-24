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
using System.Linq;
using System;

namespace TodoApi.Functions
{
    public class ApiHttpTrigger
    {
        private readonly CosmosClient _cosmosClient;
        public ApiHttpTrigger(CosmosClient client) => _cosmosClient = client;

        [FunctionName("PostTodo")]
        public async Task<IActionResult> PostTodo([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req)
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
                    Name = name,
                    Completed = false,
                    SubTasks = new List<Models.Task>()
                }).ConfigureAwait(false);
            }

            string responseMessage = string.IsNullOrEmpty(name)
                ? "Invalid name."
                : $"Todo {name} created successfully";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("FetchAllTodos")]
        public async Task<IActionResult> FetchAllTodos([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos")] HttpRequest req)
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

        [FunctionName("UpdateTodo")]
        public async Task<IActionResult> UpdateTodo([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(false);
            TodoItem requestData = JsonConvert.DeserializeObject<TodoItem>(requestBody);

            if (requestData == null || requestData.Id == null)
            {
                return new OkObjectResult("Please provide a valid Todo id.");
            }

            var container = _cosmosClient.GetContainer("todo-db", "todos");
            try
            {
                TodoItem fetchedTodo = await container.ReadItemAsync<TodoItem>(requestData.Id, new PartitionKey(requestData.Id)).ConfigureAwait(false);
                await container.ReplaceItemAsync(requestData, requestData.Id, new PartitionKey(requestData.Id)).ConfigureAwait(false);
            }
            catch
            {
                return new OkObjectResult("Could not find existing todo.");
            }

            string responseMessage = JsonConvert.SerializeObject(requestData);
            return new OkObjectResult(responseMessage);
        }
    }
}
