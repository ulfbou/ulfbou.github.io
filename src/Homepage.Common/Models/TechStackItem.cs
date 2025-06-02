namespace Homepage.Common.Models
{
    /// <summary>
    /// Represents a single technology item within a project's tech stack.
    /// </summary>
    public class TechStackItem
    {
        public string Name { get; set; } = string.Empty;
        public string? IconCssClass { get; set; }
        public string? Tooltip { get; set; }
    }
}
