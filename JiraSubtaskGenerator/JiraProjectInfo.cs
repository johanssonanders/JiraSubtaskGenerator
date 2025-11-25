using Newtonsoft.Json;

namespace JiraSubtaskGenerator
{
    public sealed class JiraProjectInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; } = default!;
    }
}