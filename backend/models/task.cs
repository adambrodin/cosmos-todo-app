using Newtonsoft.Json;

namespace TodoApi.Models
{
    public class Task
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}