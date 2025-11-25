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
    }

    public async Task<string> CreateSubtaskAsync(string projectKey, string parentKey, Subtask subtask)
    {
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
            name = "Deluppgift" // TOOD: Check with this is in Swedish on my jira project. Do I need to have this configurable? See https://developer.atlassian.com/server/jira/platform/jira-rest-api-examples/#creating-a-sub-task
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
  }
}