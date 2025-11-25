namespace JiraSubtaskGenerator
{
  public class JiraSubtaskBatch
  {

    public JiraSubtaskBatch(string projectKey, string parentKey, IEnumerable<Subtask> subtasks)
    {
      this.ProjectKey = projectKey;
      this.ParentKey = parentKey;
      this.Subtasks = new List<Subtask>(subtasks);
    }
    public string ProjectKey { get; }

    public string ParentKey { get; }

    public IReadOnlyList<Subtask> Subtasks { get; }
  }
}