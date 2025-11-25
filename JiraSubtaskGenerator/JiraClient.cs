using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JiraSubtaskGenerator
{
  public class JiraClient
  {
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public JiraClient(string baseUrl, string email, string apiToken, ILogger logger)
    {
      _logger = logger;
      _httpClient = new HttpClient
      {
        BaseAddress = new Uri(baseUrl)
      };

      var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{email}:{apiToken}"));
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
      _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<IReadOnlyList<JiraIssueType>> GetSubtaskIssueTypesForProjectKeyAsync(string projectKey, CancellationToken cancellationToken = default)
    {
      var projectId = await this.GetProjectIdAsync(projectKey, cancellationToken);
      return await this.GetSubtaskIssueTypesForProjectAsync(projectId, cancellationToken);
    }

    private async Task<string> GetProjectIdAsync(string projectKey, CancellationToken cancellationToken = default)
    {
      var url = $"/rest/api/3/project/{Uri.EscapeDataString(projectKey)}";

      using var response = await _httpClient.GetAsync(url, cancellationToken);
      response.EnsureSuccessStatusCode();

      var json = await response.Content.ReadAsStringAsync(cancellationToken);

      var project = JsonConvert.DeserializeObject<JiraProjectInfo>(json);
      return project?.Id
          ?? throw new InvalidOperationException($"Project '{projectKey}' not found.");
    }

    public async Task<string> CreateSubtaskAsync(string projectKey, JiraIssueType subtaskType, string parentKey, Subtask subtask)
    {
      ArgumentNullException.ThrowIfNull(subtaskType);

      if (!subtaskType.IsSubtask)
      {
        throw new InvalidOperationException("subtaskType must be a subtask issue type");
      }

      var payload = new
      {
        fields = new
        {
          project = new
          {
            key = projectKey
          },
          parent = new
          {
            key = parentKey
          },
          summary = subtask.Title,
          description = new
          {
            type = "doc",
            version = 1,
            content = new[] {
              new {
                type = "paragraph",
                content = new[] {
                  new {
                    type = "text",
                    text = subtask.Description,
                  }
                }
              }
            }
          },
          issuetype = new
          {
            id = subtaskType.Id
          },
          timetracking = new
          {
            originalEstimate = $"{subtask.EstimatedHours}h"
          }
        }
      };

      var json = JsonConvert.SerializeObject(payload);
      var content = new StringContent(json, Encoding.UTF8, "application/json");

      var response = await _httpClient.PostAsync("/rest/api/3/issue", content);

      if (!response.IsSuccessStatusCode)
      {
        var err = await response.Content.ReadAsStringAsync();
        _logger.LogError($"Jira API Error: {response.StatusCode} - {err}");
        throw new Exception("Failed to create Jira subtask.");
      }

      var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(await response.Content.ReadAsStringAsync());
      return result?["key"]?.ToString() ?? "Unknown";
    }


    private async Task<IReadOnlyList<JiraIssueType>> GetSubtaskIssueTypesForProjectAsync(string projectId, CancellationToken cancellationToken = default)
    {
      var all = await this.GetIssueTypesForProjectAsync(projectId, cancellationToken);
      return all.Where(t => t.IsSubtask).ToList();
    }

    private async Task<IReadOnlyList<JiraIssueType>> GetIssueTypesForProjectAsync(string projectId, CancellationToken cancellationToken = default)
    {
      var url = $"/rest/api/3/issuetype/project?projectId={Uri.EscapeDataString(projectId)}";

      using var response = await _httpClient.GetAsync(url, cancellationToken);
      response.EnsureSuccessStatusCode();

      var json = await response.Content.ReadAsStringAsync(cancellationToken);

      // Use Newtonsoft to deserialize the JSON array directly
      var issueTypes = JsonConvert.DeserializeObject<List<JiraIssueType>>(json);

      return issueTypes ?? new List<JiraIssueType>();
    }
  }
}