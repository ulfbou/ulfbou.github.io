using System.Text.Json.Serialization;

namespace Homepage.Common.Models
{
    /// <summary>Represents a project entry for display in the portfolio.</summary>
    public class ProjectEntry
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? RepoUrl { get; set; }
        public string? LiveDemoUrl { get; set; }
        public int Year { get; set; }
        public string ProjectType { get; set; } = string.Empty;
        public List<TechStackItem> TechStack { get; set; } = new();

        [JsonIgnore]
        public bool IsPinned { get; set; } = false;
    }
}
