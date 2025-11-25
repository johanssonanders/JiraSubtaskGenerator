using Newtonsoft.Json;

namespace JiraSubtaskGenerator
{
    public sealed class JiraIssueType
    {
        [JsonProperty("id")]
        public string Id { get; set; } = default!;

        [JsonProperty("name")]
        public string Name { get; set; } = default!;

        // Jira returns: "subtask": true/false
        [JsonProperty("subtask")]
        public bool IsSubtask { get; set; }
    }
}