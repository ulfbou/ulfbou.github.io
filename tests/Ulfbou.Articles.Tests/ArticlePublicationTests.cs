using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;

public sealed class ArticlePublicationTests
{
    [Fact]
    public void CandidateProducesContractDefinedEmptyPublication()
    {
        using var fixture = new Fixture();
        fixture.WriteArticle("candidate", approved: false);

        fixture.Generate();

        fixture.AssertEmptyPublication();
        Assert.Contains("No articles are currently published.", fixture.Read("articles/index.html"));
        Assert.Contains("<updated>1970-01-01T00:00:00Z</updated>", fixture.Read("articles/feed.xml"));
    }

    [Fact]
    public void UnapprovedPublishedArticleFailsWithoutChangingOutputs()
    {
        using var fixture = new Fixture();
        fixture.WriteArticle("candidate", approved: false);
        fixture.Generate();
        var before = fixture.OutputDigest();
        fixture.WriteArticle("published", approved: false);

        var error = Assert.Throws<InvalidDataException>(() => fixture.Generate());

        Assert.Contains("ARTICLE009", error.Message);
        Assert.Equal(before, fixture.OutputDigest());
    }

    [Theory]
    [InlineData("not-a-date")]
    [InlineData("YYYY-MM-DD")]
    public void InvalidOrPlaceholderApprovalDateFails(string approvalDate)
    {
        using var fixture = new Fixture();
        fixture.WriteArticle("published", approved: true, approvalDate: approvalDate);

        var error = Assert.Throws<InvalidDataException>(() => fixture.Generate());

        Assert.Contains("approvedDate", error.Message);
    }

    [Fact]
    public void ApprovedArticleProducesEveryPublicRepresentation()
    {
        using var fixture = new Fixture();
        fixture.WriteArticle("published", approved: true);

        fixture.Generate();

        Assert.True(fixture.Exists("articles/bias/index.html"));
        Assert.Contains("/articles/bias/", fixture.Read("articles/index.html"));
        Assert.Contains("/articles/bias/", fixture.Read("articles/feed.xml"));
        Assert.Contains("/articles/bias/", fixture.Read("sitemap.xml"));
        using var projection = JsonDocument.Parse(fixture.Read("data/generated/articles.json"));
        var published = Assert.Single(projection.RootElement.GetProperty("articles").EnumerateArray().ToArray());
        Assert.Equal("published", published.GetProperty("status").GetString());
    }

    [Fact]
    public void WithdrawalRemovesAllPublicRepresentations()
    {
        using var fixture = new Fixture();
        fixture.WriteArticle("published", approved: true);
        fixture.Generate();
        fixture.WriteArticle("withdrawn", approved: false);

        fixture.Generate();

        fixture.AssertEmptyPublication();
    }

    [Fact]
    public void UnknownJsonPropertyFailsBeforeOutputReplacement()
    {
        using var fixture = new Fixture();
        fixture.WriteArticle("candidate", approved: false, unknownProperty: true);

        var error = Assert.Throws<JsonException>(() => fixture.Generate());

        Assert.Contains("unexpectedProperty", error.Message);
        Assert.False(fixture.Exists("articles"));
    }

    [Fact]
    public void EscapingBodyPathFails()
    {
        using var fixture = new Fixture();
        fixture.WriteArticle("candidate", approved: false, bodyPath: "../outside.md");

        var error = Assert.Throws<InvalidDataException>(() => fixture.Generate());

        Assert.Contains("ARTICLE012", error.Message);
    }

    [Fact]
    public void MissingPublishedSocialAssetFails()
    {
        using var fixture = new Fixture(createSocialAsset: false);
        fixture.WriteArticle("published", approved: true);

        var error = Assert.Throws<InvalidDataException>(() => fixture.Generate());

        Assert.Contains("ARTICLE010", error.Message);
    }

    [Fact]
    public void InjectedReplacementFailureRestoresCompletePreviousOutputSet()
    {
        using var fixture = new Fixture();
        fixture.WriteArticle("published", approved: true, title: "First title");
        fixture.Generate();
        var before = fixture.OutputDigest();
        fixture.WriteArticle("published", approved: true, title: "Changed title");

        var error = Assert.Throws<IOException>(() => fixture.Generate(failAfterReplacement: 1));

        Assert.Contains("ARTICLE025", error.Message);
        Assert.Equal(before, fixture.OutputDigest());
        Assert.Contains("First title", fixture.Read("articles/bias/index.html"));
    }

    [Fact]
    public void PerRecordSchemaVersionIsRejectedByClosedSourceContract()
    {
        using var fixture = new Fixture();
        fixture.WriteArticle("candidate", approved: false, perRecordSchemaVersion: true);

        Assert.Throws<JsonException>(() => fixture.Generate());
    }

