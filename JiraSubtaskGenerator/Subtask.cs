namespace JiraSubtaskGenerator
{
  public class Subtask
  {
    public Subtask(string title, string description, double estimatedHours)
    {
      this.Title = title;
      this.Description = description;
      this.EstimatedHours = estimatedHours;
    }

    public string Title
    {
      get;
    }

    public string Description
    {
      get;
    }

    public double EstimatedHours
    {
      get;
    }
  }

}