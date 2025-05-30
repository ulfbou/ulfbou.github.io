namespace Homepage.Common.Models
{
    public class TocEntry
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public int Level { get; set; } // e.g., 2 for h2, 3 for h3
    }
}