    [Fact]
    public void DuplicateArticleIdsFailBeforeOutputReplacement()
    {
        using var fixture = new Fixture();
        fixture.WriteArticles(Fixture.Article("article-bias", "bias"), Fixture.Article("article-bias", "other"));

        var error = Assert.Throws<InvalidDataException>(() => fixture.Generate());

        Assert.Contains("duplicate article id", error.Message);
        Assert.False(fixture.Exists("articles"));
    }

    [Fact]
    public void DuplicateCanonicalRoutesFailBeforeOutputReplacement()
    {
        using var fixture = new Fixture();
        fixture.WriteArticles(Fixture.Article("article-bias", "bias"), Fixture.Article("article-other", "bias"));

        var error = Assert.Throws<InvalidDataException>(() => fixture.Generate());

        Assert.Contains("duplicate canonical route", error.Message);
        Assert.False(fixture.Exists("articles"));
    }

    [Fact]
    public void InvalidArticleOutputRootIsRejected()
    {
        using var fixture = new Fixture();
        fixture.WriteArticle("candidate", approved: false);

        var error = Assert.Throws<InvalidDataException>(() =>
            ArticlePublisher.Generate(fixture.SourceRoot, Path.Combine(fixture.WebRoot, "work"), fixture.WebRoot));

        Assert.Contains("ARTICLE021", error.Message);
    }

    [Fact]
    public void GenerationWithdrawalAndReplacementFailurePreserveSiblingRoutes()
    {
        using var fixture = new Fixture();
        fixture.WriteSiblingFiles();
        var siblings = fixture.SiblingDigest();
        fixture.WriteArticle("published", approved: true);
        fixture.Generate();
        Assert.Equal(siblings, fixture.SiblingDigest());

        fixture.WriteArticle("withdrawn", approved: false);
        fixture.Generate();
        Assert.Equal(siblings, fixture.SiblingDigest());

        fixture.WriteArticle("published", approved: true, title: "Changed title");
        Assert.Throws<IOException>(() => fixture.Generate(failAfterReplacement: 1));
        Assert.Equal(siblings, fixture.SiblingDigest());
    }

    [Fact]
    public void UnknownSeriesStatusFails()
    {
        using var fixture = new Fixture();
        fixture.WriteArticle("candidate", approved: false);
        File.WriteAllText(
            Path.Combine(fixture.SourceRoot, "series.json"),
            "{\"schemaVersion\":\"1.0\",\"series\":[{\"id\":\"confidently-wrong\",\"title\":\"Confidently Wrong\",\"description\":\"Series\",\"status\":\"unknown\"}]}\n",
            new UTF8Encoding(false));

        var error = Assert.Throws<InvalidDataException>(() => fixture.Generate());

        Assert.Contains("ARTICLE026", error.Message);
    }

