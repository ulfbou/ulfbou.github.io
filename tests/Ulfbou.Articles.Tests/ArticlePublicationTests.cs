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
    public void PublishedArticleContainsRequiredSeoAndSocialMetadata()
    {
        using var fixture = new Fixture();
        fixture.WriteArticle("published", approved: true);

        fixture.Generate();

        var html = fixture.Read("articles/bias/index.html");

        Assert.Contains(
            "<link rel=\"canonical\" href=\"https://ulfbou.github.io/articles/bias/\">",
            html);
        Assert.Contains(
            "<meta name=\"description\"",
            html);
        Assert.Contains(
            "<meta property=\"og:type\" content=\"article\">",
            html);
        Assert.Contains(
            "<meta property=\"og:title\"",
            html);
        Assert.Contains(
            "<meta property=\"og:description\"",
            html);
        Assert.Contains(
            "<meta property=\"og:url\" content=\"https://ulfbou.github.io/articles/bias/\">",
            html);
        Assert.Contains(
            "<meta property=\"og:image\" content=\"https://ulfbou.github.io/assets/social/articles/bias.png\">",
            html);
        Assert.Contains(
            "<meta name=\"twitter:card\" content=\"summary_large_image\">",
            html);
        Assert.Contains(
            "<script type=\"application/ld+json\">",
            html);

        using var document = JsonDocument.Parse(ExtractJsonLd(html));
        var structuredData = document.RootElement;

        Assert.Equal("https://schema.org", structuredData.GetProperty("@context").GetString());
        Assert.Equal("Article", structuredData.GetProperty("@type").GetString());
        Assert.Equal(
            "https://ulfbou.github.io/articles/bias/",
            structuredData.GetProperty("mainEntityOfPage").GetString());
        Assert.Equal(
            "https://ulfbou.github.io/assets/social/articles/bias.png",
            structuredData.GetProperty("image").GetString());
        Assert.Equal(
            "Ulf Bourelius",
            structuredData.GetProperty("author").GetProperty("name").GetString());
    }

    [Fact]
    public void PublishedArticleEmitsLinkedInCompatibleImageMetadata()
    {
        using var fixture = new Fixture();
        fixture.WriteArticle("published", approved: true);
        fixture.Generate();
        var html = fixture.Read("articles/bias/index.html");
        Assert.Contains("property=\"og:image:type\" content=\"image/png\"", html);
        Assert.Contains("property=\"og:image:width\" content=\"1200\"", html);
        Assert.Contains("property=\"og:image:height\" content=\"627\"", html);
        Assert.Contains("property=\"og:image:alt\" content=\"Bias article social preview.\"", html);
    }
    [Fact]
    public void PublishedArticleContainsOneCanonicalLinkedInShareLink()
    {
        using var fixture = new Fixture();
        fixture.WriteArticle("published", approved: true);
        fixture.Generate();

        var html = fixture.Read("articles/bias/index.html");
        const string expectedUrl =
            "https://www.linkedin.com/sharing/share-offsite/" +
            "?url=https%3A%2F%2Fulfbou.github.io%2Farticles%2Fbias%2F";

        Assert.Equal(1, CountOccurrences(html, "class=\"article-sharing\""));
        Assert.Equal(1, CountOccurrences(html, expectedUrl));
        Assert.Equal(1, CountOccurrences(html, "Share on LinkedIn"));
        Assert.Contains("aria-label=\"Share this article\"", html);
        Assert.Contains("target=\"_blank\"", html);
        Assert.Contains("rel=\"noopener noreferrer\"", html);
    }

    [Theory]
    [InlineData("draft")]
    [InlineData("candidate")]
    [InlineData("planned")]
    [InlineData("withdrawn")]
    [InlineData("private")]
    public void NonpublishedArticleProducesNoLinkedInShareControl(string status)
    {
        using var fixture = new Fixture();
        fixture.WriteArticle(status, approved: false);
        fixture.Generate();

        var indexHtml = fixture.Read("articles/index.html");

        Assert.DoesNotContain("Share on LinkedIn", indexHtml);
        Assert.DoesNotContain(
            "linkedin.com/sharing/share-offsite",
            indexHtml,
            StringComparison.OrdinalIgnoreCase);
        Assert.False(fixture.Exists("articles/bias/index.html"));
    }

    [Fact]
    public void LinkedInSharingIntroducesNoApiSdkOrClientScript()
    {
        using var fixture = new Fixture();
        fixture.WriteArticle("published", approved: true);
        fixture.Generate();

        var html = fixture.Read("articles/bias/index.html");

        Assert.DoesNotContain("platform.linkedin.com", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("api.linkedin.com", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("oauth", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<script src=", html, StringComparison.OrdinalIgnoreCase);
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
    public void GeneratedPagesUseSharedIdentityAndRemainStatic()
    {
        using var fixture = new Fixture();
        fixture.WriteArticle("published", approved: true);
        fixture.Generate();

        foreach (var relative in new[] { "articles/index.html", "articles/bias/index.html" })
        {
            var html = fixture.Read(relative);
            Assert.Contains("class=\"masthead\"", html);
            Assert.Contains("class=\"monogram\"", html);
            Assert.Contains("href=\"/css/articles.css\"", html);
            Assert.Contains("aria-current=\"page\">Articles", html);
            Assert.DoesNotContain("_framework/blazor", html, StringComparison.OrdinalIgnoreCase);
        }
        var article = fixture.Read("articles/bias/index.html");
        Assert.Contains("class=\"article-deck\"", article);
        Assert.Contains("class=\"article-meta\"", article);
        Assert.Contains("<article", article);
        Assert.Contains("href=\"/css/app.css\"", article);
        Assert.Contains("href=\"/css/accessibility.css\"", article);
        Assert.Contains("href=\"/css/articles.css\"", article);
        Assert.DoesNotContain("<script src=", article);
        Assert.DoesNotContain("blazor", article, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApprovedMarkdownLinkBecomesEncodedAnchorAndUnsafeLinkDoesNot()
    {
        using var fixture = new Fixture();
        fixture.WriteBody("# Bias\n\n[Useful source](https://example.com/a?b=1&c=2)\n\n[Unsafe](javascript:alert(1))\n");
        fixture.WriteArticle("published", approved: true);
        fixture.Generate();

        var html = fixture.Read("articles/bias/index.html");
        Assert.Contains("<a href=\"https://example.com/a?b=1&amp;c=2\">Useful source</a>", html);
        Assert.DoesNotContain("href=\"javascript:", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("[Unsafe](javascript:alert(1))", html);
    }

    [Fact]
    public void ApprovedMarkdownEmphasisRendersInsideAndOutsideLinksWithoutChangingHref()
    {
        using var fixture = new Fixture();
        fixture.WriteBody("# Bias\n\nBefore _outside_ after.\n\n[The _journal title_ issue](https://example.com/a_path?b=1&c=2)\n\n[The *other title* issue](https://example.com/plain)\n\n[Plain label](https://example.com/plain)\n\nUnmatched _ delimiter and * delimiter.\n");
        fixture.WriteArticle("published", approved: true);
        fixture.Generate();

        var html = fixture.Read("articles/bias/index.html");
        Assert.Contains("Before <em>outside</em> after.", html);
        Assert.Contains("<a href=\"https://example.com/a_path?b=1&amp;c=2\">The <em>journal title</em> issue</a>", html);
        Assert.Contains("<a href=\"https://example.com/plain\">The <em>other title</em> issue</a>", html);
        Assert.Contains("<a href=\"https://example.com/plain\">Plain label</a>", html);
        Assert.Contains("Unmatched _ delimiter and * delimiter.", html);
    }

    [Fact]
    public void ArticleIndexContainsEditorialMetadata()
    {
        using var fixture = new Fixture();
        fixture.WriteArticle("published", approved: true);
        fixture.Generate();

        var indexHtml = fixture.Read("articles/index.html");

        Assert.Contains("class=\"article-summary\"", indexHtml);
        Assert.Contains("class=\"article-summary-meta\"", indexHtml);
        Assert.Contains("<time datetime=", indexHtml);
        Assert.Contains("min read", indexHtml);
        Assert.Contains("/articles/bias/", indexHtml);
    }

    [Fact]
    public void RepeatedGenerationIsByteDeterministic()
    {
        using var fixture = new Fixture();
        fixture.WriteArticle("published", approved: true);
        fixture.Generate();
        var first = fixture.OutputDigest();
        fixture.Generate();
        Assert.Equal(first, fixture.OutputDigest());
    }

    [Fact]
    public void ArticleStylesContainNarrowPrintAndReducedMotionRules()
    {
        var root = RepositoryRoot();
        var articleCss = File.ReadAllText(Path.Combine(root, "src/Ulfbou.Site/wwwroot/css/articles.css"));
        var accessibilityCss = File.ReadAllText(Path.Combine(root, "src/Ulfbou.Site/wwwroot/css/accessibility.css"));
        Assert.Contains("@media (max-width: 850px)", articleCss);
        Assert.Contains("@media (max-width: 420px)", articleCss);
        Assert.Contains("@media print", articleCss);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", accessibilityCss);
        Assert.Contains("width: min(100%, 72ch)", articleCss);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Ulfbou.Portfolio.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("repository root not found");
    }

    private static int CountOccurrences(string value, string marker)
    {
        var count = 0;
        var offset = 0;

        while ((offset = value.IndexOf(marker, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += marker.Length;
        }

        return count;
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
                var path = Path.Combine(WebRoot, "assets", "social", "articles", "bias.png");
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                WritePngHeader(path, 1200, 627);
            }
        }

        private static void WritePngHeader(string path, int width, int height)
        {
            var bytes = new byte[24];
            new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }.CopyTo(bytes, 0);
            Encoding.ASCII.GetBytes("IHDR").CopyTo(bytes, 12);
            System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(16, 4), width);
            System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(20, 4), height);
            File.WriteAllBytes(path, bytes);
        }
        public void WriteBody(string markdown) =>
            File.WriteAllText(Path.Combine(SourceRoot, "bodies", "bias.md"), markdown, new UTF8Encoding(false));

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
                ["socialImage"] = "/assets/social/articles/bias.png",
                ["socialImageAlt"] = "Bias article social preview.",
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
            ["socialImage"] = "/assets/social/articles/bias.png",
                ["socialImageAlt"] = "Bias article social preview.",
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

    private static string ExtractJsonLd(string html)
    {
        const string startMarker = "<script type=\"application/ld+json\">";
        const string endMarker = "</script>";

        var start = html.IndexOf(startMarker, StringComparison.Ordinal);
        Assert.True(start >= 0, "Article HTML must contain JSON-LD structured data.");

        start += startMarker.Length;

        var end = html.IndexOf(endMarker, start, StringComparison.Ordinal);
        Assert.True(end >= 0, "The JSON-LD script element must be closed.");

        return html[start..end];
    }
}
