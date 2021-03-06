using System.Collections.Generic;
using Newtonsoft.Json;

namespace TodoApi.Models
{
    public class TodoItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("completed")]
        public bool Completed { get; set; }

        [JsonProperty("subtasks")]
        public List<Task> SubTasks { get; set; }
    }
}