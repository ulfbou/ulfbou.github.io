using System.ComponentModel.DataAnnotations;

public class PostMetadata
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public int Id { get; set; }

    [Required]
    public string Title { get; set; }
    [Required]
    public string Summary { get; set; }
    [Required]
    [Url]
    public string Url { get; set; }
    public DateTime Date { get; set; }
    public ICollection<string> Categories { get; set; } = new List<string>();
    public ICollection<string> Tags { get; set; } = new List<string>();
    public ICollection<string> Keywords { get; set; } = new List<string>();
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}