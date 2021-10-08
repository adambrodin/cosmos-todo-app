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
            TodoItem requestTodo = JsonConvert.DeserializeObject<TodoItem>(requestBody);

            if (requestTodo == null || requestTodo.Id == null)
            {
                return new OkObjectResult("Please provide a valid Todo id.");
            }

            try
            {
                var container = _cosmosClient.GetContainer("todo-db", "todos");
                TodoItem fetchedTodo = await container.ReadItemAsync<TodoItem>(requestTodo.Id, new PartitionKey(requestTodo.Id)).ConfigureAwait(false);
                await container.ReplaceItemAsync(requestTodo, requestTodo.Id, new PartitionKey(requestTodo.Id)).ConfigureAwait(false);
            }
            catch
            {
                return new OkObjectResult("Could not find existing todo.");
            }

            string responseMessage = JsonConvert.SerializeObject(requestTodo);
            return new OkObjectResult(responseMessage);
        }

        [FunctionName("DeleteTodo")]
        public async Task<ActionResult> DeleteTodo([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(false);
            dynamic requestData = JsonConvert.DeserializeObject(requestBody);
            string todoId = requestData.id;

            if (todoId == null)
            {
                return new OkObjectResult("Please provide a valid Todo id.");
            }

            try
            {
                var container = _cosmosClient.GetContainer("todo-db", "todos");
                await container.DeleteItemAsync<TodoItem>(todoId, new PartitionKey(todoId)).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                return new OkObjectResult("Could not find existing todo. " + e.Message);
            }

            string responseMessage = $"Deleted todo with id {requestData.id}";
            return new OkObjectResult(responseMessage);
        }
        
        [FunctionName("LoaderIO")]
        public async Task<ActionResult> LoaderIO([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "loaderio-1803404b4626322198fae5e5afa38ffb")] HttpRequest req)
        {
            return new OkObjectResult("loaderio-1803404b4626322198fae5e5afa38ffb");
        }
    }
}
