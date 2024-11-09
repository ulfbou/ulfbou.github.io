namespace Homepage.Common.Models;

public class ProjectModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> Technologies { get; set; }
    public string GitHubLink { get; set; }

    public IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            Name = "Project";
            yield return "Name is required";
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            Description = "Description";
            yield return "Description is required";
        }

        if (Technologies == null || Technologies.Count == 0)
        {
            Technologies = new List<string> { "C#", "ASP.NET Core", "Blazor" };
            yield return "At least one Technology is required";
        }

        if (string.IsNullOrWhiteSpace(GitHubLink))
        {
            GitHubLink = "https://github.com/ulfbou/ZentientFramework";
            yield return "GitHubLink is required";
        }
    }
}