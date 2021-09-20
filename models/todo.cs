using System;
using Newtonsoft.Json;

namespace CosmosApi.Models
{
    public class TodoItem
    {
        [JsonProperty("id")]
        public String Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

    }
}