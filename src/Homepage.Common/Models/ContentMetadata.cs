using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Homepage.Common.Models
{
    public class ContentMetadata
    {
        [JsonPropertyName("slug")]
        [Required]
        public string Slug { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        [Required]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("keywords")]
        public List<string> Keywords { get; set; } = new();

        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; } = new();

        [JsonPropertyName("publishDate")]
        public DateTime PublishDate { get; set; } = DateTime.MinValue;

        [JsonPropertyName("targetAudiences")]
        public List<string> TargetAudiences { get; set; } = new();

        [JsonPropertyName("contentPath")]
        [Required]
        public string ContentPath { get; set; } = string.Empty;

        [JsonPropertyName("author")]
        public string? Author { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("featuredImage")]
        public string? FeaturedImage { get; set; }

        [JsonPropertyName("isFeatured")]
        public bool IsFeatured { get; set; } = false;

        public static ContentMetadata CreateDummy(int index = 0)
        {
            var faker = new Bogus.Faker();
            return new()
            {
                Slug = faker.Lorem.Slug(),
                Title = faker.Lorem.Sentence(5),
                Description = faker.Lorem.Paragraph(2),
                Tags = faker.Make(faker.Random.Int(1, 3), () => faker.Lorem.Word()).ToList(),
                Categories = faker.Make(faker.Random.Int(1, 2), () => faker.Commerce.Categories(1)[0]).ToList(),
                PublishDate = faker.Date.Past(2),
                TargetAudiences = faker.PickRandom(new List<string> { "developer", "techlead", "recruiter" }, faker.Random.Int(1, 2)).ToList(),
                ContentPath = $"{faker.Lorem.Slug()}.md",
                Author = "Ulf Bou",
                Version = "1.0",
                FeaturedImage = faker.Image.PicsumUrl(800, 400),
                IsFeatured = faker.Random.Bool()
            };
        }
    }
}
