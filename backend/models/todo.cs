using System;
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

        [JsonProperty("tasks")]
        public List<Task> Tasks { get; set; }
    }
}