namespace Homepage.Common.Models
{
    /// <summary>
    /// Model for caching ContentMetadata along with its HTTP Last-Modified timestamp.
    /// </summary>
    public class ContentMetadataCache
    {
        public List<ContentMetadata> Metadata { get; set; } = new List<ContentMetadata>();
        public DateTimeOffset LastModified { get; set; }
        public string? ETag { get; set; }
    }
}
