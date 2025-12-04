using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace JiraSubtaskGenerator
{

  class Program
  {
    static async Task<int> Main(string[] args)
    {
      Console.OutputEncoding = Encoding.UTF8;
      var help = args.Contains("--help") || args.Contains("--readme");

      if (help)
      {
        ShowReadme();
        return 0;
      }

      var verbose = args.Contains("--verbose");

      using var loggerFactory = verbose
        ? LoggerFactory.Create(builder => builder.AddSimpleConsole())
        : LoggerFactory.Create(builder => { });
      var logger = loggerFactory.CreateLogger<Program>();

      JiraConfig jiraConfig;


      if (TryGetArgValue(args, "--config", out var configFilePath))
      {
        try
        {
          jiraConfig = ParseJiraConfigFile(configFilePath);
        }
        catch (Exception e)
        {
          Console.WriteLine($"Error when parsing {configFilePath}: {e.ToString}");
          return -1;
        }
      }
      else
      {
        var jiraUrl = Environment.GetEnvironmentVariable("JIRA_URL");
        var jiraEmail = Environment.GetEnvironmentVariable("JIRA_EMAIL");
        var jiraToken = Environment.GetEnvironmentVariable("JIRA_TOKEN");

        if (string.IsNullOrWhiteSpace(jiraUrl) || string.IsNullOrWhiteSpace(jiraEmail) || string.IsNullOrWhiteSpace(jiraToken))
        {
          Console.WriteLine("JIRA_URL, JIRA_EMAIL, and JIRA_TOKEN must be set as environment variables if not provided via --config.");
          return 1;
        }

        jiraConfig = new JiraConfig(jiraUrl, jiraEmail, jiraToken);
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

        foreach (var s in batch.Subtasks)
        {
          Console.WriteLine($"\t'{s.Title}' - {s.EstimatedHours}h");
        }

        if (!dryRun)
        {
          Console.Write($"Do you want to continue and add {batch.Subtasks.Count} subtasks to the parent {batch.ParentKey}? (y/n): ");
          var key = Console.ReadKey();
          Console.WriteLine();

          if (key.Key != ConsoleKey.Y)
          {
            Console.WriteLine("Aborting...");
            return 0;
          }

          var jira = new JiraClient(jiraConfig, logger);
          var subTaskTypes = await jira.GetSubtaskIssueTypesForProjectKeyAsync(batch.ProjectKey);
          foreach (var s in subTaskTypes)
          {
            logger.LogInformation($"Found subtask issuetype with name '{s.Name}' and id '{s.Id}'");
          }

          var subtaskType = subTaskTypes.FirstOrDefault();
          if (subtaskType == null)
          {
            Console.WriteLine("No subtask issue type found in the project, aborting");
            logger.LogError("No subtask issuetypes found in the project.");
            return 2;
          }

          Console.WriteLine("Please wait while creating subtasks");

          foreach (var s in batch.Subtasks)
          {
            var issueKey = await jira.CreateSubtaskAsync(batch.ProjectKey, subtaskType, batch.ParentKey, s);
            logger.LogInformation($"Created subtask: {issueKey} for '{s.Title}'");
            Console.Write(".");
          }

          Console.WriteLine();
          Console.WriteLine("Done with creating subtasks");
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

    public static JiraConfig ParseJiraConfigFile(string filePath)
    {
      if (!File.Exists(filePath))
        throw new FileNotFoundException("Config file not found", filePath);

      string? jiraUrl = null;
      string? jiraEmail = null;
      string? jiraToken = null;

      var lines = File.ReadAllLines(filePath);

      foreach (var line in lines)
      {
        if (string.IsNullOrWhiteSpace(line))
          continue;

        var parts = line.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
          continue;

        var key = parts[0];
        var value = parts[1];

        switch (key)
        {
          case "JIRA_URL":
            jiraUrl = value;
            break;

          case "JIRA_EMAIL":
            jiraEmail = value;
            break;

          case "JIRA_TOKEN":
            jiraToken = value;
            break;
        }
      }

      if (jiraUrl is null || jiraEmail is null || jiraToken is null)
        throw new InvalidDataException("Config file missing required fields");

      return new JiraConfig(jiraUrl, jiraEmail, jiraToken);
    }

    static void ShowReadme()
    {
      var md = LoadReadme();
      RenderMarkdown(md);
    }

    // Load the embedded README.md
    static string LoadReadme()
    {
      var assembly = Assembly.GetExecutingAssembly();

      var resourceName = assembly
          .GetManifestResourceNames()
          .Single(n => n.EndsWith("README.md", StringComparison.OrdinalIgnoreCase));

      using var stream = assembly.GetManifestResourceStream(resourceName)
          ?? throw new InvalidOperationException("Embedded README.md not found");

      using var reader = new StreamReader(stream, Encoding.UTF8);
      return reader.ReadToEnd();
    }

    // Very simple Markdown → console renderer
    static void RenderMarkdown(string md)
    {
      var doc = Markdown.Parse(md);

      foreach (var block in doc)
      {
        switch (block)
        {
          case Markdig.Syntax.HeadingBlock h:
            RenderHeading(h);
            break;

          case Markdig.Syntax.ParagraphBlock p:
            if (p.Inline != null)
            {
              Console.WriteLine(InlineToText(p.Inline));
              Console.WriteLine();
            }
            break;

          case Markdig.Syntax.ListBlock list:
            RenderList(list);
            break;

          case Markdig.Syntax.FencedCodeBlock code:
            RenderCodeBlock(code);
            break;
        }
      }
    }

    static void RenderHeading(Markdig.Syntax.HeadingBlock h)
    {
      if (h.Inline != null)
      {
        var text = InlineToText(h.Inline);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('#', h.Level) + " " + text);
        Console.ResetColor();
        Console.WriteLine();
      }
    }

    static void RenderList(Markdig.Syntax.ListBlock list)
    {
      foreach (var item in list)
      {
        var li = (Markdig.Syntax.ListItemBlock)item;

        // Find the first paragraph in the list item
        var paragraph = li.Descendants<Markdig.Syntax.ParagraphBlock>().FirstOrDefault();
        if (paragraph?.Inline != null)
        {
          var text = InlineToText(paragraph.Inline);
          Console.WriteLine("* " + text);
        }
      }

      Console.WriteLine();
    }

    static void RenderCodeBlock(Markdig.Syntax.FencedCodeBlock code)
    {
      Console.ForegroundColor = ConsoleColor.DarkYellow;
      Console.WriteLine("```");
      foreach (var line in code.Lines.Lines)
        Console.WriteLine(line.ToString());
      Console.WriteLine("```");
      Console.ResetColor();
      Console.WriteLine();
    }

    static string InlineToText(ContainerInline container)
    {
      var sb = new StringBuilder();

      for (var child = container?.FirstChild; child != null; child = child.NextSibling)
      {
        switch (child)
        {
          case LiteralInline lit:
            sb.Append(lit.Content.ToString());
            break;

          case CodeInline code:
            // Inline code: wrap with backticks to keep it readable
            sb.Append('`');
            sb.Append(code.Content);
            sb.Append('`');
            break;

          case EmphasisInline emph:
            // Just render the content inside emphasis for now
            sb.Append(InlineToText(emph));
            break;

          case LineBreakInline:
            sb.AppendLine();
            break;

          case ContainerInline nested:
            sb.Append(InlineToText(nested));
            break;

          default:
            // Fallback if some other inline type appears
            sb.Append(child.ToString());
            break;
        }
      }

      return sb.ToString();
    }
  }
}