    private sealed class Fixture : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "ulfbou-articles-tests", Guid.NewGuid().ToString("N"));
        public string SourceRoot => Path.Combine(root, "content", "articles");
        public string WebRoot => Path.Combine(root, "wwwroot");
        public string ArticlesRoot => Path.Combine(WebRoot, "articles");

        public Fixture(bool createSocialAsset = true)
        {
            Directory.CreateDirectory(Path.Combine(SourceRoot, "bodies"));
            Directory.CreateDirectory(WebRoot);
            File.WriteAllText(Path.Combine(SourceRoot, "bodies", "bias.md"), "# Bias\n\nComplete body.\n", new UTF8Encoding(false));
            File.WriteAllText(
                Path.Combine(SourceRoot, "series.json"),
                "{\n  \"schemaVersion\": \"1.0\",\n  \"series\": [{\"id\":\"confidently-wrong\",\"title\":\"Confidently Wrong\",\"description\":\"Series\",\"status\":\"ongoing\"}]\n}\n",
                new UTF8Encoding(false));
            if (createSocialAsset)
            {
                var path = Path.Combine(WebRoot, "assets", "social", "articles", "bias.svg");
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, "<svg xmlns=\"http://www.w3.org/2000/svg\"/>\n", new UTF8Encoding(false));
            }
        }

        public void WriteArticle(
            string status,
            bool approved,
            string approvalDate = "2026-11-20",
            string bodyPath = "bodies/bias.md",
            bool unknownProperty = false,
            bool perRecordSchemaVersion = false,
            string title = "Bias")
        {
            var article = new Dictionary<string, object?>
            {
                ["id"] = "article-bias",
                ["slug"] = "bias",
                ["status"] = status,
                ["title"] = title,
                ["deck"] = "Deck",
                ["description"] = "Description",
                ["author"] = "Ulf Bourelius",
                ["language"] = "en",
                ["publishedDate"] = status == "published" ? "2026-11-20" : null,
                ["modifiedDate"] = status == "published" ? "2026-11-20" : null,
                ["readingMinutes"] = 8,
                ["bodyPath"] = bodyPath,
                ["seriesId"] = "confidently-wrong",
                ["seriesPart"] = 1,
                ["socialImage"] = "/assets/social/articles/bias.svg",
                ["canonicalPath"] = "/articles/bias/",
                ["editorialApproval"] = new Dictionary<string, object?>
                {
                    ["approved"] = approved,
                    ["approvedBy"] = approved ? "Ulf Bourelius" : null,
                    ["approvedDate"] = approved ? approvalDate : null
                }
            };
            if (unknownProperty) article["unexpectedProperty"] = true;
            if (perRecordSchemaVersion) article["schemaVersion"] = "1.0";
            var registry = new Dictionary<string, object?> { ["schemaVersion"] = "1.0", ["articles"] = new[] { article } };
            File.WriteAllText(
                Path.Combine(SourceRoot, "articles.json"),
                JsonSerializer.Serialize(registry, new JsonSerializerOptions { WriteIndented = true }) + "\n",
                new UTF8Encoding(false));
        }

        public static Dictionary<string, object?> Article(string id, string slug) => new()
        {
            ["id"] = id, ["slug"] = slug, ["status"] = "candidate", ["title"] = id,
            ["deck"] = "Deck", ["description"] = "Description", ["author"] = "Ulf Bourelius",
            ["language"] = "en", ["publishedDate"] = null, ["modifiedDate"] = null,
            ["readingMinutes"] = 8, ["bodyPath"] = "bodies/bias.md",
            ["seriesId"] = "confidently-wrong", ["seriesPart"] = 1,
            ["socialImage"] = "/assets/social/articles/bias.svg",
            ["canonicalPath"] = $"/articles/{slug}/",
            ["editorialApproval"] = new Dictionary<string, object?>
            { ["approved"] = false, ["approvedBy"] = null, ["approvedDate"] = null }
        };

        public void WriteArticles(params Dictionary<string, object?>[] articles)
        {
            var registry = new Dictionary<string, object?>
            { ["schemaVersion"] = "1.0", ["articles"] = articles };
            File.WriteAllText(
                Path.Combine(SourceRoot, "articles.json"),
                JsonSerializer.Serialize(registry, new JsonSerializerOptions { WriteIndented = true }) + "\n",
                new UTF8Encoding(false));
        }

        public void WriteSiblingFiles()
        {
            foreach (var relative in new[] { "index.html", "work/index.html", "aws/index.html", "data/generated/portfolio.json" })
            {
                var path = Path.Combine(WebRoot, relative.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, $"sibling:{relative}\n", new UTF8Encoding(false));
            }
        }

        public string SiblingDigest()
        {
            using var combined = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            foreach (var relative in new[] { "index.html", "work/index.html", "aws/index.html", "data/generated/portfolio.json" })
            {
                combined.AppendData(Encoding.UTF8.GetBytes(relative));
                combined.AppendData(File.ReadAllBytes(Path.Combine(WebRoot, relative.Replace('/', Path.DirectorySeparatorChar))));
            }
            return Convert.ToHexStringLower(combined.GetHashAndReset());
        }

        public void Generate(int? failAfterReplacement = null) =>
            ArticlePublisher.Generate(SourceRoot, ArticlesRoot, WebRoot, failAfterReplacement);

        public bool Exists(string relative) =>
            File.Exists(Path.Combine(WebRoot, relative.Replace('/', Path.DirectorySeparatorChar)))
            || Directory.Exists(Path.Combine(WebRoot, relative.Replace('/', Path.DirectorySeparatorChar)));

        public string Read(string relative) =>
            File.ReadAllText(Path.Combine(WebRoot, relative.Replace('/', Path.DirectorySeparatorChar)), Encoding.UTF8);

        public void AssertEmptyPublication()
        {
            Assert.False(Exists("articles/bias/index.html"));
            Assert.DoesNotContain("/articles/bias/", Read("articles/index.html"));
            Assert.DoesNotContain("/articles/bias/", Read("articles/feed.xml"));
            Assert.DoesNotContain("/articles/bias/", Read("sitemap.xml"));
            using var projection = JsonDocument.Parse(Read("data/generated/articles.json"));
            Assert.Empty(projection.RootElement.GetProperty("articles").EnumerateArray());
        }

        public string OutputDigest()
        {
            var paths = new[] { "articles", "data/generated/articles.json", "sitemap.xml" };
            using var combined = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            foreach (var relative in paths)
            {
                var path = Path.Combine(WebRoot, relative.Replace('/', Path.DirectorySeparatorChar));
                IEnumerable<string> files = Directory.Exists(path)
                    ? Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).OrderBy(value => value, StringComparer.Ordinal)
                    : new[] { path };
                foreach (var file in files)
                {
                    combined.AppendData(Encoding.UTF8.GetBytes(Path.GetRelativePath(WebRoot, file).Replace('\\', '/')));
                    combined.AppendData(File.ReadAllBytes(file));
                }
            }
            return Convert.ToHexStringLower(combined.GetHashAndReset());
        }

        public void Dispose()
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }
}
