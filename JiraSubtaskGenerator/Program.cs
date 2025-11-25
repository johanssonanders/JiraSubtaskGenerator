using Microsoft.Extensions.Logging;

namespace JiraSubtaskGenerator
{

  class Program
  {
    static async Task<int> Main(string[] args)
    {
      using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

      var logger = loggerFactory.CreateLogger<Program>();

      var jiraUrl = Environment.GetEnvironmentVariable("JIRA_URL");
      var jiraEmail = Environment.GetEnvironmentVariable("JIRA_EMAIL");
      var jiraToken = Environment.GetEnvironmentVariable("JIRA_TOKEN");

      if (string.IsNullOrWhiteSpace(jiraUrl) ||
          string.IsNullOrWhiteSpace(jiraEmail) ||
          string.IsNullOrWhiteSpace(jiraToken))
      {
        Console.WriteLine("JIRA_URL, JIRA_EMAIL, and JIRA_TOKEN must be set as environment variables.");
        return 1;
      }

      var dryRun = args.Contains("--dry-run");

      if (!TryGetArgValue(args, "--file", out var filePath) || !File.Exists(filePath))
      {
        logger.LogError("Valid markdown file path is required. Use --file [path]");
        return 1;
      }

      try
      {
        var markdown = File.ReadAllText(filePath);
        if (!MarkdownParser.TryParse(markdown, out var batch))
        {
          logger.LogError("Invalid markdown, parsing failed");
          return 1;
        }

        logger.LogInformation($"Project: {batch.ProjectKey}, Parent: {batch.ParentKey}");
        logger.LogInformation($"Found {batch.Subtasks.Count} subtasks.");

        if (dryRun)
        {
          foreach (var s in batch.Subtasks)
          {
            Console.WriteLine($"[Dry Run] Would create subtask: '{s.Title}' - {s.EstimatedHours}h");
          }
        }
        else
        {
          var jira = new JiraClient(jiraUrl, jiraEmail, jiraToken, logger);
          var subTaskTypes = await jira.GetSubtaskIssueTypesForProjectKeyAsync(batch.ProjectKey);
          foreach (var s in subTaskTypes)
          {
            logger.LogInformation($"Found subtask issuetype with name '{s.Name}' and id '{s.Id}'");
          }

          var subtaskType = subTaskTypes.FirstOrDefault();
          if (subtaskType == null)
          {
            logger.LogError("No subtask issuetypes found in the project.");
            return 2;
          }

          foreach (var s in batch.Subtasks)
          {
            var issueKey = await jira.CreateSubtaskAsync(batch.ProjectKey, subtaskType , batch.ParentKey, s);
            logger.LogInformation($"Created subtask: {issueKey} for '{s.Title}'");
          }
        }
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Unexpected error occurred.");
        return 3;
      }

      return 0;
    }

    static bool TryGetArgValue(string[] args, string key, out string value)
    {
      var index = Array.IndexOf(args, key);
      value = (index >= 0 && index < args.Length - 1)
        ? args[index + 1] 
        : string.Empty;

      return !string.IsNullOrEmpty(value);
    }
  }
}