using Bogus;

namespace Homepage.Common.Models;

public class ContentItem
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public string Title { get; set; }
    public string Summary { get; set; }
    public string Url { get; set; }
    public string Date { get; set; }
    public ICollection<string> Categories { get; set; } = new List<string>();
    public ICollection<string> Tags { get; set; } = new List<string>();
    public ICollection<string>? Keywords { get; set; } = new List<string>();
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public static ContentItem Create()
    {
        Faker faker = new Faker();

        return new()
        {
            Title = faker.Lorem.Sentence(),
            Summary = faker.Lorem.Paragraph(),
            Url = faker.Internet.Url(),
            Date = DateTime.Now.ToString("yyyy")
        };
    }

    public static IEnumerable<ContentItem> GetAll(int size = 10)
    {
        for (int i = 0; i < size; i++)
        {
            yield return Create();
        }
    }
}