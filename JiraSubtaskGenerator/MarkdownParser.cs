using System.Text;
using System.Text.RegularExpressions;

namespace JiraSubtaskGenerator
{
  public static class MarkdownParser
  {
    public static bool TryParse(string markdown, out JiraSubtaskBatch batch)
    {
      var lines = markdown.Split(new [] {"\r\n", "\n", "\r"}, StringSplitOptions.None);

      var headerPattern = new Regex(@"^@([A-Za-z0-9]+):([A-Za-z0-9\-]+)");
      var subtaskPattern = new Regex(@"^##\s*(.+?)\s*\|\|\s*(\d+)");

      var projectKey = string.Empty;
      var parentKey = string.Empty;
      var subtasks = new List<Subtask>();

      if (lines.Length > 0 && headerPattern.IsMatch(lines[0]))
      {
        var match = headerPattern.Match(lines[0]);
        projectKey = match.Groups[1].Value;
        parentKey = match.Groups[2].Value;
      }
      // TODO: Log and fail

      var description = new StringBuilder();
      var title = string.Empty;
      var estimatedHours = 0.0;

      for (var i = 1; i < lines.Length; i++)
      {
        var line = lines[i].Trim();

        if (subtaskPattern.IsMatch(line))
        {
          if (!string.IsNullOrEmpty(title))
          {
            var subtask = new Subtask(title, description.ToString(), estimatedHours);
            subtasks.Add(subtask);
          }

          var match = subtaskPattern.Match(line);
          title = match.Groups[1].Value.Trim();
          estimatedHours = double.Parse(match.Groups[2].Value);
          description.Clear();
        }
        else
        {
          description.AppendLine(line);
        }
      }

      if (!string.IsNullOrEmpty(title))
      {
        var subtask = new Subtask(title, description.ToString(), estimatedHours);
        subtasks.Add(subtask);
      }

      batch = !string.IsNullOrEmpty(projectKey) && !string.IsNullOrEmpty(parentKey)
        ? new JiraSubtaskBatch(projectKey, parentKey, subtasks)
        : new JiraSubtaskBatch(string.Empty, string.Empty, Enumerable.Empty<Subtask>());

      return !string.IsNullOrEmpty(batch.ProjectKey);
    }
  }
}