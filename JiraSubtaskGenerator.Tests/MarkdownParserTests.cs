using JiraSubtaskGenerator;
using Xunit;

namespace JiraSubtaskGenerator.Tests
{
  public class MarkdownParserTests
  {
    [Fact]
    public void ParsesHeaderAndSubtasksCorrectly()
    {
      var markdown = "@PROJ1:STORY-123\r\n## Task One || 2\r\nSome test\r\non multiple lines\r\n## Task Two || 4";
      var result = MarkdownParser.TryParse(markdown, out var batch);

      Assert.True(result);
      Assert.Equal("PROJ1", batch.ProjectKey);
      Assert.Equal("STORY-123", batch.ParentKey);
      Assert.Equal(2, batch.Subtasks.Count);

      var subtask1 = batch.Subtasks[0];
      Assert.Equal("Task One", subtask1.Title);
      Assert.Equal(2, subtask1.EstimatedHours);
      Assert.Equal("Some test\r\non multiple lines\r\n", subtask1.Description);

      var subtask2 = batch.Subtasks[1];
      Assert.Equal("Task Two", subtask2.Title);
      Assert.Equal(4, subtask2.EstimatedHours);
    }

    [Fact]
    public void ReturnsEmptyListWhenNoSubtasks()
    {
      var markdown = "@PROJ1:STORY-123";
      var result = MarkdownParser.TryParse(markdown, out var batch);

      Assert.True(result);
      Assert.Equal("PROJ1", batch.ProjectKey);
      Assert.Equal("STORY-123", batch.ParentKey);
      Assert.Empty(batch.Subtasks);
    }
  }
}