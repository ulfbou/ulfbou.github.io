using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using Ulfbou.Articles;

return ArticlePublisher.Run(args);

public static class ArticlePublisher
{
    private const string SchemaVersion = "1.0";
    private const string EmptyFeedUpdated = "1970-01-01T00:00:00Z";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };
    private static readonly UTF8Encoding Utf8 = new(false);
    private static readonly Regex Slug = new("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant);
    private static readonly Regex Link = new(@"\[([^\]]+)\]\((https?://[^\s)]+)\)", RegexOptions.CultureInvariant);
    private static readonly HashSet<string> AllowedStatuses = new(
        ["draft", "candidate", "planned", "published", "withdrawn", "private"],
        StringComparer.Ordinal);

    public static int Run(string[] args)
    {
        if (args.Length != 3)
        {
            Console.Error.WriteLine("Usage: Ulfbou.Articles <content/articles> <wwwroot/articles> <wwwroot>");
            return 2;
        }

        try
        {
            Generate(Path.GetFullPath(args[0]), Path.GetFullPath(args[1]), Path.GetFullPath(args[2]));
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error.Message);
            return 1;
        }
    }

    public static void Generate(
        string sourceRoot,
        string articlesRoot,
        string webRoot,
        int? failAfterReplacement = null)
    {
        sourceRoot = Path.GetFullPath(sourceRoot);
        articlesRoot = Path.GetFullPath(articlesRoot);
        webRoot = Path.GetFullPath(webRoot);
        RequireOwnedPath(webRoot, articlesRoot, "articles");

        var registry = Load<ArticleRegistry>(Path.Combine(sourceRoot, "articles.json"));
        var series = Load<SeriesRegistry>(Path.Combine(sourceRoot, "series.json"));
        Validate(sourceRoot, registry, series, webRoot);

        var published = registry.Articles
            .Where(article => article.Status == "published")
            .OrderByDescending(article => article.PublishedDate, StringComparer.Ordinal)
            .ThenBy(article => article.Id, StringComparer.Ordinal)
            .ToArray();
        var seriesById = series.Series.ToDictionary(item => item.Id, StringComparer.Ordinal);

        var stagingRoot = Path.Combine(
            Path.GetDirectoryName(webRoot) ?? throw new InvalidOperationException("ARTICLE015: web root has no parent"),
            $".{Path.GetFileName(webRoot)}.articles-stage-{Guid.NewGuid():N}");
        var stagingArticles = Path.Combine(stagingRoot, "articles");
        var stagingProjection = Path.Combine(stagingRoot, "data", "generated", "articles.json");
        var stagingSitemap = Path.Combine(stagingRoot, "sitemap.xml");

        try
        {
            Directory.CreateDirectory(stagingArticles);
            Write(Path.Combine(stagingArticles, "index.html"), IndexPage(published));
            Write(Path.Combine(stagingArticles, "feed.xml"), Feed(published));
            foreach (var article in published)
            {
                var markdown = File.ReadAllText(SafePath(sourceRoot, article.BodyPath), Encoding.UTF8);
                Write(
                    Path.Combine(stagingArticles, article.Slug, "index.html"),
                    ArticlePage(article, seriesById[article.SeriesId], markdown));
            }

            var projection = new PublicationProjection(
                SchemaVersion,
                published.Select(article => new PublishedArticle(
                    article.Id,
                    $"articles.json#{article.Id}",
                    article.BodyPath,
                    article.Status,
                    article.Slug,
                    article.CanonicalPath,
                    $"articles/{article.Slug}/index.html",
                    article.Title,
                    article.Description,
                    article.PublishedDate!,
                    article.ModifiedDate!,
                    article.SeriesId,
                    article.SeriesPart,
                    Hash(File.ReadAllBytes(SafePath(sourceRoot, article.BodyPath))))).ToArray());
            Write(
                stagingProjection,
                JsonSerializer.Serialize(
                    projection,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true }) + "\n");
            Write(stagingSitemap, Sitemap(published));

            ValidateStagedOutput(stagingRoot, published);
            ReplaceOwnedOutputs(
                webRoot,
                stagingRoot,
                [
                    new OwnedOutput("articles", true),
                    new OwnedOutput("data/generated/articles.json", false),
                    new OwnedOutput("sitemap.xml", false)
                ],
                failAfterReplacement);
        }
        finally
        {
            if (Directory.Exists(stagingRoot)) Directory.Delete(stagingRoot, true);
        }
    }

    private static T Load<T>(string path) =>
        JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), JsonOptions)
        ?? throw new InvalidDataException($"ARTICLE001: null document: {path}");

    private static void Validate(
        string sourceRoot,
        ArticleRegistry registry,
        SeriesRegistry series,
        string webRoot)
    {
        if (registry.SchemaVersion != SchemaVersion || series.SchemaVersion != SchemaVersion)
            throw new InvalidDataException("ARTICLE002: unsupported registry schemaVersion");
        if (registry.Articles is null || series.Series is null)
            throw new InvalidDataException("ARTICLE016: required collection is null");

        Unique(registry.Articles.Select(item => item.Id), "article id");
        Unique(registry.Articles.Select(item => item.CanonicalPath), "canonical route");
        Unique(series.Series.Select(item => item.Id), "series id");
        var seriesIds = series.Series.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var item in series.Series)
        {
            Required(item.Id, "series id");
            Required(item.Title, "series title", item.Id);
            Required(item.Description, "series description", item.Id);
            if (item.Status is not ("ongoing" or "complete"))
                throw new InvalidDataException($"ARTICLE026: invalid series status for {item.Id}");
        }

        foreach (var article in registry.Articles)
        {
            Required(article.Id, "id");
            if (!AllowedStatuses.Contains(article.Status))
                throw new InvalidDataException($"ARTICLE003: invalid status for {article.Id}");
            if (!Slug.IsMatch(article.Slug) || article.CanonicalPath != $"/articles/{article.Slug}/")
                throw new InvalidDataException($"ARTICLE004: invalid slug or canonical route for {article.Id}");
            if (!article.CanonicalPath.StartsWith("/articles/", StringComparison.Ordinal))
                throw new InvalidDataException($"ARTICLE005: route outside /articles/: {article.Id}");
            if (!seriesIds.Contains(article.SeriesId))
                throw new InvalidDataException($"ARTICLE006: unknown series for {article.Id}");

            var body = SafePath(sourceRoot, article.BodyPath);
            if (!File.Exists(body) || !string.Equals(Path.GetExtension(body), ".md", StringComparison.Ordinal))
                throw new InvalidDataException($"ARTICLE007: missing Markdown body for {article.Id}");

            if (article.Status == "published")
                ValidatePublished(article, webRoot);
            else
                ValidateNotPublished(article);
        }
    }

    private static void ValidatePublished(Article article, string webRoot)
    {
        if (!article.EditorialApproval.Approved)
            throw new InvalidDataException($"ARTICLE009: published article is not approved: {article.Id}");
        Required(article.EditorialApproval.ApprovedBy, "approvedBy", article.Id);

        var published = ExactDate(article.PublishedDate, "publishedDate", article.Id);
        var modified = ExactDate(article.ModifiedDate, "modifiedDate", article.Id);
        var approved = ExactDate(article.EditorialApproval.ApprovedDate, "approvedDate", article.Id);
        if (modified < published)
            throw new InvalidDataException($"ARTICLE008: modifiedDate predates publishedDate for {article.Id}");
        if (approved < modified)
            throw new InvalidDataException($"ARTICLE017: approvedDate predates finalized modifiedDate for {article.Id}");

        Required(article.Title, "title", article.Id);
        Required(article.Deck, "deck", article.Id);
        Required(article.Description, "description", article.Id);
        Required(article.Author, "author", article.Id);
        Required(article.Language, "language", article.Id);
        if (article.ReadingMinutes < 1)
            throw new InvalidDataException($"ARTICLE009: invalid readingMinutes for {article.Id}");

        if (!article.SocialImage.StartsWith("/assets/social/articles/", StringComparison.Ordinal))
            throw new InvalidDataException($"ARTICLE010: social image is outside article assets for {article.Id}");
        var asset = SafePath(webRoot, article.SocialImage.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(asset))
            throw new InvalidDataException($"ARTICLE010: missing social image for {article.Id}");
    }

    private static void ValidateNotPublished(Article article)
    {
        if (article.EditorialApproval.Approved
            || !string.IsNullOrWhiteSpace(article.EditorialApproval.ApprovedBy)
            || !string.IsNullOrWhiteSpace(article.EditorialApproval.ApprovedDate))
            throw new InvalidDataException($"ARTICLE018: nonpublished article impersonates completed approval: {article.Id}");
        if (IsPlaceholder(article.PublishedDate) || IsPlaceholder(article.ModifiedDate))
            throw new InvalidDataException($"ARTICLE019: placeholder date for {article.Id}");
    }

    private static DateOnly ExactDate(string? value, string field, string id)
    {
        if (IsPlaceholder(value)
            || !DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            throw new InvalidDataException($"ARTICLE008: invalid {field} for {id}");
        return date;
    }

    private static bool IsPlaceholder(string? value) =>
        value is not null && value.Contains("YYYY", StringComparison.OrdinalIgnoreCase);

    private static void Required(string? value, string field, string? id = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidDataException($"ARTICLE020: missing {field}{(id is null ? string.Empty : $" for {id}")}");
    }

    private static string SafePath(string root, string relative)
    {
        if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative))
            throw new InvalidDataException($"ARTICLE011: invalid relative path: {relative}");
        var full = Path.GetFullPath(Path.Combine(root, relative));
        var prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!full.StartsWith(prefix, StringComparison.Ordinal))
            throw new InvalidDataException($"ARTICLE012: escaping path: {relative}");
        return full;
    }

    private static void RequireOwnedPath(string root, string path, string expectedRelative)
    {
        var expected = Path.GetFullPath(Path.Combine(root, expectedRelative));
        if (!string.Equals(expected, path, StringComparison.Ordinal))
            throw new InvalidDataException($"ARTICLE021: output path is not owned: {path}");
    }

    private static void Unique(IEnumerable<string> values, string name)
    {
        if (values.GroupBy(value => value, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
            throw new InvalidDataException($"ARTICLE013: duplicate {name}");
    }

    private static void ValidateStagedOutput(string stagingRoot, Article[] published)
    {
        var articles = Path.Combine(stagingRoot, "articles");
        var projection = Path.Combine(stagingRoot, "data", "generated", "articles.json");
        var sitemap = Path.Combine(stagingRoot, "sitemap.xml");
        foreach (var required in new[] { Path.Combine(articles, "index.html"), Path.Combine(articles, "feed.xml"), projection, sitemap })
            if (!File.Exists(required)) throw new InvalidDataException($"ARTICLE022: staged output missing: {required}");

        var parsed = Load<PublicationProjection>(projection);
        if (parsed.Articles.Length != published.Length || parsed.Articles.Any(item => item.Status != "published"))
            throw new InvalidDataException("ARTICLE023: staged projection differs from publication set");
        var feed = File.ReadAllText(Path.Combine(articles, "feed.xml"), Encoding.UTF8);
        var map = File.ReadAllText(sitemap, Encoding.UTF8);
        foreach (var article in published)
        {
            if (!File.Exists(Path.Combine(articles, article.Slug, "index.html"))
                || !feed.Contains(article.CanonicalPath, StringComparison.Ordinal)
                || !map.Contains(article.CanonicalPath, StringComparison.Ordinal))
                throw new InvalidDataException($"ARTICLE024: incomplete staged publication for {article.Id}");
        }
    }

    private static void ReplaceOwnedOutputs(
        string webRoot,
        string stagingRoot,
        OwnedOutput[] outputs,
        int? failAfterReplacement)
    {
        var backupRoot = Path.Combine(
            Path.GetDirectoryName(webRoot)!,
            $".{Path.GetFileName(webRoot)}.articles-backup-{Guid.NewGuid():N}");
        var replaced = 0;
        var processed = new List<OwnedOutput>();
        Directory.CreateDirectory(backupRoot);
        try
        {
            foreach (var output in outputs)
            {
                var destination = Path.Combine(webRoot, output.RelativePath.Replace('/', Path.DirectorySeparatorChar));
                var staged = Path.Combine(stagingRoot, output.RelativePath.Replace('/', Path.DirectorySeparatorChar));
                var backup = Path.Combine(backupRoot, output.RelativePath.Replace('/', Path.DirectorySeparatorChar));
                if (Exists(destination, output.Directory)) Move(destination, backup, output.Directory);
                processed.Add(output);
                Move(staged, destination, output.Directory);
                replaced++;
                if (failAfterReplacement == replaced)
                    throw new IOException($"ARTICLE025: injected replacement failure after {replaced} output(s)");
            }
            Directory.Delete(backupRoot, true);
        }
        catch
        {
            foreach (var output in processed.AsEnumerable().Reverse())
            {
                var destination = Path.Combine(webRoot, output.RelativePath.Replace('/', Path.DirectorySeparatorChar));
                var backup = Path.Combine(backupRoot, output.RelativePath.Replace('/', Path.DirectorySeparatorChar));
                Delete(destination, output.Directory);
                if (Exists(backup, output.Directory)) Move(backup, destination, output.Directory);
            }
            if (Directory.Exists(backupRoot)) Directory.Delete(backupRoot, true);
            throw;
        }
    }

    private static bool Exists(string path, bool directory) => directory ? Directory.Exists(path) : File.Exists(path);
    private static void Delete(string path, bool directory)
    {
        if (directory && Directory.Exists(path)) Directory.Delete(path, true);
        if (!directory && File.Exists(path)) File.Delete(path);
    }
    private static void Move(string source, string destination, bool directory)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        if (directory) Directory.Move(source, destination); else File.Move(source, destination);
    }

    private static void Write(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content.Replace("\r\n", "\n"), Utf8);
    }

    private static string E(string value) => WebUtility.HtmlEncode(value);

    private static string Layout(
        string title,
        string description,
        string canonical,
        string body,
        string? image = null,
        string? jsonLd = null)
    {
        var social = image is null
            ? string.Empty
            : $"<meta property=\"og:type\" content=\"article\"><meta property=\"og:image\" content=\"https://ulfbou.github.io{E(image)}\"><meta name=\"twitter:card\" content=\"summary_large_image\">";
        var structured = jsonLd is null ? string.Empty : $"<script type=\"application/ld+json\">{jsonLd}</script>";
        return $"<!doctype html>\n<html lang=\"en\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><title>{E(title)}</title><meta name=\"description\" content=\"{E(description)}\"><link rel=\"canonical\" href=\"https://ulfbou.github.io{E(canonical)}\"><meta property=\"og:title\" content=\"{E(title)}\"><meta property=\"og:description\" content=\"{E(description)}\"><meta property=\"og:url\" content=\"https://ulfbou.github.io{E(canonical)}\">{social}<link rel=\"stylesheet\" href=\"/css/app.css\"><link rel=\"stylesheet\" href=\"/css/accessibility.css\"><link rel=\"stylesheet\" href=\"/css/articles.css\">{structured}</head><body><a class=\"skip-link\" href=\"#main-content\">Skip to content</a><header class=\"masthead\"><a class=\"identity\" href=\"/\"><span class=\"monogram\" aria-hidden=\"true\">UB</span><span>Ulf Bourelius<small>Cloud · AI · software systems</small></span></a><nav aria-label=\"Primary navigation\"><a href=\"/\">Home</a><a href=\"/work/\">Work</a><a href=\"/articles/\" aria-current=\"page\">Articles</a></nav></header>{body}<footer><span>Ulf Bourelius · Stockholm</span><span>Durable technical writing.</span><a href=\"#main-content\">Back to top ↑</a></footer></body></html>\n";
    }

    private static string IndexPage(Article[] articles)
    {
        var list = articles.Length == 0
            ? "<p class=\"empty-state\">No articles are currently published.</p>"
            : "<div class=\"article-stack\">" + string.Join(string.Empty, articles.Select((article, index) => $"<article class=\"article-summary\"><span class=\"article-index\">{index + 1:00}</span><div class=\"article-summary-copy\"><p class=\"overline\">{E(article.SeriesId.Replace('-', ' '))} · Part {article.SeriesPart}</p><h2><a href=\"{E(article.CanonicalPath)}\">{E(article.Title)}</a></h2><p>{E(article.Deck)}</p></div><p class=\"article-summary-meta\"><time datetime=\"{article.PublishedDate}\">{article.PublishedDate}</time><span>{article.ReadingMinutes} min read</span></p><a class=\"article-explore\" href=\"{E(article.CanonicalPath)}\" aria-label=\"Read {E(article.Title)}\">Read</a></article>")) + "</div>";
        return Layout(
            "Articles · Ulf Bourelius",
            "Durable technical writing by Ulf Bourelius.",
            "/articles/",
            "<main id=\"main-content\"><section class=\"article-hero\" aria-labelledby=\"articles-title\"><p class=\"overline\">Technical articles</p><h1 id=\"articles-title\">Articles</h1><p class=\"intro\">Technical explanations, architecture analyses, and reflections.</p></section><section class=\"articles-field\" aria-label=\"Published articles\">" + list + "</section></main>");
    }

    private static string ArticlePage(Article article, Series series, string markdown)
    {
        var canonicalUrl = $"https://ulfbou.github.io{article.CanonicalPath}";
        var socialImageUrl = $"https://ulfbou.github.io{article.SocialImage}";

        var data = JsonSerializer.Serialize(
            new Dictionary<string, object?>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "Article",
                ["headline"] = article.Title,
                ["description"] = article.Description,
                ["author"] = new Dictionary<string, object?>
                {
                    ["@type"] = "Person",
                    ["name"] = article.Author
                },
                ["datePublished"] = article.PublishedDate,
                ["dateModified"] = article.ModifiedDate,
                ["mainEntityOfPage"] = canonicalUrl,
                ["image"] = socialImageUrl
            },
            new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Default
            });
        var updated = article.ModifiedDate == article.PublishedDate
            ? string.Empty
            : $"<span>Updated <time datetime=\"{article.ModifiedDate}\">{article.ModifiedDate}</time></span>";
        var body = $"<main id=\"main-content\" class=\"article-page\"><article><header class=\"article-header\"><a class=\"back-link\" href=\"/articles/\">← All articles</a><p class=\"overline\">{E(series.Title)} · Part {article.SeriesPart}</p><h1>{E(article.Title)}</h1><p class=\"article-deck\">{E(article.Deck)}</p><p class=\"article-meta\"><span>By {E(article.Author)}</span><span>Published <time datetime=\"{article.PublishedDate}\">{article.PublishedDate}</time></span>{updated}<span>{article.ReadingMinutes} min read</span></p></header><div class=\"article-body\">{Markdown(markdown, suppressTitle: true)}</div></article></main>";

        return Layout(article.Title, article.Description, article.CanonicalPath, body, article.SocialImage, data);
    }

    private static string Markdown(string markdown, bool suppressTitle = false)
    {
        if (markdown.Contains('<')) throw new InvalidDataException("ARTICLE014: raw HTML is not allowed");
        var result = new StringBuilder();
        var list = false;
        foreach (var raw in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            var line = raw.TrimEnd();
            if (line.StartsWith("- "))
            {
                if (!list) { result.Append("<ul>"); list = true; }
                result.Append("<li>").Append(Inline(line[2..])).Append("</li>");
                continue;
            }
            if (list) { result.Append("</ul>"); list = false; }
            if (line.Length == 0) continue;
            if (line.StartsWith("### "))
            {
                var heading = Inline(line[4..]);
                var css = string.Equals(line[4..], "Sources", StringComparison.OrdinalIgnoreCase) ? " class=\"sources-heading\"" : string.Empty;
                result.Append("<h3").Append(css).Append(">").Append(heading).Append("</h3>");
            }
            else if (line.StartsWith("## ")) result.Append("<h2>").Append(Inline(line[3..])).Append("</h2>");
            else if (line.StartsWith("# "))
            {
                if (!suppressTitle) result.Append("<h1>").Append(Inline(line[2..])).Append("</h1>");
            }
            else result.Append("<p>").Append(Inline(line)).Append("</p>");
        }
        if (list) result.Append("</ul>");
        return result.ToString();
    }

    private static string Inline(string value)
    {
        var encoded = E(value);
        return Link.Replace(encoded, match => $"<a href=\"{match.Groups[2].Value}\">{match.Groups[1].Value}</a>");
    }

    private static string Feed(Article[] articles)
    {
        var updated = articles.Length == 0
            ? EmptyFeedUpdated
            : articles.OrderBy(article => article.ModifiedDate, StringComparer.Ordinal).Last().ModifiedDate + "T00:00:00Z";
        return "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<feed xmlns=\"http://www.w3.org/2005/Atom\"><id>https://ulfbou.github.io/articles/</id><title>Ulf Bourelius articles</title><updated>" + updated + "</updated><link href=\"https://ulfbou.github.io/articles/feed.xml\" rel=\"self\"/>" + string.Join(string.Empty, articles.Select(article => $"<entry><id>https://ulfbou.github.io{article.CanonicalPath}</id><title>{E(article.Title)}</title><link href=\"https://ulfbou.github.io{article.CanonicalPath}\"/><updated>{article.ModifiedDate}T00:00:00Z</updated><published>{article.PublishedDate}T00:00:00Z</published><summary>{E(article.Description)}</summary></entry>")) + "</feed>\n";
    }

    private static string Sitemap(Article[] articles)
    {
        var paths = new[] { "/", "/work/", "/aws/", "/articles/" }.Concat(articles.Select(article => article.CanonicalPath));
        return "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">" + string.Join(string.Empty, paths.Select(path => $"<url><loc>https://ulfbou.github.io{path}</loc></url>")) + "</urlset>\n";
    }

    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

    private sealed record OwnedOutput(string RelativePath, bool Directory);
}
