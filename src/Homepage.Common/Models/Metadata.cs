public class Metadata
{
    public int Id { get; set; }
    public ICollection<PostMetadata> Posts { get; set; } = new List<PostMetadata>();
}
