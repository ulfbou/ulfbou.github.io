public class ContentMetadata
{
    public string ContentId { get; set; }
    public string Section { get; set; }
    public List<HierarchicalCategory> Categories { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public DateTime PublishDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public string Author { get; set; }
    public string TargetAudience { get; set; }
    public string ContentFormat { get; set; }

    public class HierarchicalCategory
    {
        public string Name { get; set; }
        public List<HierarchicalCategory> Children { get; set; } = new();
        public int Weight { get; set; }
    }
}
