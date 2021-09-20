using System;
using Newtonsoft.Json;

namespace TodoApi.Models
{
    public class TodoItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}